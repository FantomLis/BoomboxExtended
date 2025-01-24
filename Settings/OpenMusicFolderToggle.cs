using System.Diagnostics;
using System.IO;
using FantomLis.BoomboxExtended.Interfaces;
using FantomLis.BoomboxExtended.Locales;
using UnityEngine;
using Zorro.Settings;

namespace FantomLis.BoomboxExtended.Settings;

[ContentWarningSetting]
public class OpenMusicFolderToggle : BoolSetting, IDefaultSetting
{
    public override void ApplyValue()
    {
        if (Value == true)
        {
            Process.Start(
                Path.Combine(Application.dataPath,MusicLoadManager.RootPath, MusicLoadManager.DefaultFolder));
            Value = false;
        }
    }

    protected override bool GetDefaultValue() => false;

    public SettingCategory GetSettingCategory() => SettingCategory.Mods;

    public string GetDisplayName() => BoomboxLocalization.OpenMusicFolderButton;

    public string GetDefaultDisplayName() => "Open music folder";
}