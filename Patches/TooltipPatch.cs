using System;
using System.Collections.Generic;
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
            x.Add(new ItemKeyTooltip(LocalizationStrings.Boombox_ToolTip1, new InteractKeybindSetting(), new List<ControllerGlyphs.GlyphType>([ControllerGlyphs.GlyphType.UseItem])));
            x.Add(new ItemKeyTooltip(LocalizationStrings.Boombox_ToolTip2, new ToggleSelfieModeKeybindSetting(), new List<ControllerGlyphs.GlyphType>([ControllerGlyphs.GlyphType.SecondaryUseItem])));
            __result = x;
            return;
        }

        if (__exception != null) throw __exception;
    }
}