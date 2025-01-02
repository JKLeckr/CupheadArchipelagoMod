/// Copyright 2025 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

using CupheadArchipelago.AP;
using HarmonyLib;

namespace CupheadArchipelago.Hooks.PlayerHooks {
    internal class LevelPlayerAnimationControllerHook {
        internal static void Hook() {
            //Harmony.CreateAndPatchAll(typeof(Update));
        }

        [HarmonyPatch(typeof(LevelPlayerAnimationController), "Update")]
        internal static class Update {
            static bool Prefix(LevelPlayerAnimationController __instance) {
                //Logging.Log(__instance.player.motor.Ducking);
                return true;
            }
        }
    }
}
