/// Copyright 2025 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

using HarmonyLib;

namespace CupheadArchipelago.Hooks.PlayerHooks.LevelPlayerHooks {
    internal class LevelPlayerWeaponManagerHook {
        internal static void Hook() {}

        //private static PlayerData.PlayerLoadouts loadouts;

        [HarmonyPatch(typeof(LevelPlayerWeaponManager), "HandleWeaponFiring")]
        internal static class HandleWeaponFiring {
            static bool Prefix(LevelPlayerWeaponManager __instance) => 
                PlayerData.Data.Loadouts.GetPlayerLoadout(__instance.player.id).primaryWeapon != Weapon.None;
        }
    }
}
