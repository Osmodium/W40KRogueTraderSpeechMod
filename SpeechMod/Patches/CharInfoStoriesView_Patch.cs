using HarmonyLib;
using Kingmaker.Code.UI.MVVM.View.ServiceWindows.CharacterInfo.Sections.Alignment.AlignmentHistory;
using Kingmaker.Code.UI.MVVM.View.ServiceWindows.CharacterInfo.Sections.Stories;
using SpeechMod.Unity.Extensions;
using UnityEngine;

namespace SpeechMod.Patches;

[HarmonyPatch]
public static class CharInfoStoriesView_Patch
{
    [HarmonyPatch(typeof(CharInfoStoriesView), nameof(CharInfoStoriesView.BindViewImplementation))]
    [HarmonyPostfix]
    public static void AddBiography(CharInfoStoriesView __instance)
    {
        if (!Main.Enabled)
            return;

#if DEBUG
        Debug.Log($"{nameof(CharInfoStoriesView)}_{nameof(CharInfoStoriesView.BindViewImplementation)}_Postfix");
#endif

        __instance.m_BiographyTitle.HookupTextToSpeech();
        __instance.m_BiographyText.HookupTextToSpeech();
        __instance.m_EmptyBiographyText.HookupTextToSpeech();
    }

    [HarmonyPatch(typeof(CharInfoSoulMarkShiftRecordPCView), nameof(CharInfoSoulMarkShiftRecordPCView.BindViewImplementation))]
    [HarmonyPostfix]
    public static void AddSoulMarks(CharInfoSoulMarkShiftRecordPCView __instance)
    {
        if (!Main.Enabled)
            return;

        __instance.m_Description.HookupTextToSpeech();
    }

    [HarmonyPatch(typeof(CharInfoChoicesMadeView), nameof(CharInfoChoicesMadeView.Initialize))]
    [HarmonyPostfix]
    public static void AddHistoryLabel(CharInfoChoicesMadeView __instance)
    {
        if (!Main.Enabled)
            return;

        __instance.m_Label.HookupTextToSpeech();
    }
}