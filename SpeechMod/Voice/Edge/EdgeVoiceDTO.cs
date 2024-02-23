namespace SpeechMod.Voice.Edge;

public struct EdgeVoiceDto(string text, string voice, int pitch, int rate, int volume)
{
    public readonly string Text = text;
    public readonly string Voice = voice;
    public readonly int Pitch = pitch;
    public readonly int Rate = rate;
    public readonly int Volume = volume;
}