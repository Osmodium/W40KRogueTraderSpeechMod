using System.Linq;
using HarmonyLib;
using Kingmaker;
using Kingmaker.Blueprints.Base;
using Kingmaker.Code.UI.MVVM.View.Dialog.Dialog;
using Owlcat.Runtime.UI.Controls.Button;
using SpeechMod.Unity;
using SpeechMod.Unity.Extensions;
using SpeechMod.Voice;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SpeechMod.Patches;

[HarmonyPatch]
public class DialogAnswerBaseView_Patch
{
    private const string DIALOG_ANSWER_BUTTON_NAME = "SpeechMod_DialogButton";

    [HarmonyPatch(typeof(DialogAnswerBaseView), nameof(DialogAnswerBaseView.BindViewImplementation))]
    [HarmonyPostfix]
    public static void AddHooks(DialogAnswerBaseView __instance)
    {
        if (!Main.Enabled)
            return;

#if DEBUG
        Debug.Log($"{nameof(DialogAnswerBaseView)}_{nameof(DialogAnswerBaseView.BindViewImplementation)}_Postfix");
#endif

        TryAddDialogButton(__instance.Text, __instance, new Vector2(0f, 0f));
    }

    private static void TryAddDialogButton(TextMeshProUGUI textMeshPro, DialogAnswerBaseView instance, Vector2? anchoredPosition = null)
    {
        var transform = textMeshPro?.transform;

#if DEBUG
        Debug.Log($"Adding a dialog answer button to {textMeshPro?.name}...");
#endif

        var playButtonGameObject = transform?.Find(DIALOG_ANSWER_BUTTON_NAME)?.gameObject;

        // 1. We don't want the button to be there.
        if (!Main.Settings.ShowPlaybackOfDialogAnswers)
        {
            // 1a. Destroy the button if it exists
            if (playButtonGameObject != null)
                Object.Destroy(playButtonGameObject.gameObject);
            return;
        }

        // 2. We want the button and it exists.
        if (playButtonGameObject != null)
            return;

        // 3. We want the button but it doesn't exist.
        playButtonGameObject = ButtonFactory.CreatePlayButton(transform, () =>
        {
            Main.Speech?.SpeakAs(textMeshPro?.text, Game.Instance?.DialogController?.ActingUnit?.Gender == Gender.Female ? VoiceType.Female : VoiceType.Male);
        });

        if (playButtonGameObject == null || playButtonGameObject.transform == null)
            return;

        if (Main.Settings?.DialogAnswerColorOnHover == true)
        {
            SetDialogAnswerColorHover(playButtonGameObject, instance);
        }

        playButtonGameObject.name = DIALOG_ANSWER_BUTTON_NAME;
        playButtonGameObject.transform.localRotation = Quaternion.Euler(0, 0, 270);
        playButtonGameObject.RectAlignTopLeft(anchoredPosition);
        playButtonGameObject.SetActive(true);
    }

    private static void SetDialogAnswerColorHover(GameObject playButtonGameObject, DialogAnswerBaseView instance)
    {
        var focus = instance?.transform.TryFind("Focus");
        if (focus == null)
            return;

        var image0 = focus.Find("Image")?.GetComponent<Image>();
        var image1 = focus.Find("Image (1)")?.GetComponent<Image>();
        var image2 = focus.Find("Image (2)")?.GetComponent<Image>();

        if (image0 == null || image1 == null || image2 == null)
            return;

        var color0Off = image0.color;
        var color1Off = image1.color;
        var color2Off = image2.color;

        var button = playButtonGameObject!.GetComponent<OwlcatMultiButton>();
        if (button == null)
            return;

        button.OnHover?.RemoveAllListeners();
        button.OnHover?.AddListener(
            hover =>
            {
                var color0On = new Color(Main.Settings!.DialogAnswerHoverColorR, Main.Settings!.DialogAnswerHoverColorG, Main.Settings!.DialogAnswerHoverColorB, 1f);
                var color1On = new Color(Main.Settings!.DialogAnswerHoverColorR, Main.Settings!.DialogAnswerHoverColorG, Main.Settings!.DialogAnswerHoverColorB, 0.2f);
                var color2On = new Color(Main.Settings!.DialogAnswerHoverColorR, Main.Settings!.DialogAnswerHoverColorG, Main.Settings!.DialogAnswerHoverColorB, 0.1f);

                image0.color = hover ? color0On : color0Off;
                image1.color = hover ? color1On : color1Off;
                image2.color = hover ? color2On : color2Off;
            }
        );
    }
}