using HarmonyLib;
using Kingmaker.Code.UI.MVVM.View.ServiceWindows.CharacterInfo.Sections.BuffsAndConditions;
using Kingmaker.Code.UI.MVVM.View.ServiceWindows.CharacterInfo.Sections.FactionsReputation;
using Kingmaker.Code.UI.MVVM.View.ServiceWindows.CharacterInfo.Sections.Summary;
using SpeechMod.Unity.Extensions;
#if DEBUG
using UnityEngine;
#endif

namespace SpeechMod.Patches;

[HarmonyPatch]
public class CharInfoSummaryPCView_Patch
{
    [HarmonyPatch(typeof(CharInfoSummaryPCView), nameof(CharInfoSummaryPCView.BindViewImplementation))]
    [HarmonyPostfix]
    public static void HookLabels(CharInfoSummaryPCView __instance)
    {
        if (!Main.Enabled)
            return;

#if DEBUG
        Debug.Log($"{nameof(CharInfoSummaryPCView)}_{nameof(CharInfoSummaryPCView.BindViewImplementation)}_Postfix");
#endif

        __instance.m_ActionPoints.HookupTextToSpeech();
        __instance.m_ActionPointsLabel.HookupTextToSpeech();
        __instance.m_MovePoints.HookupTextToSpeech();
        __instance.m_MovePointsLabel.HookupTextToSpeech();
    }

    [HarmonyPatch(typeof(CharInfoStatusEffectsView), nameof(CharInfoStatusEffectsView.RefreshView))]
    [HarmonyPostfix]
    public static void HookStatusEffects(CharInfoStatusEffectsView __instance)
    {
        if (!Main.Enabled)
            return;

#if DEBUG
        Debug.Log($"{nameof(CharInfoStatusEffectsView)}_RefreshView_Postfix");
#endif

        __instance.m_StatusEffectsTitle.HookupTextToSpeech();
        __instance.m_NoStatusEffectsLabel.HookupTextToSpeech();
        //__instance.m_WidgetEntityView?.m_Description.HookupTextToSpeech();
        __instance.m_WidgetEntityView?.m_DisplayName.HookupTextToSpeech();
        //__instance.m_WidgetEntityView?.m_FactDescription.HookupTextToSpeech();
        //__instance.m_WidgetEntityView?.m_RankText.HookupTextToSpeech();
    }

    [HarmonyPatch(typeof(CharInfoProfitFactorItemPCView), nameof(CharInfoProfitFactorItemPCView.BindViewImplementation))]
    [HarmonyPostfix]
    public static void HookFator(CharInfoProfitFactorItemPCView __instance)
    {
        if (!Main.Enabled)
            return;

#if DEBUG
        Debug.Log($"{nameof(CharInfoProfitFactorItemPCView)}_BindViewImplementation_Postfix");
#endif

        __instance.m_Title.HookupTextToSpeech();
    }
}