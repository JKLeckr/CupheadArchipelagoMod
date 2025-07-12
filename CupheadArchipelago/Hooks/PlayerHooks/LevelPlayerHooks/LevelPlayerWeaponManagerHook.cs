/// Copyright 2025 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

using CupheadArchipelago.AP;
using HarmonyLib;

namespace CupheadArchipelago.Hooks.PlayerHooks.LevelPlayerHooks {
    internal class LevelPlayerWeaponManagerHook {
        internal static void Hook() {
            Harmony.CreateAndPatchAll(typeof(StartBasic));
            Harmony.CreateAndPatchAll(typeof(StartEx));
        }

        //private static PlayerData.PlayerLoadouts loadouts;

        [HarmonyPatch(typeof(LevelPlayerWeaponManager), "HandleWeaponFiring")]
        internal static class HandleWeaponFiring {
            static bool Prefix(LevelPlayerWeaponManager __instance) => 
                PlayerData.Data.Loadouts.GetPlayerLoadout(__instance.player.id).primaryWeapon != Weapon.None;
        }

        [HarmonyPatch(typeof(LevelPlayerWeaponManager), "StartBasic")]
        internal static class StartBasic {
            static bool Prefix(LevelPlayerWeaponManager __instance) {
                if (!APData.IsCurrentSlotEnabled()) return true;
                uint basicBit = (uint)(__instance.player.stats.isChalice ? WeaponParts.CBasic : WeaponParts.Basic);
                //Logging.Log($"weapon bit: {basicBit} has: {APClient.APSessionGSPlayerData.WeaponHasBit(__instance.CurrentWeapon.id, basicBit)}");
                return APClient.APSessionGSPlayerData.WeaponHasBit(__instance.CurrentWeapon.id, basicBit);
            }
        }

        [HarmonyPatch(typeof(LevelPlayerWeaponManager), "StartEx")]
        internal static class StartEx {
            static bool Prefix(LevelPlayerWeaponManager __instance) {
                if (!APData.IsCurrentSlotEnabled()) return true;
                uint exBit = (uint)(__instance.player.stats.isChalice ? WeaponParts.CEx : WeaponParts.Ex);
                return APClient.APSessionGSPlayerData.WeaponHasBit(__instance.CurrentWeapon.id, exBit);
            }
        }
    }
}
