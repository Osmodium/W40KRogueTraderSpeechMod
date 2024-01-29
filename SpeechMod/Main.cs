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
using ModKit;
using SpeechMod.Configuration.SettingEntries;
using TMPro;
using UnityEngine;
using UnityModManagerNet;

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

    public static string NarratorVoice => Settings.NarratorVoice;
    public static string FemaleVoice => Settings.FemaleVoice;
    public static string MaleVoice => Settings.MaleVoice;

    //public static string NarratorVoice => VoicesDict?.ElementAtOrDefault(Settings.NarratorVoice).Key;
    //public static string FemaleVoice => VoicesDict?.ElementAtOrDefault(Settings.FemaleVoice).Key;
    //public static string MaleVoice => VoicesDict?.ElementAtOrDefault(Settings.MaleVoice).Key;

    //public static Dictionary<string, string> VoicesDict => Settings?.AvailableVoices?.Select(v =>
    //{
    //    var splitV = v?.Split('#');
    //    return splitV?.Length != 2
    //        ? new { Key = v, Value = "Unknown" }
    //        : new { Key = splitV[0], Value = splitV[1] };
    //}).ToDictionary(p => p.Key, p => p.Value);

    public static Voice.Edge.Voice[] AvailableVoices;

    public static ISpeech Speech;
    private static bool m_Loaded = false;

    private static bool Load(UnityModManager.ModEntry modEntry)
    {
        Debug.Log("Warhammer 40K: Rogue Trader Speech Mod Initializing...");

        Logger = modEntry?.Logger;

        if (!SetSpeech())
            return false;

        Settings = UnityModManager.ModSettings.Load<Settings>(modEntry);
        Hooks.UpdateHoverColor();

        modEntry!.OnToggle = OnToggle;
        modEntry!.OnGUI = OnGui;
        modEntry!.OnSaveGUI = OnSaveGui;

        var harmony = new Harmony(modEntry.Info?.Id);
        harmony.PatchAll(Assembly.GetExecutingAssembly());

        ModConfigurationManager.Build(harmony, modEntry, Constants.SETTINGS_PREFIX);
        SetUpSettings();
        harmony.CreateClassProcessor(typeof(SettingsUIPatches)).Patch();

        Logger?.Log(Speech?.GetStatusMessage());

        if (!SetAvailableVoices())
            return false;

        SpeechExtensions.LoadDictionary();

        Debug.Log("Warhammer 40K: Rogue Trader Speech Mod Initialized!");
        m_Loaded = true;
        return true;
    }

    private static void SetUpSettings()
    {
        if (ModConfigurationManager.Instance.GroupedSettings.TryGetValue("main", out _))
            return;

        ModConfigurationManager.Instance.GroupedSettings.Add("main", new List<ModSettingEntry> { new PlaybackStop() });
    }

    private static bool SetAvailableVoices()
    {
        var availableVoices = Speech?.GetAvailableVoices();

        if (availableVoices == null || availableVoices.Length == 0)
        {
            Logger?.Warning("No available voices found! Disabling mod!");
            return false;
        }

        Logger?.Log("Available voices:");
        foreach (var voice in availableVoices)
        {
            Logger?.Log(voice);
        }
        Logger?.Log("Setting available voices list...");



        //for (int i = 0; i < availableVoices.Length; i++)
        //{
        //    string[] splitVoice = availableVoices[i]?.Split('#');
        //    if (splitVoice?.Length != 2 || string.IsNullOrEmpty(splitVoice[1]))
        //        availableVoices[i] = availableVoices[i]?.Replace("#", "").Trim() + "#Unknown";
        //}

        // Ensure that the selected voice index falls within the available voices range
        //if (Settings?.NarratorVoice >= availableVoices.Length)
        //{
        //    Logger?.Log($"{nameof(Settings.NarratorVoice)} was out of range, resetting to first voice available.");
        //    Settings.NarratorVoice = 0;
        //}

        //if (Settings?.FemaleVoice >= availableVoices.Length)
        //{
        //    Logger?.Log($"{nameof(Settings.FemaleVoice)} was out of range, resetting to first voice available.");
        //    Settings.FemaleVoice = 0;
        //}

        //if (Settings?.MaleVoice >= availableVoices.Length)
        //{
        //    Logger?.Log($"{nameof(Settings.MaleVoice)} was out of range, resetting to first voice available.");
        //    Settings.MaleVoice = 0;
        //}

        //Settings!.AvailableVoices = availableVoices.OrderBy(v => v.Split('#').ElementAtOrDefault(1)).ToArray();

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
            //case RuntimePlatform.WindowsPlayer:
            //    Speech = new WindowsSpeech();
            //    SpeechExtensions.AddUiElements<WindowsVoiceUnity>(Constants.WINDOWS_VOICE_NAME);
            //    break;
            case RuntimePlatform.WindowsPlayer:
                Speech = new EdgeSpeech();
                SpeechExtensions.AddUiElements<EdgeVoiceUnity>(Constants.EDGE_VOICE_NAME);
                break;
            default:
                Logger?.Critical($"Warhammer 40K: Rogue Trader SpeechMod is not supported on {Application.platform}!");
                return false;
        }

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
