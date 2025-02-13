using HarmonyLib;
using Kingmaker;
using Kingmaker.Code.UI.MVVM.View.Common.PC;
using Kingmaker.Code.UI.MVVM.VM.WarningNotification;
using Kingmaker.Localization;
using SpeechMod.Configuration.Settings;
using System;
#if DEBUG
using UnityEngine;
#endif

namespace SpeechMod.KeyBinds;

public class ToggleBarks() : ModHotkeySettingEntry(KEY, TITLE, TOOLTIP, DEFAULT_VALUE)
{
    private const string KEY = "barks.toggle";
    private const string TITLE = "Toggle Barks";
    private const string TOOLTIP = "Toggles playback of Barks";
    private const string DEFAULT_VALUE = "%B;;All;false";
    private const string BIND_NAME = $"{Constants.SETTINGS_PREFIX}.newcontrols.ui.{KEY}";

    public override SettingStatus TryEnable() => TryEnableAndPatch(typeof(Patches));

    [HarmonyPatch]
    private static class Patches
    {
        private static string _barksTurnedOnText = "SpeechMod: Barks turned ON!";
        private static string _barksTurnedOffText = "SpeechMod: Barks turned OFF!";
        private static IDisposable _disposableBinding;

        [HarmonyPatch(typeof(CommonPCView), nameof(CommonPCView.BindViewImplementation))]
        [HarmonyPostfix]
        private static void AddToggleBarksHotkey(CommonPCView __instance)
        {

#if DEBUG
            Debug.Log($"{nameof(CommonPCView)}_{nameof(CommonPCView.BindViewImplementation)}_Postfix : {BIND_NAME}");
#endif

            if (LocalizationManager.Instance.CurrentPack?.TryGetText("osmodium.speechmod.feature.barks.toggle.on.notification", out var onText, false) == true)
            {
                _barksTurnedOnText = onText;
            }

            if (LocalizationManager.Instance.CurrentPack?.TryGetText("osmodium.speechmod.feature.barks.toggle.off.notification", out var offText, false) == true)
            {
                _barksTurnedOffText = offText;
            }

            if (Game.Instance.Keyboard.m_Bindings.Exists(binding => binding.Name.Equals(BIND_NAME)))
            {
#if DEBUG
                Debug.Log($"Binding {BIND_NAME} already exists! Disposing of binding...");
#endif
                _disposableBinding.Dispose();
            }

            _disposableBinding = Game.Instance!.Keyboard!.Bind(BIND_NAME, () => ToggleBarks(__instance));
            __instance?.AddDisposable(_disposableBinding);
        }

        private static void ToggleBarks(CommonPCView instance)
        {
            Main.Settings.PlaybackBarks = !Main.Settings.PlaybackBarks;
#if DEBUG
            Debug.Log($"Barks: {Main.Settings.PlaybackBarks}");
#endif
            if (instance == null || instance.m_WarningsTextView == null)
                return;

            var text = Main.Settings.PlaybackBarks ? _barksTurnedOnText : _barksTurnedOffText;
#if DEBUG
            Debug.Log(text);
#endif

            if (Main.Settings!.ShowNotificationOnPlaybackStop)
            {
                instance.m_WarningsTextView?.Show(text, WarningNotificationFormat.Common);
            }
        }
    }
}
