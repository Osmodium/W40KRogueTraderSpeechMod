using HarmonyLib;
using Kingmaker;
using Kingmaker.Blueprints.Base;
using Kingmaker.Code.UI.MVVM.View.Dialog.Dialog;
using Owlcat.Runtime.UI.Controls.Button;
using SpeechMod.Unity;
using SpeechMod.Unity.Extensions;
using SpeechMod.Voice;
using System.Collections;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using Image = UnityEngine.UI.Image;

namespace SpeechMod.Patches;

[HarmonyPatch]
public class DialogAnswerBaseView_Patch
{
    private const string DIALOG_ANSWER_BUTTON_NAME = "SpeechMod_DialogAnswerButton";

    [HarmonyPatch(typeof(DialogAnswerBaseView), nameof(DialogAnswerBaseView.BindViewImplementation))]
    [HarmonyPostfix]
    public static void AddHooks(DialogAnswerBaseView __instance)
    {
        if (!Main.Enabled)
            return;

#if DEBUG
        Debug.Log($"{nameof(DialogAnswerBaseView)}_{nameof(DialogAnswerBaseView.BindViewImplementation)}_Postfix");
#endif

        TryAddDialogButton(__instance.Text, __instance, new Vector2(-20f, -2f));
    }

    private static void TryAddDialogButton(TextMeshProUGUI textMeshPro, DialogAnswerBaseView instance, Vector2? anchoredPosition = null)
    {
        var transform = textMeshPro?.transform;

#if DEBUG
        Debug.Log($"Adding/Removing dialog answer button on {textMeshPro?.name}...");
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
        playButtonGameObject = ButtonFactory.TryCreatePlayButton(transform, () =>
        {
            if (textMeshPro == null)
                return;
            var text = textMeshPro.text;

            if (Main.Settings?.LogVoicedLines == true)
                Debug.Log(text);

            if (Main.Settings?.SayDialogAnswerNumber == true)
                text = new Regex("<alpha[^>]+>([^>]+)<alpha[^>]+><indent[^>]+>([^<>]*)</indent>").Replace(text, "$1 <silence msec=\"500\"/> $2");
            else
                text = new Regex("<alpha[^>]+>[^>]+<alpha[^>]+><indent[^>]+>([^<>]*)</indent>").Replace(text, "$1");

            text = text.PrepareText();

            Main.Speech?.SpeakAs(text, Game.Instance?.Player?.MainCharacterEntity?.Gender == Gender.Female ? VoiceType.Female : VoiceType.Male);
        });

        if (playButtonGameObject == null || playButtonGameObject.transform == null)
            return;

        if (Main.Settings?.DialogAnswerColorOnHover == true)
        {
            SetDialogAnswerColorHover(playButtonGameObject, instance);
        }

        playButtonGameObject.name = DIALOG_ANSWER_BUTTON_NAME;
        playButtonGameObject.transform.localRotation = Quaternion.Euler(0, 0, 270);
        playButtonGameObject.transform.localScale = new Vector3(0.8f, 0.8f, 1f);
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

                if (hover)
                {
                    image0.color = hover ? color0On : color0Off;
                    image1.color = hover ? color1On : color1Off;
                    image2.color = hover ? color2On : color2Off;
                }
                else
                {
                    button.StartCoroutine(ResetColor(image0, color0Off, image1, color1Off, image2, color2Off));
                }
            }
        );
    }

    private static IEnumerator ResetColor(Image image0, Color color0, Image image1, Color color1, Image image2, Color color2)
    {
        yield return new WaitForEndOfFrame();
        image0.color = color0;
        image1.color = color1;
        image2.color = color2;
    }
}