/// Copyright 2024 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;

namespace CupheadArchipelago.Hooks {
    internal class PlayerStatsManagerHook {
        internal static void Hook() {
            Harmony.CreateAndPatchAll(typeof(DebugFillSuper));
        }

        public const float DEFAULT_SUPER_ADD_AMOUNT = 10f;
        public const float DEFAULT_SUPER_FILL_AMOUNT = 50f;

        private static float superFillAmount = DEFAULT_SUPER_FILL_AMOUNT;

        [HarmonyPatch(typeof(PlayerStatsManager), "DebugFillSuper")]
        internal static class DebugFillSuper {
            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
                List<CodeInstruction> codes = new List<CodeInstruction>(instructions);

                bool success = false;

                for (int i=0; i<codes.Count; i++) {
                    if (codes[i].opcode == OpCodes.Ldc_R4 && (float)codes[i].operand == 50f) {
                        codes[i] = new CodeInstruction(OpCodes.Ldsfld, superFillAmount);
                        success = true;
                        break;
                    }
                }
                if (!success) {
                    throw new Exception("[PlayerStatsManagerHook] Failed to Patch DebugFillSuper");
                }

                return codes;
            }
        }

        public static float GetSuperFillAmount() => superFillAmount;
        public static void SetSuperFillAmount(float set) => superFillAmount = set;

        public static void AddEx(PlayerStatsManager instance, float add) {
            SetSuper(instance, instance.SuperMeter + add);
        }
        public static void SetSuper(PlayerStatsManager instance, float set) {
            superFillAmount = set;
            instance.DebugFillSuper();
            superFillAmount = DEFAULT_SUPER_FILL_AMOUNT;
        }
    }
}