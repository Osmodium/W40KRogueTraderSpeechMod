using ModConfiguration.Localization;
using Kingmaker;
using Kingmaker.Settings;
using Kingmaker.Settings.Entities;
using Kingmaker.UI.Models.SettingsUI.SettingAssets;
using System;
using UnityEngine;

namespace ModConfiguration.Settings;

public abstract class ModHotkeySettingEntry : ModSettingEntry
{
    public readonly SettingsEntityKeyBindingPair SettingEntity;
    public UISettingsEntityKeyBinding UiSettingEntity { get; private set; }

    public static bool ReSavingRequired { get; private set; } = false;

    public ModHotkeySettingEntry(string key, string title, string tooltip, string DefaultKeyPairString)
        : base(key, title, tooltip)
    {
        SettingEntity = new(SettingsController.Instance, $"{PREFIX}.newcontrols.{Key}", new(DefaultKeyPairString), false, true);
    }

    public override UISettingsEntityBase GetUISettings() => UiSettingEntity;

    public string GetBindName() => $"{PREFIX}.newcontrols.ui.{Key}";
    public override void BuildUIAndLink()
    {
        UiSettingEntity = MakeKeyBind();
        UiSettingEntity.LinkSetting(SettingEntity);
        (SettingEntity as IReadOnlySettingEntity<KeyBindingPair>).OnValueChanged += delegate
        {
            TryEnable();
        };
    }

    private UISettingsEntityKeyBinding MakeKeyBind()
    {
        var keyBindSetting = ScriptableObject.CreateInstance<UISettingsEntityKeyBinding>();
        keyBindSetting.m_Description = ModLocalizationManager.CreateString($"{PREFIX}.feature.{Key}.description", Title);
        keyBindSetting.m_TooltipDescription = ModLocalizationManager.CreateString($"{PREFIX}.feature.{Key}.tooltip-description", Tooltip);
        keyBindSetting.name = $"{PREFIX}.newcontrols.ui.{Key}";
        keyBindSetting.m_EncyclopediaDescription = new();
        return keyBindSetting;
    }

    protected void RegisterKeybind()
    {
        if (Status != SettingStatus.NOT_APPLIED) return;

        var currentValue = SettingEntity.GetValue();

        if (currentValue.Binding1.Key != UnityEngine.KeyCode.None)
        {
            Game.Instance.Keyboard.RegisterBinding(
                GetBindName(),
                currentValue.Binding1,
                currentValue.GameModesGroup,
                currentValue.TriggerOnHold);
            Main.log.Log($"{Title} binding 1 registered: {currentValue.Binding1}");
        }
        else
        {
            Main.log.Log($"{Title} binding 1 empty");
        }

        if (currentValue.Binding2.Key != UnityEngine.KeyCode.None)
        {
            Game.Instance.Keyboard.RegisterBinding(
                GetBindName(),
                currentValue.Binding2,
                currentValue.GameModesGroup,
                currentValue.TriggerOnHold);
            Main.log.Log($"{Title} binding 2 registered: {currentValue.Binding2}");
        }
        else
        {
            Main.log.Log($"{Title} binding 2 empty");
        }
    }

    protected SettingStatus TryEnableAndPatch(Type type)
    {
        TryFix();
        if (Status != SettingStatus.NOT_APPLIED) return Status;
        RegisterKeybind();
        var currentValue = SettingEntity.GetValue();
        if (currentValue.Binding1.Key != UnityEngine.KeyCode.None || currentValue.Binding2.Key != UnityEngine.KeyCode.None)
        {
            return TryPatchInternal(type);
        }
        else
        {
            Main.log.Log($"{Title} binding 1 and binding 2 empty, setting integration skipped");
        }
        return SettingStatus.NOT_APPLIED;
    }

    /// <summary>
    /// If hotkey group or trigger changes, those values need to be updated manually
    /// and later saved
    /// </summary>
    private void TryFix()
    {
        var curValue = SettingEntity.GetValue();
        var defaultGroup = SettingEntity.DefaultValue.GameModesGroup;
        var defaultTrigger = SettingEntity.DefaultValue.TriggerOnHold;
        if (curValue.GameModesGroup != defaultGroup || curValue.TriggerOnHold != defaultTrigger)
        {
            curValue.GameModesGroup = defaultGroup;
            curValue.TriggerOnHold = defaultTrigger;
            SettingEntity.SetValueAndConfirm(curValue);
            ReSavingRequired = true;
            Main.log.Log($"{Title} had outdated hotkey settings, migrated.");
        }
    }
}
