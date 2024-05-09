using HarmonyLib;
using Kingmaker;
using Kingmaker.Code.UI.MVVM.View.Common.PC;
using Kingmaker.Code.UI.MVVM.VM.WarningNotification;
using Kingmaker.Localization;
using SpeechMod.Configuration.Settings;
#if DEBUG
using UnityEngine;
#endif

namespace SpeechMod.Keybinds;

public class ToggleBarks : ModHotkeySettingEntry
{
    private const string _key = "barks.toggle";
    private const string _title = "Toggle Barks";
    private const string _tooltip = "Toggles playback of Barks";
    private const string _defaultValue = "%B;;All;false";
    private const string BIND_NAME = $"{Constants.SETTINGS_PREFIX}.newcontrols.ui.{_key}";

    public ToggleBarks() : base(_key, _title, _tooltip, _defaultValue)
    { }

    public override SettingStatus TryEnable() => TryEnableAndPatch(typeof(Patches));

    [HarmonyPatch]
    private static class Patches
    {
        [HarmonyPatch(typeof(CommonPCView), nameof(CommonPCView.BindViewImplementation))]
        [HarmonyPostfix]
        private static void Add(CommonPCView __instance)
        {
#if DEBUG
        Debug.Log($"{nameof(CommonPCView)}_{nameof(CommonPCView.BindViewImplementation)}_Postfix");
#endif
            __instance?.AddDisposable(Game.Instance!.Keyboard!.Bind(BIND_NAME, delegate { StopPlayback(__instance); }));
        }

        private static void StopPlayback(CommonPCView instance)
        {
            Main.Settings.PlaybackBarks = !Main.Settings.PlaybackBarks;

            if (instance != null && instance.m_WarningsTextView != null)
            {
                var text = "";
                if (Main.Settings.PlaybackBarks)
                {
                    if (!LocalizationManager.Instance!.CurrentPack!.TryGetText("osmodium.speechmod.feature.barks.toggle.on.notification", out text, false))
                    {
                        text = "SpeechMod: Barks turned ON!";
                    }
                }
                else
                {
                    if (!LocalizationManager.Instance!.CurrentPack!.TryGetText("osmodium.speechmod.feature.barks.toggle.off.notification", out text, false))
                    {
                        text = "SpeechMod: Barks turned OFF!";
                    }
                }

                if (Main.Settings!.ShowNotificationOnPlaybackStop)
                {
                    instance.m_WarningsTextView?.Show(text, WarningNotificationFormat.Common);
                }
            }
        }
    }
}