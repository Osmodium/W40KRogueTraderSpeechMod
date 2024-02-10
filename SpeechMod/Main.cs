using HarmonyLib;
using SpeechMod.Configuration;
using SpeechMod.Configuration.Settings;
using SpeechMod.Unity;
using SpeechMod.Unity.Extensions;
using SpeechMod.Voice;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SpeechMod.Configuration.SettingEntries;
using SpeechMod.Voice.Edge;
using TMPro;
using UnityEngine;
using UnityModManagerNet;
using SpeechMod.Unity.GUIs;

namespace SpeechMod;

#if DEBUG
[EnableReloading]
#endif
public static class Main
{
    public static UnityModManager.ModEntry.ModLogger Logger;
    public static Settings Settings;
    public static bool Enabled;

    public static string[] FontStyleNames = Enum.GetNames(typeof(FontStyles));

    public static string NarratorVoice => Settings?.NarratorVoice;
    public static string FemaleVoice => Settings?.FemaleVoice;
    public static string MaleVoice => Settings?.MaleVoice;

    public static string[] AvailableVoices;

    public static EdgeVoiceInfo[] EdgeAvailableVoices;
    public static Dictionary<string, EdgeVoiceInfo[]> EdgeVoicesDict => EdgeAvailableVoices?
        .Select(lang => lang?.Locale?.Substring(0, 2))
        .Distinct()
        .ToDictionary(locale => locale?.Substring(0, 2), locale => EdgeAvailableVoices.Where(voice => voice.Locale.StartsWith(locale!.Substring(0, 2))).ToArray());

    public static ISpeech Speech;
    private static bool m_Loaded = false;

    private static bool Load(UnityModManager.ModEntry modEntry)
    {
        Debug.Log("Warhammer 40K: Rogue Trader Speech Mod Initializing...");

        Logger = modEntry?.Logger;

        Settings = UnityModManager.ModSettings.Load<Settings>(modEntry);
        Hooks.UpdateHoverColor();

        if (!SetSpeech())
            return false;

        modEntry!.OnToggle = OnToggle;
        modEntry!.OnGUI = OnGui;
        modEntry!.OnSaveGUI = OnSaveGui;

        var harmony = new Harmony(modEntry.Info?.Id);
        harmony.PatchAll(Assembly.GetExecutingAssembly());

        ModConfigurationManager.Build(harmony, modEntry, Constants.SETTINGS_PREFIX);
        SetUpIngameSettings();
        harmony.CreateClassProcessor(typeof(SettingsUIPatches)).Patch();

        Logger?.Log(Speech?.GetStatusMessage());

        Debug.Log("Warhammer 40K: Rogue Trader Speech Mod Initialized!");
        m_Loaded = true;
        return true;
    }

    private static void SetUpIngameSettings()
    {
        if (ModConfigurationManager.Instance.GroupedSettings.TryGetValue("main", out _))
            return;

        ModConfigurationManager.Instance.GroupedSettings.Add("main", new List<ModSettingEntry> { new PlaybackStop() });
    }

    private static bool SetAvailableVoices()
    {
        Speech?.SetAvailableVoices();

        switch (Speech)
        {
            case EdgeSpeech when (EdgeAvailableVoices == null || EdgeAvailableVoices.Length == 0):
                Logger?.Warning("No available voices found! Disabling mod!");
                return false;
            case WindowsSpeech or AppleSpeech when (AvailableVoices == null || AvailableVoices.Length == 0):
                Logger?.Warning("No available voices found! Disabling mod!");
                return false;
        }

        Logger?.Log("Available voices:");
        switch (Speech)
        {
            case EdgeSpeech:
                foreach (var voice in EdgeAvailableVoices!)
                {
                    Logger?.Log(voice.ShortName);
                }
                break;
            case WindowsSpeech or AppleSpeech:
                foreach (var voice in AvailableVoices!)
                {
                    Logger?.Log(voice);
                }
                break;
        }

        return true;
    }

    private static bool SetSpeech()
    {
        switch (Application.platform)
        {
            case RuntimePlatform.OSXPlayer:
                Speech = new AppleSpeech();
                SpeechExtensions.AddUiElements<AppleVoiceUnity>(Constants.APPLE_VOICE_NAME);
                break;
            case RuntimePlatform.WindowsPlayer:
                if (Settings.UseEdgeVoice)
                    Speech = new EdgeSpeech();
                else
                {
                    Speech = new WindowsSpeech();
                    SpeechExtensions.AddUiElements<WindowsVoiceUnity>(Constants.WINDOWS_VOICE_NAME);
                    SpeechExtensions.LoadDictionary();
                }
                break;
            default:
                Logger?.Critical($"Warhammer 40K: Rogue Trader SpeechMod is not supported on {Application.platform}!");
                return false;
        }

        if (!SetAvailableVoices())
            return false;

        MenuGUI.SetupVoicePickers();

        return true;
    }

    private static bool OnToggle(UnityModManager.ModEntry modEntry, bool value)
    {
        Enabled = value;
        return true;
    }

    private static void OnGui(UnityModManager.ModEntry modEntry)
    {
        if (m_Loaded)
            MenuGUI.OnGui();
    }

    private static void OnSaveGui(UnityModManager.ModEntry modEntry)
    {
        Hooks.UpdateHoverColor();
        Settings?.Save(modEntry);
    }
}
