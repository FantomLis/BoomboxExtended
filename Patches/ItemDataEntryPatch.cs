using System;
using HarmonyLib;

namespace FantomLis.BoomboxExtended.Patches;
[HarmonyPatch(typeof(ItemInstanceData))]
public class ItemDataEntryPatch
{
    [HarmonyPrefix]
    [HarmonyPatch(nameof(ItemInstanceData.GetEntryIdentifier))]
    public static void GetEntryIdentifier(System.Type type, ref byte __result)
    {
        if (type == typeof (BatteryEntry))
            __result = 1;
        else if (type == typeof (OnOffEntry))
            __result = 2;
        else if (type == typeof (VideoInfoEntry))
            __result = 3;
        else if (type == typeof (FlashcardEntry))
            __result = 4;
        else if (type == typeof (LifeTimeEntry))
            __result = 5;
        else if (type == typeof (StashAbleEntry))
            __result = 6;
        else if (type == typeof (IntRangeEntry))
            __result = 7;
        else if (type == typeof (TimeEntry))
            __result = 8;
        else if (type == typeof (IntEntry))
            __result = 9;
        else if (type == typeof (TitleCardDataEntry))
            __result = 10;
        else if (type == typeof (MusicEntry))
            __result = 11;
        else if (type == typeof (VolumeEntry))
            __result = 12;
        else throw new Exception(string.Format("Unknown entry type: {0}", (object) type));
    }
    
    [HarmonyPrefix]
    [HarmonyPatch(nameof(ItemInstanceData.GetEntryType))]
    public static void GetEntryType(byte identifier, ref ItemDataEntry __result)
    {
        switch (identifier)
        {
            case 1:
                __result = (ItemDataEntry) new BatteryEntry();
                break;
            case 2:
                __result = (ItemDataEntry) new OnOffEntry();
                break;
            case 3:
                __result = (ItemDataEntry) new VideoInfoEntry();
                break;
            case 4:
                __result = (ItemDataEntry) new FlashcardEntry();
                break;
            case 5:
                __result = (ItemDataEntry) new LifeTimeEntry();break;
            case 6:
                __result = (ItemDataEntry) new StashAbleEntry();break;
            case 7:
                __result = (ItemDataEntry) new IntRangeEntry();break;
            case 8:
                __result = (ItemDataEntry) new TimeEntry();break;
            case 9:
                __result = (ItemDataEntry) new IntEntry();break;
            case 10:
                __result = (ItemDataEntry) new TitleCardDataEntry();break;
            case 11:
                __result = (ItemDataEntry) new MusicEntry();break;
            case 12:
                __result = (ItemDataEntry) new VolumeEntry();break;
            default:
                throw new Exception(string.Format("Unknown entry identifier: {0}", (object) identifier));
        }
    }
}