using Kingmaker;
using Kingmaker.Blueprints.Base;
using SpeechMod.Unity;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

namespace SpeechMod.Voice;

public class WindowsSpeech : ISpeech
{
    private static string SpeakBegin => "<speak version=\"1.0\" xmlns=\"http://www.w3.org/2001/10/synthesis\" xmlns:mstts=\"http://www.w3.org/2001/mstts\">";
    private static string SpeakEnd => "</speak>";

    private static string NarratorVoice => $"<voice required=\"Name={Main.NarratorVoice}\">";
    private static string NarratorPitch => $"<pitch absmiddle=\"{Main.Settings?.NarratorPitch}\"/>";
    private static string NarratorRate => $"<rate absspeed=\"{Main.Settings?.NarratorRate}\"/>";
    private static string NarratorVolume => $"<volume level=\"{Main.Settings?.NarratorVolume}\"/>";

    private static string FemaleVoice => $"<voice required=\"Name={Main.FemaleVoice}\">";
    private static string FemaleVolume => $"<volume level=\"{Main.Settings?.FemaleVolume}\"/>";
    private static string FemalePitch => $"<pitch absmiddle=\"{Main.Settings?.FemalePitch}\"/>";
    private static string FemaleRate => $"<rate absspeed=\"{Main.Settings?.FemaleRate}\"/>";

    private static string MaleVoice => $"<voice required=\"Name={Main.MaleVoice}\">";
    private static string MaleVolume => $"<volume level=\"{Main.Settings?.MaleVolume}\"/>";
    private static string MalePitch => $"<pitch absmiddle=\"{Main.Settings?.MalePitch}\"/>";
    private static string MaleRate => $"<rate absspeed=\"{Main.Settings?.MaleRate}\"/>";

    private static string ProtagonistVoice => $"<voice required=\"Name={Main.ProtagonistVoice}\">";
    private static string ProtagonistVolume => $"<volume level=\"{Main.Settings?.ProtagonistVolume}\"/>";
    private static string ProtagonistPitch => $"<pitch absmiddle=\"{Main.Settings?.ProtagonistPitch}\"/>";
    private static string ProtagonistRate => $"<rate absspeed=\"{Main.Settings?.ProtagonistRate}\"/>";

    public string CombinedNarratorVoiceStart => $"{NarratorVoice}{NarratorPitch}{NarratorRate}{NarratorVolume}";
    public string CombinedFemaleVoiceStart => $"{FemaleVoice}{FemalePitch}{FemaleRate}{FemaleVolume}";
    public string CombinedMaleVoiceStart => $"{MaleVoice}{MalePitch}{MaleRate}{MaleVolume}";
    public string CombinedProtagonistVoiceStart => $"{ProtagonistVoice}{ProtagonistPitch}{ProtagonistRate}{ProtagonistVolume}";

    public virtual string CombinedDialogVoiceStart
    {
        get
        {
            if (Game.Instance?.DialogController?.CurrentSpeaker == null)
                return CombinedNarratorVoiceStart;

            // Check per-character voice settings first
            var characterVoiceStart = TryGetCharacterVoiceStart(Game.Instance.DialogController.CurrentSpeaker);
            if (characterVoiceStart != null)
                return characterVoiceStart;

            if (Game.Instance?.DialogController?.CurrentSpeaker.IsMainCharacter == true)
                return CombinedProtagonistVoiceStart;

            return Game.Instance?.DialogController?.CurrentSpeaker.Gender switch
            {
                Gender.Female => CombinedFemaleVoiceStart,
                Gender.Male => CombinedMaleVoiceStart,
                _ => CombinedNarratorVoiceStart
            };
        }
    }

    private static string TryGetCharacterVoiceStart(Kingmaker.EntitySystem.Entities.BaseUnitEntity speaker)
    {
        if (speaker == null || Main.Settings?.CharacterVoices == null || Main.Settings.CharacterVoices.Count == 0)
            return null;

        var characterId = speaker.Blueprint?.AssetGuid?.ToString();
        if (string.IsNullOrWhiteSpace(characterId))
            characterId = speaker.CharacterName?.GetHashCode().ToString("X8");

        if (string.IsNullOrWhiteSpace(characterId))
            return null;

        if (!Main.Settings.CharacterVoices.TryGetValue(characterId, out var charVoice))
            return null;

        var voiceKey = Main.VoicesDict?.ElementAt(charVoice.VoiceIndex >= 0 && charVoice.VoiceIndex < (Main.VoicesDict?.Count ?? 0) ? charVoice.VoiceIndex : 0).Key;
        if (string.IsNullOrWhiteSpace(voiceKey))
            return null;

        return $"<voice required=\"Name={voiceKey}\"><pitch absmiddle=\"{charVoice.Pitch}\"/><rate absspeed=\"{charVoice.Rate}\"/><volume level=\"{charVoice.Volume}\"/>";
    }

    /// <summary>
    /// Try to get character voice SSML start tag for the current dialog speaker.
    /// Returns null if no per-character voice is configured.
    /// </summary>
    private static string TryGetCharacterVoiceStartFromCurrentSpeaker()
    {
        var speaker = Game.Instance?.DialogController?.CurrentSpeaker;
        if (speaker == null)
            return null;
        return TryGetCharacterVoiceStart(speaker);
    }

    /// <summary>
    /// Try to get character voice SSML start tag for SpeakAs calls (protagonist answers).
    /// Returns null if no per-character voice is configured.
    /// </summary>
    private static string TryGetCharacterVoiceStartForSpeakAs(VoiceType voiceType)
    {
        if (voiceType == VoiceType.Protagonist || voiceType == VoiceType.Female || voiceType == VoiceType.Male)
        {
            var protagonist = Game.Instance?.Player?.MainCharacterEntity;
            if (protagonist != null)
                return TryGetCharacterVoiceStart(protagonist);
        }
        return null;
    }

    /// <summary>
    /// Prepare dialog text using a per-character voice, handling narrator color sections.
    /// </summary>
    private string PrepareDialogTextWithCharacterVoice(string text, string characterVoiceStart)
    {
        text = text.PrepareText();
        text = new Regex("<b><color[^>]+><link([^>]+)?>([^<>]*)</link></color></b>").Replace(text, "$2");

        // Replace narrator-colored sections with narrator voice, the rest with character voice
        text = text.Replace($"<i><color=#{Constants.NARRATOR_COLOR_CODE}>", $"</voice>{CombinedNarratorVoiceStart}");
        text = text.Replace("</color></i>", $"</voice>{characterVoiceStart}");

        if (text.StartsWith("</voice>"))
            text = text.Remove(0, 8);
        else
            text = characterVoiceStart + text;

        if (text.EndsWith(characterVoiceStart!))
            text = text.Remove(text.Length - characterVoiceStart.Length, characterVoiceStart.Length);

        if (!text.EndsWith("</voice>"))
            text += "</voice>";

        return text;
    }

    public static int Length(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return 0;

        var arr = new[] { "—", "-", "\"" };

        return arr.Aggregate(text, (current, t) => current.Replace(t, "")).Length;
    }

    private string FormatGenderSpecificVoices(string text)
    {
        text = text.Replace($"<i><color=#{Constants.NARRATOR_COLOR_CODE}>", $"</voice>{CombinedNarratorVoiceStart}");
        text = text.Replace("</color></i>", $"</voice>{CombinedDialogVoiceStart}");

        if (text.StartsWith("</voice>"))
            text = text.Remove(0, 8);
        else
            text = CombinedDialogVoiceStart + text;

        if (text.EndsWith(CombinedDialogVoiceStart!))
            text = text.Remove(text.Length - CombinedDialogVoiceStart.Length, CombinedDialogVoiceStart.Length);

        if (!text.EndsWith("</voice>"))
            text += "</voice>";
        return text;
    }

    private static void SpeakInternal(string text, float delay = 0f)
    {
        text = SpeakBegin + text + SpeakEnd;
        if (Main.Settings?.LogVoicedLines == true)
            UnityEngine.Debug.Log(text);
        WindowsVoiceUnity.Speak(text, Length(text), delay);
    }

    public bool IsSpeaking()
    {
        return WindowsVoiceUnity.IsSpeaking;
    }

    public void SpeakPreview(string text, VoiceType voiceType)
    {
        if (string.IsNullOrEmpty(text))
        {
            Main.Logger?.Warning("No text to speak!");
            return;
        }

        text = new Regex("<[^>]+>").Replace(text, "");
        text = text.PrepareText();

        text = voiceType switch
        {
            VoiceType.Narrator => $"{CombinedNarratorVoiceStart}{text}</voice>",
            VoiceType.Female => $"{CombinedFemaleVoiceStart}{text}</voice>",
            VoiceType.Male => $"{CombinedMaleVoiceStart}{text}</voice>",
            VoiceType.Protagonist => $"{CombinedProtagonistVoiceStart}{text}</voice>",
            _ => throw new ArgumentOutOfRangeException(nameof(voiceType), voiceType, null)
        };

        SpeakInternal(text);
    }

    public string PrepareSpeechText(string text)
    {
        text = new Regex("<[^>]+>").Replace(text, "");
        text = text.PrepareText();
        text = $"{CombinedNarratorVoiceStart}{text}</voice>";
        return text;
    }

    public string PrepareDialogText(string text)
    {
        text = text.PrepareText();
        text = new Regex("<b><color[^>]+><link([^>]+)?>([^<>]*)</link></color></b>").Replace(text, "$2");
        text = FormatGenderSpecificVoices(text);
        return text;
    }

    public void SpeakDialog(string text, float delay = 0f)
    {
        if (string.IsNullOrEmpty(text))
        {
            Main.Logger?.Warning("No text to speak!");
            return;
        }

        if (Main.Settings?.LogVoicedLines == true)
        {
            Debug.Log(text);
        }

        // Always check per-character voice first
        var charVoice = TryGetCharacterVoiceStartFromCurrentSpeaker();
        if (charVoice != null)
        {
            text = PrepareDialogTextWithCharacterVoice(text, charVoice);
            SpeakInternal(text, delay);
            return;
        }

        if (!Main.Settings.UseGenderSpecificVoices)
        {
            Speak(text, delay);
            return;
        }

        text = PrepareDialogText(text);

        SpeakInternal(text, delay);
    }

    public void SpeakAs(string text, VoiceType voiceType, float delay = 0f)
    {
        if (string.IsNullOrEmpty(text))
        {
            Main.Logger?.Warning("No text to speak!");
            return;
        }

        // Check per-character voice for the relevant speaker
        var charVoice = TryGetCharacterVoiceStartForSpeakAs(voiceType);
        if (charVoice != null)
        {
            text = $"{charVoice}{text}</voice>";
            SpeakInternal(text, delay);
            return;
        }

        if (Main.Settings!.UseProtagonistSpecificVoice && voiceType == VoiceType.Protagonist)
        {
            text = $"{CombinedProtagonistVoiceStart}{text}</voice>";
            SpeakInternal(text, delay);
            return;
        }

        if (!Main.Settings!.UseGenderSpecificVoices)
        {
            Speak(text, delay);
            return;
        }

        text = voiceType switch
        {
            VoiceType.Narrator => $"{CombinedNarratorVoiceStart}{text}</voice>",
            VoiceType.Female => $"{CombinedFemaleVoiceStart}{text}</voice>",
            VoiceType.Male => $"{CombinedMaleVoiceStart}{text}</voice>",
            VoiceType.Protagonist => $"{CombinedProtagonistVoiceStart}{text}</voice>",
            _ => throw new ArgumentOutOfRangeException(nameof(voiceType), voiceType, null)
        };

        SpeakInternal(text, delay);
    }

    public void Speak(string text, float delay = 0f)
    {
        if (string.IsNullOrEmpty(text))
        {
            Main.Logger?.Warning("No text to speak!");
            return;
        }

        text = PrepareSpeechText(text);

        SpeakInternal(text, delay);
    }

    public void Stop()
    {
        WindowsVoiceUnity.Stop();
    }

    public string[] GetAvailableVoices()
    {
        return WindowsVoiceUnity.GetAvailableVoices();
    }

    public string GetStatusMessage()
    {
        return WindowsVoiceUnity.GetStatusMessage();
    }
}