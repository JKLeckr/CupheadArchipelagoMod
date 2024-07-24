/// Copyright 2024 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

using System.Collections;
using System.Collections.Generic;
using HarmonyLib;

namespace CupheadArchipelago.Auxiliary {
    public class Aux {
        public static string BlankIsNull(string str) => str.Length>0?str:null;

        public static string CollectionToString(IEnumerable collection) {
            bool first = true;
            string res = "[";
            foreach (var obj in collection) {
                string prefix = !first?", ":"";
                if (first) first = false;
                res += prefix + obj.ToString();
            }
            res += "]";
            return res;
        }

        private static void LogCodeInstructions(IEnumerable<CodeInstruction> codes) {
            foreach (CodeInstruction code in codes) {
                Plugin.Log(code.opcode + " -: " + code.operand, LoggingFlags.Transpiler);
            }
        }
    }
}