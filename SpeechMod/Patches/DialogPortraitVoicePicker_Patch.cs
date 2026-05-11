using HarmonyLib;
using Kingmaker;
using Kingmaker.Code.UI.MVVM.View.Dialog.Dialog;
using Kingmaker.Code.UI.MVVM.View.Dialog.SurfaceDialog;
using SpeechMod.Unity;
using SpeechMod.Unity.Extensions;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.EventSystems;

namespace SpeechMod.Patches;

/// <summary>
/// Adds right-click event on the speaker portrait in the dialog window
/// to open the per-character voice picker panel.
/// </summary>
[HarmonyPatch]
public static class DialogPortraitVoicePicker_Patch
{
    private const string VOICE_PICKER_TRIGGER_NAME = "SpeechMod_VoicePickerTrigger";

    // Speaker portrait (left side)
    private const string SURFACE_PORTRAIT_PATH = "/SurfacePCView(Clone)/SurfaceStaticPartPCView/StaticCanvas/SurfaceDialogPCView(Clone)/LeftSide/DeviceBack/SpeakerPortrait";
    private const string SPACE_PORTRAIT_PATH = "/SpacePCView(Clone)/SpaceStaticPartPCView/StaticCanvas/SurfaceDialogPCView(Clone)/LeftSide/DeviceBack/SpeakerPortrait";

    // Answer portrait (right side - protagonist)
    private const string SURFACE_ANSWER_PORTRAIT_PATH = "/SurfacePCView(Clone)/SurfaceStaticPartPCView/StaticCanvas/SurfaceDialogPCView(Clone)/RightSide/DeviceBack/AnswerPortrait";
    private const string SPACE_ANSWER_PORTRAIT_PATH = "/SpacePCView(Clone)/SpaceStaticPartPCView/StaticCanvas/SurfaceDialogPCView(Clone)/RightSide/DeviceBack/AnswerPortrait";

    [HarmonyPatch(typeof(SurfaceDialogBaseView<DialogAnswerPCView>), nameof(SurfaceDialogBaseView<>.Initialize))]
    [HarmonyPostfix]
    public static void HookPortraitRightClick()
    {
        if (!Main.Enabled)
            return;

#if DEBUG
        Debug.Log($"[SpeechMod] DialogPortraitVoicePicker: Attempting to hook portrait right-click");
#endif

        // Hook left speaker portrait
        HookPortrait(SURFACE_PORTRAIT_PATH, SPACE_PORTRAIT_PATH, false);

        // Hook right answer portrait (protagonist)
        HookPortrait(SURFACE_ANSWER_PORTRAIT_PATH, SPACE_ANSWER_PORTRAIT_PATH, true);
    }

    private static void HookPortrait(string surfacePath, string spacePath, bool isAnswerPortrait)
    {
        var portrait = UIHelper.TryFind(surfacePath);
        if (portrait == null)
            portrait = UIHelper.TryFind(spacePath);

        if (portrait == null)
        {
#if DEBUG
            Debug.LogWarning($"[SpeechMod] DialogPortraitVoicePicker: Could not find {(isAnswerPortrait ? "answer" : "speaker")} portrait!");
#endif
            return;
        }

        // Check if already hooked
        if (portrait.TryFind(VOICE_PICKER_TRIGGER_NAME) != null)
            return;

        // Add an invisible trigger object
        var triggerGo = new GameObject(VOICE_PICKER_TRIGGER_NAME);
        triggerGo.transform.SetParent(portrait, false);
        var triggerRt = triggerGo.AddComponent<RectTransform>();
        triggerRt.anchorMin = Vector2.zero;
        triggerRt.anchorMax = Vector2.one;
        triggerRt.sizeDelta = Vector2.zero;
        triggerRt.offsetMin = Vector2.zero;
        triggerRt.offsetMax = Vector2.zero;

        // Need an image for raycast target (invisible)
        var img = triggerGo.AddComponent<UnityEngine.UI.Image>();
        img.color = new Color(0, 0, 0, 0); // Fully transparent
        img.raycastTarget = true;

        // Add pointer click observable
        var trigger = triggerGo.AddComponent<ObservablePointerClickTrigger>();
        trigger.OnPointerClickAsObservable().Subscribe(data =>
        {
            if (data.button == PointerEventData.InputButton.Right)
            {
                if (isAnswerPortrait)
                    OpenVoicePickerForProtagonist();
                else
                    OpenVoicePickerForCurrentSpeaker();
            }
        });

#if DEBUG
        Debug.Log($"[SpeechMod] DialogPortraitVoicePicker: Successfully hooked {(isAnswerPortrait ? "answer" : "speaker")} portrait right-click");
#endif
    }

    private static void OpenVoicePickerForProtagonist()
    {
        var protagonist = Game.Instance?.Player?.MainCharacterEntity;
        if (protagonist == null)
        {
#if DEBUG
            Debug.Log("[SpeechMod] DialogPortraitVoicePicker: No protagonist found");
#endif
            return;
        }

        var characterId = protagonist.Blueprint?.AssetGuid;
        if (string.IsNullOrWhiteSpace(characterId))
            characterId = protagonist.CharacterName?.GetHashCode().ToString("X8");

        var characterName = protagonist.CharacterName ?? "Protagonist";

#if DEBUG
        Debug.Log($"[SpeechMod] Opening voice picker for protagonist: {characterName} (ID: {characterId})");
#endif

        CharacterVoicePickerPanel.Toggle(characterId, characterName);
    }

    private static void OpenVoicePickerForCurrentSpeaker()
    {
        var dialogController = Game.Instance?.DialogController;
        if (dialogController == null)
            return;

        var speaker = dialogController.CurrentSpeaker;
        if (speaker == null)
        {
#if DEBUG
            Debug.Log("[SpeechMod] DialogPortraitVoicePicker: No current speaker");
#endif
            return;
        }

        // Get character identifier - use blueprint ID for persistence
        var characterId = speaker.Blueprint?.AssetGuid;
        if (string.IsNullOrWhiteSpace(characterId))
        {
            // Fallback: use character name hash
            characterId = speaker.CharacterName?.GetHashCode().ToString("X8");
        }

        var characterName = speaker.CharacterName ?? "Unknown";

#if DEBUG
        Debug.Log($"[SpeechMod] Opening voice picker for: {characterName} (ID: {characterId})");
#endif

        CharacterVoicePickerPanel.Toggle(characterId, characterName);
    }

    /// <summary>
    /// Close the picker when dialog ends.
    /// </summary>
    [HarmonyPatch(typeof(SurfaceDialogBaseView<DialogAnswerPCView>), nameof(SurfaceDialogBaseView<>.DestroyViewImplementation))]
    [HarmonyPrefix]
    public static void CloseOnDialogEnd()
    {
        CharacterVoicePickerPanel.Close();
    }
}



