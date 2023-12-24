using HarmonyLib;
using Kingmaker.Code.UI.MVVM.View.ServiceWindows.CharacterInfo.Sections.NameAndPortrait;
using SpeechMod.Unity.Extensions;
using UnityEngine;

namespace SpeechMod.Patches;

[HarmonyPatch(typeof(ShipNameAndPortraitPCView), "BindViewImplementation")]
public static class ShipNameAndPortraitPCView_Patch
{
    private const string INFORMATION_LABEL_PATH = "/SurfacePCView(Clone)/SurfaceStaticPartPCView/StaticCanvas/ServiceWindowsPCView/InventoryLeftCanvas/Background/ShipPart/InformationLabel";
    private const string DESCRIPTION_PATH = "/SurfacePCView(Clone)/SurfaceStaticPartPCView/StaticCanvas/ServiceWindowsPCView/InventoryLeftCanvas/Background/ShipPart/StarShipDescription";

    public static void Postfix()
    {
        if (!Main.Enabled)
            return;

#if DEBUG
        Debug.Log($"{nameof(ShipNameAndPortraitPCView)}_BindViewImplementation_Postfix");
#endif

        Hooks.HookUpTextToSpeechOnTransformWithPath(INFORMATION_LABEL_PATH);
        Hooks.HookUpTextToSpeechOnTransformWithPath(DESCRIPTION_PATH);
    }
}