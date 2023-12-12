using HarmonyLib;
using Kingmaker.UI.MVVM.View.Tutorial.PC;
using SpeechMod.Unity;
using UnityEngine;
namespace SpeechMod.Patches;

[HarmonyPatch(typeof(TutorialHintWindowPCView), "SetContent")]
public class TutorialWindowView_Patch_Small
{
    public static void Postfix()
    {
        if (!Main.Enabled)
            return;

#if DEBUG
        Debug.Log($"{nameof(TutorialHintWindowPCView)}_SetContent_Postfix");
#endif

        UIHelper.HookUpTextToSpeechOnTransformWithPath("/CommonPCView(Clone)/CommonCanvas/TutorialPCView/SmallWindowpPCView/Window/Content/Header/Title");
        UIHelper.HookUpTextToSpeechOnTransformWithPath("/CommonPCView(Clone)/CommonCanvas/TutorialPCView/SmallWindowpPCView/Window/Content/Body/ScrollView/Viewport/Content/TutorialText");
    }
}

[HarmonyPatch(typeof(TutorialModalWindowPCView), "BindViewImplementation")]
//[HarmonyPatch(typeof(TutorialModalWindowPCView), "OnNext")]
public class TutorialWindowView_Patch_Big
{
    public static void Postfix()
    {
        if (!Main.Enabled)
            return;

#if DEBUG
        Debug.Log($"{nameof(TutorialHintWindowPCView)}_SetContent_Postfix");
#endif

        UIHelper.HookUpTextToSpeechOnTransformWithPath("/CommonPCView(Clone)/CommonCanvas/TutorialPCView/BigWindowPCView/Window/Content/Header/TitleGroup/Title");
        UIHelper.HookUpTextToSpeechOnTransformWithPath("/CommonPCView(Clone)/CommonCanvas/TutorialPCView/BigWindowPCView/Window/Content/Body/Bottom/ScrollView/Viewport/Content/TutorialText");
    }
}