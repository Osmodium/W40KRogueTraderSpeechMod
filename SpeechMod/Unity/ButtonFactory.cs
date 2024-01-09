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

    private static GameObject ArrowButton => UIHelper.TryFind(ARROW_BUTTON_PATH)?.gameObject;

    private static GameObject m_ArrowButtonPrefab = null;

    public static void Instantiate()
    {
        Debug.Log("ButtonFactory Instantiate!");
        if (ArrowButton == null)
        {
#if DEBUG
            Debug.LogWarning("ArrowButton is null!");
            return;
#endif
        }

        m_ArrowButtonPrefab = Object.Instantiate(ArrowButton);
        var button = m_ArrowButtonPrefab!.GetComponent<OwlcatMultiButton>();
        if (button == null)
        {
            button = m_ArrowButtonPrefab.AddComponent<OwlcatMultiButton>();
        }
        button.SetInteractable(false);
        button.OnLeftClick.RemoveAllListeners();

        Object.DontDestroyOnLoad(m_ArrowButtonPrefab);
    }

    public static GameObject CreatePlayButton(Transform parent, UnityAction action)
    {
        return CreatePlayButton(parent, action, null, null);
    }

    private static GameObject CreatePlayButton(Transform parent, UnityAction action, string text, string toolTip)
    {
        if (m_ArrowButtonPrefab == null)
        {
#if DEBUG
            Debug.LogWarning("ArrowButtonPrefab is null!");
            return null;
#endif
        }

        var buttonGameObject = Object.Instantiate(m_ArrowButtonPrefab, parent);
        SetLeftClickAction(buttonGameObject, action, text, toolTip);
        return buttonGameObject;
    }

    private static void SetLeftClickAction(GameObject buttonGameObject, UnityAction action, string text, string toolTip)
    {
        var button = buttonGameObject!.GetComponent<OwlcatMultiButton>();
        button.OnLeftClick.AddListener(action);

        if (!string.IsNullOrWhiteSpace(text))
            button.SetTooltip(new TooltipTemplateSimple(text, toolTip));

        button.SetInteractable(true);
    }

    public static void TryAddButton(this TextMeshProUGUI textMeshPro, string buttonName, Vector2? anchoredPosition = null, [CanBeNull] TextMeshProUGUI[] textMeshProUguis = null)
    {
        var transform = textMeshPro?.transform;
        var tmpButton = transform.TryFind(buttonName)?.gameObject;
        if (tmpButton != null)
            return;

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
            return;

        button.name = buttonName;
        button.transform.localRotation = Quaternion.Euler(0, 0, 270);
        button.RectAlignTopLeft(anchoredPosition);
        button.SetActive(true);
    }
}
