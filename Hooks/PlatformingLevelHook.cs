using CupheadArchipelago.Auxiliary;
using HarmonyLib;

namespace CupheadArchipelago.Hooks {
    internal class PlatformingLevelHook {
        internal static void Hook() {
            Harmony.CreateAndPatchAll(typeof(Start));
        }

        [HarmonyPatch(typeof(PlatformingLevel), "Start")]
        internal static class Start {
            static void Postfix(PlatformingLevel __instance) {
                Plugin.Log("[PlatformingLevel] Coins: "+Aux.CollectionToString(__instance.LevelCoinsIDs));
            }
        }
    }
}