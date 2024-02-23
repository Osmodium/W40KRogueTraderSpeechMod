//using SpeechMod.Configuration.Settings;

//namespace SpeechMod.Configuration.SettingEntries;

//public class NarratorVoiceDropdown : ModDropdownSettingEntry
//{
//    private const string _key = "narrator.voice";
//    private const string _title = "Narrator Voice";
//    private const string _tooltip = "Select the voice of the narrator.";

//    public NarratorVoiceDropdown() : base(_key, _title, _tooltip)
//    { }

//    public override SettingStatus TryEnable()
//    {
//        Main.Settings!.NarratorVoice = SelectedIndex;
//        return SettingStatus.WORKING;
//    }
//}