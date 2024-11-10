/// Copyright 2024 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using CupheadArchipelago.AP;
using HarmonyLib;

namespace CupheadArchipelago.Hooks.PlayerHooks {
    internal class PlanePlayerWeaponManagerHook {
        internal static void Hook() {
            Harmony.CreateAndPatchAll(typeof(Start));
            Harmony.CreateAndPatchAll(typeof(CheckBasic));
            Harmony.CreateAndPatchAll(typeof(StartBasic));
            Harmony.CreateAndPatchAll(typeof(CheckEx));
            Harmony.CreateAndPatchAll(typeof(HandleWeaponSwitch));
        }

        [HarmonyPatch(typeof(PlanePlayerWeaponManager), "Start")]
        internal static class Start {
            static void Postfix(PlanePlayerWeaponManager __instance, ref Weapon ___currentWeapon) {
                if (__instance.player.stats.isChalice && !PlayerData.Data.IsUnlocked(__instance.player.id, Weapon.plane_chalice_weapon_3way) && PlayerData.Data.IsUnlocked(__instance.player.id, Weapon.plane_chalice_weapon_bomb)) {
                    ___currentWeapon = Weapon.plane_chalice_weapon_bomb;
                }
                else if (!PlayerData.Data.IsUnlocked(__instance.player.id, Weapon.plane_weapon_peashot) && PlayerData.Data.IsUnlocked(__instance.player.id, Weapon.plane_weapon_bomb)) {
                    ___currentWeapon = Weapon.plane_weapon_bomb;
                }
            }
        }

        [HarmonyPatch(typeof(PlanePlayerWeaponManager), "CheckBasic")]
        internal static class CheckBasic {
            static bool Prefix(PlanePlayerWeaponManager __instance) {
                return PlayerData.Data.IsUnlocked(__instance.player.id, Weapon.plane_weapon_peashot) || PlayerData.Data.IsUnlocked(__instance.player.id, Weapon.plane_weapon_bomb);
            }
        }

        [HarmonyPatch(typeof(PlanePlayerWeaponManager), "StartBasic")]
        internal static class StartBasic {
            static bool Prefix(PlanePlayerWeaponManager __instance) {
                return PlayerData.Data.IsUnlocked(__instance.player.id, Weapon.plane_weapon_peashot) || PlayerData.Data.IsUnlocked(__instance.player.id, Weapon.plane_weapon_bomb);
            }
        }

        [HarmonyPatch(typeof(PlanePlayerWeaponManager), "CheckEx")]
        internal static class CheckEx {
            static bool Prefix(PlanePlayerWeaponManager __instance) {
                return PlayerData.Data.IsUnlocked(__instance.player.id, Weapon.plane_weapon_peashot) || PlayerData.Data.IsUnlocked(__instance.player.id, Weapon.plane_weapon_bomb);
            }
            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
                List<CodeInstruction> codes = new(instructions);
                bool success = false;
                bool debug = false;

                MethodInfo _mi_get_SuperMeter = typeof(PlayerStatsManager).GetProperty("SuperMeter", BindingFlags.Public | BindingFlags.Instance).GetGetMethod();
                MethodInfo _mi_get_SuperMeterMax = typeof(PlayerStatsManager).GetProperty("SuperMeterMax", BindingFlags.Public | BindingFlags.Instance).GetGetMethod();

                if (debug) {
                    foreach (CodeInstruction code in codes) {
                        Logging.Log($"{code.opcode}: {code.operand}");
                    }
                }
                for (int i=0;i<codes.Count-8;i++) {
                    if (codes[i].opcode == OpCodes.Ldarg_0 && codes[i+1].opcode == OpCodes.Call && codes[i+2].opcode == OpCodes.Callvirt &&
                        codes[i+3].opcode == OpCodes.Callvirt && (MethodInfo)codes[i+3].operand == _mi_get_SuperMeter && codes[i+4].opcode == OpCodes.Ldarg_0 &&
                        codes[i+5].opcode == OpCodes.Call && codes[i+6].opcode == OpCodes.Callvirt && codes[i+7].opcode == OpCodes.Callvirt &&
                        (MethodInfo)codes[i+7].operand == _mi_get_SuperMeterMax && codes[i+8].opcode == OpCodes.Blt_Un) {
                            List<Label> orig_labels = codes[i].labels;
                            codes[i].labels = [];
                            Label lskip = (Label)codes[i+8].operand;
                            List<CodeInstruction> ncodes = [
                                CodeInstruction.Call(() => APCanSuper()),
                                new CodeInstruction(OpCodes.Brfalse, lskip),
                            ];
                            codes.InsertRange(i, ncodes);
                            codes[i].labels = orig_labels;
                            success = true;
                            break;
                    }
                }
                if (!success) throw new Exception($"{nameof(HandleWeaponSwitch)}: Patch Failed!");
                if (debug) {
                    Logging.Log("---");
                    foreach (CodeInstruction code in codes) {
                        Logging.Log($"{code.opcode}: {code.operand}");
                    }
                }

                return codes;
            }

            private static bool APCanSuper() {
                return !APData.IsCurrentSlotEnabled() || APClient.APSessionGSPlayerData.plane_super;
            }
        }

        [HarmonyPatch(typeof(PlanePlayerWeaponManager), "HandleWeaponSwitch")]
        internal static class HandleWeaponSwitch {
            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
                List<CodeInstruction> codes = new(instructions);
                bool success = false;
                bool debug = false;

                MethodInfo _mi_get_Data = typeof(PlayerData).GetProperty("Data", BindingFlags.Public | BindingFlags.Static)?.GetGetMethod();
                MethodInfo _mi_IsUnlocked = typeof(PlayerData).GetMethod(
                    "IsUnlocked", BindingFlags.Public | BindingFlags.Instance, null, new Type[] {typeof(PlayerId), typeof(Weapon)}, null);
                MethodInfo _mi_IsInSwitchableState = typeof(HandleWeaponSwitch).GetMethod("IsInSwitchableState", BindingFlags.NonPublic | BindingFlags.Static);

                if (debug) {
                    foreach (CodeInstruction code in codes) {
                        Logging.Log($"{code.opcode}: {code.operand}");
                    }
                }
                for (int i=0;i<codes.Count-8;i++) {
                    if (codes[i].opcode == OpCodes.Call && (MethodInfo)codes[i].operand == _mi_get_Data && codes[i+1].opcode == OpCodes.Ldarg_0 &&
                        codes[i+2].opcode == OpCodes.Call && codes[i+3].opcode == OpCodes.Callvirt && codes[i+4].opcode == OpCodes.Ldc_I4 &&
                        (int)codes[i+4].operand == (int)Weapon.plane_weapon_bomb && codes[i+5].opcode == OpCodes.Callvirt &&
                        (MethodInfo)codes[i+5].operand == _mi_IsUnlocked && codes[i+6].opcode == OpCodes.Brtrue && codes[i+7].opcode == OpCodes.Call &&
                        codes[i+8].opcode == OpCodes.Brtrue) {
                            List<Label> orig_labels = codes[i].labels;
                            codes.RemoveRange(i, 8);
                            List<CodeInstruction> ncodes = [
                                new CodeInstruction(OpCodes.Ldarg_0),
                                new CodeInstruction(OpCodes.Call, _mi_IsInSwitchableState)
                            ];
                            codes.InsertRange(i, ncodes);
                            codes[i].labels = orig_labels;
                            success = true;
                            break;
                    }
                }
                if (!success) throw new Exception($"{nameof(HandleWeaponSwitch)}: Patch Failed!");
                if (debug) {
                    foreach (CodeInstruction code in codes) {
                        Logging.Log("---");
                        Logging.Log($"{code.opcode}: {code.operand}");
                    }
                }

                return codes;
            }

            private static bool IsInSwitchableState(PlanePlayerWeaponManager instance) {
                PlayerId player = instance.player.id;
                return 
                    (PlayerData.Data.IsUnlocked(player, Weapon.plane_weapon_peashot) &&
                    PlayerData.Data.IsUnlocked(player, Weapon.plane_weapon_bomb)) ||
                    (!APData.IsCurrentSlotEnabled() && Level.IsTowerOfPower);
            }
        }
    }
}
