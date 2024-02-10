using System.Collections.Generic;

namespace SpeechMod.Voice.Edge;

public class EdgeVoiceInfo
{
    public string Name { get; set; }
    public string ShortName { get; set; }
    public string Gender { get; set; }
    public string Locale { get; set; }
    public string SuggestedCodec { get; set; }
    public string FriendlyName { get; set; }
    public string Status { get; set; }
    public EdgeVoiceInfoTag VoiceTag { get; set; }
}

public class EdgeVoiceInfoTag
{
    public List<string> ContentCategories { get; set; }
    public List<string> VoicePersonalities { get; set; }
}