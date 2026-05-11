using Newtonsoft.Json;

namespace SpeechMod.Voice;

public class CharacterVoiceSettings
{
    [JsonProperty] public int VoiceIndex = 0;
    [JsonProperty] public int Rate = 0;
    [JsonProperty] public int Volume = 100;
    [JsonProperty] public int Pitch = 0;
}

