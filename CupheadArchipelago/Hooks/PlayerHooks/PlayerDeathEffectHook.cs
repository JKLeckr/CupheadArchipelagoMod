/// Copyright 2025 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

using System;
using HarmonyLib;

namespace CupheadArchipelago.Hooks.PlayerHooks {
    internal class PlayerDeathEffectHook {
        internal static void Hook() {
            //Harmony.CreateAndPatchAll(typeof(Init));
            //Harmony.CreateAndPatchAll(typeof(float_cr));
        }

        private static PlayerDeathEffect current1 = null;
        private static PlayerDeathEffect current2 = null;

        [HarmonyPatch(typeof(PlayerDeathEffect), "Init")]
        internal static class Init {
            static bool Prefix(PlayerDeathEffect __instance, PlayerId playerId) {
                if (playerId == PlayerId.PlayerTwo) {
                    if (current2 != null && !ReferenceEquals(__instance, current2)) {
                        Logging.LogError("PlayerDeathEffect current2 Exists");
                    }
                    current2 = __instance;
                }
                else {
                    if (current1 != null && !ReferenceEquals(__instance, current1)) {
                        Logging.LogError("PlayerDeathEffect current1 Exists");
                    }
                    current1 = __instance;
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(PlayerDeathEffect), "float_cr", MethodType.Enumerator)]
        internal static class float_cr { }
        
        internal static void Relocate(PlayerId playerId) {}
    }
}
