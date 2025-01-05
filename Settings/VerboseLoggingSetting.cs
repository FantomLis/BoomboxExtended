using FantomLis.BoomboxExtended.Interfaces;
using FantomLis.BoomboxExtended.Locales;
using FantomLis.BoomboxExtended.Utils;
using Zorro.Settings;

namespace FantomLis.BoomboxExtended.Settings;

[ContentWarningSetting]
public class VerboseLoggingSetting : BoolSetting, IDefaultSetting
{
    public override void ApplyValue() => LogUtils.LogDebug($"Parameter {GetDefaultDisplayName()} is set to {Value}");
    protected override bool GetDefaultValue() => false;

    public SettingCategory GetSettingCategory() => SettingCategory.Mods;

    public string GetDisplayName() => BoomboxLocalization.VerboseLoggingSetting;
    public string GetDefaultDisplayName() => "Boombox Verbose Log";
}
