using FantomLis.BoomboxExtended.Interfaces;
using FantomLis.BoomboxExtended.Utils;
using Unity.Mathematics;
using UnityEngine;
using Zorro.Settings;

namespace FantomLis.BoomboxExtended.Settings;

[ContentWarningSetting]
public class BoomboxPriceSetting : IntSetting, IDefaultSetting
{
    public override void ApplyValue() => LogUtils.LogDebug($"Parameter {GetDefaultDisplayName()} is set to {Value}");

    public override int GetDefaultValue() => 100;
    public SettingCategory GetSettingCategory() => SettingCategory.Mods;
    public string GetDisplayName() => LocalizationStrings.Boombox_BoomboxPriceSetting;
    public string GetDefaultDisplayName() => "Boombox Price";
}