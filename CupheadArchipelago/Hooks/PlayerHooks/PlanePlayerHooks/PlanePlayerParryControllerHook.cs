/// Copyright 2025-2026 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

using CupheadArchipelago.AP;
using HarmonyLib;

namespace CupheadArchipelago.Hooks.PlayerHooks.PlanePlayerHooks {
    internal class PlanePlayerParryControllerHook {
        internal static void Hook() {
            Harmony.CreateAndPatchAll(typeof(UpdateReady));
        }

        [HarmonyPatch(typeof(PlanePlayerParryController), "UpdateReady")]
        internal static class UpdateReady {
            static bool Prefix() {
                return !APData.IsCurrentSlotEnabled() || APData.CurrentSData.playerData.plane_parry;
            }
        }
    }
}
