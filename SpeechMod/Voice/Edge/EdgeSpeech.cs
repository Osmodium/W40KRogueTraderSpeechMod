using Kingmaker;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Kingmaker.Blueprints.Base;
using UnityEngine;
using SpeechMod.Unity.Voices;

namespace SpeechMod.Voice.Edge;

public class EdgeSpeech : ISpeech
{
    private EdgeVoiceDto[] GetDialogs(string text)
    {
        Debug.Log("GetDialogs");
        if (Main.Settings?.LogVoicedLines == true)
            Debug.Log(text);

        text = new Regex("<b><color[^>]+><link([^>]+)?>([^<>]*)</link></color></b>").Replace(text, "$2");

        if (Main.Settings?.LogVoicedLines == true)
            Debug.Log(text);

        return ExtractOrderedStrings(text);
    }

    private static EdgeVoiceDto[] ExtractOrderedStrings(string text)
    {
        var orderedList = new List<EdgeVoiceDto>();

        var regex = new Regex("<i><color=#544709>(.*?)</color></i>");
        var matches = regex.Matches(text);

        var currentIndex = 0;
        foreach (Match match in matches)
        {
            var textBetweenMatches = text.Substring(currentIndex, match.Index - currentIndex);

            var genderVoice = CreateGenderEdgeVoiceDto(textBetweenMatches);
            if (genderVoice.HasValue)
                orderedList.Add(genderVoice.Value);

            var narrator = CreateNarratorEdgeVoiceDto(match.Groups[1].Value);
            if (narrator.HasValue)
                orderedList.Add(narrator.Value);

            currentIndex = match.Index + match.Length;
        }

        if (currentIndex < text.Length)
        {
            var genderVoice = CreateGenderEdgeVoiceDto(text.Substring(currentIndex));
            if (genderVoice.HasValue)
                orderedList.Add(genderVoice.Value);
        }

        return orderedList.ToArray();
    }

    private static EdgeVoiceDto? CreateNarratorEdgeVoiceDto(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return null;

        return new EdgeVoiceDto(text, Main.Settings.NarratorVoice, Main.Settings.NarratorPitch, Main.Settings.NarratorRate, Main.Settings.NarratorVolume);
    }

    private static EdgeVoiceDto? CreateGenderEdgeVoiceDto(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return null;

        return Game.Instance?.DialogController?.CurrentSpeaker?.Gender switch
        {
            Gender.Female => new EdgeVoiceDto(text, Main.Settings.FemaleVoice, Main.Settings.FemalePitch, Main.Settings.FemaleRate, Main.Settings.FemaleVolume),
            Gender.Male => new EdgeVoiceDto(text, Main.Settings.MaleVoice, Main.Settings.MalePitch, Main.Settings.MaleRate, Main.Settings.MaleVolume),
            _ => new EdgeVoiceDto(text, Main.Settings.NarratorVoice, Main.Settings.NarratorPitch, Main.Settings.NarratorRate, Main.Settings.NarratorVolume)
        };
    }

    public string GetStatusMessage()
    {
        return EdgeVoiceUnity.GetStatusMessage();
    }

    public void SetAvailableVoices()
    {
        EdgeVoiceUnity.SetAvailableVoices();
    }

    public bool IsSpeaking()
    {
        return EdgeVoiceUnity.IsSpeaking();
    }

    public void SpeakPreview(string text, VoiceType voiceType)
    {
        if (string.IsNullOrEmpty(text))
        {
            Main.Logger?.Warning("No text to speak!");
            return;
        }

        switch (voiceType)
        {
            case VoiceType.Narrator:
                EdgeVoiceUnity.Speak(new EdgeVoiceDto(text, Main.Settings.NarratorVoice, Main.Settings.NarratorPitch, Main.Settings.NarratorRate, Main.Settings.NarratorVolume));
                return;
            case VoiceType.Female:
                EdgeVoiceUnity.Speak(new EdgeVoiceDto(text, Main.Settings.FemaleVoice, Main.Settings.FemalePitch, Main.Settings.FemaleRate, Main.Settings.FemaleVolume));
                return;
            case VoiceType.Male:
                EdgeVoiceUnity.Speak(new EdgeVoiceDto(text, Main.Settings.MaleVoice, Main.Settings.MalePitch, Main.Settings.MaleRate, Main.Settings.MaleVolume));
                return;
            default:
                throw new ArgumentOutOfRangeException(nameof(voiceType), voiceType, null);
        }
    }

    public void SpeakDialog(string text, float delay = 0)
    {
        Debug.Log("SpeakDialog");

        if (string.IsNullOrEmpty(text))
        {
            Main.Logger?.Warning("No text to speak!");
            return;
        }

        if (!Main.Settings.UseGenderSpecificVoices)
        {
            Speak(text, delay);
            return;
        }

        EdgeVoiceUnity.SpeakMulti(GetDialogs(text));
    }

    public void SpeakAs(string text, VoiceType type, float delay = 0)
    {
        if (string.IsNullOrEmpty(text))
        {
            Main.Logger?.Warning("No text to speak!");
            return;
        }

        if (Main.Settings?.LogVoicedLines == true)
            UnityEngine.Debug.Log(text);

        if (!Main.Settings!.UseGenderSpecificVoices)
        {
            Speak(text, delay);
            return;
        }

        switch (type)
        {
            case VoiceType.Narrator:
                EdgeVoiceUnity.Speak(new EdgeVoiceDto(text, Main.Settings.NarratorVoice, Main.Settings.NarratorPitch, Main.Settings.NarratorRate, Main.Settings.NarratorVolume));
                return;
            case VoiceType.Female:
                EdgeVoiceUnity.Speak(new EdgeVoiceDto(text, Main.Settings.FemaleVoice, Main.Settings.FemalePitch, Main.Settings.FemaleRate, Main.Settings.FemaleVolume));
                return;
            case VoiceType.Male:
                EdgeVoiceUnity.Speak(new EdgeVoiceDto(text, Main.Settings.MaleVoice, Main.Settings.MalePitch, Main.Settings.MaleRate, Main.Settings.MaleVolume));
                return;
            default:
                Main.Logger?.Warning($"Unknown voice type: {type}");
                return;
        }
    }

    public void Speak(string text, float delay = 0)
    {
        if (string.IsNullOrEmpty(text))
        {
            Main.Logger?.Warning("No text to speak!");
            return;
        }

        EdgeVoiceUnity.Speak(new EdgeVoiceDto(text, Main.Settings.NarratorVoice, Main.Settings.NarratorPitch, Main.Settings.NarratorRate, Main.Settings.NarratorVolume));
    }

    public void Stop()
    {
        EdgeVoiceUnity.Stop();
    }
}