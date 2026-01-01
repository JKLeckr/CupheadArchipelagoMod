/// Copyright 2025-2026 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using CupheadArchipelago.AP;
using CupheadArchipelago.Unity;
using HarmonyLib;

namespace CupheadArchipelago.Hooks.PlayerHooks {
    internal class PlayerStatsManagerHook {
        internal static void Hook() {
            Harmony.CreateAndPatchAll(typeof(LevelInit));
            Harmony.CreateAndPatchAll(typeof(CalculateHealthMax));
            Harmony.CreateAndPatchAll(typeof(TakeDamage));
            Harmony.CreateAndPatchAll(typeof(DebugAddSuper));
            Harmony.CreateAndPatchAll(typeof(DebugFillSuper));
        }

        public const float DEFAULT_SUPER_ADD_AMOUNT = 10f;
        public const float DEFAULT_SUPER_FILL_AMOUNT = 50f;
        public const bool DEFAULT_PLAY_SUPER_CHANGED_EFFECT = true;
        public const float REVERSE_CONTROLS_TIME = 10f;

        private static float superFillAmount = DEFAULT_SUPER_FILL_AMOUNT;
        private static bool playSuperChangedEffect = DEFAULT_PLAY_SUPER_CHANGED_EFFECT;
        private static StatsCommands statCommand = StatsCommands.Orig;
        private static object[] statCommandArgs = null;

        private static MethodInfo _mi_set_Health = typeof(PlayerStatsManager).GetProperty("Health", BindingFlags.Public | BindingFlags.Instance).GetSetMethod(true);
        private static MethodInfo _mi_set_ReverseTime = typeof(PlayerStatsManager).GetProperty("ReverseTime", BindingFlags.Public | BindingFlags.Instance).GetSetMethod(true);
        private static MethodInfo _mi_set_SuperMeter = typeof(PlayerStatsManager).GetProperty("SuperMeter")?.GetSetMethod(true);
        private static MethodInfo _mi_OnSuperChanged = typeof(PlayerStatsManager).GetMethod("OnSuperChanged", BindingFlags.NonPublic | BindingFlags.Instance);
        private static MethodInfo _mi_OnStatsDeath = typeof(PlayerStatsManager).GetMethod("OnStatsDeath", BindingFlags.NonPublic | BindingFlags.Instance);

        internal enum StatsCommands {
            Orig,
            Death,
            ReverseControls,
        }

        [HarmonyPatch(typeof(PlayerStatsManager), "LevelInit")]
        internal static class LevelInit {
            static bool Prefix(PlayerStatsManager __instance) {
                __instance.gameObject.AddComponent<PlayerStatsInterface>().Init(__instance);
                return true;
            }
        }

        [HarmonyPatch(typeof(PlayerStatsManager), "TakeDamage")]
        internal static class TakeDamage {
            static bool Prefix(PlayerStatsManager __instance) {
                Overrides obit = (__instance.basePlayer.id == PlayerId.PlayerTwo) ? Overrides.DamageOverrideP2 : Overrides.DamageOverrideP1;
                if (APData.CurrentSData.IsOverridden(obit)) {
                    Vibrator.Vibrate(1f, 0.2f, __instance.basePlayer.id);
                    __instance.StartCoroutine("hit_cr");
                    return false;
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(PlayerStatsManager), "CalculateHealthMax")]
        internal static class CalculateHealthMax {
            private const int DEFAULT_HEALTH_MAX = 3;

            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator il) {
                List<CodeInstruction> codes = new(instructions);
                int success = 0;
                bool debug = false;

                PropertyInfo _pi_HealthMax = typeof(PlayerStatsManager).GetProperty("HealthMax", BindingFlags.Public | BindingFlags.Instance);
                MethodInfo _mi_get_HealthMax = _pi_HealthMax?.GetGetMethod();
                MethodInfo _mi_set_HealthMax = _pi_HealthMax?.GetSetMethod(true);
                MethodInfo _mi_get_Health = typeof(PlayerStatsManager).GetProperty("Health", BindingFlags.Public | BindingFlags.Instance)?.GetGetMethod();
                MethodInfo _mi_APGetStartMaxHealth = typeof(CalculateHealthMax).GetMethod("APGetStartMaxHealth", BindingFlags.NonPublic | BindingFlags.Static);
                MethodInfo _mi_APCalcMaxHealth = typeof(CalculateHealthMax).GetMethod("APCalcMaxHealth", BindingFlags.NonPublic | BindingFlags.Static);

                Label cvanilla = il.DefineLabel();

                if (debug) {
                    Dbg.LogCodeInstructions(codes);
                }
                for (int i = 0; i < codes.Count - 3; i++) {
                    if ((success & 1) == 0 && codes[i].opcode == OpCodes.Ldarg_0 && codes[i + 1].opcode == OpCodes.Ldc_I4_3 && codes[i + 2].opcode == OpCodes.Call &&
                        codes[i + 2].operand == _mi_set_HealthMax
                    ) {
                        codes[i + 1] = new(OpCodes.Call, _mi_APGetStartMaxHealth);
                        codes.Insert(i + 1, new(OpCodes.Ldarg_0));
                        success |= 1;
                    }
                    if ((success & 2) == 0 && codes[i].opcode == OpCodes.Ldarg_0 && codes[i + 1].opcode == OpCodes.Call && (MethodInfo)codes[i + 1].operand == _mi_get_HealthMax &&
                        codes[i + 2].opcode == OpCodes.Ldc_I4_S && (sbyte)codes[i + 2].operand == 9 && codes[i + 3].opcode == OpCodes.Ble
                    ) {
                        CodeInstruction[] ncodes = [
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
                        ncodes[0].labels = codes[i].labels;
                        codes[i].labels = [cvanilla];
                        codes.InsertRange(i, ncodes);
                        i += ncodes.Length;
                        success |= 2;
                    }
                    if (success == 3) break;
                }
                if (success != 3) throw new Exception($"{nameof(CalculateHealthMax)}: Patch Failed! {success}");
                if (debug) {
                    Logging.Log("---");
                    Dbg.LogCodeInstructions(codes);
                }

                return codes;
            }

            private static int APGetStartMaxHealth(PlayerStatsManager instance) {
                if (APData.IsCurrentSlotEnabled()) {
                    int healthMax = instance.basePlayer.id == PlayerId.PlayerTwo ? APSettings.StartMaxHealthP2 : APSettings.StartMaxHealth;
                    Logging.LogDebug($"Start Health {instance.basePlayer.id}: {healthMax}");
                    return healthMax > 0 ? healthMax : DEFAULT_HEALTH_MAX;
                }
                return DEFAULT_HEALTH_MAX;
            }
            private static int APCalcMaxHealth(int health, int healthMax) {
                int nhealthMax = healthMax + APData.CurrentSData.playerData.healthupgrades;
                return (health > healthMax) ? health : nhealthMax;
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

        [HarmonyPatch(typeof(PlayerStatsManager), "DebugAddSuper")]
        internal static class DebugAddSuper {
            static bool Prefix(PlayerStatsManager __instance, ref float ___timeSinceReversed) {
                switch (statCommand) {
                    case StatsCommands.Death:
                        _mi_set_Health.Invoke(__instance, [0]);
                        Vibrator.Vibrate(1f, 0.2f, __instance.basePlayer.id);
                        _mi_OnStatsDeath.Invoke(__instance, []);
                        break;
                    case StatsCommands.ReverseControls:
                        _mi_set_ReverseTime.Invoke(__instance, [__instance.ReverseTime]);
                        ___timeSinceReversed = 0f;
                        break;
                    default:
                        return true;
                }
                return false;
            }
        }

        internal static void IssueStatsCommand(
            PlayerStatsManager instance,
            StatsCommands command,
            params object[] args
        ) {
            statCommand = command;
            statCommandArgs = args;
            instance.DebugAddSuper();
            statCommandArgs = null;
            statCommand = StatsCommands.Orig;
        }

        internal static void SetSuper(PlayerStatsManager instance, float set) {
            float orig = superFillAmount;
            superFillAmount = set;
            instance.DebugFillSuper();
            superFillAmount = orig;
        }
    }
}
