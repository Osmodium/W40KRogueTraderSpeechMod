using Kingmaker.Blueprints;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Templates;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Owlcat.Runtime.UI.Controls.Button;
using SpeechMod.Unity.Extensions;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using GameObject = UnityEngine.GameObject;
using Object = UnityEngine.Object;

namespace SpeechMod.Unity;

public static class ButtonFactory
{
    private const string ARROW_BUTTON_PATH = "/SurfacePCView(Clone)/SurfaceStaticPartPCView/StaticCanvas/SurfaceHUD/SurfaceActionBarPCView/MainContainer/ActionBarContainer/LeftSide/BackgroundContainer/Mask/Container/SurfaceActionBarPatyWeaponsView/CurrentSet/Layout/WeaponSlotsContainer/ConvertButton";
    private const string ARROW_BUTTON_PREFAB_NAME = "SpeechMod_ArrowButtonPrefab";

    private const string TEMP_ARROW_BUTTON_PATH = "SurfaceStaticPartPCView/StaticCanvas/SurfaceHUD/SurfaceActionBarPCView/MainContainer/ActionBarContainer/LeftSide/BackgroundContainer/Mask/Container/SurfaceActionBarPatyWeaponsView/CurrentSet/Layout/WeaponSlotsContainer/ConvertButton";
    private const string TEMP_ARROW_BUTTON_PREFAB_NAME = "SpeechMod_Temporary_ArrowButtonPrefab";

    private static GameObject ArrowButton => UIHelper.TryFind(ARROW_BUTTON_PATH)?.gameObject;

    private static GameObject s_ArrowButtonPrefab;
    private static GameObject s_SpaceUITempArrowButtonPrefab;

    private static bool IsArrowButtonAvailable()
    {
        if (s_ArrowButtonPrefab != null)
        {
            return true;
        }

        s_ArrowButtonPrefab = GameObject.Find(ARROW_BUTTON_PREFAB_NAME);

        if (s_ArrowButtonPrefab != null)
        {
            return true;
        }

        if (ArrowButton != null)
        {
            SetupArrowPrefab(ArrowButton);
        }

        return s_ArrowButtonPrefab != null;
    }

    private static void LoadArrowButtonPrefabFromResource()
    {
        Debug.LogWarning("Load surface ui asset...");
        // "6dda9b696601b7847996fe3926c42b50" is the GUID of the assets inside the "surfacepcview.res" resource file
        var asset = ResourcesLibrary.TryGetResource<GameObject>("6dda9b696601b7847996fe3926c42b50");

        var arrow = asset?.transform.TryFind(TEMP_ARROW_BUTTON_PATH)?.gameObject;
        if (arrow != null)
        {
            s_SpaceUITempArrowButtonPrefab = Object.Instantiate(arrow);
            s_SpaceUITempArrowButtonPrefab!.name = TEMP_ARROW_BUTTON_PREFAB_NAME;
            s_SpaceUITempArrowButtonPrefab.transform!.localRotation = Quaternion.Euler(0, 0, 270);
        }
    }

    private static void SetupArrowPrefab(GameObject arrowPrefab)
    {
        s_ArrowButtonPrefab = Object.Instantiate(arrowPrefab);
        s_ArrowButtonPrefab!.name = ARROW_BUTTON_PREFAB_NAME;
        s_ArrowButtonPrefab.transform!.localRotation = Quaternion.Euler(0, 0, 270);
        Object.DontDestroyOnLoad(s_ArrowButtonPrefab);
    }

    private static GameObject CreateTemporaryPlayButton(Transform parent)
    {
#if DEBUG
        Debug.Log("Creating temporary play button...");
#endif
        s_SpaceUITempArrowButtonPrefab = GameObject.Find(TEMP_ARROW_BUTTON_PREFAB_NAME);

        if (s_SpaceUITempArrowButtonPrefab == null)
        {
            LoadArrowButtonPrefabFromResource();
        }

        return Object.Instantiate(s_SpaceUITempArrowButtonPrefab, parent);
    }

    private static GameObject CreatePlayButton(Transform parent, UnityAction action, string text, string toolTip)
    {
        if (!IsArrowButtonAvailable())
        {
            var temporaryPlayButton = CreateTemporaryPlayButton(parent);

            SetupOwlcatMultiButton(temporaryPlayButton, action, text, toolTip);

            return temporaryPlayButton;
        }

        var buttonGameObject = Object.Instantiate(s_ArrowButtonPrefab, parent);

        SetupOwlcatMultiButton(buttonGameObject, action, text, toolTip);

        return buttonGameObject;
    }

    private static void SetupOwlcatMultiButton(GameObject buttonGameObject, UnityAction action, string text, string toolTip)
    {
        if (buttonGameObject == null)
            return;

        var button = buttonGameObject.GetComponent<OwlcatMultiButton>();
        if (button == null)
        {
            button = buttonGameObject.AddComponent<OwlcatMultiButton>();
        }

        button!.OnLeftClick!.RemoveAllListeners();
        button.OnLeftClick.AddListener(action);

        if (!string.IsNullOrWhiteSpace(text))
            button.SetTooltip(new TooltipTemplateSimple(text, toolTip));

        button.SetInteractable(true);
    }

    public static GameObject TryCreatePlayButton(Transform parent, UnityAction action, string text = null, string tooltip = null)
    {
        return CreatePlayButton(parent, action, text, tooltip);
    }

    public static GameObject TryAddButtonToTextMeshPro(this TextMeshProUGUI textMeshPro, string buttonName, Vector2? anchoredPosition = null, Vector3? scale = null, TextMeshProUGUI[] textMeshProUguis = null)
    {
        var transform = textMeshPro?.transform;
        var tmpButton = transform.TryFind(buttonName)?.gameObject;
        if (tmpButton != null)
            return null;

#if DEBUG
        Debug.Log($"Adding playbutton to {textMeshPro?.name}...");
#endif

        var button = TryCreatePlayButton(transform, () =>
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
        button.RectAlignTopLeft(anchoredPosition);

        if (scale.HasValue)
            button.transform!.localScale = scale.Value;

        button.SetActive(true);
        return button;
    }
}
