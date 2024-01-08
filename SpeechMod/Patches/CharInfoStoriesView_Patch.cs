using HarmonyLib;
using Kingmaker.Code.UI.MVVM.View.ServiceWindows.CharacterInfo.Sections.Stories;
using SpeechMod.Unity.Extensions;
using UnityEngine;

namespace SpeechMod.Patches;

[HarmonyPatch(typeof(CharInfoStoriesView), nameof(CharInfoStoriesView.BindViewImplementation))]
public static class CharInfoStoriesView_Patch
{
    public static void Postfix(CharInfoStoriesView __instance)
    {
        if (!Main.Enabled)
            return;

#if DEBUG
        Debug.Log($"{nameof(CharInfoStoriesView)}_{nameof(CharInfoStoriesView.BindViewImplementation)}_Postfix");
#endif

        __instance.m_BiographyTitle.HookupTextToSpeech();
        __instance.m_BiographyText.HookupTextToSpeech();
        __instance.m_EmptyBiographyText.HookupTextToSpeech();
    }
}