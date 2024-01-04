using HarmonyLib;
using Kingmaker;
using Kingmaker.Blueprints.Base;
using Kingmaker.Code.UI.MVVM.VM.Bark;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.GameModes;
using Kingmaker.Mechanics.Entities;
using SpeechMod.Voice;
using System;
using UnityEngine;

namespace SpeechMod.Patches;

public static class BarkExtensions
{
    public static bool PlayBark()
    {
        if (!Main.Enabled)
            return false;

        if (!Main.Settings!.PlaybackBarks)
            return false;

        // Don't play barks if we are in a dialog.
        if (Game.Instance == null || Game.Instance.IsModeActive(GameModeType.Dialog))
            return false;

        if (Main.Settings.PlaybackBarkOnlyIfSilence && Main.Speech?.IsSpeaking() == true)
            return false;

        if (!Main.Settings.PlaybackBarksInVicinity)
        {
            var stackTrace = Environment.StackTrace;
            if (stackTrace.Contains("UnitsProximityController.TickOnUnit") ||
                stackTrace.Contains("Cutscenes.Commands.CommandBark"))
                return false;
        }

        return true;
    }

    public static void DoBark(Entity entity, string text, string voiceOver)
    {
        if (!string.IsNullOrWhiteSpace(voiceOver))
            return;

        SpeakBark(text, entity);
    }

    public static void SpeakBark(string text, Entity entity)
    {
        if (entity is not LightweightUnitEntity lightweightUnitEntity)
        {
            if (entity is AbstractUnitEntity unitEntity)
            {
                SpeakBark(text, unitEntity.Gender);
            }
            else
            {
                SpeakBark(text);
            }
        }
        else
        {
            SpeakBark(text, lightweightUnitEntity.Gender);
        }
    }

    public static void SpeakBark(string text, Gender? gender = null)
    {
#if DEBUG
        Debug.LogFormat("SpeakBark as {0}", gender.HasValue ? gender : "Narrator");
#endif

        switch (gender)
        {
            case Gender.Male:
                Main.Speech?.SpeakAs(text, VoiceType.Male);
                break;
            case Gender.Female:
                Main.Speech?.SpeakAs(text, VoiceType.Female);
                break;
            default:
                Main.Speech?.SpeakAs(text, VoiceType.Narrator);
                break;
        }
    }
}

[HarmonyPatch(typeof(BarkPlayer), "Bark", typeof(Entity), typeof(string), typeof(float), typeof(string), typeof(BaseUnitEntity), typeof(bool))]
public class BarkPlayer_Bark_Patch
{
    [HarmonyPostfix]
    public static void Bark_Postfix(Entity entity, string text, float duration = -1f, string voiceOver = null, BaseUnitEntity interactUser = null, bool synced = true)
    {
        if (!BarkExtensions.PlayBark())
            return;

#if DEBUG
        Debug.Log($"{nameof(BarkPlayer)}_Bark_Postfix");
#endif

        BarkExtensions.DoBark(entity, text, voiceOver);
    }
}

[HarmonyPatch(typeof(BarkPlayer), "BarkExploration", typeof(Entity), typeof(string), typeof(float), typeof(string))]
[HarmonyPatch(typeof(BarkPlayer), "BarkExploration", typeof(Entity), typeof(string), typeof(string), typeof(float), typeof(string))]
public class BarkPlayer_BarkExploration_Patch
{
    [HarmonyPostfix]
    public static void BarkExploration_1_Postfix(Entity entity, string text, float duration = -1f, string voiceOver = null)
    {
        if (!BarkExtensions.PlayBark())
            return;

#if DEBUG
        Debug.Log($"{nameof(BarkPlayer)}_BarkExploration_1_Postfix");
#endif

        BarkExtensions.DoBark(entity, text, voiceOver);
    }

    [HarmonyPostfix]
    public static void BarkExploration_2_Postfix(Entity entity, string text, string encyclopediaLink, float duration = -1f, string voiceOver = null)
    {
        if (!BarkExtensions.PlayBark())
            return;

#if DEBUG
        Debug.Log($"{nameof(BarkPlayer)}_BarkExploration_2_Postfix");
#endif

        BarkExtensions.DoBark(entity, text, voiceOver);
    }
}
