/// Copyright 2024 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections;
using System.Collections.Generic;
using HarmonyLib;

namespace CupheadArchipelago.Util {
    public class Aux {
        public static string BlankIsNull(string str) => str.Length>0?str:null;

        public static bool IntAsBool(long i) => i != 0;

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

        public static int ArrayNullCount(object[] arr) {
            int c = 0;
            foreach (object o in arr) {
                if (o==null) c++;
            }
            return c;
        }

        public static T[] ArrayRange<T>(T[] arr, int start, int end) {
            if (start>=end || start < 0 || end > arr.Length)
                throw new IndexOutOfRangeException();

            T[] res = new T[end-start];

            for (int i=0;i<res.Length;i++) {
                res[i] = arr[start+i];
            }

            return res;
        }
        public static T[] ArrayRange<T>(T[] arr, int end) => ArrayRange(arr, 0, end);

        private static void LogCodeInstructions(IEnumerable<CodeInstruction> codes) {
            foreach (CodeInstruction code in codes) {
                Plugin.Log(code.opcode + " -: " + code.operand, LoggingFlags.Transpiler);
            }
        }
    }
}
