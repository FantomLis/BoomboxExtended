using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using FantomLis.BoomboxExtended.Interfaces;
using FantomLis.BoomboxExtended.Locales;
using FantomLis.BoomboxExtended.Utils;
using UnityEngine.Localization.Settings;
using Zorro.Settings;

namespace FantomLis.BoomboxExtended.Settings;

[ContentWarningSetting]
public class MusicSelectionMethodSetting : EnumSetting, IDefaultSetting
{
    public enum BoomboxMusicSelectionMethod
    {
        SelectionUIScroll,
        SelectionUIMouse,
        ScrollWheel,
        Original,
        Default
    }

    public List<Action<MusicSelectionMethodSetting>> UpdateValueActionList = new([((a) =>
    {
        {LogUtils.LogDebug($"Parameter {a.GetDefaultDisplayName()} is set to {a.Value}");}
    })]);

    public SettingCategory GetSettingCategory() => SettingCategory.Mods;

    public string GetDisplayName() => BoomboxLocalization.MusicSelectionMethodSetting;
    public string GetDefaultDisplayName() => "Boombox Music Selection Method";
    public override void ApplyValue() => UpdateValueActionList.ForEach(x => x.Invoke(this));

    public override int GetDefaultValue() => (int) BoomboxMusicSelectionMethod.Default;

    public override List<string> GetChoices() => Enum.GetNames(
        typeof(BoomboxMusicSelectionMethod)).ToList()
        .Select(x => BoomboxLocalization.ResourceManager.GetString("SelectionMethod_"+x, CultureInfo.GetCultureInfo(LocalizationSettings.SelectedLocale.Identifier.CultureInfo.LCID)) 
                     ?? string.Format(BoomboxLocalization.NoLocalizationStringError, x)).ToList();
}