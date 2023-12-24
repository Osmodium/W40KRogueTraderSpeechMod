using HarmonyLib;
using Kingmaker.Code.UI.MVVM.View.MessageBox.PC;
using SpeechMod.Unity.Extensions;
#if DEBUG
using UnityEngine;
#endif

namespace SpeechMod.Patches;

[HarmonyPatch(typeof(MessageBoxPCView), "BindViewImplementation")]
public class MessageBoxPCView_Patch
{
    private const string MESSAGE_BOX_TEXT_PATH = "/CommonPCView(Clone)/CommonCanvas/MessageBoxPCView/CommonModalWindow/Panel/Content/Layout/Label_Message";
    private const string INPUT_BOX_TEXT_PATH = "/MainMenuPCView(Clone)/UICanvas/CharGenContextPCView/CharGenPCView/Content/PhaseDetailedViews/CharGenShipPhaseDetailedPCView/CharGenChangeNameMessageBoxPCView/CommonModalWindow/Panel/Content/Layout/Label_Message";

    public static void Postfix()
    {
        if (!Main.Enabled)
            return;

#if DEBUG
        Debug.Log($"{nameof(MessageBoxPCView)}_BindViewImplementation_Postfix");
#endif

        Hooks.HookUpTextToSpeechOnTransformWithPath(MESSAGE_BOX_TEXT_PATH);
        Hooks.HookUpTextToSpeechOnTransformWithPath(INPUT_BOX_TEXT_PATH);
    }
}