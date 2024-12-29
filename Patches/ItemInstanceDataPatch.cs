using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using FantomLis.BoomboxExtended.Entries;
using HarmonyLib;

namespace FantomLis.BoomboxExtended.Patches;
[HarmonyPatch(typeof(ItemInstanceData))]
public class ItemInstanceDataPatch
{

    [HarmonyFinalizer]
    [HarmonyPatch(nameof(ItemInstanceData.GetEntryIdentifier))]
    static Exception GetEntryIdentifier(Exception __exception,System.Type type,ref byte __result)
    {
        List<Type> BaseEntries = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(assembly => assembly.GetTypes())
            .Where(type => type.IsSubclassOf(typeof(BaseEntry))).ToList();
        if (BaseEntries.Contains(type)) __result = ((BaseEntry)Activator.CreateInstance(type)).ID();
        else if (__exception != null) throw __exception;
        return null;
    }
    
    [HarmonyFinalizer]
    [HarmonyPatch(nameof(ItemInstanceData.GetEntryType))]
    public static Exception GetEntryType(Exception __exception,byte identifier, ref ItemDataEntry __result)
    {
        List<BaseEntry> BaseEntriesInstance = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(assembly => assembly.GetTypes())
            .Where(type => type.IsSubclassOf(typeof(BaseEntry))).Select(x => (BaseEntry) Activator.CreateInstance(x)).ToList();
        if (BaseEntriesInstance.Exists(x => x.ID() == identifier))
            __result = BaseEntriesInstance.Find(x => x.ID() == identifier);
        if (__exception != null) throw __exception;
        return null;
    }
}