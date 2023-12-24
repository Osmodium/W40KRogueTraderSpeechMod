using HarmonyLib;
using Kingmaker;
using Kingmaker.Code.UI.MVVM.View.Surface.PC;
using SpeechMod.Unity;
using SpeechMod.Unity.Extensions;
using UnityEngine;

namespace SpeechMod.Patches;

[HarmonyPatch(typeof(SurfaceStaticPartPCView), "Initialize")]
public static class SurfaceStaticPartPCView_Patch
{
    private const string SCROLL_VIEW_PATH = "/SurfacePCView(Clone)/SurfaceStaticPartPCView/StaticCanvas/SurfaceDialogPCView/LeftSide/CueAndHistoryPlace/ScrollView";

    public static void Postfix()
    {
        if (!Main.Enabled)
            return;

#if DEBUG
        var sceneName = Game.Instance!.CurrentlyLoadedArea!.ActiveUIScene!.SceneName;
        Debug.Log($"{nameof(SurfaceStaticPartPCView)}_Initialize_Postfix @ {sceneName}");
#endif

        AddDialogSpeechButton();
    }

    private static void AddDialogSpeechButton()
    {

#if DEBUG
        Debug.Log("Adding speech button to dialog ui.");
#endif

        var parent = UIHelper.TryFind(SCROLL_VIEW_PATH);

        if (parent == null)
        {
            Debug.LogWarning("Parent not found!");
            return;
        }

        var buttonGameObject = ButtonFactory.CreatePlayButton(parent, () =>
        {
            Main.Speech.SpeakDialog(Game.Instance?.DialogController?.CurrentCue?.DisplayText);
        });

        buttonGameObject.name = "SpeechButton";
        buttonGameObject.RectAlignTopLeft(new Vector2(40, 10));
        buttonGameObject.transform.localRotation = Quaternion.Euler(0, 0, 270);

        buttonGameObject.SetActive(true);
    }
}