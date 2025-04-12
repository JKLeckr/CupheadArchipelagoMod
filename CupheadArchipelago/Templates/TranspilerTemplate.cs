/// Copyright 2025 JKLeckr
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
                    Dbg.LogCodeInstructions(codes);
                }
                // Body here
                if (!success) throw new Exception($"{nameof(Patch)}: Patch Failed!");
                if (debug) {
                    Logging.Log("---");
                    Dbg.LogCodeInstructions(codes);
                }

                return codes;
            }
        }
    }
}
