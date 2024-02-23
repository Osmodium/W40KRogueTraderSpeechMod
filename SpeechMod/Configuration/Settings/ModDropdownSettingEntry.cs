//using System.Linq;
//using Kingmaker.Settings;
//using Kingmaker.Settings.Entities;
//using Kingmaker.UI.Models.SettingsUI.SettingAssets;
//using Kingmaker.UI.Models.SettingsUI.SettingAssets.Dropdowns;
//using ModConfiguration.Localization;
//using UnityEngine;

//namespace SpeechMod.Configuration.Settings;

//public abstract class ModDropdownSettingEntry : ModSettingEntry
//{
//    public readonly SettingsEntityInt SettingEntity;
//    public UISettingsEntityDropdownInt UiSettingEntity { get; private set; }
//    public int SelectedIndex {get; private set; }

//    protected ModDropdownSettingEntry(string key, string title, string tooltip) : base(key, title, tooltip)
//    {
//        SettingEntity = new(SettingsController.Instance, $"{ModConfigurationManager.Instance?.SettingsPrefix}.newcontrols.{Key}", 0, false, true);
//    }

//    public override UISettingsEntityBase GetUISettings() => UiSettingEntity;

//    public override void BuildUIAndLink()
//    {
//        UiSettingEntity = ScriptableObject.CreateInstance<UISettingsEntityDropdownInt>();
//        if (UiSettingEntity == null)
//            return;
//        UiSettingEntity.m_Description = ModLocalizationManager.CreateString($"{ModConfigurationManager.Instance?.SettingsPrefix}.feature.{Key}.description", Title);
//        UiSettingEntity.m_TooltipDescription = ModLocalizationManager.CreateString($"{ModConfigurationManager.Instance?.SettingsPrefix}.feature.{Key}.tooltip-description", Tooltip);
//        UiSettingEntity.LinkSetting(SettingEntity);
//        UiSettingEntity.SetLocalizedValues(Main.Settings?.AvailableVoices?.ToList());
//        (((IReadOnlySettingEntity<int>)SettingEntity)!).OnValueChanged += delegate(int i)
//        {
//            SelectedIndex = i;
//            TryEnable();
//        };
//    }
//}