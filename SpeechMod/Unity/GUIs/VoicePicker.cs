using Owlcat.Runtime.UI.Utility;
using SpeechMod.Voice;
using System.Linq;
using UnityEngine;

namespace SpeechMod.Unity.GUIs;

public class VoicePicker : VoicePickerBase
{
    public VoicePicker(string label, string voiceShortName, string previewString, VoiceType voiceType) : base(label, previewString, voiceType)
    {
        VoiceIndex = Main.AvailableVoices.IndexOf(voiceShortName);
    }

    public override void OnGUI(ref string voiceShortName, ref int rate, ref int pitch, ref int volume)
    {
        GUILayout.BeginVertical("", GUI.skin.box);

        GUILayout.BeginHorizontal();
        GUILayout.Label($"<color=green><b>{Label}</b></color>", GUI.skin.box);
        GUILayout.EndHorizontal();

        char glyph = Selecting ? '\u25b2' : '\u25bc';
        Selecting = GUILayout.Toggle(Selecting, $"<color=yellow><b>Select Voice</b> {glyph}</color>", GUI.skin.button);

        if (Selecting)
        {
            GUILayout.BeginVertical("", GUI.skin.box);

            if (VoiceIndex < 0 || VoiceIndex > Main.AvailableVoices?.Length - 1)
                VoiceIndex = 0;

            GUILayout.BeginHorizontal();
            GUILayout.Label("<color=cyan><b>Voice</b></color>", GUILayout.ExpandWidth(false));
            GUILayout.EndHorizontal();
            VoiceIndex = GUILayout.SelectionGrid(VoiceIndex, Main.AvailableVoices, 6);

            voiceShortName = Main.AvailableVoices?.ElementAt(VoiceIndex);

            GUILayout.BeginHorizontal();
            GUILayout.Label(voiceShortName, GUILayout.ExpandWidth(false));
            GUILayout.EndHorizontal();

            GUILayout.EndVertical();
        }

        GUILayout.BeginHorizontal();
        GUILayout.Label("<color=cyan>Speech rate %</color>", GUILayout.ExpandWidth(false));
        GUILayout.Space(10);
        rate = (int)GUILayout.HorizontalSlider(rate, -100, 100, GUILayout.Width(300f));
        GUILayout.Label($" {rate}", GUILayout.ExpandWidth(false));
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label("<color=cyan>Speech pitch %</color>", GUILayout.ExpandWidth(false));
        pitch = (int)GUILayout.HorizontalSlider(pitch, -100, 100, GUILayout.Width(300f));
        GUILayout.Label($" {pitch}", GUILayout.ExpandWidth(false));
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label("<color=cyan>Speech volume dB</color>", GUILayout.ExpandWidth(false));
        GUILayout.Space(10);
        volume = (int)GUILayout.HorizontalSlider(volume, -50, 50, GUILayout.Width(300f));
        GUILayout.Label($" {volume}", GUILayout.ExpandWidth(false));
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label("<color=green>Preivew voice</color>", GUILayout.ExpandWidth(false));
        GUILayout.Space(10);
        PreviewString = GUILayout.TextField(PreviewString, GUILayout.Width(700f));
        if (GUILayout.Button("Play", GUILayout.ExpandWidth(true)))
            Main.Speech?.SpeakPreview(PreviewString, VoiceType);
        GUILayout.EndHorizontal();

        GUILayout.EndVertical();
    }
}
