using EdgeVoiceUnity = SpeechMod.Unity.EdgeVoiceUnity;

namespace SpeechMod.Voice;

public class EdgeSpeech : ISpeech
{
    public string GetStatusMessage()
    {
        return EdgeVoiceUnity.GetStatusMessage();
    }

    public string[] GetAvailableVoices()
    {
        return EdgeVoiceUnity.GetAvailableVoices();
    }

    public bool IsSpeaking()
    {
        return EdgeVoiceUnity.IsSpeaking();
    }

    public void SpeakPreview(string text, VoiceType voiceType)
    {
        EdgeVoiceUnity.Speak(text);
    }

    public void SpeakDialog(string text, float delay = 0)
    {
        EdgeVoiceUnity.Speak(text);
    }

    public void SpeakAs(string text, VoiceType type, float delay = 0)
    {
        EdgeVoiceUnity.Speak(text);
    }

    public void Speak(string text, float delay = 0)
    {
        EdgeVoiceUnity.Speak(text);
    }

    public void Stop()
    {
        EdgeVoiceUnity.Stop();
    }
}