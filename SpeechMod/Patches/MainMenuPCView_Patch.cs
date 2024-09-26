using HarmonyLib;
using Kingmaker.Code.UI.MVVM.View.MainMenu.PC;
using SpeechMod.Unity.Extensions;
#if DEBUG
using UnityEngine;
#endif

namespace SpeechMod.Patches;

[HarmonyPatch(typeof(MainMenuPCView), nameof(MainMenuPCView.BindViewImplementation))]
public class MainMenuPCView_Patch
{
    private const string MAIN_MENU_WELCOME_TEXT_PATH = "/MainMenuPCView(Clone)/UICanvas/WelcomeWindowPCView/Background/ScrollContainer/ServiceWindowStandardScrollView/Viewport/Content/text";

    public static void Postfix()
    {
        if (!Main.Enabled)
            return;

#if DEBUG
        Debug.Log($"{nameof(MainMenuPCView)}_BindViewImplementation_Postfix");
#endif

        Hooks.HookUpTextToSpeechOnTransformWithPath(MAIN_MENU_WELCOME_TEXT_PATH);
    }
}