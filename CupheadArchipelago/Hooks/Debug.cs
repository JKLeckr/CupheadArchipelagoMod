/// Copyright 2025 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

using System.Collections.Generic;
using HarmonyLib;

namespace CupheadArchipelago.Hooks {
    internal class Dbg {
        internal static bool DbgT(int num) {
            Logging.Log($"T:{num}");
            return true;
        }

        internal static bool C(bool cond) {
            Logging.Log($"C:{cond}");
            return cond;
        }

        internal static void LogCodeInstructions(IEnumerable<CodeInstruction> codes) {
            foreach (CodeInstruction code in codes) {
                Logging.Log($"{code.opcode}: {code.operand}");
            }
        }
    }
}
