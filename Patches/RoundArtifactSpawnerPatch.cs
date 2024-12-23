using HarmonyLib;

namespace FantomLis.BoomboxExtended.Patches;

[HarmonyPatch(typeof(RoundArtifactSpawner))]
public static class RoundArtifactSpawnerPatch
{
    [HarmonyPatch("Awake")]
    [HarmonyPostfix]
    public static void Awake()
    {
        Boombox.RegisterBoomboxAsArtifact();
    }
}