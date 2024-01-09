using JetBrains.Annotations;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Templates;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Owlcat.Runtime.UI.Controls.Button;
using SpeechMod.Unity.Extensions;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace SpeechMod.Unity;

public static class ButtonFactory
{
    private const string ARROW_BUTTON_PATH = "/SurfacePCView(Clone)/SurfaceStaticPartPCView/StaticCanvas/SurfaceHUD/SurfaceActionBarPCView/MainContainer/ActionBarContainer/LeftSide/BackgroundContainer/Mask/Container/SurfaceActionBarPatyWeaponsView/CurrentSet/Layout/WeaponSlotsContainer/ConvertButton";
    private const string ARROW_BUTTON_PREFAB_NAME = "SpeechMod_ArrowButtonPrefab";

    private static GameObject ArrowButton => UIHelper.TryFind(ARROW_BUTTON_PATH)?.gameObject;

    private static GameObject m_ArrowButtonPrefab = null;

    private static void TryTakeArrowBackup()
    {
        if (m_ArrowButtonPrefab != null)
            return;

        m_ArrowButtonPrefab = GameObject.Find(ARROW_BUTTON_PREFAB_NAME);

        if (m_ArrowButtonPrefab != null)
            return;

        m_ArrowButtonPrefab = Object.Instantiate(ArrowButton);
        Object.DontDestroyOnLoad(m_ArrowButtonPrefab);
    }

    public static GameObject CreatePlayButton(Transform parent, UnityAction action)
    {
        return CreatePlayButton(parent, action, null, null);
    }

    private static GameObject CreatePlayButton(Transform parent, UnityAction action, string text, string toolTip)
    {
        if (ArrowButton == null)
        {
#if DEBUG
            Debug.LogWarning("ArrowButton is null!");
#endif
            if (m_ArrowButtonPrefab == null)
            {
#if DEBUG
                Debug.LogWarning("m_ArrowButtonPrefab is null!");
#endif
                return null;
            }

            var prefabButtonGameObject = Object.Instantiate(m_ArrowButtonPrefab, parent);
            SetupButton(prefabButtonGameObject, action, text, toolTip);
            return prefabButtonGameObject;
        }

        TryTakeArrowBackup();

        var buttonGameObject = Object.Instantiate(ArrowButton, parent);
        SetupButton(buttonGameObject, action, text, toolTip);

        return buttonGameObject;
    }

    private static void SetupButton(GameObject buttonGameObject, UnityAction action, string text, string toolTip)
    {
        var button = buttonGameObject!.GetComponent<OwlcatMultiButton>();
        if (button == null)
        {
            button = buttonGameObject.AddComponent<OwlcatMultiButton>();
        }

        button.OnLeftClick.RemoveAllListeners();
        button.OnLeftClick.AddListener(action);

        if (!string.IsNullOrWhiteSpace(text))
            button.SetTooltip(new TooltipTemplateSimple(text, toolTip));

        button.SetInteractable(true);
    }

    public static GameObject TryAddButton(this TextMeshProUGUI textMeshPro, string buttonName, Vector2? anchoredPosition = null, Vector3? scale = null, [CanBeNull] TextMeshProUGUI[] textMeshProUguis = null)
    {
        var transform = textMeshPro?.transform;
        var tmpButton = transform.TryFind(buttonName)?.gameObject;
        if (tmpButton != null)
            return null;

#if DEBUG
        Debug.Log($"Adding playbutton to {textMeshPro?.name}...");
#endif

        var button = CreatePlayButton(transform, () =>
        {
            var text = textMeshPro?.text;
            if (textMeshProUguis != null)
            {
                text = textMeshProUguis.Where(textOverride => textOverride != null).Select(to => to.text).Aggregate("", (previous, current) => $"{previous}, {current}");
            }
            Main.Speech?.Speak(text);
        });

        if (button == null || button.transform == null)
            return null;

        button.name = buttonName;
        button.transform.localRotation = Quaternion.Euler(0, 0, 270);
        button.RectAlignTopLeft(anchoredPosition);

        if (scale.HasValue)
            button.transform!.localScale = scale.Value;

        button.SetActive(true);
        return button;
    }
}
