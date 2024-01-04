using ModConfiguration.Localization;
using Kingmaker.Settings;
using Kingmaker.Settings.Entities;
using Kingmaker.UI.Models.SettingsUI.SettingAssets;
using System;
using UnityEngine;

namespace ModConfiguration.Settings;

public abstract class ModToggleSettingEntry : ModSettingEntry
{
    public readonly SettingsEntityBool SettingEntity;
    public UISettingsEntityBool UiSettingEntity { get; private set; }

    public ModToggleSettingEntry(string key, string title, string tooltip, bool defaultValue)
        : base(key, title, tooltip)
    {
        SettingEntity = new(SettingsController.Instance, $"{PREFIX}.newcontrols.{Key}", defaultValue, false, true);
    }

    public override UISettingsEntityBase GetUISettings() => UiSettingEntity;

    public override void BuildUIAndLink()
    {
        UiSettingEntity = ScriptableObject.CreateInstance<UISettingsEntityBool>();
        UiSettingEntity.m_Description = ModLocalizationManager.CreateString($"{PREFIX}.feature.{Key}.description", Title);
        UiSettingEntity.m_TooltipDescription = ModLocalizationManager.CreateString($"{PREFIX}.feature.{Key}.tooltip-description", Tooltip);
        UiSettingEntity.DefaultValue = false;
        UiSettingEntity.LinkSetting(SettingEntity);
        (SettingEntity as IReadOnlySettingEntity<bool>).OnValueChanged += delegate
        {
            TryEnable();
        };
    }

    protected SettingStatus TryEnableAndPatch(Type type)
    {
        var currentValue = SettingEntity.GetValue();
        if (currentValue)
        {
            return TryPatchInternal(type);
        }
        else
        {
            Main.log.Log($"{Title} disabled, setting integration skipped");
        }
        return SettingStatus.NOT_APPLIED;
    }
}
