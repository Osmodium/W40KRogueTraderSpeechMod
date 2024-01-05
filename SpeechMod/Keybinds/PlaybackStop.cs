using HarmonyLib;
using Kingmaker;
using Kingmaker.Code.UI.MVVM.View.Common.PC;
using ModConfiguration.Settings;

namespace SpeechMod.Keybinds;

public class PlaybackStop : ModHotkeySettingEntry
{
    private const string _key = "playback.stop";
    private const string _title = "Stop playback";
    private const string _tooltip = "Stops playback of SpeechMod TTS.";
    private const string _defaultValue = "%S;;All;false";
    private const string BIND_NAME = $"{Constants.SETTINGS_PREFIX}.newcontrols.ui.{_key}";

    public PlaybackStop() : base(_key, _title, _tooltip, _defaultValue)
    { }

    public override SettingStatus TryEnable() => TryEnableAndPatch(typeof(Patches));

    [HarmonyPatch]
    private static class Patches
    {
        /// <summary>
        /// Binds key after CommonPCView is initialized.
        /// </summary>
        [HarmonyPatch(typeof(CommonPCView), nameof(CommonPCView.BindViewImplementation))]
        [HarmonyPostfix]
        private static void Add(CommonPCView __instance)
        {
            __instance.AddDisposable(Game.Instance.Keyboard.Bind(BIND_NAME, StopPlayback));
        }

        private static void StopPlayback()
        {
            // TODO: Add notification?
            Main.Speech?.Stop();
        }
    }
}