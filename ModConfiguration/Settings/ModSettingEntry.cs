using Kingmaker.UI.Models.SettingsUI.SettingAssets;
using System;

namespace ModConfiguration.Settings;

public abstract class ModSettingEntry
{
    public const string PREFIX = "alterasc.enhancedcontrols";

    public readonly string Key;
    public readonly string Title;
    public readonly string Tooltip;

    public SettingStatus Status { get; private set; } = SettingStatus.NOT_APPLIED;

    protected ModSettingEntry(string key, string title, string tooltip)
    {
        Key = key;
        Title = title;
        Tooltip = tooltip;
    }

    public abstract SettingStatus TryEnable();

    public abstract void BuildUIAndLink();

    public abstract UISettingsEntityBase GetUISettings();

    protected SettingStatus TryPatchInternal(params Type[] type)
    {
        if (Status != SettingStatus.NOT_APPLIED) return Status;
        try
        {
            foreach (Type t in type)
            {
                Main.HarmonyInstance.CreateClassProcessor(t).Patch();
            }
            Status = SettingStatus.WORKING;
            Main.log.Log($"{Title} patch succeeded");
        }
        catch (Exception ex)
        {
            Main.log.Error($"{Title} patch exception: {ex.Message}");
            Status = SettingStatus.ERROR;
        }
        return Status;
    }
}
