﻿using HarmonyLib;
using Kingmaker.Code.UI.MVVM.VM.Bark;
using UnityEngine;

namespace SpeechMod.Patches;

[HarmonyPatch(typeof(BaseBarkVM), nameof(BaseBarkVM.ShowBark))]
public class BaseBarkVM_Patch
{
    public static void Postfix(string text)
    {
        if (!Main.Enabled)
            return;

#if DEBUG
        Debug.Log($"{nameof(BaseBarkVM)}_ShowBark_Postfix");
#endif

        if (string.IsNullOrWhiteSpace(text))
            return;

        Main.Speech?.Speak(text);
    }
}