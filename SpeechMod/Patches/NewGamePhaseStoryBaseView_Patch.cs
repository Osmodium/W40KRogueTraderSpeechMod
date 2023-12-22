using HarmonyLib;
using Kingmaker.Code.UI.MVVM.View.NewGame.Base;
using SpeechMod.Unity;
using UnityEngine;

namespace SpeechMod.Patches;

[HarmonyPatch(typeof(NewGamePhaseStoryBaseView), "BindViewImplementation")]
public class NewGamePhaseStoryBaseView_Patch
{
    private const string NEWGAME_STORY_INTRODUCTION_PATH = "/MainMenuPCView(Clone)/UICanvas/NewGamePCView/Device/Background (1)/Background/ContentGroup/NewGameTabGameModePCView/PaperGroup/Paper/Content/Description/ServiceWindowStandartScrollVew/Viewport/Content/Text (TMP)";

    public static void Postfix()
    {
        if (!Main.Enabled)
            return;

#if DEBUG
        Debug.Log($"{nameof(NewGamePhaseStoryBaseView)}_BindViewImplementation_Postfix");
#endif

        UIHelper.HookUpTextToSpeechOnTransformWithPath(NEWGAME_STORY_INTRODUCTION_PATH);
    }
}