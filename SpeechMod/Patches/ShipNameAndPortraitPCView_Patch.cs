using HarmonyLib;
using Kingmaker;
using Kingmaker.Code.UI.MVVM.View.ServiceWindows.CharacterInfo.Sections.NameAndPortrait;
using Kingmaker.GameModes;
using SpeechMod.Unity.Extensions;
#if DEBUG
using UnityEngine;
#endif

namespace SpeechMod.Patches;

[HarmonyPatch(typeof(ShipNameAndPortraitPCView), nameof(ShipNameAndPortraitPCView.BindViewImplementation))]
public static class ShipNameAndPortraitPCView_Patch
{
    private const string SPACE_INFORMATION_LABEL_PATH = "/SpacePCView(Clone)/SpaceStaticPartPCView/StaticCanvas/ServiceWindowsPCView/InventoryLeftCanvas/Background/ShipPart/InformationLabel";
    private const string SPACE_DESCRIPTION_PATH = "/SpacePCView(Clone)/SpaceStaticPartPCView/StaticCanvas/ServiceWindowsPCView/InventoryLeftCanvas/Background/ShipPart/StarShipDescription";
    private const string SURFACE_INFORMATION_LABEL_PATH = "/SurfacePCView(Clone)/SurfaceStaticPartPCView/StaticCanvas/ServiceWindowsPCView/InventoryLeftCanvas/Background/ShipPart/InformationLabel";
    private const string SURFACE_DESCRIPTION_PATH = "/SurfacePCView(Clone)/SurfaceStaticPartPCView/StaticCanvas/ServiceWindowsPCView/InventoryLeftCanvas/Background/ShipPart/StarShipDescription";

    public static void Postfix()
    {
        if (!Main.Enabled)
            return;

#if DEBUG
        Debug.Log($"{nameof(ShipNameAndPortraitPCView)}_BindViewImplementation_Postfix");
#endif

        if (Game.Instance!.IsModeActive(GameModeType.StarSystem))
        {
            Hooks.HookUpTextToSpeechOnTransformWithPath(SPACE_INFORMATION_LABEL_PATH);
            Hooks.HookUpTextToSpeechOnTransformWithPath(SPACE_DESCRIPTION_PATH);
            return;
        }

        Hooks.HookUpTextToSpeechOnTransformWithPath(SURFACE_INFORMATION_LABEL_PATH);
        Hooks.HookUpTextToSpeechOnTransformWithPath(SURFACE_DESCRIPTION_PATH);
    }
}