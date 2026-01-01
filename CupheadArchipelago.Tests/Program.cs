/// Copyright 2025-2026 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using CupheadArchipelago.Tests.Core;
using NUnitLite;

namespace CupheadArchipelago.Tests {
    public class TestMain {
        public static int Main(string[] args) {
            Console.WriteLine($"-- CupheadArchipelago Test Suite --");
            TLogging.SetupLogging();
            TestData.Setup();
            Console.WriteLine();
            // The tests only work with 1 thread.
            List<string> largs = [.. args,
                "--noresult", "--workers=1",
            ];
            Console.WriteLine($"Running NUnitLite with args [{ArgsToString(largs)}]");
            Console.WriteLine();
            return new AutoRun().Execute(largs.ToArray());
        }

        private static string ArgsToString(IEnumerable<string> args) {
            string res = "";
            bool sep = false;
            foreach (string arg in args) {
                if (sep) res += " ";
                res += $"\"{arg}\"";
                sep = true;
            }
            return res;
        }
    }
}
