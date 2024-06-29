using HarmonyLib;
using Kingmaker.Code.UI.MVVM.View.InfoWindow;
using Kingmaker.Code.UI.MVVM.View.Tooltip.Bricks;
using Kingmaker.Code.UI.MVVM.VM.InfoWindow;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.Tooltips;
using SpeechMod.Unity.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

namespace SpeechMod.Patches;

[HarmonyPatch]
public static class Tooltip_Patch
{
    [HarmonyPatch(typeof(InfoBaseView<InfoBaseVM>), "SetPart")]
    [HarmonyPostfix]
    public static void SetPart(IEnumerable<TooltipBaseBrickVM> bricks, RectTransform container)
    {
        if (!Main.Enabled)
            return;

#if DEBUG
        Debug.Log($"{nameof(InfoBaseView<InfoBaseVM>)}_SetPart_Postfix");
#endif

        if (container == null)
            return;

        HookBricksByContainer(container);
    }

    [HarmonyPatch(typeof(TooltipBrickIconStatValueView), "BindViewImplementation")]
    [HarmonyPostfix]
    public static void BrickIconStatValue(TooltipBrickIconStatValueView __instance)
    {
        if (!Main.Enabled)
            return;

#if DEBUG
        Debug.Log($"{nameof(TooltipBrickIconStatValueView)}_BindViewImplementation_Postfix");
#endif

        __instance.m_AddValue.HookupTextToSpeech();
        __instance.m_IconText.HookupTextToSpeech();
        __instance.m_Label.HookupTextToSpeech();
        __instance.m_Value.HookupTextToSpeech();
    }

    private static void HookBricksByContainer(RectTransform container)
    {
#if DEBUG
        //Debug.Log(container?.transform.GetGameObjectPath());
#endif
        var allTexts = container?.transform?.parent?.GetComponentsInChildren<TextMeshProUGUI>(true)?
            .Where(t => ApplyFilter(t?.transform))
            .ToArray();

        allTexts.HookupTextToSpeech();

        // TODO Find a way to force hooking up the TTS when on specific bricks that are structured differently
        //foreach(var text in allTexts)
        //{
        //    if (text.transform.GetComponent<TooltipBrickFeatureHeaderView>() != null || text.transform.GetComponentInParents<TooltipBrickFeatureHeaderView>() != null)
        //        text.transform.HookupTextToSpeechOnTransform(true);
        //    else
        //        text.transform.HookupTextToSpeechOnTransform();
        //}
    }

    private static bool ApplyFilter(Transform transform)
    {
        if (transform == null)
            return false;

        if (string.IsNullOrWhiteSpace(transform.name))
            return false;

        return !(transform.GetGameObjectPath().Contains("ComparativeTooltipPCView")
            || transform.name.Trim().Equals("bracket", StringComparison.InvariantCultureIgnoreCase)
            || transform.name.Trim().Equals("Acronim", StringComparison.InvariantCultureIgnoreCase)
            || transform.name.Trim().Equals("decortext", StringComparison.InvariantCultureIgnoreCase)
            || transform.name.Trim().Equals("Text (TMP) (1)", StringComparison.InvariantCultureIgnoreCase)
            || transform.name.Trim().Equals("Text (TMP) (2)", StringComparison.InvariantCultureIgnoreCase)
            || transform.GetGameObjectPath()!.Contains("/LeftBlock (1)/Empty/Text (TMP)")
            || transform.parent?.GetComponent<OwlcatButton>() != null);
    }
}