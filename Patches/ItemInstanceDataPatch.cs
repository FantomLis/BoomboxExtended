using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using FantomLis.BoomboxExtended.Entries;
using HarmonyLib;
using UnityEngine;

namespace FantomLis.BoomboxExtended.Patches;
[HarmonyPatch(typeof(ItemInstanceData))]
public class ItemInstanceDataPatch
{
    private static byte __current_index = 40; // 
    static List<Type> BaseEntries = AppDomain.CurrentDomain.GetAssemblies()
        .SelectMany(assembly => assembly.GetTypes())
        .Where(type => type.IsSubclassOf(typeof(BaseEntry))).ToList();

    [HarmonyFinalizer]
    [HarmonyPatch(nameof(ItemInstanceData.GetEntryIdentifier))]
    static Exception GetEntryIdentifier(Exception __exception,System.Type type,ref byte __result)
    {
        if (BaseEntries.Contains(type)) __result = ((byte) (__current_index + BaseEntries.IndexOf(type)));
        else if (__exception != null) throw __exception;
        return null;
    }
    
    [HarmonyFinalizer]
    [HarmonyPatch(nameof(ItemInstanceData.GetEntryType))]
    public static Exception GetEntryType(Exception __exception,byte identifier, ref ItemDataEntry __result)
    {
        if (BaseEntries.Count > (identifier-__current_index) && identifier >= __current_index)
            __result = (ItemDataEntry) Activator.CreateInstance(BaseEntries[identifier-__current_index]);
        else if (__exception != null) throw __exception;
        return null;
    }
}