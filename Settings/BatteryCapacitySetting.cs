using Unity.Mathematics;
using Zorro.Settings;

namespace FantomLis.BoomboxExtended.Settings;

[ContentWarningSetting]
public class BatteryCapacitySetting : FloatSetting, IExposedSetting
{
    public override void ApplyValue() => Boombox.log.LogDebug($"Parameter {GetDisplayName()} is set to {Value}");
    protected override float GetDefaultValue() => 250f;
    public static float DefaultValue() => 250f;
    protected override float2 GetMinMaxValue() => new float2(-1, 1000);
    public SettingCategory GetSettingCategory() => SettingCategory.Mods;
    public string GetDisplayName() => "Battery Capacity (-1 to infinite)";
}