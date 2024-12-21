using System;
using System.Collections.Generic;
using System.Linq;
using FantomLis.BoomboxExtended.Utils;
using UnityEngine.UIElements;
using Zorro.Settings;

namespace FantomLis.BoomboxExtended.Settings;

[ContentWarningSetting]
public class MusicSelectionMethodSetting : EnumSetting, IExposedSetting
{
    public enum BoomboxMusicSelectionMethod : byte
    {
        SelectionUIScroll = 1,
        SelectionUIMouse = 4,
        ScrollWheel = 2,
        Original = 3,
        Default = 0
    }

    public List<Action<MusicSelectionMethodSetting>> UpdateValueActionList = new([((a) =>
    {
        {LogUtils.LogDebug($"Parameter {a.GetDisplayName()} is set to {a.Value}");}
    })]);

    public SettingCategory GetSettingCategory() => SettingCategory.Mods;

    public string GetDisplayName() => "Boombox Music Selection Method";
    public override void ApplyValue() => UpdateValueActionList.ForEach(x => x.Invoke(this));

    public override int GetDefaultValue() => (int) BoomboxMusicSelectionMethod.Default;

    public override List<string> GetChoices() => Enum.GetNames(typeof(BoomboxMusicSelectionMethod)).ToList().Select(x => LocalizationStrings.ResourceManager.GetString("SelectionMethod."+x) ?? "#BAD_NAME").ToList();
}