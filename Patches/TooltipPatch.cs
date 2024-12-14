using System;
using System.Collections.Generic;
using HarmonyLib;

namespace FantomLis.BoomboxExtended.Patches;

[HarmonyPatch(typeof(Item))]
public class TooltipPatch
{
    [HarmonyFinalizer]
    [HarmonyPatch(nameof(Item.GetLocalizedDisplayName))]
    public static void GetLocalizedDisplayName(Exception __exception,Item __instance, ref string __result)
    {
        if (__instance.name.ToLower().Trim().Equals(Boombox.ItemName.ToLower()))
        {
            __result = __instance.name;
        }
    }

    [HarmonyFinalizer]
    [HarmonyPatch(nameof(Item.GetTootipData))]
    public static void GetTooltipData(Exception __exception,Item __instance, ref IEnumerable<IHaveUIData> __result)
    {
        if (__instance.name.ToLower().Trim().Equals(Boombox.ItemName.ToLower()))
        {
            var x = new List<IHaveUIData>();
            x.Add(new ItemKeyTooltip(__instance.name, new InteractKeybindSetting(), new List<ControllerGlyphs.GlyphType>([ControllerGlyphs.GlyphType.UseItem])));
            __result = x;
        }
        
        if (__instance.name.ToLower().Trim().Equals("Boombox_ToolTips".ToLower()))
        {
            var x = new List<IHaveUIData>();
            x.Add(new ItemKeyTooltip(__instance.name, new InteractKeybindSetting(), new List<ControllerGlyphs.GlyphType>([ControllerGlyphs.GlyphType.UseItem])));
            __result = x;
        }
        
    }
}