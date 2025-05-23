/// Copyright 2025 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

using CupheadArchipelago.AP;
using HarmonyLib;

namespace CupheadArchipelago.Hooks.PlayerHooks.LevelPlayerHooks {
    internal class LevelPlayerWeaponManagerHook {
        internal static void Hook() {
            Harmony.CreateAndPatchAll(typeof(StartEx));
        }

        // TODO: Handle weapon firing for when basic is not available

        //private static PlayerData.PlayerLoadouts loadouts;

        [HarmonyPatch(typeof(LevelPlayerWeaponManager), "HandleWeaponFiring")]
        internal static class HandleWeaponFiring {
            static bool Prefix(LevelPlayerWeaponManager __instance) => 
                PlayerData.Data.Loadouts.GetPlayerLoadout(__instance.player.id).primaryWeapon != Weapon.None;
        }

        [HarmonyPatch(typeof(LevelPlayerWeaponManager), "StartEx")]
        internal static class StartEx {
            static bool Prefix(LevelPlayerWeaponManager __instance) {
                uint exBit = (uint)(__instance.player.stats.isChalice ? WeaponParts.CEx : WeaponParts.Ex);
                return !APData.IsCurrentSlotEnabled() || APClient.APSessionGSPlayerData.WeaponHasBit(__instance.CurrentWeapon.id, exBit);
            }
        }
    }
}
