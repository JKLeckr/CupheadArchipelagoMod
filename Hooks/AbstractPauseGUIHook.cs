/// Copyright 2025 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;

namespace CupheadArchipelago.Hooks {
    internal class AbstractPauseGUIHook {
        internal static void Hook() {
            Harmony.CreateAndPatchAll(typeof(UpdateInput));
        }

        internal static bool CanPause { get; private set; } = false;
        internal static bool Paused { get; private set; } = false;

        [HarmonyPatch(typeof(AbstractPauseGUI), "UpdateInput")]
        internal static class UpdateInput {
            static bool Prefix(AbstractPauseGUI __instance) {
                Paused = __instance.state == AbstractPauseGUI.State.Paused;
                return true;
            }
            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
                List<CodeInstruction> codes = new(instructions);
                bool success = false;
                bool debug = false;

                MethodInfo _mi_get_CanPause = typeof(AbstractPauseGUI).GetProperty("CanPause", BindingFlags.NonPublic | BindingFlags.Instance).GetGetMethod(true);
                MethodInfo _mi_WriteCanPause = typeof(UpdateInput).GetMethod("WriteCanPause", BindingFlags.NonPublic | BindingFlags.Static);

                if (debug) {
                    foreach (CodeInstruction code in codes) {
                        Logging.Log($"{code.opcode}: {code.operand}");
                    }
                }
                for (int i=0;i<codes.Count-2;i++) {
                    if (codes[i].opcode == OpCodes.Ldarg_0 && codes[i+1].opcode == OpCodes.Callvirt &&
                        (MethodInfo)codes[i+1].operand == _mi_get_CanPause && codes[i+2].opcode == OpCodes.Brtrue) {
                            codes.Insert(i+2, new CodeInstruction(OpCodes.Call, _mi_WriteCanPause));
                            success = true;
                            break;
                    }
                }
                if (!success) throw new Exception($"{nameof(UpdateInput)}: Patch Failed!");
                if (debug) {
                    Logging.Log("---");
                    foreach (CodeInstruction code in codes) {
                        Logging.Log($"{code.opcode}: {code.operand}");
                    }
                }

                return codes;
            }
            private static bool WriteCanPause(bool write) {
                CanPause = write;
                return write;
            }
        }
    }
}
