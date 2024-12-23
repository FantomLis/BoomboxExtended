using FantomLis.BoomboxExtended.Utils;
using Zorro.Settings;

namespace FantomLis.BoomboxExtended.Settings;

[ContentWarningSetting]
public class VerboseLoggingSetting : BoolSetting, IExposedSetting
{
    public override void ApplyValue() => LogUtils.LogDebug($"Parameter {GetDisplayName()} is set to {Value}");
    protected override bool GetDefaultValue() => false;

    public SettingCategory GetSettingCategory() => SettingCategory.Mods;

    public string GetDisplayName() => "Boombox Verbose Log";
}
