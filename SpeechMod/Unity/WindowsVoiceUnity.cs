using SpeechMod.Unity.Extensions;
using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace SpeechMod.Unity;

public class WindowsVoiceUnity : MonoBehaviour
{
    public enum WindowsVoiceStatus { Uninitialized, Ready, Speaking, Terminated, Error }

    [DllImport(Constants.WINDOWS_VOICE_DLL)]
    private static extern void initSpeech(int rate, int volume);

    [DllImport(Constants.WINDOWS_VOICE_DLL)]
    private static extern void destroySpeech();

    [DllImport(Constants.WINDOWS_VOICE_DLL)]
    private static extern void addToSpeechQueue(string s);

    [DllImport(Constants.WINDOWS_VOICE_DLL)]
    private static extern void clearSpeechQueue();

    [DllImport(Constants.WINDOWS_VOICE_DLL)]
    [return: MarshalAs(UnmanagedType.BStr)]
    private static extern string getStatusMessage();

    [DllImport(Constants.WINDOWS_VOICE_DLL)]
    [return: MarshalAs(UnmanagedType.BStr)]
    private static extern string getVoicesAvailable();

    [DllImport(Constants.WINDOWS_VOICE_DLL)]
    private static extern int getWordLength();

    [DllImport(Constants.WINDOWS_VOICE_DLL)]
    private static extern int getWordPosition();

    [DllImport(Constants.WINDOWS_VOICE_DLL)]
    private static extern WindowsVoiceStatus getSpeechState();

    private static WindowsVoiceUnity s_TheVoice;
    private static int s_CurrentWordCount;

    public static bool IsSpeaking => getSpeechState() == WindowsVoiceStatus.Speaking;
    public static WindowsVoiceStatus VoiceStatus => getSpeechState();

    private static void Init()
    {
        initSpeech(1, 100);
    }
    private static bool IsVoiceInitialized()
    {
        if (s_TheVoice != null)
            return true;

        Main.Logger.Critical("No voice initialized!");
        return false;
    }

    void Start()
    {
        s_CurrentWordCount = 0;
        if (s_TheVoice != null)
        {
            Destroy(gameObject);
        }
        else
        {
            s_TheVoice = this;
            Init();
        }
    }

    public static string[] GetAvailableVoices()
    {
        var voicesDelimited = getVoicesAvailable();
        if (string.IsNullOrWhiteSpace(voicesDelimited))
            return [];
        var voices = voicesDelimited.Split(['\n'], StringSplitOptions.RemoveEmptyEntries);
        return voices;
    }

    public static void Speak(string text, int length, float delay = 0f)
    {
        if (!IsVoiceInitialized())
            return;

        if (Main.Settings.InterruptPlaybackOnPlay && IsSpeaking)
            Stop();

        s_CurrentWordCount = length;
        if (delay <= 0f)
            addToSpeechQueue(text);
        else
            s_TheVoice.ExecuteLater(delay, () => Speak(text, length));
    }

    public static string GetStatusMessage()
    {
        return getStatusMessage();
    }

    public static int WordPosition => getWordPosition();

    public static int WordCount => s_CurrentWordCount;

    public static int WordLength => getWordLength();

    public static float GetNormalizedProgress()
    {
        if (s_CurrentWordCount <= 0)
            return 0f;

        return 1 - (float)(s_CurrentWordCount - getWordPosition()) / s_CurrentWordCount;
    }

    public static void Stop()
    {
        if (!IsVoiceInitialized())
            return;

        if (!IsSpeaking)
            return;

        destroySpeech();
        Init();
    }

    public static void ClearQueue()
    {
        clearSpeechQueue();
    }

    void OnDestroy()
    {
        if (s_TheVoice != this)
            return;

        Debug.Log("Destroying speech");
        destroySpeech();
        Debug.Log("Speech destroyed");
        s_TheVoice = null;
    }
}