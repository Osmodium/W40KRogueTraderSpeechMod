using SpeechMod.Voice;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using UnityModManagerNet;

namespace SpeechMod;

/// <summary>
/// XML-serializable entry for per-character voice settings.
/// </summary>
public class CharacterVoiceEntry
{
    [XmlAttribute] public string CharacterId;
    public int VoiceIndex;
    public int Rate;
    public int Volume = 100;
    public int Pitch;
}

public class Settings : UnityModManager.ModSettings
{
    public bool LogVoicedLines = false;

    /// <summary>
    /// XML-serializable list backing the per-character voice dictionary.
    /// </summary>
    public List<CharacterVoiceEntry> CharacterVoiceEntries = new();

    /// <summary>
    /// Runtime dictionary built from CharacterVoiceEntries.  Not serialized.
    /// </summary>
    [XmlIgnore]
    public Dictionary<string, CharacterVoiceSettings> CharacterVoices
    {
        get
        {
            if (_characterVoicesCache != null)
                return _characterVoicesCache;

            _characterVoicesCache = CharacterVoiceEntries
                .Where(e => !string.IsNullOrWhiteSpace(e.CharacterId))
                .ToDictionary(
                    e => e.CharacterId,
                    e => new CharacterVoiceSettings
                    {
                        VoiceIndex = e.VoiceIndex,
                        Rate = e.Rate,
                        Volume = e.Volume,
                        Pitch = e.Pitch
                    });
            return _characterVoicesCache;
        }
    }

    [XmlIgnore]
    private Dictionary<string, CharacterVoiceSettings> _characterVoicesCache;

    /// <summary>
    /// Sync the runtime dictionary back into the serializable list and invalidate cache.
    /// Call this before saving.
    /// </summary>
    public void SyncCharacterVoiceEntries()
    {
        if (_characterVoicesCache == null)
            return;

        CharacterVoiceEntries = _characterVoicesCache
            .Select(kvp => new CharacterVoiceEntry
            {
                CharacterId = kvp.Key,
                VoiceIndex = kvp.Value.VoiceIndex,
                Rate = kvp.Value.Rate,
                Volume = kvp.Value.Volume,
                Pitch = kvp.Value.Pitch
            }).ToList();
    }

    /// <summary>
    /// Invalidate the dictionary cache so it will be rebuilt from entries on next access.
    /// </summary>
    public void InvalidateCharacterVoicesCache()
    {
        _characterVoicesCache = null;
    }

    public string[] AvailableVoices;

    public int NarratorVoice = 0;
    public int NarratorRate = 0;
    public int NarratorVolume = 100;
    public int NarratorPitch = 0;

    public bool UseGenderSpecificVoices = false;

    public int FemaleVoice = 0;
    public int FemaleRate = 0;
    public int FemaleVolume = 100;
    public int FemalePitch = 0;

    public int MaleVoice = 0;
    public int MaleRate = 0;
    public int MaleVolume = 100;
    public int MalePitch = 0;

    public bool UseProtagonistSpecificVoice = false;
    public int ProtagonistVoice = 0;
    public int ProtagonistRate = 0;
    public int ProtagonistVolume = 100;
    public int ProtagonistPitch = 0;

    public bool AutoPlay = false;
    public bool AutoPlayIgnoreVoice = false;

    public bool ColorOnHover = false;
    public float HoverColorR = 0f;
    public float HoverColorG = 0f;
    public float HoverColorB = 0f;
    public float HoverColorA = 1f;

    public bool FontStyleOnHover = true;
    public bool[] FontStyles = [false, false, false, true, false, false, false, false, false, false, false];

    public bool InterruptPlaybackOnPlay = true;
    public bool PlaybackBarks = true;
    public bool PlaybackBarkOnlyIfSilence = true;
    public bool PlaybackBarksInVicinity = false;
    public bool ShowNotificationOnPlaybackStop = true;

    public bool ShowPlaybackOfDialogAnswers = true;
    public bool SayDialogAnswerNumber = false;
    public bool DialogAnswerColorOnHover = true;
    public float DialogAnswerHoverColorR = 0.15f;
    public float DialogAnswerHoverColorG = 0.75f;
    public float DialogAnswerHoverColorB = 0.75f;

    public bool AutoStopPlaybackOnLoading = false;

    public override void Save(UnityModManager.ModEntry modEntry)
    {
        SyncCharacterVoiceEntries();
        Save(this, modEntry);
    }
}
