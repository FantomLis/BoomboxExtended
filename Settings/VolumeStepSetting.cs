using FantomLis.BoomboxExtended.Interfaces;
using FantomLis.BoomboxExtended.Locales;
using FantomLis.BoomboxExtended.Utils;
using Unity.Mathematics;
using Zorro.Settings;

namespace FantomLis.BoomboxExtended.Settings;

[ContentWarningSetting]
public class VolumeStepSetting : FloatSetting, IDefaultSetting
{
    public override void ApplyValue() => LogUtils.LogDebug($"Parameter {GetDefaultDisplayName()} is set to {Value}");
    protected override float GetDefaultValue() => 10;
    public static float DefaultValue() => 10;
    protected override float2 GetMinMaxValue() => new float2(1, 100);
    public SettingCategory GetSettingCategory() => SettingCategory.Mods;
    public string GetDisplayName() => BoomboxLocalization.VolumeStepSetting;
    public string GetDefaultDisplayName() => "Volume change step";
}