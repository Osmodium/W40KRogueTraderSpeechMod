using SpeechMod.Unity.Extensions;
using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace SpeechMod.Unity;

public class VoxBridgeUnity : MonoBehaviour
{
    public enum VoxBridgeStatus { Uninitialized, Ready, Speaking, Terminated, Error }

    [DllImport(Constants.VOXBRIDGE_DLL)]
    private static extern void initSpeech(int rate, int volume);

    [DllImport(Constants.VOXBRIDGE_DLL)]
    private static extern void destroySpeech();

    [DllImport(Constants.VOXBRIDGE_DLL)]
    private static extern void addToSpeechQueue([MarshalAs(UnmanagedType.LPWStr)] string s);

    [DllImport(Constants.VOXBRIDGE_DLL)]
    private static extern void clearSpeechQueue();

    [DllImport(Constants.VOXBRIDGE_DLL)]
    private static extern void stopSpeech();

    [DllImport(Constants.VOXBRIDGE_DLL)]
    [return: MarshalAs(UnmanagedType.BStr)]
    private static extern string getStatusMessage();

    [DllImport(Constants.VOXBRIDGE_DLL)]
    [return: MarshalAs(UnmanagedType.BStr)]
    private static extern string getVoicesAvailable();

    [DllImport(Constants.VOXBRIDGE_DLL)]
    private static extern int getWordLength();

    [DllImport(Constants.VOXBRIDGE_DLL)]
    private static extern int getWordPosition();

    [DllImport(Constants.VOXBRIDGE_DLL)]
    private static extern VoxBridgeStatus getSpeechState();

    [DllImport(Constants.VOXBRIDGE_DLL)]
    private static extern uint getApiVersion();

    private static VoxBridgeUnity s_TheVoice;
    private static int s_CurrentWordCount;

    public static VoxBridgeStatus VoiceStatus => getSpeechState();
    public static bool IsSpeaking => VoiceStatus == VoxBridgeStatus.Speaking;

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

    public static bool IsNativeLibraryUsable()
    {
        uint apiVersion;
        try
        {
            apiVersion = getApiVersion();
        }
        catch (Exception ex) when (ex is DllNotFoundException || ex is BadImageFormatException)
        {
            Main.Logger?.Critical($"{Constants.VOXBRIDGE_DLL}.dll could not be loaded ({ex.GetType().Name}). Reinstall the mod: {Constants.VOXBRIDGE_DLL}.dll must be in the mod folder.");
            return false;
        }
        catch (EntryPointNotFoundException)
        {
            Main.Logger?.Critical($"{Constants.VOXBRIDGE_DLL}.dll is too old: it has no API version. Expected API version {Constants.VOXBRIDGE_API_VERSION}.");
            return false;
        }

        if (apiVersion == Constants.VOXBRIDGE_API_VERSION)
            return true;

        Main.Logger?.Critical($"{Constants.VOXBRIDGE_DLL}.dll has API version {apiVersion}, expected {Constants.VOXBRIDGE_API_VERSION}.");
        return false;
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

        stopSpeech();
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