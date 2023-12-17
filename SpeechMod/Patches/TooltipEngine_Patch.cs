using HarmonyLib;
using Kingmaker.Code.UI.MVVM.View.InfoWindow;
using Kingmaker.Code.UI.MVVM.View.Tooltip;
using Kingmaker.Code.UI.MVVM.View.Tooltip.Bricks;
using Kingmaker.Code.UI.MVVM.VM.InfoWindow;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Templates;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Kingmaker.UI.MVVM.View.Tooltip.PC.Bricks.CombatLog;
using Owlcat.Runtime.UI.Tooltips;
using SpeechMod.Unity;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace SpeechMod.Patches;

[HarmonyPatch(typeof(InfoBaseView<InfoBaseVM>), "SetPart")]
public static class Tooltip_Patch
{
    public static void Postfix(IEnumerable<TooltipBaseBrickVM> bricks, RectTransform container)
    {
        if (!Main.Enabled)
            return;

        if (container == null)
            return;

        HookBricksByContainer(container);
    }

    private static void HookBricksByContainer(RectTransform container)
    {
#if DEBUG
        Debug.Log(container?.transform.GetGameObjectPath());
#endif
        var allTexts = container?.transform?.parent?.GetComponentsInChildren<TextMeshProUGUI>(true);
        allTexts.HookupTextToSpeech();
    }
}

//[HarmonyPatch(typeof(TooltipEngine), nameof(TooltipEngine.GetBrickView))]
//static class TooltipEngine_Patch
//{
//    // ReSharper disable once InconsistentNaming
//    public static void Postfix(ref MonoBehaviour __result)
//    {
//        if (!Main.Enabled)
//            return;

//        if (__result == null)
//            return;

//        switch (__result)
//        {
//            case TooltipBrickTextView:
//            case TooltipBrickIconValueStatView:
//            case TooltipBrickTextValueView:
//            case TooltipBrickEntityHeaderView:
//            case TooltipBrickIconStatValueView:
//            case TooltipBrickFeatureHeaderView:
//            case TooltipBrickTitleView:
//            case TooltipBrickFeatureView:
//            case TooltipBrickTwoColumnsStatView:
//            case TooltipBrickIconPatternView:
//                HookBrickAsType(__result);
//                break;
//            default:
//                return;
//        }

//        //switch (__result)
//        //{
//        //    case TooltipBrickTextView:
//        //        HookBrickAsType<TooltipBrickTextView>(__result);
//        //        break;
//        //    case TooltipBrickIconValueStatView:
//        //        HookBrickAsType<TooltipBrickIconValueStatView>(__result);
//        //        break;
//        //    case TooltipBrickTextValueView:
//        //        HookBrickAsType<TooltipBrickTextValueView>(__result);
//        //        break;
//        //    case TooltipBrickEntityHeaderView:
//        //        HookBrickAsType<TooltipBrickEntityHeaderView>(__result);
//        //        break;
//        //    case TooltipBrickIconStatValueView:
//        //        HookBrickAsType<TooltipBrickIconStatValueView>(__result);
//        //        break;
//        //    case TooltipBrickFeatureHeaderView:
//        //        HookBrickAsType<TooltipBrickFeatureHeaderView>(__result);
//        //        break;
//        //    case TooltipBrickTitleView:
//        //        HookBrickAsType<TooltipBrickTitleView>(__result);
//        //        break;
//        //    case TooltipBrickFeatureView:
//        //        HookBrickAsType<TooltipBrickFeatureView>(__result);
//        //        break;
//        //    case TooltipBrickTwoColumnsStatView:
//        //        HookBrickAsType<TooltipBrickTwoColumnsStatView>(__result);
//        //        break;
//        //    case TooltipBrickIconPatternView:
//        //        HookBrickAsType<TooltipBrickIconPatternView>(__result);
//        //        break;
//        //    default:
//        //        return;
//        //}
//    }

//    private static void HookBrickAsType(MonoBehaviour brick)
//    {

//#if DEBUG
//        Debug.Log(brick!.transform.GetGameObjectPath());
//#endif

//        //if (IsInvalid(brick?.transform?.parent))
//        //    return;

//        if (brick?.transform?.parent == null)
//        {
//            return;
//        }

//        var allTexts = brick.transform.parent.GetComponentsInChildren<TextMeshProUGUI>(true);
//        allTexts.HookupTextToSpeech();
//    }

//    //    private static void HookBrickAsType<T>(MonoBehaviour brick) where T : MonoBehaviour
//    //    {

//    //#if DEBUG
//    //        Debug.Log(brick!.transform.GetGameObjectPath());
//    //#endif

//    //        var brickView = brick as T;
//    //        if (IsInvalid(brickView?.transform?.parent))
//    //            return;

//    //        var allTexts = brickView?.GetComponentsInChildren<TextMeshProUGUI>();
//    //        allTexts.HookupTextToSpeech();
//    //    }

//    // TODO: Better way of telling if inside hover tooltip.
//    private static bool IsInvalid(Transform transform)
//    {
//        return transform is null;
//    }
//}