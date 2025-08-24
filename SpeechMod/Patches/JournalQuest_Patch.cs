using HarmonyLib;
using Kingmaker.Code.UI.MVVM.View.ServiceWindows.Journal;
using Kingmaker.Code.UI.MVVM.View.ServiceWindows.Journal.Base;
using Kingmaker.UI.TMPExtention.ScrambledTextMeshPro;
using SpeechMod.Unity;
using SpeechMod.Unity.Extensions;
using System.Linq;
using UnityEngine;

namespace SpeechMod.Patches;

[HarmonyPatch]
public static class JournalQuest_Patch
{
    private const string BUTTON_NAME = "SpeechMod_JQButton";
    private const string DESTINATION_BUTTON_NAME = "SpeechMod_JQDestinationButton";
    private const string SURFACE_BLOCKING_IMAGE_PATH = "/SurfacePCView(Clone)/SurfaceStaticPartPCView/StaticCanvas/ServiceWindowsPCView/JournalView/Device/ContentGroup/Screen_view/ItemView/JournalQuestPCView/BodyGroup/ContentGroup/ObjectivesGroup/ServiceWindowStandardScrollView";
    private const string SPACE_BLOCKING_IMAGE_PATH = "/SpacePCView(Clone)/SpaceStaticPartPCView/StaticCanvas/ServiceWindowsPCView/JournalView/Device/ContentGroup/Screen_view/ItemView/JournalQuestPCView/BodyGroup/ContentGroup/ObjectivesGroup/ServiceWindowStandardScrollView";

    [HarmonyPatch(typeof(JournalQuestPCView), nameof(JournalQuestPCView.BindViewImplementation))]
    [HarmonyPostfix]
    public static void AddDescription(JournalQuestPCView __instance)
    {
        if (!Main.Enabled)
            return;

#if DEBUG
        Debug.Log($"{nameof(JournalQuestPCView)}_{nameof(JournalQuestPCView.BindViewImplementation)}_Postfix");
#endif

        UIHelper.FixBlockingUi(SURFACE_BLOCKING_IMAGE_PATH);
        UIHelper.FixBlockingUi(SPACE_BLOCKING_IMAGE_PATH);

        if (__instance == null)
        {
            Debug.LogWarning($"[{nameof(JournalQuestPCView)}_{nameof(JournalQuestPCView.BindViewImplementation)}_Postfix] JournalQuestPCView instance is null!");
            return;
        }

        var headerScrambledTmp = __instance.GetComponentsInChildren<ScrambledTMP>()?.FirstOrDefault();
        if (headerScrambledTmp != null)
        {
            headerScrambledTmp.m_TextComponent.HookupTextToSpeech();
        }

        __instance.m_DescriptionLabel.HookupTextToSpeech();
        __instance.m_PlaceLabel.HookupTextToSpeech();
        __instance.m_CompletionLabel.HookupTextToSpeech();
        __instance.m_ServiceMessageLabel.HookupTextToSpeech();
        __instance.m_StatusLabel.HookupTextToSpeech();
        __instance.m_TitleLabel.m_TextComponent.HookupTextToSpeech();
    }

    [HarmonyPatch(typeof(JournalQuestObjectiveBaseView), nameof(JournalQuestObjectiveBaseView.BindViewImplementation))]
    [HarmonyPostfix]
    public static void AddButtonsToParts(JournalQuestObjectiveBaseView __instance)
    {
        if (!Main.Enabled)
            return;

#if DEBUG
        Debug.Log($"{nameof(JournalQuestObjectiveBaseView)}_{nameof(JournalQuestObjectiveBaseView.BindViewImplementation)}_Postfix");
#endif

        __instance.m_Title.TryAddButtonToTextMeshPro(BUTTON_NAME, new Vector2(18f, -11f), new Vector3(0.8f, 0.8f, 1f));
        __instance.m_Destination.TryAddButtonToTextMeshPro(DESTINATION_BUTTON_NAME, new Vector2(-40f, -4f), new Vector3(0.8f, 0.8f, 1f));
        __instance.m_Description.HookupTextToSpeech();
        __instance.m_EtudeCounter.HookupTextToSpeech();
        __instance.m_ObjectiveNummer.HookupTextToSpeech();
    }

    [HarmonyPatch(typeof(JournalQuestObjectiveAddendumBaseView), nameof(JournalQuestObjectiveAddendumBaseView.BindViewImplementation))]
    [HarmonyPostfix]
    public static void AddAddendums(JournalQuestObjectiveAddendumBaseView __instance)
    {
        if (!Main.Enabled)
            return;

#if DEBUG
        Debug.Log($"{nameof(JournalQuestObjectiveAddendumBaseView)}_{nameof(JournalQuestObjectiveAddendumBaseView.BindViewImplementation)}_Postfix");
#endif

        __instance.m_Description.HookupTextToSpeech();
        __instance.m_EtudeCounter.HookupTextToSpeech();
    }
}