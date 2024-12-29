using FantomLis.BoomboxExtended.Interfaces;
using FantomLis.BoomboxExtended.Utils;
using Unity.Mathematics;
using UnityEngine;
using Zorro.Settings;

namespace FantomLis.BoomboxExtended.Settings;

[ContentWarningSetting]
public class VolumeDownSetting : KeyCodeSetting, IDefaultSetting
{
    public override void ApplyValue() => LogUtils.LogDebug($"Parameter {GetDefaultDisplayName()} is set to {Value}");

    public override int GetDefaultValue() => (int) KeyCode.Minus;
    protected override KeyCode GetDefaultKey() => KeyCode.Minus;

    public SettingCategory GetSettingCategory() => SettingCategory.MouseKeyboard;

    public string GetDisplayName() => LocalizationStrings.Boombox_VolumeDownToolTip;
    public string GetDefaultDisplayName() => "Boombox Volume Down Key";
}