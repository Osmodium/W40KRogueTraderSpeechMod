//using System;
//using HarmonyLib;
//using Kingmaker.Code.UI.MVVM.View.ServiceWindows.CharacterInfo;
//using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.CharacterInfo;
//using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.PagesMenu;
//using SpeechMod.Unity;
//using UnityEngine;

//namespace SpeechMod.Patches;

//[HarmonyPatch(typeof(CharacterInfoVM), "OnPageSelected")]
//public static class CharacterInfoVM_Patch
//{
//    public static void Postfix(CharInfoPageType pageType)
//    {
//        if (!Main.Enabled)
//            return;

//#if DEBUG
//        Debug.Log($"{nameof(CharacterInfoVM)}_OnPageSelected_Postfix @ {pageType}");
//#endif

//        if (pageType is not (CharInfoPageType.Summary or CharInfoPageType.Features))
//            return;

//        var stories = UIHelper.GetUICanvas()?.GetComponentsInChildren<CharInfoPagesMenuEntityVM>();

//        for (int i = 0; i < stories.Length; ++i)
//        {
//            try
//            {
//                var story = stories[i];
//                var textBox = story?.transform?.Find("StoryFull/StoryContent/TextBox");
//                if (textBox == null)
//                {
//                    Debug.LogWarning($"{nameof(CharacterInfoVM)}_OnPageSelected_Postfix - TextBox not found for pagetype {pageType}!");
//                    continue;
//                }

//                textBox.HookupTextToSpeechOnTransform();
//            }
//            catch(Exception ex)
//            {
//                Debug.LogWarning($"Failed hooking story text on story '{stories[i].name}'. {ex.Message}");
//            }
//        }
//    }
//}