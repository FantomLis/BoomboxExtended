using FantomLis.BoomboxExtended.Interfaces;
using FantomLis.BoomboxExtended.Locales;
using FantomLis.BoomboxExtended.Utils;
using UnityEngine;
using Zorro.Settings;

namespace FantomLis.BoomboxExtended.Settings;

[ContentWarningSetting]
public class VolumeUpSetting : KeyCodeSetting, IDefaultSetting
{
    public override void ApplyValue() => LogUtils.LogDebug($"Parameter {GetDefaultDisplayName()} is set to {Value}");

    public override int GetDefaultValue() => (int) KeyCode.Equals;
    protected override KeyCode GetDefaultKey() => KeyCode.Equals;

    public SettingCategory GetSettingCategory() => SettingCategory.MouseKeyboard;

    public string GetDisplayName() => BoomboxLocalization.VolumeUpSetting;
    public string GetDefaultDisplayName() => "Boombox Volume Up Key";
}