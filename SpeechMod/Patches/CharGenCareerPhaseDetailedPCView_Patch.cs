using HarmonyLib;
using Kingmaker.UI.MVVM.View.CharGen.PC.Phases.Career;
using SpeechMod.Unity.Extensions;
#if DEBUG
using UnityEngine;
#endif
using UnityEngine.UI;

namespace SpeechMod.Patches;

[HarmonyPatch(typeof(CharGenCareerPhaseDetailedPCView), "BindViewImplementation")]
public class CharGenCareerPhaseDetailedPCView_Patch
{
    private const string CHARGEN_DESCRIPTION_PATH = "/MainMenuPCView(Clone)/UICanvas/CharGenContextPCView/CharGenPCView/Content/PhaseDetailedViews/CharGenCareerPhaseDetailedPCView/CharGenDescription/LevelupDescriptionView/Viewport";
    private const string CHARGEN_ABILITY_HEADER_PATH = "/MainMenuPCView(Clone)/UICanvas/CharGenContextPCView/CharGenPCView/Content/PhaseDetailedViews/CharGenCareerPhaseDetailedPCView/Device/Content/UnitProgressionPCView/CareerPathProgressionPCView/Content/RightContainer/CareerPathSelectionPartPCView/TooltipBrickTitleView/Title-H0/Title-H0";
    private const string CHARGEN_ABILITY_FEATURE_TEXT_PATH = "/MainMenuPCView(Clone)/UICanvas/CharGenContextPCView/CharGenPCView/Content/PhaseDetailedViews/CharGenCareerPhaseDetailedPCView/Device/Content/UnitProgressionPCView/CareerPathProgressionPCView/Content/RightContainer/CareerPathSelectionPartPCView/Content/ContentPanel/RankEntryFeatureSelectionPCView/FeaturesContainer/Viewport/NoFeaturesText";
    private const string CHARGEN_ABILITY_TEXT_PATH = "/MainMenuPCView(Clone)/UICanvas/CharGenContextPCView/CharGenPCView/Content/PhaseDetailedViews/CharGenCareerPhaseDetailedPCView/Device/Content/UnitProgressionPCView/CareerPathProgressionPCView/Content/RightContainer/CareerPathSelectionPartPCView/Content/ContentPanel/RankEntryFeatureDescriptionPCView/ChargenDescriptionView/Header/TooltipBrickFeatureHeaderView(Clone)/TextBlock/Title-H1";

    public static void Postfix()
    {
        if (!Main.Enabled)
            return;

#if DEBUG
        Debug.Log($"{nameof(CharGenCareerPhaseDetailedPCView)}_BindViewImplementation_Postfix");
#endif

        UIHelper.TryFind(CHARGEN_DESCRIPTION_PATH)?
            .GetComponent<Image>()?
            .SetRaycastTarget(false);

        Hooks.HookUpTextToSpeechOnTransformWithPath(CHARGEN_ABILITY_HEADER_PATH);
        Hooks.HookUpTextToSpeechOnTransformWithPath(CHARGEN_ABILITY_FEATURE_TEXT_PATH);
        Hooks.HookUpTextToSpeechOnTransformWithPath(CHARGEN_ABILITY_TEXT_PATH, true);
    }
}