using Unity.Mathematics;
using UnityEngine;
using Zorro.Settings;

namespace FantomLis.BoomboxExtended.Settings;

[ContentWarningSetting]
public class BoomboxPriceSetting : IntSetting, IExposedSetting
{
    public override void ApplyValue() => Boombox.log.LogDebug($"Parameter {GetDisplayName()} is set to {Value}");
    public override GameObject GetSettingUICell() // huh?
    {
        throw new System.NotImplementedException();
    }

    protected override int GetDefaultValue() => 100;
    public SettingCategory GetSettingCategory() => SettingCategory.Mods;
    public string GetDisplayName() => "Battery Capacity (-1 to infinite)";
}