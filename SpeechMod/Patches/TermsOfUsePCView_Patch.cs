using HarmonyLib;
using Kingmaker.Code.UI.MVVM.View.TermOfUse;
using SpeechMod.Unity;
using UnityEngine;

namespace SpeechMod.Patches;

[HarmonyPatch(typeof(TermsOfUsePCView), "BindViewImplementation")]
public class TermsOfUsePCView_Patch
{
    public static void Postfix()
    {
        if (!Main.Enabled)
            return;

#if DEBUG
        Debug.Log($"{nameof(TermsOfUsePCView)}_BindViewImplementation_Postfix");
#endif

        UIHelper.HookUpTextToSpeechOnTransformWithPath("/MainMenuPCView(Clone)/UICanvas/TermsOfUsePCView/Body/Device/ContentGroup/TabsGroup/Title");
        UIHelper.HookUpTextToSpeechOnTransformWithPath("/MainMenuPCView(Clone)/UICanvas/TermsOfUsePCView/Body/Device/ContentGroup/Screen_view/ItemView/MainContent/Scroll View/Viewport/Content/Licence");
    }
}