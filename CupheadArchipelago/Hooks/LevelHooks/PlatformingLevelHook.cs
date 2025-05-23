/// Copyright 2025 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

using CupheadArchipelago.Util;
using HarmonyLib;

namespace CupheadArchipelago.Hooks.LevelHooks {
    internal class PlatformingLevelHook {
        internal static void Hook() {
            //Harmony.CreateAndPatchAll(typeof(Start));
        }

        [HarmonyPatch(typeof(PlatformingLevel), "Start")]
        internal static class Start {
            static void Postfix(PlatformingLevel __instance) {
                Logging.Log("[PlatformingLevel] Coins: "+Aux.CollectionToString(__instance.LevelCoinsIDs));
            }
        }
    }
}
