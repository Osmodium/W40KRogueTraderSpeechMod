using Kingmaker.Code.UI.MVVM.VM.Tooltip.Templates;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Owlcat.Runtime.UI.Controls.Button;
using UnityEngine;
using UnityEngine.Events;

namespace SpeechMod.Unity;

public static class ButtonFactory
{
    //private static GameObject m_ButtonPrefab = null;
    //private const string ARROW_BUTTON_PATH = "/MainMenuPCView(Clone)/UICanvas/CreditsPCView/SafeZone/RaycastImage/Content/LeftPanel/BottomPanel/PagesGroup/RightButton";
    private const string ARROW_BUTTON_PATH = "/SurfacePCView(Clone)/SurfaceStaticPartPCView/StaticCanvas/SurfaceHUD/SurfaceActionBarPCView/MainContainer/ActionBarContainer/LeftSide/BackgroundContainer/Mask/Container/SurfaceActionBarPatyWeaponsView/CurrentSet/Layout/WeaponSlotsContainer/ConvertButton";

    private static GameObject ArrowButton => Extensions.UIHelper.TryFind(ARROW_BUTTON_PATH)?.gameObject;

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
            return null;
#endif
        }

        var buttonGameObject = Object.Instantiate(ArrowButton, parent);
        SetAction(buttonGameObject, action, text, toolTip);
        return buttonGameObject;
    }

    private static void SetAction(GameObject buttonGameObject, UnityAction action, string text, string toolTip)
    {
        var button = buttonGameObject!.GetComponent<OwlcatMultiButton>();
        if (button == null)
        {
            button = buttonGameObject.AddComponent<OwlcatMultiButton>();
        }
        button.OnLeftClick.RemoveAllListeners();
        //button.OnLeftClick.SetPersistentListenerState(0, UnityEventCallState.Off); // Is this needed here?
        button.OnLeftClick.AddListener(action);

        if (!string.IsNullOrWhiteSpace(text))
            button.SetTooltip(new TooltipTemplateSimple(text, toolTip));
    }

    //public static GameObject CreateSquareButton()
    //{
    //    if (m_ButtonPrefab != null)
    //        return Object.Instantiate(m_ButtonPrefab);

    //    var staticRoot = Game.Instance.UI.Canvas.transform;
    //    var buttonsContainer = staticRoot.TryFind("HUDLayout/IngameMenuView/ButtonsPart/Container");
    //    m_ButtonPrefab = buttonsContainer.GetChild(0).gameObject;
    //    return Object.Instantiate(m_ButtonPrefab);
    //}
}
