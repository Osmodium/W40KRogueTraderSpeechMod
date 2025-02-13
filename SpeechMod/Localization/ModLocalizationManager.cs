using Kingmaker.Localization;
using Kingmaker.Localization.Enums;
using Kingmaker.Localization.Shared;
using Newtonsoft.Json;
using SpeechMod.Configuration;
using System.Collections.Generic;
using System.IO;

namespace SpeechMod.Localization;

internal class ModLocalizationManager
{
    private static ModLocalizationPack _enPack;

    public static void Init()
    {
        _enPack = LoadPack(Locale.enGB);

        ApplyLocalization(LocalizationManager.Instance!.CurrentLocale);

        (LocalizationManager.Instance as ILocalizationProvider).LocaleChanged += ApplyLocalization;
    }

    public static void ApplyLocalization(Locale currentLocale)
    {
        var currentPack = LocalizationManager.Instance.CurrentPack;
        if (currentPack == null) return;
        foreach (var entry in _enPack.Strings)
        {
            currentPack.PutString(entry.Key, entry.Value.Text);
        }

        if (currentLocale != Locale.enGB)
        {
            var localized = LoadPack(currentLocale);
            foreach (var entry in localized.Strings)
            {
                currentPack.PutString(entry.Key, entry.Value.Text);
            }
        }
#if DEBUG
        var localizationFolder = Path.Combine(ModConfigurationManager.Instance?.ModEntry?.Path!, "Localization");
        var packFile = Path.Combine(localizationFolder, Locale.enGB + ".json");
        using StreamWriter file = new(packFile);
        using JsonWriter jsonReader = new JsonTextWriter(file);
        JsonSerializer serializer = new();
        serializer.Serialize(jsonReader, _enPack);
#endif
    }

    private static ModLocalizationPack LoadPack(Locale locale)
    {
        var localizationFolder = Path.Combine(ModConfigurationManager.Instance?.ModEntry?.Path!, "Localization");
        var packFile = Path.Combine(localizationFolder, locale + ".json");
        if (File.Exists(packFile))
        {
            try
            {
                using var file = File.OpenText(packFile);
                using JsonReader jsonReader = new JsonTextReader(file);
                JsonSerializer serializer = new();
                var enLocalization = serializer.Deserialize<ModLocalizationPack>(jsonReader);
                return enLocalization;
            }
            catch (System.Exception ex)
            {
                ModConfigurationManager.Instance?.ModEntry?.Logger?.Error($"Failed to read or parse {locale} mod localization pack: {ex.Message}");
            }
        }
        else
        {
            ModConfigurationManager.Instance?.ModEntry?.Logger?.Log($"Missing localization pack for {locale}");
        }
        return new() { Strings = new() };
    }

    public static LocalizedString CreateString(string key, string value)
    {
        if (_enPack.Strings.ContainsKey(key))
        {
            return new LocalizedString { m_ShouldProcess = false, m_Key = key };
        }
        else
        {
            ModConfigurationManager.Instance?.ModEntry?.Logger?.Log($"Missing localization string {key}");
#if DEBUG
            _enPack.Strings[key] = new() { Text = value };
#endif
            return new LocalizedString { m_ShouldProcess = false, m_Key = key };
        }
    }
}

public record ModLocalizationPack
{
    [JsonProperty]
    public Dictionary<string, ModLocalizationEntry> Strings;
}

public struct ModLocalizationEntry
{
    [JsonProperty]
    public string Text;
};