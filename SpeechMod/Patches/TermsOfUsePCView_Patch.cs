using HarmonyLib;
using Kingmaker.Code.UI.MVVM.View.TermOfUse;
using SpeechMod.Unity;
using UnityEngine;

namespace SpeechMod.Patches;

[HarmonyPatch(typeof(TermsOfUsePCView), "BindViewImplementation")]
public class TermsOfUsePCView_Patch
{
    private const string TERMS_OF_USE_TITLE_PATH = "/MainMenuPCView(Clone)/UICanvas/TermsOfUsePCView/Body/Device/ContentGroup/TabsGroup/Title";
    private const string TERMS_OF_USE_LICENCE_PATH = "/MainMenuPCView(Clone)/UICanvas/TermsOfUsePCView/Body/Device/ContentGroup/Screen_view/ItemView/MainContent/Scroll View/Viewport/Content/Licence";

    public static void Postfix()
    {
        if (!Main.Enabled)
            return;

#if DEBUG
        Debug.Log($"{nameof(TermsOfUsePCView)}_BindViewImplementation_Postfix");
#endif

        UIHelper.HookUpTextToSpeechOnTransformWithPath(TERMS_OF_USE_TITLE_PATH);
        UIHelper.HookUpTextToSpeechOnTransformWithPath(TERMS_OF_USE_LICENCE_PATH);
    }
}