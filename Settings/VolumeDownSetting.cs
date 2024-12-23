using FantomLis.BoomboxExtended.Utils;
using Unity.Mathematics;
using UnityEngine;
using Zorro.Settings;

namespace FantomLis.BoomboxExtended.Settings;

[ContentWarningSetting]
public class VolumeDownSetting : KeyCodeSetting, IExposedSetting
{
    public override void ApplyValue() => LogUtils.LogDebug($"Parameter {GetDisplayName()} is set to {Value}");

    public override int GetDefaultValue() => (int) KeyCode.Minus;
    protected override KeyCode GetDefaultKey() => KeyCode.Minus;

    public SettingCategory GetSettingCategory() => SettingCategory.MouseKeyboard;

    public string GetDisplayName() => "Boombox Volume Down Key";
}