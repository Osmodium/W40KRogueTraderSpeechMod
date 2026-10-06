#include "pch.h"
#include "WindowsVoice.h"

#include <atomic>
#include <cwchar>
#include <list>
#include <mutex>
#include <string>
#include <thread>

using namespace std;

namespace WindowsVoice
{
	mutex theMutex; // guards theSpeechQueue and stopRequested
	list<wstring> theSpeechQueue;
	bool stopRequested = false;
	mutex theStatusMutex; // guards theStatusMessage
	wstring theStatusMessage;
	thread* theSpeechThread = nullptr; // never destroyed while joinable, which would call terminate()
	atomic<bool> shouldTerminate{ false };
	atomic<ULONG> wordLength{ 0 };
	atomic<ULONG> wordPosition{ 0 };
	atomic<speech_state_enum> speechState{ speech_state_enum::uninitialized };

	// --- Status reporting ---

	void setStatusMessage(const wstring& message)
	{
		lock_guard<mutex> lock(theStatusMutex);
		theStatusMessage = message;
	}

	void setState(const speech_state_enum state, const wstring& message)
	{
		setStatusMessage(message);
		speechState = state;
	}

	void setError(const wchar_t* message, const HRESULT hr)
	{
		wchar_t buffer[128];
		swprintf_s(buffer, _countof(buffer), L"Error: %s (HRESULT 0x%08X)", message, static_cast<unsigned int>(hr));
		setState(speech_state_enum::error, buffer);
	}

	void resetWordProgress()
	{
		wordLength = 0;
		wordPosition = 0;
	}

	// --- Speech queue (shared with the caller's thread) ---

	// Returns whether a stop was requested, and clears the request.
	bool takeStopRequest()
	{
		lock_guard<mutex> lock(theMutex);
		const bool stop = stopRequested;
		stopRequested = false;
		return stop;
	}

	// Pops the next line to speak, or returns an empty string if there is none.
	wstring dequeueLine()
	{
		lock_guard<mutex> lock(theMutex);
		// Wait for the pending stop to purge first, or it would cut off this line.
		if (stopRequested || theSpeechQueue.empty())
			return wstring();

		wstring line = move(theSpeechQueue.front());
		theSpeechQueue.pop_front();
		return line;
	}

	// Drops the line if it was queued again while playing.
	void dropRequeuedLine(const wstring& line)
	{
		lock_guard<mutex> lock(theMutex);
		// Skipped when a stop is pending: anything queued after the stop must play.
		if (!stopRequested && !theSpeechQueue.empty() && theSpeechQueue.front() == line)
			theSpeechQueue.pop_front();
	}

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
		explicit SpeechPlayer(ISpVoice* voice) : voice(voice) {}

		void run()
		{
			while (!shouldTerminate)
			{
				if (takeStopRequest())
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
		wstring currentLine;

		void purge()
		{
			voice->Speak(nullptr, SPF_PURGEBEFORESPEAK, nullptr);
			voice->WaitUntilDone(100);
			currentLine.clear();
			resetWordProgress();
			setState(speech_state_enum::ready, L"Speech stopped.");
		}

		void onSpeaking()
		{
			setState(speech_state_enum::speaking, L"Speaking: " + currentLine);

			SPVOICESTATUS voiceStatus;
			if (SUCCEEDED(voice->GetStatus(&voiceStatus, nullptr)))
			{
				wordLength = voiceStatus.ulInputWordLen;
				wordPosition = voiceStatus.ulInputWordPos;
			}

			// Empty while a purge is still finishing; nothing to compare against then.
			if (!currentLine.empty())
				dropRequeuedLine(currentLine);
		}

		void onIdle()
		{
			setState(speech_state_enum::ready, L"Waiting");
			currentLine = dequeueLine();
			if (currentLine.empty())
				return;

			// Rebind to the current default output device in case it changed since the last line.
			voice->SetOutput(nullptr, TRUE);
			voice->Speak(currentLine.c_str(), SPF_IS_XML | SPF_ASYNC, nullptr);
		}
	};

	void speechThreadFunc(const int rate, const int volume)
	{
		const ComScope com;
		if (FAILED(com.hr))
		{
			setError(L"Failed to initialize COM for Voice.", com.hr);
			return;
		}

		{
			// Scoped so the voice is released before COM is uninitialized.
			CComPtr<ISpVoice> voice;
			const HRESULT hr = voice.CoCreateInstance(CLSID_SpVoice);
			if (FAILED(hr))
			{
				setError(L"Failed to create Voice instance.", hr);
				return;
			}

			voice->SetRate(rate);
			voice->SetVolume(volume);
			setState(speech_state_enum::ready, L"Speech ready.");

			SpeechPlayer(voice).run();
		}

		setState(speech_state_enum::terminated, L"Speech thread terminated.");
	}

	// --- Exported API ---

	void addToSpeechQueue(const wchar_t* text)
	{
		if (text == nullptr || *text == L'\0')
			return;

		lock_guard<mutex> lock(theMutex);
		theSpeechQueue.emplace_back(text);
	}

	void clearSpeechQueue()
	{
		lock_guard<mutex> lock(theMutex);
		theSpeechQueue.clear();
	}

	void stopSpeech()
	{
		lock_guard<mutex> lock(theMutex);
		theSpeechQueue.clear();
		stopRequested = true;
	}

	void initSpeech(int rate, int volume)
	{
		if (theSpeechThread != nullptr)
		{
			setStatusMessage(L"Windows Voice thread already started.");
			return;
		}
		{
			lock_guard<mutex> lock(theMutex);
			stopRequested = false;
		}
		shouldTerminate = false;
		setStatusMessage(L"Starting Windows Voice.");
		theSpeechThread = new thread(speechThreadFunc, rate, volume);
	}

	void destroySpeech()
	{
		if (theSpeechThread == nullptr)
		{
			setStatusMessage(L"Warning: Speech thread already destroyed or not started.");
			return;
		}
		setStatusMessage(L"Destroying speech.");
		shouldTerminate = true;
		theSpeechThread->join();
		delete theSpeechThread;
		theSpeechThread = nullptr;
		clearSpeechQueue();
		resetWordProgress();
		setState(speech_state_enum::uninitialized, L"Speech destroyed.");
	}

	BSTR getStatusMessage()
	{
		lock_guard<mutex> lock(theStatusMutex);
		if (theStatusMessage.empty())
		{
			theStatusMessage = L"WindowsVoice not yet initialized!";
		}
		return SysAllocString(theStatusMessage.c_str());
	}

	UINT32 getSpeechState()
	{
		return static_cast<UINT32>(speechState.load());
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
		return wordLength;
	}

	UINT32 getWordPosition()
	{
		return wordPosition;
	}
}

BOOL APIENTRY DllMain(HMODULE, DWORD ul_reason_for_call, LPVOID)
{
	switch (ul_reason_for_call)
	{
	case DLL_PROCESS_ATTACH:
	case DLL_THREAD_ATTACH:
	case DLL_THREAD_DETACH:
	case DLL_PROCESS_DETACH:
		break;
	}

	return TRUE;
}
