using Owlcat.Runtime.UI.Utility;
using SpeechMod.Voice;
using System.Linq;
using UnityEngine;

namespace SpeechMod.Unity.GUIs;

public class EdgeVoicePicker : VoicePickerBase
{
    private int _localeIndex;
    private int _voiceIndex;

    public EdgeVoicePicker(string label, string voiceShortName, string previewString, VoiceType voiceType) : base(label, previewString, voiceType)
    {
        var voice = Main.EdgeAvailableVoices.FirstOrDefault(v => v.ShortName.Equals(voiceShortName));
        _localeIndex = Main.EdgeVoicesDict.Keys.IndexOf(voice.Locale.Substring(0, 2));
        _voiceIndex = Main.EdgeVoicesDict.ElementAt(_localeIndex).Value.IndexOf(voice);
    }

    public override void OnGUI(ref string voiceShortName, ref int rate, ref int pitch, ref int volume)
    {
        GUILayout.BeginVertical("", GUI.skin.box);

        GUILayout.BeginHorizontal();
        GUILayout.Label($"<color=green><b>{Label}</b></color>", GUI.skin.box);
        GUILayout.EndHorizontal();

        char glyph = Selecting ? '\u25b2' : '\u25bc';
        Selecting = GUILayout.Toggle(Selecting, $"<color=yellow><b>Select Edge Voice</b> {glyph}</color>", GUI.skin.button);

        if (Selecting)
        {
            GUILayout.BeginVertical("", GUI.skin.box);

            GUILayout.BeginHorizontal();
            GUILayout.Label("<color=cyan><b>Language</b></color>", GUILayout.ExpandWidth(false));
            GUILayout.EndHorizontal();
            _localeIndex = GUILayout.SelectionGrid(_localeIndex, Main.EdgeVoicesDict.Keys.ToArray(), 10);

            var localeVoices = Main.EdgeVoicesDict.ElementAt(_localeIndex).Value;
            if (localeVoices != null && localeVoices.Any())
            {
                if (_voiceIndex > localeVoices.Length - 1)
                    _voiceIndex = 0;

                GUILayout.BeginHorizontal();
                GUILayout.Label("<color=cyan><b>Voice</b></color>", GUILayout.ExpandWidth(false));
                GUILayout.EndHorizontal();
                _voiceIndex = GUILayout.SelectionGrid(_voiceIndex, localeVoices.Select(v => v.ShortName).ToArray(), 7);

                voiceShortName = localeVoices.ElementAt(_voiceIndex).ShortName;
            }

            GUILayout.EndVertical();
        }

        GUILayout.BeginHorizontal();
        GUILayout.Label(voiceShortName, GUILayout.ExpandWidth(false));
        GUILayout.EndHorizontal();

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
