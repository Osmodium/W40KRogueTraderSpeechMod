using HarmonyLib;
using Kingmaker;
using Kingmaker.Settings;
using Kingmaker.UI.InputSystems;
using Kingmaker.UI.Models.SettingsUI;
using ModConfiguration.Localization;
using ModConfiguration.Settings;
using ModConfiguration.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using static UnityModManagerNet.UnityModManager;

namespace ModConfiguration;

public class ModConfigurationManager
{
    public Dictionary<string, List<ModSettingEntry>> GroupedSettings = new();
    public Harmony HarmonyInstance { get; protected set; }
    public ModEntry ModEntry { get; protected set; }
    public string SettingsPrefix = Guid.NewGuid().ToString();

    private ModConfigurationManager() { }

    public static void Build(Harmony harmonyInstance, ModEntry modEntry, string settingsPrefix)
    {
        Instance.HarmonyInstance = harmonyInstance;
        Instance.ModEntry = modEntry;
        Instance.SettingsPrefix = settingsPrefix;
        ModLocalizationManager.Init();
    }

    private bool Initialized = false;

    public void Initialize()
    {
        if (Initialized) return;
        Initialized = true;

        foreach (var setting in GroupedSettings.SelectMany(settings => settings.Value))
        {
            setting.BuildUIAndLink();
            setting.TryEnable();
        }

        if (ModHotkeySettingEntry.ReSavingRequired)
        {
            SettingsController.Instance.SaveAll();
            Instance.ModEntry.Logger.Log("Hotkey settings were migrated");
        }
    }

    private static readonly ModConfigurationManager instance = new();
    public static ModConfigurationManager Instance => instance;
}

[HarmonyPatch]
public static class SettingsUIPatches
{
    /// <summary>
    /// Initializes mod features and adds setting group to Controls section of game settings
    /// </summary>
    [HarmonyPatch(typeof(UISettingsManager), nameof(UISettingsManager.Initialize))]
    [HarmonyPostfix]
    static void AddSettingsGroup()
    {
        if (Game.Instance.UISettingsManager.m_ControlSettingsList.Any(group => group.name?.StartsWith(ModConfigurationManager.Instance.SettingsPrefix) ?? false))
        {
            return;
        }

        ModConfigurationManager.Instance?.Initialize();

        foreach (var settings in ModConfigurationManager.Instance.GroupedSettings)
        {
            Game.Instance.UISettingsManager.m_ControlSettingsList.Add(
                OwlcatUITools.MakeSettingsGroup($"{ModConfigurationManager.Instance.SettingsPrefix}.group.{settings.Key}", "Speech Mod",
                    settings.Value.Select(x => x.GetUISettings()).ToArray()
                ));
        }
    }

    /// <summary>
    /// Allows registration of any key combination for custom hotkeys.
    /// This is because original Owlcat validation is too strict and
    /// prevents usage of same key even if executed actions between keys do not conflict
    /// </summary>
    [HarmonyPatch(typeof(KeyboardAccess), nameof(KeyboardAccess.CanBeRegistered))]
    [HarmonyPrefix]
    public static bool CanRegisterAnything(ref bool __result, string name)
    {
        if (name != null && name.StartsWith(ModConfigurationManager.Instance.SettingsPrefix))
        {
            __result = true;
            return false;
        }
        return true;
    }
}