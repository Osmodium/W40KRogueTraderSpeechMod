using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace SpeechMod.Voice;

public static class PhoneticDictionary
{
    private static Dictionary<string, string> s_PhoneticDictionary;
    private static List<(Regex Pattern, string Replacement)> s_CompiledPatterns;

    private static readonly Regex s_DatePattern = new(@"([0-9]{2})\/([0-9]{2})\/([0-9]{4})", RegexOptions.Compiled);

    private static string SpaceOutDate(string text)
    {
        return s_DatePattern.Replace(text, "$1 / $2 / $3");
    }

    public static string PrepareText(this string text)
    {
        if (s_PhoneticDictionary == null)
            LoadDictionary();

        text = text.ToLower();
        text = text.Replace("\"", "");
        text = text.Replace("\r\n", ". ");
        text = text.Replace("\n", ". ");
        text = text.Replace("\r", ". ");
        text = text.Trim();

        text = SpaceOutDate(text);

        // Apply pre-compiled regex patterns from dictionary
        if (s_CompiledPatterns != null)
        {
            foreach (var (pattern, replacement) in s_CompiledPatterns)
            {
                text = pattern.Replace(text, replacement);
            }
        }

        return text;
    }

    /// <summary>
    /// Load a dictionary directly (useful for testing).
    /// </summary>
    public static void LoadDictionary(Dictionary<string, string> dictionary)
    {
        s_PhoneticDictionary = dictionary ?? new Dictionary<string, string>();
        CompilePatterns();
    }

    public static void LoadDictionary()
    {
        Main.Logger?.Log("Loading phonetic dictionary...");
        try
        {
            var file = Path.Combine(Constants.LOCAL_LOW_PATH!,
                "Owlcat Games",
                "Warhammer 40000 Rogue Trader",
                "UnityModManager",
                "W40KSpeechMod",
                "PhoneticDictionary.json");
            var json = File.ReadAllText(file, Encoding.UTF8);
            s_PhoneticDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
        }
        catch (Exception ex)
        {
            Main.Logger?.LogException(ex);
            Main.Logger?.Warning("Loading backup dictionary!");
            LoadBackupDictionary();
        }

        if (s_PhoneticDictionary == null || s_PhoneticDictionary.Count == 0)
        {
            Main.Logger?.Warning("Dictionary was empty, loading backup!");
            LoadBackupDictionary();
        }

        CompilePatterns();

#if DEBUG
        foreach (var entry in s_PhoneticDictionary)
        {
            Main.Logger?.Log($"{entry.Key}={entry.Value}");
        }
#endif
    }

    private static void CompilePatterns()
    {
        if (s_PhoneticDictionary == null)
        {
            s_CompiledPatterns = null;
            return;
        }

        s_CompiledPatterns = new List<(Regex, string)>();
        foreach (var entry in s_PhoneticDictionary)
        {
            try
            {
                s_CompiledPatterns.Add((new Regex(entry.Key, RegexOptions.Compiled), entry.Value));
            }
            catch (ArgumentException ex)
            {
                Main.Logger?.Warning($"Invalid regex pattern '{entry.Key}' in phonetic dictionary, skipping. Error: {ex.Message}");
            }
        }
    }

    private static void LoadBackupDictionary()
    {
        s_PhoneticDictionary = new Dictionary<string, string>
        {
            { "servitor", "servitur" }
        };
    }
}