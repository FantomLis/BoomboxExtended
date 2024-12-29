using System;
using FantomLis.BoomboxExtended.Entries;
using HarmonyLib;
using UnityEngine;

namespace FantomLis.BoomboxExtended.Patches;
[HarmonyPatch(typeof(ItemInstanceData))]
public class ItemInstanceDataPatch
{

    [HarmonyFinalizer]
    [HarmonyPatch(nameof(ItemInstanceData.GetEntryIdentifier))]
    static Exception GetEntryIdentifier(Exception __exception,System.Type type,ref byte __result)
    {
        if (type == typeof (MusicEntry))
            __result = 11;
        else if (type == typeof (VolumeEntry))
            __result = 12;
        else if (__exception != null) throw __exception;
        return null;
    }
    
    [HarmonyFinalizer]
    [HarmonyPatch(nameof(ItemInstanceData.GetEntryType))]
    public static Exception GetEntryType(Exception __exception,byte identifier, ref ItemDataEntry __result)
    {
        switch (identifier)
        {
            case 11:
                __result = (ItemDataEntry) new MusicEntry();break;
            case 12:
                __result = (ItemDataEntry) new VolumeEntry();break;
            default: 
                if (__exception != null) throw __exception;
                break;
        }
        return null;
    }
}