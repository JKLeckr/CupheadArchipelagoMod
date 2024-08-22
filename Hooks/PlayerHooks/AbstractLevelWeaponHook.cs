/// Copyright 2024 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

using CupheadArchipelago.Unity;
using HarmonyLib;

namespace CupheadArchipelago.Hooks.PlayerHooks {
    internal class AbstractLevelWeaponHook {
        internal static void Hook() {
            Harmony.CreateAndPatchAll(typeof(rapidFireRate));
        }

        // FIXME: Slowfire might be worked around by pressing the button repeatedly.
        private const float SLOWFIRE_RATE_MULTIPLIER = 1.5f;

        [HarmonyPatch(typeof(AbstractLevelWeapon), "rapidFireRate", MethodType.Getter)]
        internal static class rapidFireRate {
            static void Postfix(ref float __result) {
                if (APManager.Current?.IsSlowFired() ?? false) {
                    __result *= SLOWFIRE_RATE_MULTIPLIER;
                }
            }
        }
    }
}