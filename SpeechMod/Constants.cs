using System.IO;
using System;

namespace SpeechMod;

public static class Constants
{
    public const string WINDOWS_VOICE_DLL = "WindowsVoice";
    public const string WINDOWS_VOICE_NAME = "WindowsVoice";
    public const string APPLE_VOICE_NAME = "AppleVoice";
    public const string SETTINGS_PREFIX = "osmodium.speechmod";
    public const string BREAK_TOKEN_SHORT = " _speechmod_break_short_ ";
    public const string BREAK_TOKEN_MEDIUM = " _speechmod_break_medium_ ";
    public const string BREAK_TOKEN_LONG = " _speechmod_break_long_ ";

    public static readonly string LOCAL_LOW_PATH = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)) + "Low";
}
