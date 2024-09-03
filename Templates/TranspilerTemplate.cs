/// Copyright 2024 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using HarmonyLib;

namespace CupheadArchipelago.Templates {
    internal class TranspilerTemplate {
        internal static class Patch {
            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
                List<CodeInstruction> codes = new(instructions);
                bool success = false;
                bool debug = false;

                if (debug) {
                    foreach (CodeInstruction code in codes) {
                        Logging.Log($"{code.opcode}: {code.operand}");
                    }
                }
                // Body here
                //if (!success) throw new Exception($"{nameof(method)}: Patch Failed!");
                if (debug) {
                    Logging.Log("---");
                    foreach (CodeInstruction code in codes) {
                        Logging.Log($"{code.opcode}: {code.operand}");
                    }
                }

                return codes;
            }
        }
    }
}