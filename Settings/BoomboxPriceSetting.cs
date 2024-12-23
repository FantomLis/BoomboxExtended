using FantomLis.BoomboxExtended.Utils;
using Unity.Mathematics;
using UnityEngine;
using Zorro.Settings;

namespace FantomLis.BoomboxExtended.Settings;

[ContentWarningSetting]
public class BoomboxPriceSetting : IntSetting, IExposedSetting
{
    public override void ApplyValue() => LogUtils.LogDebug($"Parameter {GetDisplayName()} is set to {Value}");

    public override int GetDefaultValue() => 100;
    public SettingCategory GetSettingCategory() => SettingCategory.Mods;
    public string GetDisplayName() => "Boombox Price";
}