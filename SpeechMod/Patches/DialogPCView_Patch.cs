using HarmonyLib;
using Kingmaker;
using Kingmaker.Code.UI.MVVM.View.Dialog.Dialog;
using Kingmaker.Code.UI.MVVM.View.Dialog.SurfaceDialog;
using Kingmaker.GameModes;
using SpeechMod.Unity;
using SpeechMod.Unity.Extensions;
using UnityEngine;

namespace SpeechMod.Patches;

[HarmonyPatch]
public static class DialogPCView_Patch
{
    private const string SPEECHMOD_DIALOGBUTTON_NAME = "SpeechMod_DialogButton";
    private const string SURFACE_SCROLL_VIEW_PATH = "/SurfacePCView(Clone)/SurfaceStaticPartPCView/StaticCanvas/SurfaceDialogPCView(Clone)/LeftSide/CueAndHistoryPlace/ScrollView";
    private const string SPACE_SCROLL_VIEW_PATH = "/SpacePCView(Clone)/SpaceStaticPartPCView/StaticCanvas/SurfaceDialogPCView(Clone)/LeftSide/CueAndHistoryPlace/ScrollView";

    [HarmonyPatch(typeof(SurfaceDialogBaseView<DialogAnswerPCView>), "Initialize")]
    [HarmonyPostfix]
    public static void AddDialogButton()
    {
        if (!Main.Enabled)
            return;

#if DEBUG
        var sceneName = Game.Instance!.CurrentlyLoadedArea!.ActiveUIScene!.SceneName;
        Debug.Log($"{nameof(SurfaceDialogBaseView<DialogAnswerPCView>)}_Initialize_Postfix @ {sceneName}");
#endif

        AddDialogButtonByPath(Game.Instance!.IsModeActive(GameModeType.StarSystem) ? SPACE_SCROLL_VIEW_PATH : SURFACE_SCROLL_VIEW_PATH);
    }

    private static void AddDialogButtonByPath(string path)
    {

#if DEBUG
        Debug.Log($"Adding speech button to dialog ui on '{path}'");
#endif

        var parent = UIHelper.TryFind(path);

        if (parent == null)
        {
            Debug.LogWarning("Parent not found!");
            return;
        }


        if (parent.TryFind(SPEECHMOD_DIALOGBUTTON_NAME) != null)
        {
            Debug.LogWarning("Button already exists!");
            return;
        }

        var buttonGameObject = ButtonFactory.TryCreatePlayButton(parent, () =>
        {
            Main.Speech?.SpeakDialog(Game.Instance?.DialogController?.CurrentCue?.DisplayText);
        });

        if (buttonGameObject == null)
        {
            return;
        }

        buttonGameObject.name = SPEECHMOD_DIALOGBUTTON_NAME;
        buttonGameObject.RectAlignTopLeft(new Vector2(40, 10));

        buttonGameObject.SetActive(true);
    }
}