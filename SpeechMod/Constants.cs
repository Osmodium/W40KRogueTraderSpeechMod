using System;
using System.IO;

namespace SpeechMod;

public static class Constants
{
    public const string WINDOWS_VOICE_DLL = "WindowsVoice";
    public const string WINDOWS_VOICE_NAME = "WindowsVoice";
    public const string APPLE_VOICE_NAME = "AppleVoice";
    public const string SETTINGS_PREFIX = "osmodium.speechmod";
    public const string NARRATOR_COLOR_CODE = "3c2d0a";

    public static readonly string LOCAL_LOW_PATH = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)) + "Low";
}
