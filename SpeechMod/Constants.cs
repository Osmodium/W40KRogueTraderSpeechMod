using System.IO;
using System;

namespace SpeechMod;

public static class Constants
{
    public const string WINDOWS_VOICE_DLL = "WindowsVoice";
    public const string WINDOWS_VOICE_NAME = "WindowsVoice";
    public const string APPLE_VOICE_NAME = "AppleVoice";
    //public const string ARROW_BUTTON_PATH = "NestedCanvas1/DialogPCView/Body/View/Scroll View/ButtonEdge";

    public static readonly string LOCAL_LOW_PATH = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)) + "Low";
}
