using HarmonyLib;
using Kingmaker;
using Kingmaker.Code.UI.MVVM.View.Space.PC;
using Kingmaker.Code.UI.MVVM.View.Surface;
using Kingmaker.Code.UI.MVVM.View.Surface.PC;
using SpeechMod.Unity;
using SpeechMod.Unity.Extensions;
using UnityEngine;

namespace SpeechMod.Patches;

[HarmonyPatch]
public static class SurfaceStaticPartPCView_Patch
{
    private const string SURFACE_SCROLL_VIEW_PATH = "/SurfacePCView(Clone)/SurfaceStaticPartPCView/StaticCanvas/SurfaceDialogPCView/LeftSide/CueAndHistoryPlace/ScrollView";
    private const string SPACE_SCROLL_VIEW_PATH = "/SpacePCView(Clone)/SpaceStaticPartPCView/StaticCanvas/SurfaceDialogPCView/LeftSide/CueAndHistoryPlace/ScrollView";

    [HarmonyPatch(typeof(SurfaceBaseView), "Initialize")]
    [HarmonyPostfix]
    public static void InstantiateArrowButtonPrefab()
    {
        if (!Main.Enabled)
            return;

#if DEBUG
        Debug.Log($"{nameof(SurfaceBaseView)}_Initialize_Postfix");
#endif

        ButtonFactory.Instantiate();
    }

    [HarmonyPatch(typeof(SurfaceStaticPartPCView), "Initialize")]
    [HarmonyPostfix]
    public static void AddSurfaceDialogButton()
    {
        if (!Main.Enabled)
            return;

#if DEBUG
        var sceneName = Game.Instance!.CurrentlyLoadedArea!.ActiveUIScene!.SceneName;
        Debug.Log($"{nameof(SurfaceStaticPartPCView)}_Initialize_Postfix @ {sceneName}");
#endif

        AddDialogSpeechButton(SURFACE_SCROLL_VIEW_PATH);
    }

    [HarmonyPatch(typeof(SpaceStaticPartPCView), "Initialize")]
    [HarmonyPostfix]
    public static void AddSpaceDialogButton()
    {
        if (!Main.Enabled)
            return;

#if DEBUG
        var sceneName = Game.Instance!.CurrentlyLoadedArea!.ActiveUIScene!.SceneName;
        Debug.Log($"{nameof(SpaceStaticPartPCView)}_Initialize_Postfix @ {sceneName}");
#endif

        AddDialogSpeechButton(SPACE_SCROLL_VIEW_PATH);
    }

    private static void AddDialogSpeechButton(string path)
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

        var buttonGameObject = ButtonFactory.CreatePlayButton(parent, () =>
        {
            Main.Speech.SpeakDialog(Game.Instance?.DialogController?.CurrentCue?.DisplayText);
        });

        buttonGameObject.name = "SpeechMod_DialogButton";
        buttonGameObject.RectAlignTopLeft(new Vector2(40, 10));
        buttonGameObject.transform.localRotation = Quaternion.Euler(0, 0, 270);

        buttonGameObject.SetActive(true);
    }
}