using HarmonyLib;
using Kingmaker.Code.UI.MVVM.View.MessageBox.PC;
using SpeechMod.Unity;
using UnityEngine;

namespace SpeechMod.Patches;

[HarmonyPatch(typeof(MessageBoxPCView), "BindViewImplementation")]
public class MessageModal_Patch
{
    public static void Postfix()
    {
        if (!Main.Enabled)
            return;

#if DEBUG
        Debug.Log($"{nameof(MessageBoxPCView)}_BindViewImplementation_Postfix");
#endif

        UIHelper.HookUpTextToSpeechOnTransformWithPath("/CommonPCView(Clone)/CommonCanvas/MessageBoxPCView/CommonModalWindow/Panel/Content/Layout/Label_Message");
    }
}