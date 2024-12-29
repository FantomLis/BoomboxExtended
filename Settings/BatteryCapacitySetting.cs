using FantomLis.BoomboxExtended.Interfaces;
using FantomLis.BoomboxExtended.Utils;
using Unity.Mathematics;
using Zorro.Settings;

namespace FantomLis.BoomboxExtended.Settings;

[ContentWarningSetting]
public class BatteryCapacitySetting : FloatSetting, IDefaultSetting
{
    public override void ApplyValue() => LogUtils.LogDebug($"Parameter {GetDefaultDisplayName()} is set to {Value}");
    protected override float GetDefaultValue() => 250f;
    public static float DefaultValue() => 250f;
    protected override float2 GetMinMaxValue() => new float2(-1, 1000);
    public SettingCategory GetSettingCategory() => SettingCategory.Mods;
    public string GetDisplayName() => BoomboxLocalization.BatteryCapacitySetting;
    public string GetDefaultDisplayName() => "Battery Capacity (-1 to infinite)";
}