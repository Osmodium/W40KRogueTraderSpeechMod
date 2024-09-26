using HarmonyLib;
using Kingmaker.Code.UI.MVVM.View.NewGame.Base;
using SpeechMod.Unity.Extensions;
using UnityEngine.UI;
#if DEBUG
using UnityEngine;
#endif

namespace SpeechMod.Patches;

[HarmonyPatch]
public class NewGamePhaseStoryBaseView_Patch
{
    private const string NEWGAME_STORY_FRAME_PATH = "/MainMenuPCView(Clone)/UICanvas/NewGamePCView/Device/Background (1)/Background/ContentGroup/NewGameTabGameModePCView/Screen_view/ItemView (1)/Description/ServiceWindowStandartScrollVew/Viewport/Content/Frame";
    [HarmonyPatch(typeof(NewGamePhaseStoryBaseView), nameof(NewGamePhaseStoryBaseView.BindViewImplementation))]
    [HarmonyPostfix]
    public static void Postfix(NewGamePhaseStoryBaseView __instance)
    {
        if (!Main.Enabled)
            return;

#if DEBUG
        Debug.Log($"{nameof(NewGamePhaseStoryBaseView)}_BindViewImplementation_Postfix");
#endif

        UIHelper.TryFind(NEWGAME_STORY_FRAME_PATH)?
            .GetComponent<Image>()?
            .SetRaycastTarget(false);

        __instance.m_StoryDescription.HookupTextToSpeech();
    }
}