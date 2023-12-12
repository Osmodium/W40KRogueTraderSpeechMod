using HarmonyLib;
using Kingmaker.Code.UI.MVVM.View.MainMenu.PC;
using SpeechMod.Unity;
using UnityEngine;

namespace SpeechMod.Patches;

[HarmonyPatch(typeof(MainMenuPCView), "BindViewImplementation")]
public class MainMenuPCView_Patch
{
    public static void Postfix()
    {
        if (!Main.Enabled)
            return;

#if DEBUG
        Debug.Log($"{nameof(MainMenuPCView)}_BindViewImplementation_Postfix");
#endif

        UIHelper.HookUpTextToSpeechOnTransformWithPath("ServiceWindowStandardScrollView/Viewport/Content/text");
    }
}