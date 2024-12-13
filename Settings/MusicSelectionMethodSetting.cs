using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UIElements;
using Zorro.Settings;

namespace FantomLis.BoomboxExtended.Settings;

public class MusicSelectionMethodSetting : EnumSetting, IExposedSetting
{
    public enum BoomboxMusicSelectionMethod : byte
    {
        SelectionUI = 1,
        ScrollWheel = 2,
        Original = 3,
        Default = 0
    }

    public SettingCategory GetSettingCategory() => SettingCategory.Mods;

    public string GetDisplayName() => "Boombox Music Selection Method";
    public override void ApplyValue() => Boombox.log.LogDebug($"Parameter {GetDisplayName()} is set to {Value}");

    public override int GetDefaultValue() => (int) BoomboxMusicSelectionMethod.Default;

    public override List<string> GetChoices() => Enum.GetNames(typeof(BoomboxMusicSelectionMethod)).ToList();
}