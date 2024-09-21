/// Copyright 2024 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

using CupheadArchipelago.AP;
using CupheadArchipelago.Unity;
using HarmonyLib;

namespace CupheadArchipelago.Hooks.PlayerHooks {
    internal class LevelWeaponHook {
        internal static void Hook() {
            //Harmony.CreateAndPatchAll(typeof(rapidFireRate));
            //Harmony.CreateAndPatchAll(typeof(beginFiring));
        }

        // FIXME: Slowfire might be worked around by pressing the button repeatedly.
        private const float FASTFIRE_RATE_MULTIPLIER = 0.7f;
        private const float SLOWFIRE_RATE_MULTIPLIER = 1.3f;

        [HarmonyPatch(typeof(AbstractLevelWeapon), "rapidFireRate", MethodType.Getter)]
        internal static class rapidFireRate {
            static void Postfix(ref float __result) {
                if (APManager.Current?.IsFastFired() ?? false) {
                    __result *= FASTFIRE_RATE_MULTIPLIER;
                }
                if (APManager.Current?.IsSlowFired() ?? false) {
                    __result *= SLOWFIRE_RATE_MULTIPLIER;
                }
            }
        }

        [HarmonyPatch(typeof(AbstractLevelWeapon), "beginFiring")]
        internal static class beginFiring {
            static bool Prefix() {
                return !APData.IsCurrentSlotEnabled() || !APManager.Current.IsFingerJammed();
            }
        }
    }
}