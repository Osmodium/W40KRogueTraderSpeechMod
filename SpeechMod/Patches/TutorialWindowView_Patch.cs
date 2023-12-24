using HarmonyLib;
using Kingmaker.UI.MVVM.View.Tutorial.PC;
using SpeechMod.Unity.Extensions;
#if DEBUG
using UnityEngine;
#endif

namespace SpeechMod.Patches;

[HarmonyPatch(typeof(TutorialHintWindowPCView), "SetContent")]
public class TutorialWindowView_Patch_Small
{
    private const string TUTORIAL_SMALL_TITLE_PATH = "/CommonPCView(Clone)/CommonCanvas/TutorialPCView/SmallWindowpPCView/Window/Content/Header/Title";
    private const string TUTORIAL_SMALL_TEXT_PATH = "/CommonPCView(Clone)/CommonCanvas/TutorialPCView/SmallWindowpPCView/Window/Content/Body/ScrollView/Viewport/Content/TutorialText";

    public static void Postfix()
    {
        if (!Main.Enabled)
            return;

#if DEBUG
        Debug.Log($"{nameof(TutorialHintWindowPCView)}_SetContent_Postfix");
#endif

        Hooks.HookUpTextToSpeechOnTransformWithPath(TUTORIAL_SMALL_TITLE_PATH);
        Hooks.HookUpTextToSpeechOnTransformWithPath(TUTORIAL_SMALL_TEXT_PATH);
    }
}

[HarmonyPatch(typeof(TutorialModalWindowPCView), "BindViewImplementation")]
//[HarmonyPatch(typeof(TutorialModalWindowPCView), "OnNext")]
public class TutorialWindowView_Patch_Big
{
    private const string TUTORIAL_BIG_TITLE_PATH = "/CommonPCView(Clone)/CommonCanvas/TutorialPCView/BigWindowPCView/Window/Content/Header/TitleGroup/Title";
    private const string TUTORIAL_BIG_TEXT_PATH = "/CommonPCView(Clone)/CommonCanvas/TutorialPCView/BigWindowPCView/Window/Content/Body/Bottom/ScrollView/Viewport/Content/TutorialText";

    public static void Postfix()
    {
        if (!Main.Enabled)
            return;

#if DEBUG
        Debug.Log($"{nameof(TutorialModalWindowPCView)}_BindViewImplementation_Postfix");
#endif

        Hooks.HookUpTextToSpeechOnTransformWithPath(TUTORIAL_BIG_TITLE_PATH);
        Hooks.HookUpTextToSpeechOnTransformWithPath(TUTORIAL_BIG_TEXT_PATH);
    }
}