using HarmonyLib;
using Kingmaker.Code.UI.MVVM.View.InfoWindow;
using Kingmaker.Code.UI.MVVM.View.SaveLoad.Base;
using Kingmaker.Code.UI.MVVM.VM.InfoWindow;
using SpeechMod.Unity.Extensions;
using UnityEngine;

namespace SpeechMod.Patches;

[HarmonyPatch]
public class SaveSlotBaseView_Patch
{
    [HarmonyPatch(typeof(SaveSlotBaseView), nameof(SaveSlotBaseView.BindViewImplementation))]
    [HarmonyPostfix]
    public static void HookupLoadText(SaveSlotBaseView __instance)
    {
        if (!Main.Enabled)
            return;

#if DEBUG
        Debug.Log($"{nameof(InfoBaseView<InfoBaseVM>)}_SetPart_Postfix");
#endif

        __instance.m_DlcRequiredDescription.HookupTextToSpeech();
        __instance.m_DlcRequiredLabel.HookupTextToSpeech();
        __instance.m_LocationLabel.HookupTextToSpeech();
        __instance.m_NameLabel.HookupTextToSpeech();
        __instance.m_TimeInGameLabel.HookupTextToSpeech();
    }
}