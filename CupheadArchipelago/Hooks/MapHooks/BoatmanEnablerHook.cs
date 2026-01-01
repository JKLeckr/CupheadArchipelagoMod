/// Copyright 2025-2026 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using CupheadArchipelago.AP;
using HarmonyLib;

namespace CupheadArchipelago.Hooks.MapHooks {
    internal class BoatmanEnablerHook {
        internal static void Hook() {
            Harmony.CreateAndPatchAll(typeof(Start));
            //Harmony.CreateAndPatchAll(typeof(check_cr));
        }

        [HarmonyPatch(typeof(BoatmanEnabler), "Start")]
        internal static class Start {
            private static MethodInfo _mi_check_cr;

            static Start() {
                _mi_check_cr = typeof(BoatmanEnabler).GetMethod("check_cr", BindingFlags.NonPublic | BindingFlags.Instance);
            }

            static bool Prefix(BoatmanEnabler __instance, ref bool ___forceBoatmanUnlocking) {
                if (APData.IsCurrentSlotEnabled()) {
                    if (DLCManager.DLCEnabled() && APCondition()) {
                        ___forceBoatmanUnlocking = true;
                        __instance.StartCoroutine(_mi_check_cr.Name);
                    }
                    return false;
                }
                else return true;
            }
            private static bool APCondition() {
                return !APSettings.DLCRequiresMausoleum || PlayerData.Data.GetLevelData(Levels.Mausoleum).completed;
            }
        }

        [HarmonyPatch(typeof(BoatmanEnabler), "check_cr", MethodType.Enumerator)]
        internal static class check_cr {
            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
                List<CodeInstruction> codes = new(instructions);
                bool success = false;
                bool debug = true;

                FieldInfo _fi_forceBoatmanUnlocking = typeof(BoatmanEnabler).GetField("forceBoatmanUnlocking", BindingFlags.NonPublic | BindingFlags.Instance);
                MethodInfo _mi_get_Data = typeof(PlayerData).GetProperty("Data", BindingFlags.Public | BindingFlags.Static)?.GetGetMethod();
                MethodInfo _mi_get_CurrentMap = typeof(PlayerData).GetProperty("CurrentMap", BindingFlags.Public | BindingFlags.Instance).GetGetMethod();
                MethodInfo _mi_APCondition = typeof(check_cr).GetMethod("APCondition", BindingFlags.NonPublic | BindingFlags.Static);

                if (debug) {
                    Dbg.LogCodeInstructions(codes);
                }
                for (int i=0;i<codes.Count-7;i++) {
                    if (codes[i].opcode == OpCodes.Ldarg_0 && codes[i+1].opcode == OpCodes.Ldfld && codes[i+2].opcode == OpCodes.Ldfld && (FieldInfo)codes[i+2].operand == _fi_forceBoatmanUnlocking &&
                        codes[i+3].opcode == OpCodes.Brtrue && codes[i+4].opcode == OpCodes.Call && (MethodInfo)codes[i+4].operand == _mi_get_Data && codes[i+5].opcode == OpCodes.Callvirt &&
                        (MethodInfo)codes[i+5].operand == _mi_get_CurrentMap && codes[i+6].opcode == OpCodes.Ldc_I4_S && codes[i+7].opcode == OpCodes.Bne_Un) {
                            codes[i+7].opcode = OpCodes.Brfalse;
                            codes.Insert(i+7, new CodeInstruction(OpCodes.Call, _mi_APCondition));
                            codes.RemoveAt(i+3);
                            success = true;
                            break;
                    }
                }
                if (!success) throw new Exception($"{nameof(check_cr)}: Patch Failed!");
                if (debug) {
                    Logging.Log("---");
                    Dbg.LogCodeInstructions(codes);
                }

                return codes;
            }

            private static bool APCondition(bool a, bool b) {
                Logging.Log("boat_check: {a} {b}");
                return a || b;
            }
        }

    }
}
