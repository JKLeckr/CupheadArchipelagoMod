/// Copyright 2025-2026 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

using System.Collections.Generic;
using CupheadArchipelago.Util;
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

        internal static void LogCollection(string name, IEnumerable<string> collection) {
            Logging.Log($"{name}:");
            Logging.Log($"  {Aux.CollectionToString(collection)}");
        }

        internal static void LogCollectionDiff(string name, IEnumerable<string> og, IEnumerable<string> nw) {
            Logging.Log($"{name}:");
            if (nw != null) {
                Logging.Log($"  Orig: {Aux.CollectionToString(og)}");
                Logging.Log($"  New: {Aux.CollectionToString(nw)}");
            }
            else {
                Logging.Log($"  {Aux.CollectionToString(og)}");
            }
        }
    }
}
