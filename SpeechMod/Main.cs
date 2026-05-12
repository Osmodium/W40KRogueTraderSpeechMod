using HarmonyLib;
using SpeechMod.Configuration;
using SpeechMod.KeyBinds;
using SpeechMod.Unity;
using SpeechMod.Unity.Extensions;
using SpeechMod.Voice;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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

    public static string NarratorVoice => GetVoiceKey(Settings?.NarratorVoice ?? 0);
    public static string FemaleVoice => GetVoiceKey(Settings?.FemaleVoice ?? 0);
    public static string MaleVoice => GetVoiceKey(Settings?.MaleVoice ?? 0);
    public static string ProtagonistVoice => GetVoiceKey(Settings?.ProtagonistVoice ?? 0);

    /// <summary>
    /// Get voice key for a specific character by blueprint ID, or null if no per-character voice set.
    /// </summary>
    public static string GetCharacterVoiceKey(string characterId)
    {
        if (string.IsNullOrWhiteSpace(characterId) || Settings?.CharacterVoices == null)
            return null;
        if (!Settings.CharacterVoices.TryGetValue(characterId, out var charVoice))
            return null;
        return GetVoiceKey(charVoice.VoiceIndex);
    }

    private static Dictionary<string, string> _cachedVoicesDict;
    private static string[] _cachedVoicesSource;

    public static Dictionary<string, string> VoicesDict
    {
        get
        {
            var voices = Settings?.AvailableVoices;
            if (voices == null)
                return null;

            if (_cachedVoicesDict != null && ReferenceEquals(_cachedVoicesSource, voices))
                return _cachedVoicesDict;

            _cachedVoicesSource = voices;
            _cachedVoicesDict = voices.Select(v =>
            {
                var splitV = v?.Split('#');
                return splitV?.Length != 2
                    ? new { Key = v, Value = "Unknown" }
                    : new { Key = splitV[0], Value = splitV[1] };
            }).ToDictionary(p => p.Key, p => p.Value);

            return _cachedVoicesDict;
        }
    }

    private static string GetVoiceKey(int index)
    {
        var dict = VoicesDict;
        if (dict == null || dict.Count == 0)
            return null;

        if (index < 0 || index >= dict.Count)
            index = 0;

        return dict.ElementAt(index).Key;
    }

    public static ISpeech Speech;
    private static bool m_Loaded = false;

    /// <summary>
    /// Persist current settings to disk.  Can be called from anywhere (e.g. voice picker).
    /// </summary>
    public static void SaveSettings()
    {
        var modEntry = ModConfigurationManager.Instance?.ModEntry;
        if (modEntry == null || Settings == null)
            return;
        Settings.Save(modEntry);
    }

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

        PhoneticDictionary.LoadDictionary();

        Debug.Log("Warhammer 40K: Rogue Trader Speech Mod Initialized!");
        m_Loaded = true;
        return true;
    }

    private static void SetUpSettings()
    {
        if (ModConfigurationManager.Instance.GroupedSettings.TryGetValue("main", out _))
            return;

        ModConfigurationManager.Instance.GroupedSettings.Add("main", [new PlaybackStop(), new ToggleBarks()]);
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

        for (var i = 0; i < availableVoices.Length; i++)
        {
            var splitVoice = availableVoices[i]?.Split('#');
            if (splitVoice?.Length != 2 || string.IsNullOrEmpty(splitVoice[1]))
                availableVoices[i] = availableVoices[i]?.Replace("#", "").Trim() + "#Unknown";
        }

        // Ensure that the selected voice index falls within the available voices range
        ClampVoiceIndex(ref Settings.NarratorVoice, availableVoices.Length, nameof(Settings.NarratorVoice));
        ClampVoiceIndex(ref Settings.FemaleVoice, availableVoices.Length, nameof(Settings.FemaleVoice));
        ClampVoiceIndex(ref Settings.MaleVoice, availableVoices.Length, nameof(Settings.MaleVoice));
        ClampVoiceIndex(ref Settings.ProtagonistVoice, availableVoices.Length, nameof(Settings.ProtagonistVoice));

        Settings!.AvailableVoices = availableVoices.OrderBy(v => v.Split('#').ElementAtOrDefault(1)).ToArray();
        _cachedVoicesDict = null;
        _cachedVoicesSource = null;

        return true;
    }

    private static void ClampVoiceIndex(ref int voiceIndex, int voiceCount, string settingName)
    {
        if (voiceIndex >= 0 && voiceIndex < voiceCount)
            return;

        Logger?.Log($"{settingName} was out of range ({voiceIndex}), resetting to first voice available.");
        voiceIndex = 0;
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
                Speech = new WindowsSpeech();
                SpeechExtensions.AddUiElements<WindowsVoiceUnity>(Constants.WINDOWS_VOICE_NAME);
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
