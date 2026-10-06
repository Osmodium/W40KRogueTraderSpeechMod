#pragma once

#ifdef DLL_EXPORTS
#define DLL_API __declspec(dllexport)
#else
#define DLL_API __declspec(dllimport)
#endif

#include <sapi.h>
#include <atlbase.h>
#pragma warning(disable:4996)
#include <sphelper.h>
#pragma warning(default: 4996)

namespace WindowsVoice {
	extern "C" {
		DLL_API void __cdecl initSpeech(int rate, int volume);
		DLL_API void __cdecl addToSpeechQueue(const wchar_t* text);
		DLL_API void __cdecl clearSpeechQueue();
		DLL_API void __cdecl stopSpeech();
		DLL_API void __cdecl destroySpeech();
		DLL_API BSTR __cdecl getStatusMessage();
		DLL_API BSTR __cdecl getVoicesAvailable();
		DLL_API UINT32 __cdecl getWordLength();
		DLL_API UINT32 __cdecl getWordPosition();
		DLL_API UINT32 __cdecl getSpeechState();
	}

	enum class speech_state_enum { uninitialized, ready, speaking, terminated, error };
}
