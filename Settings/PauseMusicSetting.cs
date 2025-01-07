using FantomLis.BoomboxExtended.Containers;
using FantomLis.BoomboxExtended.Interfaces;
using FantomLis.BoomboxExtended.Locales;
using FantomLis.BoomboxExtended.Utils;
using MyceliumNetworking;
using UnityEngine;
using Zorro.Settings;

namespace FantomLis.BoomboxExtended.Settings;

[ContentWarningSetting]
public class PauseMusicSetting : BoolSetting, IDefaultSetting
{
    private DelayedModalContainer? _c;
    private float lastShown;
    public override void ApplyValue()
    {
        LogUtils.LogDebug($"Parameter {GetDefaultDisplayName()} is set to {Value}");
        if (!MyceliumNetwork.InLobby) return;
        if (_c is {isShowed:false} || lastShown + 30f > Time.time) return;
        if (_c is {
                isShowed: false
            }) _c.Cancel();
        _c = new DelayedModalContainer(BoomboxLocalization.ChangesApplyAfterRestartTitle, 
            string.Format(BoomboxLocalization.ChangesApplyAfterRestartDesc, GetDisplayName()),
            0.5f, [new ModalOption("OK")]);
        AlertUtils.DelayedModal(_c);
        lastShown = Time.time;
    }

    protected override bool GetDefaultValue() => true;

    public SettingCategory GetSettingCategory() => SettingCategory.Mods;

    public string GetDisplayName() => BoomboxLocalization.PauseMusicSetting;
    public string GetDefaultDisplayName() => "Pause Music instead of stopping";
}
