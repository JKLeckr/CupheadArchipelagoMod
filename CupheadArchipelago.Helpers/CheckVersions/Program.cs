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

            string srcRaw = FVerParse.GetFVer(args[0]);

            Console.WriteLine($"{args[0]} -> {args[1]} == {srcRaw}");

            FVersion src = new(srcRaw);
            FVersion test = new(args[1]);

            // Releases and postfixes are ignored in this check during prerelease
            test = new(test.Baseline, test.RevisionNumber, 0, test.Prefix, null);

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
