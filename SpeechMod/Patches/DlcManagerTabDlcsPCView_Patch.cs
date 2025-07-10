using HarmonyLib;
using Kingmaker.Code.UI.MVVM.View.DlcManager.Dlcs.PC;
using SpeechMod.Unity.Extensions;
using UnityEngine;

namespace SpeechMod.Patches;

[HarmonyPatch]
public static class DlcManagerTabDlcsPCView_Patch
{
    [HarmonyPostfix]
    [HarmonyPatch(typeof(DlcManagerTabDlcsPCView), nameof(DlcManagerTabDlcsPCView.BindViewImplementation))]
    public static void AddDlcTextHook(DlcManagerTabDlcsPCView __instance)
    {
        if (!Main.Enabled)
            return;

#if DEBUG
        Debug.Log($"{nameof(DlcManagerTabDlcsPCView)}_BindViewImplementation_Postfix");
#endif

        __instance.m_DlcDescription.HookupTextToSpeech();
        __instance.m_YouDontHaveThisDlc.HookupTextToSpeech();
    }
}