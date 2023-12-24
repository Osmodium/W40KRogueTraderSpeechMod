﻿using System;
using Owlcat.Runtime.UniRx;
using TMPro;
using UniRx;
using UnityEngine;

namespace SpeechMod.Unity.Extensions;

public static class Hooks
{
    private static Color m_HoverColor = Color.blue;

    public static void UpdateHoverColor()
    {
        m_HoverColor = new Color(Main.Settings.HoverColorR, Main.Settings.HoverColorG, Main.Settings.HoverColorB, Main.Settings.HoverColorA);
    }

    public static void HookUpTextToSpeechOnTransformWithPath(string path, bool force = false)
    {
        var transform = UIHelper.TryFind(path);
        if (transform == null)
        {
            Debug.LogWarning($"GameObject on '{path}' was not found!");
            return;
        }

        HookupTextToSpeechOnTransform(transform, force);
    }

    public static void HookupTextToSpeechOnTransform(this Transform transform, bool force = false)
    {
        if (transform == null)
        {
            Debug.LogWarning("Can't hook up text to speech on null transform!");
            return;
        }

        var path = transform.GetGameObjectPath();

#if DEBUG
        //Debug.Log($"Attempting to get TextMeshProUGUI in children on '{path}'...");
#endif

        var allTexts = transform.GetComponentsInChildren<TextMeshProUGUI>();
        if (allTexts?.Length == 0)
        {
            Debug.LogWarning($"No TextMeshProUGUI found on '{path}'!");
            return;
        }

#if DEBUG
        Debug.Log($"Found {allTexts?.Length} TextMeshProUGUIs on '{path}'!");
#endif

        allTexts.HookupTextToSpeech(force);
    }

    public static void HookupTextToSpeech(this TextMeshProUGUI[] textMeshPros, bool force = false)
    {
        if (textMeshPros == null)
        {
            Debug.LogWarning("No TextMeshProUGUIs to hook up!");
            return;
        }

        foreach (var textMeshPro in textMeshPros)
        {
            textMeshPro.HookupTextToSpeech(force);
        }
    }

    public static void HookupTextToSpeech(this TextMeshProUGUI textMeshPro, bool force = false)
    {
        if (textMeshPro == null)
        {
            Debug.LogWarning("No TextMeshProUGUI!");
            return;
        }

        var textMeshProTransform = textMeshPro.transform;
        if (textMeshProTransform == null)
        {
            Debug.LogWarning("Transform on TextMeshProUGUI is null!");
            return;
        }

        if (!force && textMeshProTransform.IsParentClickable())
        {
            return;
        }

        var skipEventAssignment = false;

        var defaultValues = textMeshProTransform.GetComponent<TextMeshProValues>();
        if (defaultValues == null)
            defaultValues = textMeshProTransform.gameObject?.AddComponent<TextMeshProValues>();
        else
            skipEventAssignment = true;

        defaultValues!.FontStyles = textMeshPro.fontStyle;
        defaultValues.Color = textMeshPro.color;
        defaultValues.ExtraPadding = textMeshPro.extraPadding;

        if (skipEventAssignment)
        {
#if DEBUG
            //Debug.Log("Skipping event assignment!");
#endif
            return;
        }

        textMeshPro.raycastTarget = true;

        textMeshPro.OnPointerEnterAsObservable().Subscribe(
            _ =>
            {
                if (Main.Settings!.FontStyleOnHover)
                {
                    for (int i = 0; i < Main.Settings.FontStyles!.Length; i++)
                    {
                        if (Main.Settings.FontStyles[i])
                        {
                            textMeshPro.fontStyle |= (FontStyles)Enum.Parse(typeof(FontStyles), Main.FontStyleNames![i]!, true);
                        }
                    }
                    textMeshPro.extraPadding = false;
                }

                if (Main.Settings.ColorOnHover)
                {
                    textMeshPro.color = m_HoverColor;
                }
            }
        );

        textMeshPro.OnPointerExitAsObservable().Subscribe(
            _ =>
            {
                textMeshPro.fontStyle = defaultValues.FontStyles;
                textMeshPro.color = defaultValues.Color;
                textMeshPro.extraPadding = defaultValues.ExtraPadding;
            }
        );

        textMeshPro.OnPointerClickAsObservable().Subscribe(
            clickEvent =>
            {
                if (clickEvent?.button == UnityEngine.EventSystems.PointerEventData.InputButton.Left)
                {
                    Main.Speech?.Speak(textMeshPro.text);
                }
            }
        );
    }
}