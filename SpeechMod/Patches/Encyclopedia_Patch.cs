using HarmonyLib;
using Kingmaker.Code.UI.MVVM.View.ServiceWindows.Encyclopedia.Base;
using Kingmaker.Code.UI.MVVM.View.ServiceWindows.Encyclopedia.Blocks;
using SpeechMod.Unity;
using SpeechMod.Unity.Extensions;
using UnityEngine;

namespace SpeechMod.Patches;

[HarmonyPatch]
public static class Encyclopedia_Patch
{
    private const string PAGE_VIEW_ADDITION_BUTTON_NAME = "SpeechMod_AdditionButton_EncyclopediaPageBaseView";
    private const string PAGE_VIEW_GLOSSARY_BUTTON_NAME = "SpeechMod_GlossaryButton_EncyclopediaPageBaseView";
    private const string TEXT_VIEW_GLOSSARY_BUTTON_NAME = "SpeechMod_GlossaryButton_EncyclopediaPageBlockTextPCView";
    private const string BLOCK_VIEW_GLOSSARY_BUTTON_NAME = "SpeechMod_GlossaryButton_EncyclopediaPageBlockGlossaryEntryPCView";

    [HarmonyPatch(typeof(EncyclopediaPageBaseView), nameof(EncyclopediaPageBaseView.BindViewImplementation))]
    [HarmonyPostfix]
    public static void AddTitleHook(EncyclopediaPageBaseView __instance)
    {
        if (!Main.Enabled)
            return;

#if DEBUG
        Debug.Log($"{nameof(EncyclopediaPageBaseView)}_BindViewImplementation_Postfix");
#endif

        __instance.m_Title.HookupTextToSpeech();
        __instance.m_PageAdditionText.TryAddButtonToTextMeshPro(PAGE_VIEW_ADDITION_BUTTON_NAME, new Vector2(24f, -4f), new Vector3(0.8f, 0.8f, 1f));
        __instance.m_GlossaryEntryBlockPrefab?.m_Description.TryAddButtonToTextMeshPro(PAGE_VIEW_GLOSSARY_BUTTON_NAME, new Vector2(0f, -4f), new Vector3(0.8f, 0.8f, 1f));
    }

    [HarmonyPatch(typeof(EncyclopediaPageBlockTextPCView), nameof(EncyclopediaPageBlockTextPCView.BindViewImplementation))]
    [HarmonyPostfix]
    public static void AddPageBlockTextHook(EncyclopediaPageBlockTextPCView __instance)
    {
        if (!Main.Enabled)
            return;
#if DEBUG
        Debug.Log($"{nameof(EncyclopediaPageBlockTextPCView)}_BindViewImplementation_Postfix");
#endif
        __instance.m_Text.TryAddButtonToTextMeshPro(TEXT_VIEW_GLOSSARY_BUTTON_NAME, new Vector2(-1f, -4f), new Vector3(0.8f, 0.8f, 1f));
    }

    [HarmonyPatch(typeof(EncyclopediaPageBlockGlossaryEntryPCView), nameof(EncyclopediaPageBlockGlossaryEntryPCView.BindViewImplementation))]
    [HarmonyPostfix]
    public static void AddPageBlockGlossaryHooks(EncyclopediaPageBlockGlossaryEntryPCView __instance)
    {
        if (!Main.Enabled)
            return;
#if DEBUG
        Debug.Log($"{nameof(EncyclopediaPageBlockGlossaryEntryPCView)}_BindViewImplementation_Postfix");
#endif

        __instance.m_Title.HookupTextToSpeech();
        __instance.m_Description.TryAddButtonToTextMeshPro(BLOCK_VIEW_GLOSSARY_BUTTON_NAME, new Vector2(-2f, -4f), new Vector3(0.8f, 0.8f, 1f));
    }
}
