using HarmonyLib;
using Kingmaker.UI.MVVM.View.ShipCustomization.ShipPosts;
using SpeechMod.Unity.Extensions;
using UnityEngine;

namespace SpeechMod.Patches;
[HarmonyPatch(typeof(PostsBaseView), nameof(PostsBaseView.BindViewImplementation))]
public class ShipPostsView_Patch
{
    public static void Postfix(PostsBaseView __instance)
    {
        if (!Main.Enabled)
            return;

#if DEBUG
        Debug.Log($"{nameof(PostsBaseView)}_{nameof(PostsBaseView.BindViewImplementation)}_Postfix");
#endif

        __instance.m_PostDescriptionSkill.HookupTextToSpeech();
        __instance.m_PostDescriptionSkillHeader.HookupTextToSpeech();
        __instance.m_PostDescription.HookupTextToSpeech();
        __instance.m_PostHeader.HookupTextToSpeech();
        __instance.m_PostHeaderName.HookupTextToSpeech();
        __instance.m_PostOfficerHeader.HookupTextToSpeech();
        __instance.m_PostOfficerHeaderName.HookupTextToSpeech();
        __instance.m_PostOfficerHeaderSkillName.HookupTextToSpeech();
        __instance.m_PostOfficerHeaderSkillValue.HookupTextToSpeech();
    }
}