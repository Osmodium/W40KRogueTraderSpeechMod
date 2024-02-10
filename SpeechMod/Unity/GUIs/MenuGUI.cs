﻿using SpeechMod.Voice;
using SpeechMod.Voice.Edge;
using UnityEngine;

namespace SpeechMod.Unity.GUIs;

public static class MenuGUI
{
    private const string NARRATOR_PREVIEW_TEXT = "Speech Mod for Warhammer 40K: Rogue Trader - Narrator voice speech test";
    private const string FEMALE_PREVIEW_TEXT = "Speech Mod for Warhammer 40K: Rogue Trader - Female voice speech test";
    private const string MALE_PREVIEW_TEXT = "Speech Mod for Warhammer 40K: Rogue Trader - Male voice speech test";

    private static VoicePickerBase NarratorVoicePicker;
    private static VoicePickerBase FemaleVoicePicker;
    private static VoicePickerBase MaleVoicePicker;

    public static void SetupVoicePickers()
    {
        if (NarratorVoicePicker == null)
        {
            if (Main.Speech is EdgeSpeech)
                NarratorVoicePicker = new EdgeVoicePicker("Narrator", Main.Settings.NarratorVoice, NARRATOR_PREVIEW_TEXT, VoiceType.Narrator);
            else
                NarratorVoicePicker = new VoicePicker("Narrator", Main.Settings.NarratorVoice, NARRATOR_PREVIEW_TEXT, VoiceType.Narrator);
        }

        if (FemaleVoicePicker == null)
        {
            if (Main.Speech is EdgeSpeech)
                FemaleVoicePicker = new EdgeVoicePicker("Female", Main.Settings.FemaleVoice, FEMALE_PREVIEW_TEXT, VoiceType.Female);
            else
                FemaleVoicePicker = new VoicePicker("Female", Main.Settings.FemaleVoice, FEMALE_PREVIEW_TEXT, VoiceType.Female);
        }

        if (MaleVoicePicker == null)
        {
            if (Main.Speech is EdgeSpeech)
                MaleVoicePicker = new EdgeVoicePicker("Male", Main.Settings.MaleVoice, MALE_PREVIEW_TEXT, VoiceType.Male);
            else
                MaleVoicePicker = new VoicePicker("Male", Main.Settings.MaleVoice, MALE_PREVIEW_TEXT, VoiceType.Male);
        }
    }

    public static void OnGui()
    {
#if DEBUG
        GUILayout.BeginVertical("", GUI.skin.box);

        GUILayout.BeginHorizontal();
        GUILayout.Label("Log speech", GUILayout.ExpandWidth(false));
        Main.Settings.LogVoicedLines = GUILayout.Toggle(Main.Settings.LogVoicedLines, "Enabled", GUI.skin.button);
        GUILayout.EndHorizontal();

        GUILayout.EndVertical();
#endif
        GUILayout.BeginVertical("", GUI.skin.box);

        GUILayout.BeginHorizontal();
        GUILayout.Label("<color=yellow><b>EXPERIMENTAL!</b></color> - <color=red><i>When changing this setting you need to restart the game for it to take effect!</i></color>");
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label("<color=yellow>Use Edge (Bing) Natural Voices</color>", GUILayout.ExpandWidth(false));
        GUILayout.Space(10);
        Main.Settings.UseEdgeVoice = GUILayout.Toggle(Main.Settings.UseEdgeVoice, Main.Settings.UseEdgeVoice ? "<color=red><b>These voices requires internet access and might slow the playback down!</b></color>" : "");
        GUILayout.EndHorizontal();

        GUILayout.EndVertical();

        NarratorVoicePicker?.OnGUI(ref Main.Settings.NarratorVoice, ref Main.Settings.NarratorRate, ref Main.Settings.NarratorPitch, ref Main.Settings.NarratorVolume);

        GUILayout.BeginVertical("Playback voices", GUI.skin.box);

        GUILayout.BeginHorizontal();
        GUILayout.Label("Use gender specific voices", GUILayout.ExpandWidth(false));
        Main.Settings.UseGenderSpecificVoices = GUILayout.Toggle(Main.Settings.UseGenderSpecificVoices, "Enabled");
        GUILayout.EndHorizontal();

        GUILayout.EndVertical();

        if (Main.Settings.UseGenderSpecificVoices)
        {
            FemaleVoicePicker?.OnGUI(ref Main.Settings.FemaleVoice, ref Main.Settings.FemaleRate, ref Main.Settings.FemalePitch, ref Main.Settings.FemaleVolume);
            MaleVoicePicker?.OnGUI(ref Main.Settings.MaleVoice, ref Main.Settings.MaleRate, ref Main.Settings.MalePitch, ref Main.Settings.MaleVolume);
        }

        GUILayout.BeginVertical("", GUI.skin.box);

        if (Main.Speech is WindowsSpeech)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("Interrupt speech on play", GUILayout.ExpandWidth(false));
            GUILayout.Space(10);
            Main.Settings.InterruptPlaybackOnPlay = GUILayout.Toggle(Main.Settings.InterruptPlaybackOnPlay, Main.Settings.InterruptPlaybackOnPlay ? "Interrupt and play" : "Add to queue");
            GUILayout.EndHorizontal();
        }

        GUILayout.BeginHorizontal();
        GUILayout.Label("Auto stop playback on loading", GUILayout.ExpandWidth(false));
        GUILayout.Space(10);
        Main.Settings.AutoStopPlaybackOnLoading = GUILayout.Toggle(Main.Settings.AutoStopPlaybackOnLoading, "Enabled");
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label("Auto play dialog", GUILayout.ExpandWidth(false));
        GUILayout.Space(10);
        Main.Settings.AutoPlay = GUILayout.Toggle(Main.Settings.AutoPlay, "Enabled");
        GUILayout.EndHorizontal();

        {
            GUI.enabled = Main.Settings.AutoPlay;

            GUILayout.BeginHorizontal();
            GUILayout.Label("Auto play ignores voiced dialog lines", GUILayout.ExpandWidth(false));
            GUILayout.Space(10);
            Main.Settings.AutoPlayIgnoreVoice = GUILayout.Toggle(Main.Settings.AutoPlayIgnoreVoice, "Enabled");
            GUILayout.EndHorizontal();

            GUI.enabled = true;
        }

        GUILayout.EndVertical();

        GUILayout.BeginVertical("", GUI.skin.box);

        GUILayout.BeginHorizontal();
        GUILayout.Label("Show playback button of dialog answers", GUILayout.ExpandWidth(false));
        GUILayout.Space(10);
        Main.Settings.ShowPlaybackOfDialogAnswers = GUILayout.Toggle(Main.Settings.ShowPlaybackOfDialogAnswers, "Enabled");
        GUILayout.EndHorizontal();

        if (Main.Settings.ShowPlaybackOfDialogAnswers)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("Include dialog answer number in playback", GUILayout.ExpandWidth(false));
            GUILayout.Space(10);
            Main.Settings.SayDialogAnswerNumber = GUILayout.Toggle(Main.Settings.SayDialogAnswerNumber, "Enabled");
            GUILayout.EndHorizontal();

            GUILayout.EndVertical();

            AddColorPicker("Color answer on hover", ref Main.Settings.DialogAnswerColorOnHover, "Hover color", ref Main.Settings.DialogAnswerHoverColorR, ref Main.Settings.DialogAnswerHoverColorG, ref Main.Settings.DialogAnswerHoverColorB);
        }
        else
        {
            GUILayout.EndVertical();
        }

        GUILayout.BeginVertical("", GUI.skin.box);

        GUILayout.BeginHorizontal();
        GUILayout.Label("Playback barks", GUILayout.ExpandWidth(false));
        GUILayout.Space(10);
        Main.Settings.PlaybackBarks = GUILayout.Toggle(Main.Settings.PlaybackBarks, "Enabled");
        GUILayout.EndHorizontal();

        if (Main.Settings.PlaybackBarks)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("Only playback barks if there's silence", GUILayout.ExpandWidth(false));
            GUILayout.Space(10);
            Main.Settings.PlaybackBarkOnlyIfSilence = GUILayout.Toggle(Main.Settings.PlaybackBarkOnlyIfSilence, "Enabled");
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Playback vicinity and cutscene triggered barks", GUILayout.ExpandWidth(false));
            GUILayout.Space(10);
            Main.Settings.PlaybackBarksInVicinity = GUILayout.Toggle(Main.Settings.PlaybackBarksInVicinity, "Enabled");
            GUILayout.EndHorizontal();
        }

        GUILayout.EndVertical();

        GUILayout.BeginVertical("", GUI.skin.box);

        GUILayout.BeginHorizontal();
        GUILayout.Label("Show notification on playback stop (set the keybind in the game menu under sound).", GUILayout.ExpandWidth(false));
        GUILayout.Space(10);
        Main.Settings.ShowNotificationOnPlaybackStop = GUILayout.Toggle(Main.Settings.ShowNotificationOnPlaybackStop, "Enabled");
        GUILayout.EndHorizontal();

        GUILayout.EndVertical();

        AddColorPicker("Color on text hover", ref Main.Settings.ColorOnHover, "Hover color", ref Main.Settings.HoverColorR, ref Main.Settings.HoverColorG, ref Main.Settings.HoverColorB, ref Main.Settings.HoverColorA);

        GUILayout.BeginVertical("", GUI.skin.box);

        GUILayout.BeginHorizontal();
        GUILayout.Label("Font style on text hover", GUILayout.ExpandWidth(false));
        Main.Settings.FontStyleOnHover = GUILayout.Toggle(Main.Settings.FontStyleOnHover, "Enabled");
        GUILayout.EndHorizontal();

        if (Main.Settings.FontStyleOnHover)
        {
            GUILayout.BeginHorizontal();
            for (int i = 0; i < Main.Settings.FontStyles.Length; ++i)
            {
                Main.Settings.FontStyles[i] = GUILayout.Toggle(Main.Settings.FontStyles[i], Main.FontStyleNames[i], GUILayout.ExpandWidth(true));
            }
            GUILayout.EndHorizontal();
        }

        GUILayout.EndVertical();

        GUILayout.BeginVertical("", GUI.skin.box);

        GUILayout.BeginHorizontal();
        GUILayout.Label("Phonetic dictionary", GUILayout.ExpandWidth(false));
        GUILayout.Space(10);
        if (GUILayout.Button("Reload", GUILayout.ExpandWidth(false)))
            SpeechExtensions.LoadDictionary();
        GUILayout.EndHorizontal();

        GUILayout.EndVertical();
    }

    private static void AddColorPicker(string enableLabel, ref bool enabledBool, string colorLabel, ref float r, ref float g, ref float b)
    {
        float a = 1;
        AddColorPicker(enableLabel, ref enabledBool, colorLabel, ref r, ref g, ref b, ref a, false);
    }

    private static void AddColorPicker(string enableLabel, ref bool enabledBool, string colorLabel, ref float r, ref float g, ref float b, ref float a, bool useAlpha = true)
    {
        GUILayout.BeginVertical("", GUI.skin.box);
        GUILayout.BeginHorizontal();
        GUILayout.Label(enableLabel, GUILayout.ExpandWidth(false));
        enabledBool = GUILayout.Toggle(enabledBool, "Enabled");
        GUILayout.EndHorizontal();

        if (enabledBool)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(colorLabel, GUILayout.ExpandWidth(false));
            GUILayout.Space(10);
            GUILayout.Label("R ", GUILayout.ExpandWidth(false));
            r = GUILayout.HorizontalSlider(r, 0, 1);
            GUILayout.Space(10);
            GUILayout.Label("G", GUILayout.ExpandWidth(false));
            g = GUILayout.HorizontalSlider(g, 0, 1);
            GUILayout.Space(10);
            GUILayout.Label("B", GUILayout.ExpandWidth(false));
            b = GUILayout.HorizontalSlider(b, 0, 1);
            GUILayout.Space(10);
            if (useAlpha)
            {
                GUILayout.Label("A", GUILayout.ExpandWidth(false));
                a = GUILayout.HorizontalSlider(a, 0, 1);
                GUILayout.Space(10);
            }
            else
            {
                a = 1;
            }
            GUILayout.Box(GetColorPreview(ref r, ref g, ref b, ref a), GUILayout.Width(20));
            GUILayout.EndHorizontal();
        }
        GUILayout.EndVertical();
    }

    private static Texture2D GetColorPreview(ref float r, ref float g, ref float b, ref float a)
    {
        var texture = new Texture2D(20, 20);
        for (int y = 0; y < texture.height; y++)
        {
            for (int x = 0; x < texture.width; x++)
            {
                texture.SetPixel(x, y, new Color(r, g, b, a));
            }
        }
        texture.Apply();
        return texture;
    }
}