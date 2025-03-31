/// Copyright 2025 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace CupheadArchipelago.Util {
    public static class Aux {
        public static string BlankIsNull(string str) => str.Length>0?str:null;

        public static bool IntAsBool(long i) => i != 0;

        public static string CollectionToString(IEnumerable collection) {
            bool first = true;
            StringBuilder res = new("[");
            foreach (var obj in collection) {
                string prefix = !first?", ":"";
                if (first) first = false;
                res.Append(prefix + obj.ToString());
            }
            res.Append("]");
            return res.ToString();
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

        public static void Shuffle<T>(this IList<T> list, Random rand=null) {  
            if (rand==null) rand = new Random();
            int n = list.Count;  
            while (n > 1) {  
                n--;  
                int k = rand.Next(n + 1);  
                T value = list[k];  
                list[k] = list[n];  
                list[n] = value;  
            }
        }
    }
}
