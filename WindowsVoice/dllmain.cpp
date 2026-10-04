#include "pch.h"
#include "WindowsVoice.h"

using namespace std;

namespace WindowsVoice
{
	mutex theMutex; // guards theSpeechQueue and stopRequested
	list<wstring> theSpeechQueue;
	bool stopRequested = false;
	mutex theStatusMutex; // guards theStatusMessage
	wstring theStatusMessage;
	thread* theSpeechThread = nullptr;
	atomic<bool> shouldTerminate{ false };
	atomic<ULONG> wordLength{ 0 };
	atomic<ULONG> wordPosition{ 0 };
	atomic<speech_state_enum> speechState{ speech_state_enum::uninitialized };

	void setStatusMessage(const wstring& message)
	{
		lock_guard<mutex> lock(theStatusMutex);
		theStatusMessage = message;
	}

	wstring formatError(const wchar_t* message, const HRESULT hr)
	{
		wchar_t buffer[128];
		swprintf_s(buffer, _countof(buffer), L"Error: %s (HRESULT 0x%08X)", message, static_cast<unsigned int>(hr));
		return buffer;
	}

	void speechThreadFunc(const int rate, const int volume)
	{
		HRESULT hr = ::CoInitializeEx(nullptr, COINIT_MULTITHREADED);
		if (FAILED(hr))
		{
			setStatusMessage(formatError(L"Failed to initialize COM for Voice.", hr));
			speechState = speech_state_enum::error;
			return;
		}

		ISpVoice* pVoice = nullptr;

		hr = CoCreateInstance(CLSID_SpVoice, nullptr, CLSCTX_ALL, IID_ISpVoice, reinterpret_cast<void**>(&pVoice));
		if (FAILED(hr))
		{
			setStatusMessage(formatError(L"Failed to create Voice instance.", hr));
			speechState = speech_state_enum::error;
			CoUninitialize();
			return;
		}

		setStatusMessage(L"Speech ready.");
		speechState = speech_state_enum::ready;

		pVoice->SetRate(rate);
		pVoice->SetVolume(volume);

		SPVOICESTATUS voiceStatus;
		wstring priorText;
		while (!shouldTerminate)
		{
			bool stop;
			{
				lock_guard<mutex> lock(theMutex);
				stop = stopRequested;
				stopRequested = false;
			}
			if (stop)
			{
				pVoice->Speak(nullptr, SPF_PURGEBEFORESPEAK, nullptr);
				pVoice->WaitUntilDone(100);
				priorText.clear();
				wordLength = 0;
				wordPosition = 0;
				setStatusMessage(L"Speech stopped.");
				speechState = speech_state_enum::ready;
			}

			pVoice->GetStatus(&voiceStatus, nullptr);
			if (voiceStatus.dwRunningState == SPRS_IS_SPEAKING)
			{
				if (priorText.empty())
				{
					setStatusMessage(L"Error: SPRS_IS_SPEAKING but text is empty");
					speechState = speech_state_enum::error;
				}
				else
				{
					setStatusMessage(L"Speaking: " + priorText);
					speechState = speech_state_enum::speaking;
					wordLength = voiceStatus.ulInputWordLen;
					wordPosition = voiceStatus.ulInputWordPos;
					// Drop the same line if it was queued again while playing.
					// Skipped when a stop is pending: anything queued after the stop must play.
					lock_guard<mutex> lock(theMutex);
					if (!stopRequested && !theSpeechQueue.empty() && theSpeechQueue.front() == priorText)
					{
						theSpeechQueue.pop_front();
					}
				}
			}
			else
			{
				setStatusMessage(L"Waiting");
				speechState = speech_state_enum::ready;
				priorText.clear();
				{
					lock_guard<mutex> lock(theMutex);
					// Wait for the pending stop to purge first, or it would cut off this line.
					if (!stopRequested && !theSpeechQueue.empty())
					{
						priorText = move(theSpeechQueue.front());
						theSpeechQueue.pop_front();
					}
				}
				if (!priorText.empty())
				{
					// Rebind to the current default output device in case it changed since the last line.
					pVoice->SetOutput(nullptr, TRUE);
					pVoice->Speak(priorText.c_str(), SPF_IS_XML | SPF_ASYNC, nullptr);
				}
			}
			Sleep(50);
		}
		pVoice->Pause();
		pVoice->Release();
		CoUninitialize();

		setStatusMessage(L"Speech thread terminated.");
		speechState = speech_state_enum::terminated;
	}

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
		shouldTerminate = false;
		if (theSpeechThread != nullptr)
		{
			setStatusMessage(L"Windows Voice thread already started.");
			return;
		}
		{
			lock_guard<mutex> lock(theMutex);
			stopRequested = false;
		}
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
		wordLength = 0;
		wordPosition = 0;
		shouldTerminate = true;
		theSpeechThread->join();
		clearSpeechQueue();
		delete theSpeechThread;
		theSpeechThread = nullptr;
		setStatusMessage(L"Speech destroyed.");
		speechState = speech_state_enum::uninitialized;
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

	BSTR getVoicesAvailable()
	{
		wstring voices;
		HRESULT hr;
		CComPtr<ISpObjectTokenCategory> cpSpCategory = nullptr;
		if (SUCCEEDED(hr = SpGetCategoryFromId(SPCAT_VOICES, &cpSpCategory)))
		{
			CComPtr<IEnumSpObjectTokens> cpSpEnumTokens;
			if (SUCCEEDED(hr = cpSpCategory->EnumTokens(NULL, NULL, &cpSpEnumTokens)))
			{
				ULONG vCount = 0;
				cpSpEnumTokens->GetCount(&vCount);
				CComPtr<ISpObjectToken> pSpTok;
				for (ULONG i = 0; i < vCount; ++i)
				{
					if (cpSpEnumTokens->Next(1, &pSpTok, nullptr) != S_OK)
						break;
					// try to get the Name attribute first; if failed, get the description
					CSpDynamicString voiceName;
					CComPtr<ISpDataKey> pAttribs;
					hr = pSpTok->OpenKey(SPTOKENKEY_ATTRIBUTES, &pAttribs);
					if (SUCCEEDED(hr))
					{
						hr = pAttribs->GetStringValue(L"Name", &voiceName);
					}
					if (FAILED(hr))
					{
						hr = SpGetDescription(pSpTok, &voiceName);
					}
					LANGID langid;
					if (SUCCEEDED(hr))
					{
						hr = SpGetLanguageFromVoiceToken(pSpTok, &langid);
					}
					if (SUCCEEDED(hr))
					{
						// get the locale name (e.g. "English (United States)")
						int size = GetLocaleInfoW(langid, LOCALE_SENGLISHDISPLAYNAME, NULL, 0);
						if (size != 0)
						{
							wchar_t* localeNameBuf = new wchar_t[size];
							GetLocaleInfoW(langid, LOCALE_SENGLISHDISPLAYNAME, localeNameBuf, size);
							wstring localeName = localeNameBuf;
							delete[] localeNameBuf;
							// if the voice has attribute "NaturalVoiceType", consider it "natural"
							CSpDynamicString naturalVoiceType;
							if (pAttribs != nullptr && SUCCEEDED(pAttribs->GetStringValue(L"NaturalVoiceType", &naturalVoiceType)))
							{
								// inserts "Natural" before '('
								size_t pos = localeName.find(L'(');
								if (pos != wstring::npos)
									localeName.insert(pos, L"Natural ");
								else
									localeName.append(L" Natural");
							}
							voices += voiceName;
							voices += L'#';
							voices += localeName;
							voices += L'\n';
						}
					}
					pSpTok.Release();
				}
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