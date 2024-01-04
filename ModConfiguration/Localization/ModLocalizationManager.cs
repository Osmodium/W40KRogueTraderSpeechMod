using Kingmaker.Localization;
using Kingmaker.Localization.Enums;
using Kingmaker.Localization.Shared;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;

namespace EnhancModConfigurationedControls.Localization;

internal class ModLocalizationManager
{
    private static MyLocalizationPack enPack;

    public static void Init()
    {
        enPack = LoadPack(Locale.enGB);

        ApplyLocalization(LocalizationManager.Instance.CurrentLocale);

        (LocalizationManager.Instance as ILocalizationProvider).LocaleChanged += ApplyLocalization;
    }

    public static void ApplyLocalization(Locale currentLocale)
    {
        var currentPack = LocalizationManager.Instance.CurrentPack;
        if (currentPack == null) return;
        foreach (var entry in enPack.Strings)
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
        var localizationFolder = Path.Combine(Main.ModEntry.Path, "Localization");
        var packFile = Path.Combine(localizationFolder, Locale.enGB.ToString() + ".json");
        using StreamWriter file = new(packFile);
        using JsonWriter jsonReader = new JsonTextWriter(file);
        JsonSerializer serializer = new();
        serializer.Serialize(jsonReader, enPack);
#endif
    }

    private static MyLocalizationPack LoadPack(Locale locale)
    {
        var localizationFolder = Path.Combine(Main.ModEntry.Path, "Localization");
        var packFile = Path.Combine(localizationFolder, locale.ToString() + ".json");
        if (File.Exists(packFile))
        {
            try
            {
                using StreamReader file = File.OpenText(packFile);
                using JsonReader jsonReader = new JsonTextReader(file);
                JsonSerializer serializer = new();
                var enLocalization = serializer.Deserialize<MyLocalizationPack>(jsonReader);
                return enLocalization;
            }
            catch (System.Exception ex)
            {
                Main.log.Error($"Failed to read or parse {locale} mod localization pack: {ex.Message}");
            }
        }
        else
        {
            Main.log.Log($"Missing localization pack for {locale}");
        }
        return new() { Strings = new() };
    }

    public static LocalizedString CreateString(string key, string value)
    {
        if (enPack.Strings.ContainsKey(key))
        {
            return new LocalizedString { m_ShouldProcess = false, m_Key = key };
        }
        else
        {
            Main.log.Log($"Missing localization string {key}");
#if DEBUG
            enPack.Strings[key] = new() { Text = value };
#endif
            return new LocalizedString { m_ShouldProcess = false, m_Key = key };
        }
    }
}

public record class MyLocalizationPack
{
    [JsonProperty]
    public Dictionary<string, MyLocalizationEntry> Strings;
}

public struct MyLocalizationEntry
{
    [JsonProperty]
    public string Text;
};