using HarmonyLib;
using Kingmaker.UI.MVVM.View.SaveLoad.Base;
using SpeechMod.Unity.Extensions;
#if DEBUG
using UnityEngine;
#endif

namespace SpeechMod.Patches;

[HarmonyPatch]
public class SaveSlotBaseView_Patch
{
    [HarmonyPatch(typeof(SaveLoadBaseView), nameof(SaveLoadBaseView.BindViewImplementation))]
    [HarmonyPostfix]
    public static void HookupLoadText(SaveLoadBaseView __instance)
    {
        if (!Main.Enabled)
            return;

#if DEBUG
        Debug.Log($"{nameof(SaveLoadBaseView)}_BindViewImplementation_Postfix");
#endif

        __instance.m_DetailedSaveSlotView.m_NameLabel.HookupTextToSpeech();
        __instance.m_DetailedSaveSlotView.m_LocationLabel.HookupTextToSpeech();
        __instance.m_DetailedSaveSlotView.m_TimeInGameLabel.HookupTextToSpeech();
        __instance.m_DetailedSaveSlotView.m_DlcRequiredLabel.HookupTextToSpeech();
        __instance.m_DetailedSaveSlotView.m_DlcRequiredDescription.HookupTextToSpeech();
    }
}