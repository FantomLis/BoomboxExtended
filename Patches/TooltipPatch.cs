﻿using System;
using System.Collections.Generic;
using FantomLis.BoomboxExtended.Locales;
using FantomLis.BoomboxExtended.Settings;
using HarmonyLib;
using UnityEngine;

namespace FantomLis.BoomboxExtended.Patches;

[HarmonyPatch(typeof(Item))]
// Patch to fix localized display name and tooltips for item
internal class TooltipPatch
{
    [HarmonyFinalizer]
    [HarmonyPatch(nameof(Item.GetLocalizedDisplayName))]
    public static void GetLocalizedDisplayName(Exception __exception, Item __instance, ref string __result)
    {
        if (__instance.name.ToLower().Trim().Equals(Boombox.ItemName.ToLower()))
        {
            __result = BoomboxLocalization.BoomboxName;
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
            var useText = Boombox.CurrentBoomboxMethod() switch
            {
                MusicSelectionMethodSetting.BoomboxMusicSelectionMethod.SelectionUIScroll => BoomboxLocalization.UISwitchToolTip,
                MusicSelectionMethodSetting.BoomboxMusicSelectionMethod.Default => BoomboxLocalization.UISwitchToolTip,
                MusicSelectionMethodSetting.BoomboxMusicSelectionMethod.SelectionUIMouse => BoomboxLocalization.UISwitchToolTip, 
                MusicSelectionMethodSetting.BoomboxMusicSelectionMethod.ScrollWheel => BoomboxLocalization.ScrollSwitchToolTip,
                MusicSelectionMethodSetting.BoomboxMusicSelectionMethod.Original => BoomboxLocalization.ClickSwitchToolTip,
                _ => string.Empty
            };
            x.Add(new ItemKeyTooltip(BoomboxLocalization.UseToolTip, 
                new HardcodedPrompt(ControllerGlyphs.GetSprite(0)), 
                new List<ControllerGlyphs.GlyphType>([ControllerGlyphs.GlyphType.UseItem])));
            x.Add(new ItemKeyTooltip(useText, usePrompt, 
                useGlyphTypes));
            x.Add(new ItemKeyTooltip(BoomboxLocalization.VolumeUpToolTip, new HardcodedPrompt("["+((KeyCode)Boombox.VolumeUpKey.Value).ToString()+"]"), 
                new List<ControllerGlyphs.GlyphType>([ControllerGlyphs.GlyphType.Interact])));
            x.Add(new ItemKeyTooltip(BoomboxLocalization.VolumeDownToolTip, new HardcodedPrompt(("["+(KeyCode)Boombox.VolumeDownKey.Value).ToString()+"]"), 
                new List<ControllerGlyphs.GlyphType>([ControllerGlyphs.GlyphType.Interact])));
            __result = x;
            return;
        }

        if (__exception != null) throw __exception;
    }
}