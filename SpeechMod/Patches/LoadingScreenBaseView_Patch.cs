using HarmonyLib;
using Kingmaker.Code.UI.MVVM.View.LoadingScreen;
using SpeechMod.Unity.Extensions;
#if DEBUG
using UnityEngine;
#endif

namespace SpeechMod.Patches;

[HarmonyPatch]
public static class LoadingScreenBaseView_Patch
{
    [HarmonyPatch(typeof(LoadingScreenBaseView), nameof(LoadingScreenBaseView.BindViewImplementation))]
    [HarmonyPostfix]
    public static void BindViewImplementation(LoadingScreenBaseView __instance)
    {
        if (!Main.Enabled)
            return;

#if DEBUG
        Debug.Log($"{nameof(LoadingScreenBaseView)}_BindViewImplementation_Postfix");
#endif

        __instance?.m_BottomDescriptionText.HookupTextToSpeech();
        __instance?.m_BottomTitleText.HookupTextToSpeech();
        __instance?.m_CharacterDescriptionText.HookupTextToSpeech();
        __instance?.m_CharacterNameText.HookupTextToSpeech();
        __instance?.m_LocationName.HookupTextToSpeech();
    }

    [HarmonyPatch(typeof(LoadingScreenBaseView), nameof(LoadingScreenBaseView.Show))]
    [HarmonyPostfix]
    public static void Show(LoadingScreenBaseView __instance)
    {
        if (!Main.Enabled)
            return;

#if DEBUG
        Debug.Log($"{nameof(LoadingScreenBaseView)}_BindViewImplementation_Postfix");
#endif

        if (Main.Settings?.AutoStopPlaybackOnLoading == false)
            return;

        Main.Speech?.Stop();
    }
}