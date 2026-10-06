#include "pch.h"
#include "WindowsVoice.h"

#include <sapi.h>
#include <atlbase.h>
#pragma warning(push)
#pragma warning(disable:4996)
#include <sphelper.h>
#pragma warning(pop)

#include <atomic>
#include <cwchar>
#include <list>
#include <mutex>
#include <string>
#include <thread>

using namespace std;

namespace WindowsVoice
{
	// --- Status reporting ---

	// What the speech thread reports back to the caller.
	class SpeechStatus
	{
	public:
		wstring getMessage() const
		{
			lock_guard<mutex> lock(messageMutex);
			return message;
		}

		void setMessage(const wstring& text)
		{
			lock_guard<mutex> lock(messageMutex);
			message = text;
		}

		speech_state_enum getState() const { return state; }

		void set(const speech_state_enum newState, const wstring& text)
		{
			setMessage(text);
			state = newState;
		}

		void setError(const wchar_t* text, const HRESULT hr)
		{
			wchar_t code[11]; // "0x" + 8 hex digits + null
			swprintf_s(code, _countof(code), L"0x%08X", static_cast<unsigned int>(hr));
			set(speech_state_enum::error, wstring(L"Error: ") + text + L" (HRESULT " + code + L")");
		}

		ULONG getWordLength() const { return wordLength; }
		ULONG getWordPosition() const { return wordPosition; }

		void setWordProgress(const ULONG length, const ULONG position)
		{
			wordLength = length;
			wordPosition = position;
		}

		void resetWordProgress() { setWordProgress(0, 0); }

	private:
		mutable mutex messageMutex; // guards message
		wstring message{ L"WindowsVoice not yet initialized!" };
		atomic<speech_state_enum> state{ speech_state_enum::uninitialized };
		atomic<ULONG> wordLength{ 0 };
		atomic<ULONG> wordPosition{ 0 };
	};

	// --- Speech queue ---

	// Lines waiting to be spoken, shared between the caller's thread and the speech thread.
	class SpeechQueue
	{
	public:
		void add(const wchar_t* text)
		{
			// Empty lines are rejected so dequeue() can use an empty string to mean "none".
			if (text == nullptr || *text == L'\0')
				return;

			lock_guard<mutex> lock(queueMutex);
			lines.emplace_back(text);
		}

		void clear()
		{
			lock_guard<mutex> lock(queueMutex);
			lines.clear();
		}

		// Drops the queued lines and asks the speech thread to cut off the current one.
		void stop()
		{
			lock_guard<mutex> lock(queueMutex);
			lines.clear();
			stopRequested = true;
		}

		// Drops the queued lines and any pending stop request.
		void reset()
		{
			lock_guard<mutex> lock(queueMutex);
			lines.clear();
			stopRequested = false;
		}

		// Returns whether a stop was requested, and clears the request.
		bool takeStopRequest()
		{
			lock_guard<mutex> lock(queueMutex);
			const bool stop = stopRequested;
			stopRequested = false;
			return stop;
		}

		// Pops the next line to speak, or returns an empty string if there is none.
		wstring dequeue()
		{
			lock_guard<mutex> lock(queueMutex);
			// Wait for the pending stop to purge first, or it would cut off this line.
			if (stopRequested || lines.empty())
				return wstring();

			wstring line = move(lines.front());
			lines.pop_front();
			return line;
		}

		// Drops the line if it was queued again while playing.
		void dropRequeued(const wstring& line)
		{
			lock_guard<mutex> lock(queueMutex);
			// Skipped when a stop is pending: anything queued after the stop must play.
			if (!stopRequested && !lines.empty() && lines.front() == line)
				lines.pop_front();
		}

	private:
		mutex queueMutex; // guards lines and stopRequested
		list<wstring> lines;
		bool stopRequested = false;
	};

	// --- Speech thread ---

	// Initializes COM on the calling thread for the lifetime of the object.
	class ComScope
	{
	public:
		ComScope() : hr(::CoInitializeEx(nullptr, COINIT_MULTITHREADED)) {}
		~ComScope() { if (SUCCEEDED(hr)) ::CoUninitialize(); }
		ComScope(const ComScope&) = delete;
		ComScope& operator=(const ComScope&) = delete;

		const HRESULT hr;
	};

	// Feeds queued lines to a SAPI voice and reports its progress.
	class SpeechPlayer
	{
	public:
		SpeechPlayer(ISpVoice* voice, SpeechQueue& queue, SpeechStatus& status)
			: voice(voice), queue(queue), status(status) {}

		void run(const atomic<bool>& shouldTerminate)
		{
			while (!shouldTerminate)
			{
				if (queue.takeStopRequest())
					purge();

				// Busy until SAPI has finished everything queued. Checking for SPRS_IS_SPEAKING instead
				// would treat the gap between Speak() and audio starting as idle and lose the current line.
				if (voice->WaitUntilDone(0) == S_OK)
					onIdle();
				else
					onSpeaking();

				Sleep(50);
			}
			voice->Pause();
		}

	private:
		ISpVoice* voice;
		SpeechQueue& queue;
		SpeechStatus& status;
		wstring currentLine;

		void purge()
		{
			voice->Speak(nullptr, SPF_PURGEBEFORESPEAK, nullptr);
			voice->WaitUntilDone(100);
			currentLine.clear();
			status.resetWordProgress();
		}

		void onSpeaking()
		{
			SPVOICESTATUS voiceStatus;
			if (SUCCEEDED(voice->GetStatus(&voiceStatus, nullptr)))
				status.setWordProgress(voiceStatus.ulInputWordLen, voiceStatus.ulInputWordPos);

			// Empty while a purge is still finishing; nothing to compare against then.
			if (!currentLine.empty())
				queue.dropRequeued(currentLine);
		}

		void onIdle()
		{
			currentLine = queue.dequeue();
			if (currentLine.empty())
			{
				status.set(speech_state_enum::ready, L"Waiting");
				return;
			}

			// Rebind to the current default output device in case it changed since the last line.
			voice->SetOutput(nullptr, TRUE);
			voice->Speak(currentLine.c_str(), SPF_IS_XML | SPF_ASYNC, nullptr);
			// Reported now rather than on the next tick, so callers never see the new line as idle.
			status.set(speech_state_enum::speaking, L"Speaking: " + currentLine);
		}
	};

	SpeechQueue theQueue;
	SpeechStatus theStatus;
	thread* theSpeechThread = nullptr; // never destroyed while joinable, which would call terminate()
	atomic<bool> shouldTerminate{ false };

	void speechThreadFunc(const int rate, const int volume)
	{
		const ComScope com;
		if (FAILED(com.hr))
		{
			theStatus.setError(L"Failed to initialize COM for Voice.", com.hr);
			return;
		}

		{
			// Scoped so the voice is released before COM is uninitialized.
			CComPtr<ISpVoice> voice;
			const HRESULT hr = voice.CoCreateInstance(CLSID_SpVoice);
			if (FAILED(hr))
			{
				theStatus.setError(L"Failed to create Voice instance.", hr);
				return;
			}

			voice->SetRate(rate);
			voice->SetVolume(volume);
			theStatus.set(speech_state_enum::ready, L"Speech ready.");

			SpeechPlayer(voice, theQueue, theStatus).run(shouldTerminate);
		}

		theStatus.set(speech_state_enum::terminated, L"Speech thread terminated.");
	}

	// --- Exported API ---

	void addToSpeechQueue(const wchar_t* text)
	{
		theQueue.add(text);
	}

	void clearSpeechQueue()
	{
		theQueue.clear();
	}

	void stopSpeech()
	{
		theQueue.stop();
	}

	void initSpeech(int rate, int volume)
	{
		if (theSpeechThread != nullptr)
		{
			theStatus.setMessage(L"Windows Voice thread already started.");
			return;
		}
		shouldTerminate = false;
		theStatus.setMessage(L"Starting Windows Voice.");
		theSpeechThread = new thread(speechThreadFunc, rate, volume);
	}

	void destroySpeech()
	{
		if (theSpeechThread == nullptr)
		{
			theStatus.setMessage(L"Warning: Speech thread already destroyed or not started.");
			return;
		}
		theStatus.setMessage(L"Destroying speech.");
		shouldTerminate = true;
		theSpeechThread->join();
		delete theSpeechThread;
		theSpeechThread = nullptr;
		theQueue.reset();
		theStatus.resetWordProgress();
		theStatus.set(speech_state_enum::uninitialized, L"Speech destroyed.");
	}

	BSTR getStatusMessage()
	{
		return SysAllocString(theStatus.getMessage().c_str());
	}

	UINT32 getSpeechState()
	{
		return static_cast<UINT32>(theStatus.getState());
	}

	// --- Voice enumeration ---

	// English display name of the language, e.g. "English (United States)", or empty on failure.
	wstring getLocaleName(const LANGID langid)
	{
		const int size = GetLocaleInfoW(langid, LOCALE_SENGLISHDISPLAYNAME, nullptr, 0);
		if (size == 0)
			return wstring();

		wstring localeName(size, L'\0');
		GetLocaleInfoW(langid, LOCALE_SENGLISHDISPLAYNAME, &localeName[0], size);
		localeName.resize(size - 1); // drop the terminating null
		return localeName;
	}

	// Inserts "Natural" before '(', e.g. "English Natural (United States)".
	void markNatural(wstring& localeName)
	{
		const size_t pos = localeName.find(L'(');
		if (pos != wstring::npos)
			localeName.insert(pos, L"Natural ");
		else
			localeName.append(L" Natural");
	}

	// Describes a voice as "Name#Locale", or returns empty if it can't be described.
	wstring describeVoice(ISpObjectToken* token)
	{
		// try to get the Name attribute first; if failed, get the description
		CSpDynamicString voiceName;
		CComPtr<ISpDataKey> attribs;
		HRESULT hr = token->OpenKey(SPTOKENKEY_ATTRIBUTES, &attribs);
		if (SUCCEEDED(hr))
			hr = attribs->GetStringValue(L"Name", &voiceName);
		if (FAILED(hr))
			hr = SpGetDescription(token, &voiceName);

		LANGID langid;
		if (FAILED(hr) || FAILED(SpGetLanguageFromVoiceToken(token, &langid)))
			return wstring();

		wstring localeName = getLocaleName(langid);
		if (localeName.empty())
			return wstring();

		// if the voice has attribute "NaturalVoiceType", consider it "natural"
		CSpDynamicString naturalVoiceType;
		if (attribs != nullptr && SUCCEEDED(attribs->GetStringValue(L"NaturalVoiceType", &naturalVoiceType)))
			markNatural(localeName);

		wstring description(static_cast<const wchar_t*>(voiceName));
		description += L'#';
		description += localeName;
		return description;
	}

	BSTR getVoicesAvailable()
	{
		wstring voices;
		CComPtr<ISpObjectTokenCategory> category;
		CComPtr<IEnumSpObjectTokens> tokens;
		if (SUCCEEDED(SpGetCategoryFromId(SPCAT_VOICES, &category)) &&
			SUCCEEDED(category->EnumTokens(nullptr, nullptr, &tokens)))
		{
			CComPtr<ISpObjectToken> token;
			while (tokens->Next(1, &token, nullptr) == S_OK)
			{
				const wstring voice = describeVoice(token);
				if (!voice.empty())
				{
					voices += voice;
					voices += L'\n';
				}
				token.Release();
			}
		}
		return SysAllocString(voices.c_str());
	}

	UINT32 getWordLength()
	{
		return theStatus.getWordLength();
	}

	UINT32 getWordPosition()
	{
		return theStatus.getWordPosition();
	}
}

BOOL APIENTRY DllMain(HMODULE, DWORD, LPVOID)
{
	return TRUE;
}
