using SpeechMod.Unity.Extensions;
using SpeechMod.Voice;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace SpeechMod.Unity;

/// <summary>
/// Builds a voice picker panel using the game's UI elements.
/// Shown when right-clicking a character portrait in dialog.
/// Uses a full-screen dimmed overlay so nothing behind can be interacted with.
/// Press Escape or click outside panel to close.
/// </summary>
public static class CharacterVoicePickerPanel
{
    private const string OVERLAY_NAME = "SpeechMod_VoicePickerOverlay";

    private static GameObject _overlayInstance;
    private static GameObject _panelInstance;
    private static string _currentCharacterId;
    private static string _currentCharacterName;
    private static int _selectedVoiceIndex;
    private static int _currentRate;
    private static int _currentVolume;
    private static int _currentPitch;

    private static List<GameObject> _voiceButtons = new();
    private static TextMeshProUGUI _titleLabel;
    private static TextMeshProUGUI _rateLabel;
    private static TextMeshProUGUI _volumeLabel;
    private static TextMeshProUGUI _pitchLabel;
    private static TextMeshProUGUI _voiceNameLabel;
    private static TextMeshProUGUI _nationalityLabel;

    public static bool IsOpen => _overlayInstance != null && _overlayInstance.activeSelf;

    public static void Open(string characterId, string characterName)
    {
        if (string.IsNullOrWhiteSpace(characterId))
            return;

        _currentCharacterId = characterId;
        _currentCharacterName = characterName ?? "Unknown";

        // Load existing settings for this character
        if (Main.Settings.CharacterVoices.TryGetValue(characterId, out var existing))
        {
            _selectedVoiceIndex = existing.VoiceIndex;
            _currentRate = existing.Rate;
            _currentVolume = existing.Volume;
            _currentPitch = existing.Pitch;
        }
        else
        {
            _selectedVoiceIndex = Main.Settings.NarratorVoice;
            _currentRate = Main.Settings.NarratorRate;
            _currentVolume = Main.Settings.NarratorVolume;
            _currentPitch = Main.Settings.NarratorPitch;
        }

        if (_overlayInstance != null)
        {
            UpdatePanel();
            _overlayInstance.SetActive(true);
            return;
        }

        BuildOverlayAndPanel();
    }

    public static void Close()
    {
        if (_overlayInstance != null)
        {
            _overlayInstance.SetActive(false);
        }
    }

    public static void Toggle(string characterId, string characterName)
    {
        if (IsOpen && _currentCharacterId == characterId)
            Close();
        else
            Open(characterId, characterName);
    }

    private static void SaveSettings()
    {
        if (string.IsNullOrWhiteSpace(_currentCharacterId))
            return;

        var settings = new CharacterVoiceSettings
        {
            VoiceIndex = _selectedVoiceIndex,
            Rate = _currentRate,
            Volume = _currentVolume,
            Pitch = _currentPitch
        };

        Main.Settings.CharacterVoices[_currentCharacterId] = settings;

#if DEBUG
        Debug.Log($"[SpeechMod] Saved voice settings for {_currentCharacterName} ({_currentCharacterId}): Voice={_selectedVoiceIndex}, Rate={_currentRate}, Vol={_currentVolume}, Pitch={_currentPitch}");
#endif
    }

    private static void RemoveCharacterSettings()
    {
        if (string.IsNullOrWhiteSpace(_currentCharacterId))
            return;

        Main.Settings.CharacterVoices.Remove(_currentCharacterId);
#if DEBUG
        Debug.Log($"[SpeechMod] Removed voice settings for {_currentCharacterName} ({_currentCharacterId})");
#endif
    }

    private static void BuildOverlayAndPanel()
    {
        // --- Full-screen overlay root with its own Canvas ---
        _overlayInstance = new GameObject(OVERLAY_NAME);

        var overlayCanvas = _overlayInstance.AddComponent<Canvas>();
        overlayCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        overlayCanvas.sortingOrder = 30000;

        var scaler = _overlayInstance.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        _overlayInstance.AddComponent<GraphicRaycaster>();

        // Escape key handler
        _overlayInstance.AddComponent<VoicePickerEscapeHandler>();

        // --- Dim background (full-screen, blocks all input behind) ---
        var dimGo = new GameObject("DimBackground");
        dimGo.transform.SetParent(_overlayInstance.transform, false);
        var dimRt = dimGo.AddComponent<RectTransform>();
        dimRt.anchorMin = Vector2.zero;
        dimRt.anchorMax = Vector2.one;
        dimRt.sizeDelta = Vector2.zero;
        dimRt.offsetMin = Vector2.zero;
        dimRt.offsetMax = Vector2.zero;

        var dimImage = dimGo.AddComponent<Image>();
        dimImage.color = new Color(0, 0, 0, 0.6f);
        dimImage.raycastTarget = true;

        // Click on dim background = close
        var dimBtn = dimGo.AddComponent<Button>();
        dimBtn.onClick.AddListener(Close);

        // --- Actual panel ---
        BuildPanel(_overlayInstance.transform);

        _overlayInstance.SetActive(true);
    }

    private static void BuildPanel(Transform overlayParent)
    {
        _panelInstance = new GameObject("Panel");
        _panelInstance.transform.SetParent(overlayParent, false);

        var rt = _panelInstance.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(520, 560);
        rt.anchoredPosition = Vector2.zero;

        var bgImage = _panelInstance.AddComponent<Image>();
        bgImage.color = new Color(0.08f, 0.06f, 0.04f, 0.97f);
        bgImage.raycastTarget = true;

        var vlg = _panelInstance.AddComponent<VerticalLayoutGroup>();
        vlg.padding = new RectOffset(16, 16, 12, 12);
        vlg.spacing = 6;
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;
        vlg.childControlWidth = true;
        vlg.childControlHeight = true;

        // Title row
        var titleRow = CreateRow(_panelInstance.transform, 36);
        _titleLabel = CreateLabel(titleRow.transform, $"Voice: {_currentCharacterName}", 18, TextAlignmentOptions.MidlineLeft);
        var closeBtn = CreateTextButton(titleRow.transform, "✕", Close, 30, 30);
        closeBtn.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 30);

        CreateSeparator(_panelInstance.transform);

        // Voice info row
        var infoRow = CreateRow(_panelInstance.transform, 28);
        _voiceNameLabel = CreateLabel(infoRow.transform, GetVoiceDisplayName(_selectedVoiceIndex), 14, TextAlignmentOptions.MidlineLeft);
        _nationalityLabel = CreateLabel(infoRow.transform, GetVoiceNationality(_selectedVoiceIndex), 12, TextAlignmentOptions.MidlineRight);

        // Voice list
        var voiceListContainer = CreateVoiceListArea(_panelInstance.transform);

        CreateSeparator(_panelInstance.transform);

        // Sliders
        CreateSliderRow(_panelInstance.transform, "Rate:", ref _currentRate, -10, 10, val =>
        {
            _currentRate = val;
            if (_rateLabel != null) _rateLabel.text = $"Rate: {_currentRate}";
            SaveSettings();
        }, out _rateLabel);

        if (Main.Speech is WindowsSpeech)
        {
            CreateSliderRow(_panelInstance.transform, "Volume:", ref _currentVolume, 0, 100, val =>
            {
                _currentVolume = val;
                if (_volumeLabel != null) _volumeLabel.text = $"Volume: {_currentVolume}";
                SaveSettings();
            }, out _volumeLabel);

            CreateSliderRow(_panelInstance.transform, "Pitch:", ref _currentPitch, -10, 10, val =>
            {
                _currentPitch = val;
                if (_pitchLabel != null) _pitchLabel.text = $"Pitch: {_currentPitch}";
                SaveSettings();
            }, out _pitchLabel);
        }

        CreateSeparator(_panelInstance.transform);

        // Preview + Reset buttons
        var buttonRow = CreateRow(_panelInstance.transform, 32);
        CreateTextButton(buttonRow.transform, "▶ Preview", () =>
        {
            var voiceKey = GetVoiceKey(_selectedVoiceIndex);
            if (voiceKey != null)
            {
                var text = $"<speak version=\"1.0\" xmlns=\"http://www.w3.org/2001/10/synthesis\" xmlns:mstts=\"http://www.w3.org/2001/mstts\"><voice required=\"Name={voiceKey}\"><pitch absmiddle=\"{_currentPitch}\"/><rate absspeed=\"{_currentRate}\"/><volume level=\"{_currentVolume}\"/>This is {_currentCharacterName} speaking with the selected voice.</voice></speak>";
                if (Main.Speech is WindowsSpeech)
                    WindowsVoiceUnity.Speak(text, 80);
            }
        });

        CreateTextButton(buttonRow.transform, "■ Stop", () => Main.Speech?.Stop());

        CreateTextButton(buttonRow.transform, "Reset", () =>
        {
            RemoveCharacterSettings();
            _selectedVoiceIndex = Main.Settings.NarratorVoice;
            _currentRate = Main.Settings.NarratorRate;
            _currentVolume = Main.Settings.NarratorVolume;
            _currentPitch = Main.Settings.NarratorPitch;
            UpdatePanel();
        });

        PopulateVoiceList(voiceListContainer);
    }

    private static void UpdatePanel()
    {
        if (_titleLabel != null)
            _titleLabel.text = $"Voice: {_currentCharacterName}";
        if (_voiceNameLabel != null)
            _voiceNameLabel.text = GetVoiceDisplayName(_selectedVoiceIndex);
        if (_nationalityLabel != null)
            _nationalityLabel.text = GetVoiceNationality(_selectedVoiceIndex);
        if (_rateLabel != null)
            _rateLabel.text = $"Rate: {_currentRate}";
        if (_volumeLabel != null)
            _volumeLabel.text = $"Volume: {_currentVolume}";
        if (_pitchLabel != null)
            _pitchLabel.text = $"Pitch: {_currentPitch}";

        RefreshVoiceListHighlight();
    }

    private static GameObject CreateVoiceListArea(Transform parent)
    {
        var scrollArea = new GameObject("VoiceScrollArea");
        scrollArea.transform.SetParent(parent, false);
        scrollArea.AddComponent<RectTransform>();
        var le = scrollArea.AddComponent<LayoutElement>();
        le.preferredHeight = 280;
        le.flexibleHeight = 1;

        var scrollRect = scrollArea.AddComponent<ScrollRect>();
        scrollRect.horizontal = false;
        scrollRect.vertical = true;
        scrollRect.scrollSensitivity = 30;

        var viewport = new GameObject("Viewport");
        viewport.transform.SetParent(scrollArea.transform, false);
        var viewportRt = viewport.AddComponent<RectTransform>();
        viewportRt.anchorMin = Vector2.zero;
        viewportRt.anchorMax = Vector2.one;
        viewportRt.sizeDelta = Vector2.zero;
        viewportRt.offsetMin = Vector2.zero;
        viewportRt.offsetMax = Vector2.zero;
        var mask = viewport.AddComponent<Mask>();
        mask.showMaskGraphic = false;
        var viewportImg = viewport.AddComponent<Image>();
        viewportImg.color = new Color(0.05f, 0.04f, 0.03f, 0.8f);

        var content = new GameObject("Content");
        content.transform.SetParent(viewport.transform, false);
        var contentRt = content.AddComponent<RectTransform>();
        contentRt.anchorMin = new Vector2(0, 1);
        contentRt.anchorMax = new Vector2(1, 1);
        contentRt.pivot = new Vector2(0.5f, 1);
        contentRt.sizeDelta = new Vector2(0, 0);

        var contentVlg = content.AddComponent<VerticalLayoutGroup>();
        contentVlg.padding = new RectOffset(4, 4, 4, 4);
        contentVlg.spacing = 2;
        contentVlg.childForceExpandWidth = true;
        contentVlg.childForceExpandHeight = false;
        contentVlg.childControlWidth = true;
        contentVlg.childControlHeight = true;

        var csf = content.AddComponent<ContentSizeFitter>();
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        scrollRect.viewport = viewportRt;
        scrollRect.content = contentRt;

        return content;
    }

    private static void PopulateVoiceList(GameObject contentContainer)
    {
        _voiceButtons.Clear();
        var voices = Main.VoicesDict;
        if (voices == null || voices.Count == 0) return;

        for (var i = 0; i < voices.Count; i++)
        {
            var index = i;
            var kvp = voices.ElementAt(i);

            var row = new GameObject($"VoiceRow_{i}");
            row.transform.SetParent(contentContainer.transform, false);
            row.AddComponent<RectTransform>();
            var rowLe = row.AddComponent<LayoutElement>();
            rowLe.preferredHeight = 26;
            rowLe.minHeight = 26;

            var rowBg = row.AddComponent<Image>();
            rowBg.color = index == _selectedVoiceIndex
                ? new Color(0.3f, 0.25f, 0.1f, 0.8f)
                : new Color(0.1f, 0.08f, 0.06f, 0.5f);
            rowBg.raycastTarget = true;

            var hlg = row.AddComponent<HorizontalLayoutGroup>();
            hlg.padding = new RectOffset(8, 8, 2, 2);
            hlg.spacing = 8;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = true;
            hlg.childControlWidth = true;

            var nameLabel = CreateLabel(row.transform, kvp.Key, 12, TextAlignmentOptions.MidlineLeft, flexWidth: 1f);
            if (index == _selectedVoiceIndex)
                nameLabel.color = new Color(0.9f, 0.75f, 0.3f, 1f);

            var natLabel = CreateLabel(row.transform, kvp.Value, 11, TextAlignmentOptions.MidlineRight, flexWidth: 0.4f);
            natLabel.color = new Color(0.6f, 0.6f, 0.6f, 1f);

            var btn = row.AddComponent<Button>();
            btn.onClick.AddListener(() => SelectVoice(index));

            _voiceButtons.Add(row);
        }
    }

    private static void SelectVoice(int index)
    {
        _selectedVoiceIndex = index;
        SaveSettings();
        UpdatePanel();
    }

    private static void RefreshVoiceListHighlight()
    {
        for (var i = 0; i < _voiceButtons.Count; i++)
        {
            var row = _voiceButtons[i];
            if (row == null) continue;

            var bg = row.GetComponent<Image>();
            if (bg != null)
            {
                bg.color = i == _selectedVoiceIndex
                    ? new Color(0.3f, 0.25f, 0.1f, 0.8f)
                    : new Color(0.1f, 0.08f, 0.06f, 0.5f);
            }

            var labels = row.GetComponentsInChildren<TextMeshProUGUI>();
            if (labels.Length > 0)
            {
                labels[0].color = i == _selectedVoiceIndex
                    ? new Color(0.9f, 0.75f, 0.3f, 1f)
                    : Color.white;
            }
        }

        if (_voiceNameLabel != null)
            _voiceNameLabel.text = GetVoiceDisplayName(_selectedVoiceIndex);
        if (_nationalityLabel != null)
            _nationalityLabel.text = GetVoiceNationality(_selectedVoiceIndex);
    }

    private static string GetVoiceDisplayName(int index)
    {
        var voices = Main.VoicesDict;
        if (voices == null || index < 0 || index >= voices.Count) return "None";
        return voices.ElementAt(index).Key;
    }

    private static string GetVoiceNationality(int index)
    {
        var voices = Main.VoicesDict;
        if (voices == null || index < 0 || index >= voices.Count) return "";
        return voices.ElementAt(index).Value;
    }

    private static string GetVoiceKey(int index)
    {
        var voices = Main.VoicesDict;
        if (voices == null || index < 0 || index >= voices.Count) return null;
        return voices.ElementAt(index).Key;
    }

    #region UI Helpers

    private static GameObject CreateRow(Transform parent, float height)
    {
        var row = new GameObject("Row");
        row.transform.SetParent(parent, false);
        row.AddComponent<RectTransform>();
        var le = row.AddComponent<LayoutElement>();
        le.preferredHeight = height;
        le.minHeight = height;

        var hlg = row.AddComponent<HorizontalLayoutGroup>();
        hlg.spacing = 8;
        hlg.childForceExpandWidth = true;
        hlg.childForceExpandHeight = true;
        hlg.childControlWidth = true;
        hlg.childControlHeight = true;

        return row;
    }

    private static TextMeshProUGUI CreateLabel(Transform parent, string text, float fontSize, TextAlignmentOptions alignment, float flexWidth = -1)
    {
        var labelGo = new GameObject("Label");
        labelGo.transform.SetParent(parent, false);
        labelGo.AddComponent<RectTransform>();

        if (flexWidth >= 0)
        {
            var le = labelGo.AddComponent<LayoutElement>();
            le.flexibleWidth = flexWidth;
        }

        var tmp = labelGo.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.alignment = alignment;
        tmp.color = Color.white;
        tmp.textWrappingMode = TextWrappingModes.NoWrap;
        tmp.overflowMode = TextOverflowModes.Ellipsis;
        tmp.raycastTarget = false;

        return tmp;
    }

    private static GameObject CreateTextButton(Transform parent, string text, Action onClick, float width = -1, float height = -1)
    {
        var btnGo = new GameObject("Button");
        btnGo.transform.SetParent(parent, false);
        btnGo.AddComponent<RectTransform>();

        if (width > 0 || height > 0)
        {
            var le = btnGo.AddComponent<LayoutElement>();
            if (width > 0) le.preferredWidth = width;
            if (height > 0) le.preferredHeight = height;
        }

        var bg = btnGo.AddComponent<Image>();
        bg.color = new Color(0.2f, 0.18f, 0.12f, 0.9f);
        bg.raycastTarget = true;

        var textGo = new GameObject("Text");
        textGo.transform.SetParent(btnGo.transform, false);
        var textRt = textGo.AddComponent<RectTransform>();
        textRt.anchorMin = Vector2.zero;
        textRt.anchorMax = Vector2.one;
        textRt.sizeDelta = Vector2.zero;
        textRt.offsetMin = new Vector2(4, 2);
        textRt.offsetMax = new Vector2(-4, -2);

        var tmp = textGo.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = 13;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = new Color(0.85f, 0.75f, 0.5f, 1f);
        tmp.raycastTarget = false;

        var btn = btnGo.AddComponent<Button>();
        btn.onClick.AddListener(() => onClick?.Invoke());

        return btnGo;
    }

    private static void CreateSeparator(Transform parent)
    {
        var sep = new GameObject("Separator");
        sep.transform.SetParent(parent, false);
        sep.AddComponent<RectTransform>();
        var le = sep.AddComponent<LayoutElement>();
        le.preferredHeight = 2;
        le.minHeight = 2;

        var img = sep.AddComponent<Image>();
        img.color = new Color(0.4f, 0.3f, 0.15f, 0.6f);
        img.raycastTarget = false;
    }

    private static void CreateSliderRow(Transform parent, string label, ref int value, int min, int max, Action<int> onChanged, out TextMeshProUGUI valueLabel)
    {
        var row = CreateRow(parent, 28);
        valueLabel = CreateLabel(row.transform, $"{label} {value}", 13, TextAlignmentOptions.MidlineLeft, flexWidth: 0.4f);

        var sliderGo = new GameObject("Slider");
        sliderGo.transform.SetParent(row.transform, false);
        sliderGo.AddComponent<RectTransform>();
        var sliderLe = sliderGo.AddComponent<LayoutElement>();
        sliderLe.flexibleWidth = 1;

        var bgGo = new GameObject("Background");
        bgGo.transform.SetParent(sliderGo.transform, false);
        var bgRt = bgGo.AddComponent<RectTransform>();
        bgRt.anchorMin = new Vector2(0, 0.4f);
        bgRt.anchorMax = new Vector2(1, 0.6f);
        bgRt.sizeDelta = Vector2.zero;
        bgRt.offsetMin = Vector2.zero;
        bgRt.offsetMax = Vector2.zero;
        var bgImg = bgGo.AddComponent<Image>();
        bgImg.color = new Color(0.15f, 0.12f, 0.08f, 1f);

        var fillArea = new GameObject("Fill Area");
        fillArea.transform.SetParent(sliderGo.transform, false);
        var fillAreaRt = fillArea.AddComponent<RectTransform>();
        fillAreaRt.anchorMin = new Vector2(0, 0.35f);
        fillAreaRt.anchorMax = new Vector2(1, 0.65f);
        fillAreaRt.offsetMin = new Vector2(5, 0);
        fillAreaRt.offsetMax = new Vector2(-5, 0);

        var fill = new GameObject("Fill");
        fill.transform.SetParent(fillArea.transform, false);
        var fillRt = fill.AddComponent<RectTransform>();
        fillRt.anchorMin = Vector2.zero;
        fillRt.anchorMax = Vector2.one;
        fillRt.sizeDelta = Vector2.zero;
        var fillImg = fill.AddComponent<Image>();
        fillImg.color = new Color(0.6f, 0.5f, 0.2f, 0.8f);

        var handleArea = new GameObject("Handle Slide Area");
        handleArea.transform.SetParent(sliderGo.transform, false);
        var handleAreaRt = handleArea.AddComponent<RectTransform>();
        handleAreaRt.anchorMin = Vector2.zero;
        handleAreaRt.anchorMax = Vector2.one;
        handleAreaRt.offsetMin = new Vector2(5, 0);
        handleAreaRt.offsetMax = new Vector2(-5, 0);

        var handle = new GameObject("Handle");
        handle.transform.SetParent(handleArea.transform, false);
        var handleRt = handle.AddComponent<RectTransform>();
        handleRt.sizeDelta = new Vector2(14, 0);
        var handleImg = handle.AddComponent<Image>();
        handleImg.color = new Color(0.85f, 0.7f, 0.3f, 1f);

        var slider = sliderGo.AddComponent<Slider>();
        slider.direction = Slider.Direction.LeftToRight;
        slider.minValue = min;
        slider.maxValue = max;
        slider.wholeNumbers = true;
        slider.value = value;
        slider.fillRect = fillRt;
        slider.handleRect = handleRt;
        slider.targetGraphic = handleImg;

        slider.onValueChanged.AddListener(v =>
        {
            onChanged?.Invoke((int)v);
        });
    }

    #endregion

    public static void DestroyPanel()
    {
        if (_overlayInstance != null)
        {
            Object.Destroy(_overlayInstance);
            _overlayInstance = null;
            _panelInstance = null;
        }
        _voiceButtons.Clear();
    }
}

/// <summary>
/// MonoBehaviour on the overlay to close the voice picker on Escape.
/// </summary>
public class VoicePickerEscapeHandler : MonoBehaviour
{
    private void Update()
    {
        if (!CharacterVoicePickerPanel.IsOpen)
            return;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            CharacterVoicePickerPanel.Close();
        }
    }
}
