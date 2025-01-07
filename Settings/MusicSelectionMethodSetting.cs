using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using FantomLis.BoomboxExtended.Interfaces;
using FantomLis.BoomboxExtended.Locales;
using FantomLis.BoomboxExtended.Utils;
using MyceliumNetworking;
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

    public SettingCategory GetSettingCategory() => SettingCategory.Mods;

    public string GetDisplayName() => BoomboxLocalization.MusicSelectionMethodSetting;
    public string GetDefaultDisplayName() => "Boombox Music Selection Method";

    public override void ApplyValue()
    {
        LogUtils.LogDebug($"Parameter {GetDefaultDisplayName()} is set to {Value}");
        if (!MyceliumNetwork.InLobby) return;
        if (Player.localPlayer.TryGetInventory(out var o))
        {
            var x = o.GetItems().Find(x => x.item.Equals(Boombox.BoomboxItem));
            UserInterface.Instance.equippedUI.SetData(ItemDescriptor.Empty); // Clear Equipped UI before redrawing boombox Tooltips
            UserInterface.Instance.equippedUI.SetData(x);                    // because of itemDescriptor.data != this.m_lastItemDescriptor.data
        }
    }
    public override int GetDefaultValue() => (int) BoomboxMusicSelectionMethod.Default;

    public override List<string> GetChoices() => Enum.GetNames(
        typeof(BoomboxMusicSelectionMethod)).ToList()
        .Select(x => BoomboxLocalization.ResourceManager.GetString("SelectionMethod_"+x, CultureInfo.GetCultureInfo(LocalizationSettings.SelectedLocale.Identifier.CultureInfo.LCID)) 
                     ?? string.Format(BoomboxLocalization.NoLocalizationStringError, x)).ToList();
}