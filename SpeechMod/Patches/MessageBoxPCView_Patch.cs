using HarmonyLib;
using Kingmaker.Code.UI.MVVM.View.MessageBox.PC;
using SpeechMod.Unity.Extensions;
#if DEBUG
using UnityEngine;
#endif

namespace SpeechMod.Patches;

[HarmonyPatch(typeof(MessageBoxPCView), nameof(MessageBoxPCView.BindViewImplementation))]
public class MessageBoxPCView_Patch
{
    private const string INPUT_BOX_TEXT_PATH = "/MainMenuPCView(Clone)/UICanvas/CharGenContextPCView/CharGenPCView/Content/PhaseDetailedViews/CharGenShipPhaseDetailedPCView/CharGenChangeNameMessageBoxPCView/CommonModalWindow/Panel/Content/Layout/Label_Message";

    public static void Postfix(MessageBoxPCView __instance)
    {
        if (!Main.Enabled)
            return;

#if DEBUG
        Debug.Log($"{nameof(MessageBoxPCView)}_BindViewImplementation_Postfix");
#endif

        __instance.m_MessageText.HookupTextToSpeech();
        if (__instance.m_InputField.IsActive())
        {
            Hooks.HookUpTextToSpeechOnTransformWithPath(INPUT_BOX_TEXT_PATH);
        }
    }
}