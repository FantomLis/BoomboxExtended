using Unity.Mathematics;
using UnityEngine;
using Zorro.Settings;

namespace FantomLis.BoomboxExtended.Settings;

[ContentWarningSetting]
public class VolumeDownSetting : KeyCodeSetting, IExposedSetting
{
    public override void ApplyValue() => Boombox.log.LogDebug($"Parameter {GetDisplayName()} is set to {Value}");

    protected override int GetDefaultValue() => (int) KeyCode.Minus;
    protected override KeyCode GetDefaultKey() => KeyCode.Minus;

    public SettingCategory GetSettingCategory() => SettingCategory.Controls;

    public string GetDisplayName() => "Boombox volume down key";
}