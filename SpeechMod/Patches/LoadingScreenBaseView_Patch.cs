//using HarmonyLib;
//using Kingmaker.Code.UI.MVVM.View.LoadingScreen;
//using SpeechMod.Unity.Extensions;
//using UnityEngine;

//namespace SpeechMod.Patches;

//[HarmonyPatch(typeof(LoadingScreenBaseView), "Show")]
//public static class LoadingScreenBaseView_Patch
//{
//    private const string LOADING_SCREEN_TITLE_TEXT_PATH = "/CommonPCView(Clone)/LoadingCanvas/LoadingScreenPCViewVariant/Window/Monitor/TitlePlace/Box/TitleText";
//    private const string LOADING_SCREEN_BOTTOM_TITLE_TEXT_PATH = "/CommonPCView(Clone)/LoadingCanvas/LoadingScreenPCViewVariant/Window/Monitor/MainContainer/BottomTextContainerPlace/BottomDescriptionBackground/BottomTitleText";
//    private const string LOADING_SCREEN_BOTTOM_DESCRIPTION_TEXT_PATH = "/CommonPCView(Clone)/LoadingCanvas/LoadingScreenPCViewVariant/Window/Monitor/MainContainer/BottomTextContainerPlace/BottomDescriptionBackground/BottomDescriptionText";

//    public static void Postfix()
//    {
//        if (!Main.Enabled)
//            return;

//#if DEBUG
//        Debug.Log($"{nameof(LoadingScreenBaseView)}_Show_Postfix");
//#endif

//        Hooks.HookUpTextToSpeechOnTransformWithPath(LOADING_SCREEN_TITLE_TEXT_PATH);
//        Hooks.HookUpTextToSpeechOnTransformWithPath(LOADING_SCREEN_BOTTOM_TITLE_TEXT_PATH);
//        Hooks.HookUpTextToSpeechOnTransformWithPath(LOADING_SCREEN_BOTTOM_DESCRIPTION_TEXT_PATH);
//    }
//}