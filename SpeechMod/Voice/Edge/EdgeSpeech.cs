using System;

namespace SpeechMod.Voice.Edge;

public class EdgeSpeech : ISpeech
{
    //private const string SSML_PREFIX = "<speak version='1.0' xmlns='http://www.w3.org/2001/10/synthesis' xml:lang='en-US'>";
    //private const string SSML_VOICE_END = "</prosody></voice>";
    //private const string SSML_POSFIX = "</speak>";

    //private readonly string _narratorVoiceSsmlStart = $"<voice name='{Main.Settings.NarratorVoice}'><prosody pitch='{FormatValue(Main.Settings.NarratorPitch)}Hz' rate='{FormatValue(Main.Settings.NarratorRate)}%' volume='{FormatValue(Main.Settings.NarratorVolume)}%'>";
    //private readonly string _femaleVoiceSsmlStart = $"<voice name='{Main.Settings.FemaleVoice}'><prosody pitch='{FormatValue(Main.Settings.FemalePitch)}Hz' rate='{FormatValue(Main.Settings.FemaleRate)}%' volume='{FormatValue(Main.Settings.FemaleVolume)}%'>";
    //private readonly string _maleVoiceSsmlStart = $"<voice name='{Main.Settings.MaleVoice}'><prosody pitch='{FormatValue(Main.Settings.MalePitch)}Hz' rate='{FormatValue(Main.Settings.MaleRate)}%' volume='{FormatValue(Main.Settings.MaleVolume)}%'>";

    //private static string FormatValue(int value)
    //{
    //    return (value >= 0 ? "+" : "") + value;
    //}

    //private static string GetSsml(string text, string voice, int pitch, int rate, int volume)
    //{
    //    return $"<voice name='{voice}'><prosody pitch='{FormatValue(pitch)}Hz' rate='{FormatValue(rate)}%' volume='{FormatValue(volume)}%'>{text}</prosody></voice>";
    //}

    //private string GetNarratorSsml(string text)
    //{
    //    return GetSsml(text, Main.Settings.NarratorVoice, Main.Settings.NarratorPitch, Main.Settings.NarratorRate, Main.Settings.NarratorVolume);
    //}

    //private string GetFemaleSsml(string text)
    //{
    //    return GetSsml(text, Main.Settings.FemaleVoice, Main.Settings.FemalePitch, Main.Settings.FemaleRate, Main.Settings.FemaleVolume);
    //}

    //private string GetMaleSsml(string text)
    //{
    //    return GetSsml(text, Main.Settings.MaleVoice, Main.Settings.MalePitch, Main.Settings.MaleRate, Main.Settings.MaleVolume);
    //}

    //public virtual string DialogSsmlStart
    //{
    //    get
    //    {
    //        if (Game.Instance?.DialogController?.CurrentSpeaker == null)
    //            return _narratorVoiceSsmlStart;

    //        return Game.Instance.DialogController.CurrentSpeaker.Gender switch
    //        {
    //            Gender.Female => _femaleVoiceSsmlStart,
    //            Gender.Male => _maleVoiceSsmlStart,
    //            _ => _narratorVoiceSsmlStart
    //        };
    //    }
    //}

    //private string FormatGenderSpecificVoices(string text)
    //{
    //    text = text.Replace("<i><color=#544709>", $"{SSML_VOICE_END}{_narratorVoiceSsmlStart}");
    //    text = text.Replace("</color></i>", $"{SSML_VOICE_END}{DialogSsmlStart}");

    //    if (text.StartsWith(SSML_VOICE_END))
    //        text = text.Remove(0, SSML_VOICE_END.Length);
    //    else
    //        text = DialogSsmlStart + text;

    //    if (text.EndsWith(DialogSsmlStart!))
    //        text = text.Remove(text.Length - DialogSsmlStart.Length, DialogSsmlStart.Length);

    //    if (!text.EndsWith(SSML_VOICE_END))
    //        text += SSML_VOICE_END;
    //    return text;
    //}

    //public string PrepareDialogText(string text)
    //{
    //    if (Main.Settings?.LogVoicedLines == true)
    //        UnityEngine.Debug.Log(text);

    //    text = text.PrepareText();
    //    text = new Regex("<b><color[^>]+><link([^>]+)?>([^<>]*)</link></color></b>").Replace(text, "$2");
    //    text = FormatGenderSpecificVoices(text);

    //    if (Main.Settings?.LogVoicedLines == true)
    //        UnityEngine.Debug.Log(text);

    //    return text;
    //}

    //private void WrapAndSpeak(string text)
    //{
    //    //EdgeVoiceUnity.Speak(SSML_PREFIX + text + SSML_POSFIX);
    //    //EdgeVoiceManager.Instance.Speak(new EdgeVoiceDto (text, ));

    //}

    private EdgeVoiceDto[] GetDialogs(string text)
    {
        return null;
    }

    public string GetStatusMessage()
    {
        return EdgeVoiceManager.Instance.GetStatusMessage();
    }

    public void SetAvailableVoices()
    {
        EdgeVoiceManager.Instance.SetAvailableVoices();
    }

    public bool IsSpeaking()
    {
        return EdgeVoiceManager.Instance!.IsSpeaking();
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
                EdgeVoiceManager.Instance.Speak(new EdgeVoiceDto(text, Main.Settings.NarratorVoice, Main.Settings.NarratorPitch, Main.Settings.NarratorRate, Main.Settings.NarratorVolume));
                return;
            case VoiceType.Female:
                EdgeVoiceManager.Instance.Speak(new EdgeVoiceDto(text, Main.Settings.FemaleVoice, Main.Settings.FemalePitch, Main.Settings.FemaleRate, Main.Settings.FemaleVolume));
                return;
            case VoiceType.Male:
                EdgeVoiceManager.Instance.Speak(new EdgeVoiceDto(text, Main.Settings.MaleVoice, Main.Settings.MalePitch, Main.Settings.MaleRate, Main.Settings.MaleVolume));
                return;
            default:
                throw new ArgumentOutOfRangeException(nameof(voiceType), voiceType, null);
        }
    }

    public void SpeakDialog(string text, float delay = 0)
    {
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

        EdgeVoiceManager.Instance.SpeakMulti(GetDialogs(text));
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
                EdgeVoiceManager.Instance.Speak(new EdgeVoiceDto(text, Main.Settings.NarratorVoice, Main.Settings.NarratorPitch, Main.Settings.NarratorRate, Main.Settings.NarratorVolume));
                return;
            case VoiceType.Female:
                EdgeVoiceManager.Instance.Speak(new EdgeVoiceDto(text, Main.Settings.FemaleVoice, Main.Settings.FemalePitch, Main.Settings.FemaleRate, Main.Settings.FemaleVolume));
                return;
            case VoiceType.Male:
                EdgeVoiceManager.Instance.Speak(new EdgeVoiceDto(text, Main.Settings.MaleVoice, Main.Settings.MalePitch, Main.Settings.MaleRate, Main.Settings.MaleVolume));
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

        EdgeVoiceManager.Instance.Speak(new EdgeVoiceDto(text, Main.Settings.NarratorVoice, Main.Settings.NarratorPitch, Main.Settings.NarratorRate, Main.Settings.NarratorVolume));
    }

    public void Stop()
    {
        EdgeVoiceManager.Instance.Stop();
    }
}