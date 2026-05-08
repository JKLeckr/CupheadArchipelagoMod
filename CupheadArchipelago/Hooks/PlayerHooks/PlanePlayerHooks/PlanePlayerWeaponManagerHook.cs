/// Copyright 2025-2026 JKLeckr
/// SPDX-License-Identifier: GPL-3.0-or-later

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using CupheadArchipelago.AP;
using HarmonyLib;

namespace CupheadArchipelago.Hooks.PlayerHooks.PlanePlayerHooks {
    internal class PlanePlayerWeaponManagerHook {
        internal static void Hook() {
            Harmony.CreateAndPatchAll(typeof(Start));
            Harmony.CreateAndPatchAll(typeof(OnLevelStart));
            Harmony.CreateAndPatchAll(typeof(CheckBasic));
            Harmony.CreateAndPatchAll(typeof(StartBasic));
            Harmony.CreateAndPatchAll(typeof(StartEx));
            Harmony.CreateAndPatchAll(typeof(StartSuper));
            Harmony.CreateAndPatchAll(typeof(HandleWeaponSwitch));
            Harmony.CreateAndPatchAll(typeof(SwitchWeapon));
            Harmony.CreateAndPatchAll(typeof(SwitchToWeapon));
        }

        [HarmonyPatch(typeof(PlanePlayerWeaponManager), "Start")]
        internal static class Start {
            static void Postfix(PlanePlayerWeaponManager __instance, ref Weapon ___currentWeapon) {
                if (!APData.IsCurrentSlotEnabled()) return;
                if (__instance.player.stats.isChalice) {
                    if (!PlayerData.Data.IsUnlocked(__instance.player.id, Weapon.plane_chalice_weapon_3way) && PlayerData.Data.IsUnlocked(__instance.player.id, Weapon.plane_chalice_weapon_bomb)) {
                        ___currentWeapon = Weapon.plane_chalice_weapon_bomb;
                    }
                    else {
                        ___currentWeapon = Weapon.plane_chalice_weapon_3way;
                    }
                }
                else if (!PlayerData.Data.IsUnlocked(__instance.player.id, Weapon.plane_weapon_peashot) && PlayerData.Data.IsUnlocked(__instance.player.id, Weapon.plane_weapon_bomb)) {
                    ___currentWeapon = Weapon.plane_weapon_bomb;
                }
            }
        }

        [HarmonyPatch(typeof(PlanePlayerWeaponManager), "OnLevelStart")]
        internal static class OnLevelStart {
            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
                List<CodeInstruction> codes = new(instructions);
                bool success = false;
                bool debug = false;

                FieldInfo _fi_isChalice = typeof(PlayerStatsManager).GetField("isChalice", BindingFlags.Public | BindingFlags.Instance);
                FieldInfo _fi_currentWeapon = typeof(PlanePlayerWeaponManager).GetField("currentWeapon", BindingFlags.NonPublic | BindingFlags.Instance);
                MethodInfo _mi_APCondition = typeof(OnLevelStart).GetMethod("APCondition", BindingFlags.NonPublic | BindingFlags.Static);

                if (debug) {
                    Dbg.LogCodeInstructions(codes);
                }
                for (int i = 0; i < codes.Count - 5; i++) {
                    if (
                        codes[i].opcode == OpCodes.Ldfld && (FieldInfo)codes[i].operand == _fi_isChalice &&
                        codes[i + 1].opcode == OpCodes.Brfalse && codes[i + 2].opcode == OpCodes.Ldarg_0 &&
                        codes[i + 3].opcode == OpCodes.Ldc_I4 && (int)codes[i + 3].operand == (int)Weapon.plane_chalice_weapon_3way &&
                        codes[i + 4].opcode == OpCodes.Stfld && (FieldInfo)codes[i + 4].operand == _fi_currentWeapon &&
                        codes[i + 5].opcode == OpCodes.Br
                    ) {
                        codes.Insert(i + 1, new CodeInstruction(OpCodes.Call, _mi_APCondition));
                        success = true;
                        break;
                    }
                }
                if (!success) throw new Exception($"{nameof(Patch)}: Patch Failed!");
                if (debug) {
                    Logging.Log("---");
                    Dbg.LogCodeInstructions(codes);
                }

                return codes;
            }

            private static bool APCondition(bool orig) {
                return !APData.IsCurrentSlotEnabled() && orig;
            }
        }

        [HarmonyPatch(typeof(PlanePlayerWeaponManager), "CheckBasic")]
        internal static class CheckBasic {
            static bool Prefix(PlanePlayerWeaponManager __instance) {
                return !APData.IsCurrentSlotEnabled() || IsAnyWeaponAvailable(__instance);
            }
        }

        [HarmonyPatch(typeof(PlanePlayerWeaponManager), "StartBasic")]
        internal static class StartBasic {
            private static MethodInfo _mi_EndBasic = typeof(PlanePlayerWeaponManager).GetMethod("EndBasic", BindingFlags.NonPublic | BindingFlags.Instance);

            static bool Prefix(PlanePlayerWeaponManager __instance) {
                if (!APData.IsCurrentSlotEnabled()) return true;
                if (__instance.player.stats.StoneTime > 0) {
                    _mi_EndBasic.Invoke(__instance, null);
                }
                return IsAnyWeaponAvailable(__instance);
            }
        }

        [HarmonyPatch(typeof(PlanePlayerWeaponManager), "StartEx")]
        internal static class StartEx {
            static bool Prefix(PlanePlayerWeaponManager __instance) {
                if (APData.IsCurrentSlotEnabled()) {
                    if (__instance.player.stats.isChalice) {
                        return APClient.APSessionGSPlayerData.dlc_cplane_ex;
                    }
                    return APClient.APSessionGSPlayerData.plane_ex;
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(PlanePlayerWeaponManager), "StartSuper")]
        internal static class StartSuper {
            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator il) {
                List<CodeInstruction> codes = new(instructions);
                bool debug = false;

                MethodInfo _mi_StartEx = typeof(PlanePlayerWeaponManager).GetMethod("StartEx", BindingFlags.NonPublic | BindingFlags.Instance);
                MethodInfo _mi_APCondition = typeof(StartSuper).GetMethod("APCondition", BindingFlags.NonPublic | BindingFlags.Static);

                Label l_vanilla = il.DefineLabel();
                Label l_end = il.DefineLabel();

                if (debug) {
                    Dbg.LogCodeInstructions(codes);
                }
                codes[0].labels.Add(l_vanilla);
                codes[codes.Count-1].labels.Add(l_end);
                CodeInstruction[] ncodes = [
                    new CodeInstruction(OpCodes.Call, _mi_APCondition),
                    new CodeInstruction(OpCodes.Brtrue, l_vanilla),
                    new CodeInstruction(OpCodes.Ldarg_0),
                    new CodeInstruction(OpCodes.Call, _mi_StartEx),
                    new CodeInstruction(OpCodes.Br, l_end),
                ];
                codes.InsertRange(0, ncodes);
                if (debug) {
                    Logging.Log("---");
                    foreach (CodeInstruction code in codes) {
                        Logging.Log($"{code.opcode}: {code.operand}");
                    }
                }

                return codes;
            }

            private static bool APCondition() =>
                !APData.IsCurrentSlotEnabled() || APClient.APSessionGSPlayerData.plane_super;
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
                    Dbg.LogCodeInstructions(codes);
                }
                for (int i=0;i<codes.Count-8;i++) {
                    if (codes[i].opcode == OpCodes.Call && (MethodInfo)codes[i].operand == _mi_get_Data && codes[i+1].opcode == OpCodes.Ldarg_0 &&
                        codes[i+2].opcode == OpCodes.Call && codes[i+3].opcode == OpCodes.Callvirt && codes[i+4].opcode == OpCodes.Ldc_I4 &&
                        (int)codes[i+4].operand == (int)Weapon.plane_weapon_bomb && codes[i+5].opcode == OpCodes.Callvirt &&
                        (MethodInfo)codes[i+5].operand == _mi_IsUnlocked && codes[i+6].opcode == OpCodes.Brtrue && codes[i+7].opcode == OpCodes.Call &&
                        codes[i+8].opcode == OpCodes.Brtrue) {
                            List<Label> orig_labels = codes[i].labels;
                            codes.RemoveRange(i, 8);
                            CodeInstruction[] ncodes = [
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
                    Logging.Log("---");
                    foreach (CodeInstruction code in codes) {
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

        [HarmonyPatch(typeof(PlanePlayerWeaponManager), "SwitchWeapon")]
        internal static class SwitchWeapon {
            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator il) =>
                SwitchWeaponCommonTranspiler(typeof(SwitchWeapon), instructions, il);
        }

        [HarmonyPatch(typeof(PlanePlayerWeaponManager), "SwitchToWeapon")]
        internal static class SwitchToWeapon {
            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator il) =>
                SwitchWeaponCommonTranspiler(typeof(SwitchToWeapon), instructions, il);
        }

        private static bool IsAnyWeaponAvailable(PlanePlayerWeaponManager instance) {
            if (instance.player.stats.isChalice) {
                return PlayerData.Data.IsUnlocked(instance.player.id, Weapon.plane_chalice_weapon_3way) || PlayerData.Data.IsUnlocked(instance.player.id, Weapon.plane_chalice_weapon_bomb);
            } else {
                return PlayerData.Data.IsUnlocked(instance.player.id, Weapon.plane_weapon_peashot) || PlayerData.Data.IsUnlocked(instance.player.id, Weapon.plane_weapon_bomb);
            }
        }

        private static bool IsWeaponAvailable(PlanePlayerWeaponManager instance, Weapon weapon) {
            return PlayerData.Data.IsUnlocked(instance.player.id, weapon);
        }

        private static IEnumerable<CodeInstruction> SwitchWeaponCommonTranspiler(Type baseClass, IEnumerable<CodeInstruction> instructions, ILGenerator il) {
            List<CodeInstruction> codes = new(instructions);
            bool success = false;
            bool debug = false;

            MethodInfo _mi_get_input = typeof(AbstractPlayerController).GetProperty("input", BindingFlags.Public | BindingFlags.Instance)?.GetGetMethod();;
            MethodInfo _mi_IsWeaponAvailable = typeof(PlanePlayerWeaponManagerHook).GetMethod(
                "IsWeaponAvailable", BindingFlags.NonPublic | BindingFlags.Static
            );

            Label l_end = il.DefineLabel();

            codes[codes.Count-1].labels.Add(l_end);
            if (debug) {
                Dbg.LogCodeInstructions(codes);
            }
            for (int i=0;i<codes.Count-6;i++) {
                if (codes[i].opcode == OpCodes.Ldarg_0 && codes[i+1].opcode == OpCodes.Call && codes[i+2].opcode == OpCodes.Callvirt &&
                    (MethodInfo)codes[i+2].operand == _mi_get_input && codes[i+3].opcode == OpCodes.Callvirt &&
                    codes[i+4].opcode == OpCodes.Ldc_I4_3 && codes[i+5].opcode == OpCodes.Callvirt && codes[i+6].opcode == OpCodes.Brfalse) {
                        List<CodeInstruction> ncodes = [
                            new CodeInstruction(OpCodes.Ldarg_0),
                            new CodeInstruction(OpCodes.Ldarg_1),
                            new CodeInstruction(OpCodes.Call, _mi_IsWeaponAvailable),
                            new CodeInstruction(OpCodes.Brfalse, l_end),
                        ];
                        codes.InsertRange(i, ncodes);
                        success = true;
                        break;
                }
            }
            if (!success) throw new Exception($"{nameof(baseClass)}: Patch Failed!");
            if (debug) {
                Logging.Log("---");
                Dbg.LogCodeInstructions(codes);
            }

            return codes;
        }
    }
}
