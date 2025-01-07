using FantomLis.BoomboxExtended.Interfaces;
using FantomLis.BoomboxExtended.Locales;
using FantomLis.BoomboxExtended.Utils;
using Zorro.Settings;

namespace FantomLis.BoomboxExtended.Settings;

[ContentWarningSetting]
public class PauseMusicSetting : BoolSetting, IDefaultSetting
{
    public override void ApplyValue() => LogUtils.LogDebug($"Parameter {GetDefaultDisplayName()} is set to {Value}");
    protected override bool GetDefaultValue() => true;

    public SettingCategory GetSettingCategory() => SettingCategory.Mods;

    public string GetDisplayName() => BoomboxLocalization.PauseMusicSetting;
    public string GetDefaultDisplayName() => "Pause Music instead of stopping";
}
