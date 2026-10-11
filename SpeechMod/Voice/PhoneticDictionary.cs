using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace SpeechMod.Voice;

public static class PhoneticDictionary
{
    private static Dictionary<string, string> s_PhoneticDictionary;
    private static List<(Regex Pattern, string Replacement)> s_CompiledPatterns;

    private static readonly Regex s_DatePattern = new(@"([0-9]{2})\/([0-9]{2})\/([0-9]{4})", RegexOptions.Compiled);
    private static readonly Regex s_LineBreakTagPattern = new(@"<br\s*/?>", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex s_TagPattern = new("<[^>]+>", RegexOptions.Compiled);
    // A run of line breaks becomes one sentence break. Keeps punctuation already in front of it, so "within.\n\na" becomes "within. a".
    private static readonly Regex s_LineBreakPattern = new(@"([.!?:;,])?[ \t]*[\r\n]+\s*", RegexOptions.Compiled);

    private static string SpaceOutDate(string text)
    {
        return s_DatePattern.Replace(text, "$1 / $2 / $3");
    }

    /// <summary>
    /// Removes rich text tags. &lt;br&gt; becomes a line break, so the text on each side does not run together.
    /// </summary>
    public static string StripTags(this string text)
    {
        if (string.IsNullOrEmpty(text))
            return text;

        text = s_LineBreakTagPattern.Replace(text, "\n");
        return s_TagPattern.Replace(text, "");
    }

    public static string PrepareText(this string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return text;

        if (s_PhoneticDictionary == null)
            LoadDictionary();

        text = text.ToLower();
        text = text.Replace("\"", "");
        text = s_LineBreakTagPattern.Replace(text, "\n");
        text = s_LineBreakPattern.Replace(text, m => m.Groups[1].Success ? m.Groups[1].Value + " " : ". ");
        text = text.Trim();

        text = SpaceOutDate(text);

        // Apply pre-compiled regex patterns from dictionary
        if (s_CompiledPatterns == null)
            return text;
        
        foreach (var (pattern, replacement) in s_CompiledPatterns)
        {
            text = pattern.Replace(text, replacement);
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
            var file = Path.Combine(Main.ModPath, "PhoneticDictionary.json");
            var json = File.ReadAllText(file, Encoding.UTF8);
            s_PhoneticDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
            Main.Logger?.Log($"Phonetic dictionary loaded successfully from: {file}");
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
