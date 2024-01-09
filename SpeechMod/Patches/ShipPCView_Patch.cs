using HarmonyLib;
using Kingmaker.Code.UI.MVVM.View.Slots;
using Kingmaker.Code.UI.MVVM.View.Space.PC;
using Kingmaker.UI.MVVM.View.ShipCustomization;
using SpeechMod.Unity;
using SpeechMod.Unity.Extensions;
using UnityEngine;

namespace SpeechMod.Patches;

[HarmonyPatch]
public class ShipPCView_Patch
{
    [HarmonyPatch(typeof(ShipPCView), nameof(ShipPCView.BindViewImplementation))]
    [HarmonyPostfix]
    public static void AddHoks(ShipPCView __instance)
    {
        if (!Main.Enabled)
            return;

#if DEBUG
        Debug.Log($"{nameof(ShipPCView)}_{nameof(ShipPCView.BindViewImplementation)}_Postfix");
#endif

        __instance.m_ShipName.HookupTextToSpeech();

        var textMeshProUguis = new[] { __instance.m_ExperienceText, __instance.m_Experience, __instance.m_LevelText, __instance.m_Level };
        __instance.m_ExperienceText.TryAddButton("SpeechMod_ExpButton", new Vector2(0f, 0f), new Vector3(0.8f, 0.8f, 1f), textMeshProUguis);
    }

    [HarmonyPatch(typeof(ShipComponentSlotBaseView<ItemSlotBaseView>), nameof(ShipComponentSlotBaseView<ItemSlotBaseView>.BindViewImplementation))]
    [HarmonyPostfix]
    public static void AddLabelHooks(ShipComponentSlotBaseView<ItemSlotBaseView> __instance)
    {
        if (!Main.Enabled)
            return;

#if DEBUG
        Debug.Log($"{nameof(ShipComponentSlotBaseView<ItemSlotBaseView>)}_{nameof(ShipComponentSlotBaseView<ItemSlotBaseView>.BindViewImplementation)}_Postfix");
#endif

        __instance.m_ShortDescription.HookupTextToSpeech();
        __instance.m_Count.HookupTextToSpeech();
    }
}