using System;
using System.Collections.Generic;
using FantomLis.BoomboxExtended.Settings;
using HarmonyLib;

namespace FantomLis.BoomboxExtended.Patches;

[HarmonyPatch(typeof(Item))]
public class TooltipPatch
{
    [HarmonyFinalizer]
    [HarmonyPatch(nameof(Item.GetLocalizedDisplayName))]
    public static void GetLocalizedDisplayName(Exception __exception, Item __instance, ref string __result)
    {
        if (__instance.name.ToLower().Trim().Equals(Boombox.ItemName.ToLower()))
        {
            __result = LocalizationStrings.Boombox;
            return;
        }

        if (__exception != null) throw __exception;
    }

    [HarmonyFinalizer]
    [HarmonyPatch(nameof(Item.GetTootipData))]
    public static void GetTooltipData(Exception __exception, Item __instance, ref IEnumerable<IHaveUIData> __result)
    {
        if (__instance.name.ToLower().Trim().Equals(Boombox.ItemName.ToLower()))
        {
            var x = new List<IHaveUIData>();
            var usePrompt = Boombox.CurrentBoomboxMethod() == MusicSelectionMethodSetting.BoomboxMusicSelectionMethod.ScrollWheel 
                ? new HardcodedPrompt(ControllerGlyphs.GetSprite(2)) : new HardcodedPrompt(ControllerGlyphs.GetSprite(1));
            var useGlyphTypes = Boombox.CurrentBoomboxMethod() == MusicSelectionMethodSetting.BoomboxMusicSelectionMethod.ScrollWheel 
                ? new List<ControllerGlyphs.GlyphType>([ControllerGlyphs.GlyphType.SecondaryUseItem]) : 
                new List<ControllerGlyphs.GlyphType>([ControllerGlyphs.GlyphType.ZoomOut,
                    ControllerGlyphs.GlyphType.ZoomIn]);
            x.Add(new ItemKeyTooltip(LocalizationStrings.Boombox_UseToolTip, 
                new HardcodedPrompt(ControllerGlyphs.GetSprite(0)), 
                new List<ControllerGlyphs.GlyphType>([ControllerGlyphs.GlyphType.UseItem])));
            x.Add(new ItemKeyTooltip(LocalizationStrings.Boombox_ClickSwitchToolTip, usePrompt, 
                useGlyphTypes));
            __result = x;
            return;
        }

        if (__exception != null) throw __exception;
    }
}