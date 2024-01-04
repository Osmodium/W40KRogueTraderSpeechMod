using HarmonyLib;
using Kingmaker;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows;
using ModConfiguration.Settings;

namespace SpeechMod.Keybinds;

public class PlaybackStop : ModHotkeySettingEntry
{
    private const string _key = "playback.stop";
    private const string _title = "Stop playback";
    private const string _tooltip = "Stops playback of SpeechMod TTS.";
    private const string _defaultValue = "%S;;WorldFullscreenUI;false";
    private const string BIND_NAME = $"{Constants.SETTINGS_PREFIX}.newcontrols.ui.{_key}";

    public PlaybackStop() : base(_key, _title, _tooltip, _defaultValue)
    { }

    public override SettingStatus TryEnable() => TryEnableAndPatch(typeof(Patches));

    [HarmonyPatch]
    private static class Patches
    {
        // TODO Bind in game to be used everywhere?
        /// <summary>
        /// Binds key after other service window keys are bound
        /// </summary>
        [HarmonyPatch(typeof(ServiceWindowsVM), nameof(ServiceWindowsVM.BindKeys))]
        [HarmonyPostfix]
        private static void Add(ServiceWindowsVM __instance)
        {
            __instance.AddDisposable(Game.Instance.Keyboard.Bind(BIND_NAME, StopPlayback));
        }

        private static void StopPlayback()
        {
            Main.Speech?.Stop();
        }
    }
}