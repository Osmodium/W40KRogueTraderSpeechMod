using HarmonyLib;
using Kingmaker.Code.UI.MVVM.View.InfoWindow;
using Kingmaker.Code.UI.MVVM.VM.InfoWindow;
using Owlcat.Runtime.UI.Tooltips;
using SpeechMod.Unity.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using Owlcat.Runtime.UI.Controls.Button;
using TMPro;
using UnityEngine;

namespace SpeechMod.Patches;

[HarmonyPatch(typeof(InfoBaseView<InfoBaseVM>), "SetPart")]
public static class Tooltip_Patch
{
    public static void Postfix(IEnumerable<TooltipBaseBrickVM> bricks, RectTransform container)
    {
        if (!Main.Enabled)
            return;

        if (container == null)
            return;

        HookBricksByContainer(container);
    }

    private static void HookBricksByContainer(RectTransform container)
    {
#if DEBUG
        Debug.Log(container?.transform.GetGameObjectPath());
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

        return !(transform.name.Trim().Equals("bracket", StringComparison.InvariantCultureIgnoreCase)
            || transform.name.Trim().Equals("Acronim", StringComparison.InvariantCultureIgnoreCase)
            || transform.name.Trim().Equals("decortext", StringComparison.InvariantCultureIgnoreCase)
            || transform.parent?.GetComponent<OwlcatButton>() != null);
    }
}