using HarmonyLib;
using Kingmaker;
using Kingmaker.Code.UI.MVVM.VM.Dialog.Dialog;
using Kingmaker.Localization;
#if DEBUG
using UnityEngine;
#endif

namespace SpeechMod.Patches;

[HarmonyPatch(typeof(DialogVM), nameof(DialogVM.HandleOnCueShow))]
public static class Dialog_Patch
{
    public static void Postfix()
    {
        if (!Main.Enabled)
            return;

#if DEBUG
        Debug.Log($"{nameof(DialogVM)}_HandleOnCueShow_Postfix");
#endif

        if (!Main.Settings!.AutoPlay)
        {
#if DEBUG
            Debug.Log($"{nameof(DialogVM)}: AutoPlay is disabled!");
#endif
            return;
        }

        var key = Game.Instance?.DialogController?.CurrentCue?.Text?.Key;
        if (string.IsNullOrWhiteSpace(key))
            key = Game.Instance?.DialogController?.CurrentCue?.Text?.Shared?.String?.Key;

        if (string.IsNullOrWhiteSpace(key))
            return;

        var isVoiced = string.IsNullOrWhiteSpace(LocalizationManager.Instance!.SoundPack?.GetText(key));

        if (!Main.Settings.AutoPlayIgnoreVoice && isVoiced)
        {
            Main.Speech?.Stop();
            return;
        }

        Main.Speech?.SpeakDialog(Game.Instance?.DialogController?.CurrentCue?.DisplayText, 0.5f);
    }
}