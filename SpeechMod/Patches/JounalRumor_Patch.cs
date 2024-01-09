using HarmonyLib;
using Kingmaker.Code.UI.MVVM.View.ServiceWindows.Journal;
using SpeechMod.Unity.Extensions;
using UnityEngine.UI;
#if DEBUG
using UnityEngine;
#endif

namespace SpeechMod.Patches;

[HarmonyPatch]
public class JounalRumor_Patch
{
    private const string BLOCKING_IMAGE_PATH = "/SurfacePCView(Clone)/SurfaceStaticPartPCView/StaticCanvas/ServiceWindowsPCView/JournalView/Device/ContentGroup/Screen_view/ItemView/JournalRumourPCView/BodyGroup/ServiceWindowStandardScrollView";
    private const string BLOCKING_BACKGROUND_PATH = "/SurfacePCView(Clone)/SurfaceStaticPartPCView/StaticCanvas/ServiceWindowsPCView/JournalView/Device/ContentGroup/Screen_view/ItemView/JournalRumourPCView/HeaderGroup/TitleGroup/Background";

    [HarmonyPatch(typeof(JournalRumourPCView), nameof(JournalRumourPCView.BindViewImplementation))]
    [HarmonyPostfix]
    public static void Rumor(JournalRumourPCView __instance)
    {
        if (!Main.Enabled)
            return;

#if DEBUG
        Debug.Log($"{nameof(JournalRumourPCView)}_{nameof(JournalRumourPCView.BindViewImplementation)}_Postfix");
#endif

        UnblockImage(BLOCKING_IMAGE_PATH);
        UnblockImage(BLOCKING_BACKGROUND_PATH);

        __instance.m_TitleLabel.m_TextComponent.HookupTextToSpeech();
        __instance.m_CompletionLabel.HookupTextToSpeech();
        __instance.m_NoDataText.HookupTextToSpeech();
        __instance.m_DescriptionLabel.HookupTextToSpeech();
        __instance.m_StatusLabel.HookupTextToSpeech();
    }

    private static void UnblockImage(string path)
    {
        var blockingUi = UIHelper.TryFind(path);
        if (blockingUi == null)
            return;

        var image = blockingUi.GetComponent<Image>();
        if (image != null)
            image.raycastTarget = false;
    }
}