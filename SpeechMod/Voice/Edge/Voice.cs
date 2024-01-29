using System.Collections.Generic;

namespace SpeechMod.Voice.Edge;

public class Voice
{
    public string Name { get; set; }
    public string ShortName { get; set; }
    public string Gender { get; set; }
    public string Locale { get; set; }
    public string SuggestedCodec { get; set; }
    public string FriendlyName { get; set; }
    public string Status { get; set; }
    public VoiceTag VoiceTag { get; set; }
}

public class VoiceTag
{
    public List<string> ContentCategories { get; set; }
    public List<string> VoicePersonalities { get; set; }
}