using SpeechMod.Voice;

namespace SpeechMod.Unity.GUIs;

public abstract class VoicePickerBase(string label, string previewString, VoiceType voiceType)
{
    public readonly string Label = label;
    public readonly VoiceType VoiceType = voiceType;
    public int VoiceIndex;
    public string PreviewString = previewString;

    public bool Selecting;

    public abstract void OnGUI(ref string voiceShortName, ref int rate, ref int pitch, ref int volume);
}