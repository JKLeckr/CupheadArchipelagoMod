/// Copyright 2024 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using CupheadArchipelago.AP;
using HarmonyLib;

namespace CupheadArchipelago.Hooks.PlayerHooks {
    internal class PlayerStatsManagerHook {
        internal static void Hook() {
            Harmony.CreateAndPatchAll(typeof(OnAwake));
            Harmony.CreateAndPatchAll(typeof(CalculateHealthMax));
            Harmony.CreateAndPatchAll(typeof(DebugFillSuper));
        }

        public const float DEFAULT_SUPER_ADD_AMOUNT = 10f;
        public const float DEFAULT_SUPER_FILL_AMOUNT = 50f;
        public const bool DEFAULT_PLAY_SUPER_CHANGED_EFFECT = true;

        public static PlayerStatsManager CurrentStatMngr1 { get; private set; } = null;
        public static PlayerStatsManager CurrentStatMngr2 { get; private set; } = null;

        private static float superFillAmount = DEFAULT_SUPER_FILL_AMOUNT;
        private static bool playSuperChangedEffect = DEFAULT_PLAY_SUPER_CHANGED_EFFECT;

        private static MethodInfo _mi_set_SuperMeter = typeof(PlayerStatsManager).GetProperty("SuperMeter").GetSetMethod(true);
        private static MethodInfo _mi_OnSuperChanged = typeof(PlayerStatsManager).GetMethod("OnSuperChanged", BindingFlags.Instance | BindingFlags.NonPublic);

        [HarmonyPatch(typeof(PlayerStatsManager), "OnAwake")]
        internal static class OnAwake {
            static bool Prefix(PlayerStatsManager __instance) {
                Plugin.Log("PlayerStatsManager OnAwake");
                if (__instance.basePlayer.id==PlayerId.PlayerTwo) {
                    CurrentStatMngr2 = __instance;
                } else {
                    CurrentStatMngr1 = __instance;
                }
                return true;
            }
        }
        
        [HarmonyPatch(typeof(PlayerStatsManager), "CalculateHealthMax")]
        internal static class CalculateHealthMax {
            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator il) {
                List<CodeInstruction> codes = new(instructions);
                bool success = false;
                bool debug = false;

                PropertyInfo _pi_HealthMax = typeof(PlayerStatsManager).GetProperty("HealthMax", BindingFlags.Public | BindingFlags.Instance);
                MethodInfo _mi_get_HealthMax = _pi_HealthMax.GetGetMethod();
                MethodInfo _mi_set_HealthMax = _pi_HealthMax.GetSetMethod(true);
                MethodInfo _mi_get_Health = typeof(PlayerStatsManager).GetProperty("Health", BindingFlags.Public | BindingFlags.Instance).GetGetMethod();
                MethodInfo _mi_APCalcMaxHealth = typeof(CalculateHealthMax).GetMethod("APCalcMaxHealth", BindingFlags.NonPublic | BindingFlags.Static);

                Label cvanilla = il.DefineLabel();

                if (debug) {
                    foreach (CodeInstruction code in codes) {
                        Plugin.Log($"{code.opcode}: {code.operand}");
                    }
                }
                for (int i=0;i<codes.Count-3;i++) {
                    if (codes[i].opcode == OpCodes.Ldarg_0 && codes[i+1].opcode == OpCodes.Call && (MethodInfo)codes[i+1].operand == _mi_get_HealthMax &&
                        codes[i+2].opcode == OpCodes.Ldc_I4_S && (sbyte)codes[i+2].operand == 9 && codes[i+3].opcode == OpCodes.Ble) {
                            codes[i].labels.Add(cvanilla);
                            List<CodeInstruction> ncodes = [
                                CodeInstruction.Call(() => APData.IsCurrentSlotEnabled()),
                                new CodeInstruction(OpCodes.Brfalse, cvanilla),
                                new CodeInstruction(OpCodes.Ldarg_0),
                                new CodeInstruction(OpCodes.Dup),
                                new CodeInstruction(OpCodes.Call, _mi_get_Health),
                                new CodeInstruction(OpCodes.Ldarg_0),
                                new CodeInstruction(OpCodes.Call, _mi_get_HealthMax),
                                new CodeInstruction(OpCodes.Call, _mi_APCalcMaxHealth),
                                new CodeInstruction(OpCodes.Call, _mi_set_HealthMax),
                            ];
                            codes.InsertRange(i, ncodes);
                            i+= ncodes.Count;
                            success = true;
                            break;
                    }
                }
                if (!success) throw new Exception($"{nameof(CalculateHealthMax)}: Patch Failed!");
                if (debug) {
                    foreach (CodeInstruction code in codes) {
                        Plugin.Log("---");
                        Plugin.Log($"{code.opcode}: {code.operand}");
                    }
                }

                return codes;
            }

            private static int APCalcMaxHealth(int health, int healthMax) {
                return (health>healthMax)?health:healthMax;
            }
        }
        
        [HarmonyPatch(typeof(PlayerStatsManager), "DebugFillSuper")]
        internal static class DebugFillSuper {
            static bool Prefix(PlayerStatsManager __instance) {
                _mi_set_SuperMeter.Invoke(__instance, [superFillAmount]);
                _mi_OnSuperChanged.Invoke(__instance, [playSuperChangedEffect]);
                return false;
            }
        }

        public static float GetSuperFillAmount() => superFillAmount;
        public static void SetSuperFillAmount(float set) => superFillAmount = set;

        public static void AddEx(PlayerStatsManager instance, float add) {
            SetSuper(instance, instance.SuperMeter + add);
        }
        public static void SetSuper(PlayerStatsManager instance, float set) {
            float orig = superFillAmount;
            superFillAmount = set;
            instance.DebugFillSuper();
            superFillAmount = orig;
        }
    }
}