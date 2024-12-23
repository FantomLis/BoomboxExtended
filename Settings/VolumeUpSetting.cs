using FantomLis.BoomboxExtended.Utils;
using Unity.Mathematics;
using UnityEngine;
using Zorro.Settings;

namespace FantomLis.BoomboxExtended.Settings;

[ContentWarningSetting]
public class VolumeUpSetting : KeyCodeSetting, IExposedSetting
{
    public override void ApplyValue() => LogUtils.LogDebug($"Parameter {GetDisplayName()} is set to {Value}");

    public override int GetDefaultValue() => (int) KeyCode.Equals;
    protected override KeyCode GetDefaultKey() => KeyCode.Equals;

    public SettingCategory GetSettingCategory() => SettingCategory.MouseKeyboard;

    public string GetDisplayName() => "Boombox Volume Up Key";
}