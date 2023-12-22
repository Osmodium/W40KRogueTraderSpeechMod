using HarmonyLib;
using Kingmaker.Code.UI.MVVM.View.InfoWindow;
using Kingmaker.Code.UI.MVVM.VM.InfoWindow;
using Owlcat.Runtime.UI.Tooltips;
using SpeechMod.Unity;
using System;
using System.Collections.Generic;
using System.Linq;
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
    }

    private static bool ApplyFilter(Transform transform)
    {
        if (transform == null)
            return false;

        if (string.IsNullOrWhiteSpace(transform.name))
            return false;

        return !transform.name.Trim().Equals("bracket", StringComparison.InvariantCultureIgnoreCase);
    }
}