/// Copyright 2025 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

using System;
using FVer;

namespace CheckVersions {
    internal class Program {
        private static int Main(string[] args) {
            if (args.Length != 2) {
                Console.WriteLine("FORMAT: CMD <PROJ_SEMVER> <TEST_FVER>");
                return -1;
            }

            FVersion src = new(FVerParse.GetFVer(args[0]));
            FVersion test = new(args[1]);

            if (src.Equals(test)) {
                Console.WriteLine("Match");
                return 0;
            }
            else {
                Console.WriteLine($"Mismatch: {src} != {test}");
                return 1;
            }
        }
    }
}
