/// Copyright 2025-2026 JKLeckr
/// SPDX-License-Identifier: GPL-3.0-or-later

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
