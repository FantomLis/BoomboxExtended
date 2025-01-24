using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using FantomLis.BoomboxExtended.Interfaces;
using FantomLis.BoomboxExtended.Locales;
using FantomLis.BoomboxExtended.Utils;
using MyceliumNetworking;
using Sirenix.Utilities;
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
    internal delegate void _MusicSelectionMethodUpdated();

    internal event _MusicSelectionMethodUpdated MusicSelectionMethod_Updated;

    public SettingCategory GetSettingCategory() => SettingCategory.Mods;

    public string GetDisplayName() => BoomboxLocalization.MusicSelectionMethodSetting;
    public string GetDefaultDisplayName() => "Boombox Music Selection Method";

    public override void ApplyValue()
    {
        LogUtils.LogDebug($"Parameter {GetDefaultDisplayName()} is set to {Value}");
        if (!MyceliumNetwork.InLobby) return;
        MusicSelectionMethod_Updated?.Invoke();
    }
    public override int GetDefaultValue() => (int) BoomboxMusicSelectionMethod.Default;

    public override List<string> GetChoices() => Enum.GetNames(
        typeof(BoomboxMusicSelectionMethod)).ToList()
        .Select(x => BoomboxLocalization.ResourceManager.GetString("SelectionMethod_"+x, CultureInfo.GetCultureInfo(LocalizationSettings.SelectedLocale.Identifier.CultureInfo.LCID)) 
                     ?? string.Format(BoomboxLocalization.NoLocalizationStringError, x)).ToList();
}