/// Copyright 2025-2026 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

using System;
using FVer;
using CupheadArchipelago.Helpers.FVerParser;

namespace CupheadArchipelago.Helpers.CheckVersions {
    internal class Program {
        private static int Main(string[] args) {
            if (args.Length != 2) {
                Console.WriteLine("FORMAT: CMD <PROJ_SEMVER> <TEST_FVER>");
                return -1;
            }

            RawFVer rawFVer = FVerParse.GetRawFVer(args[0]);

            string src = new FVersion(rawFVer.baseline, rawFVer.revision, rawFVer.release, rawFVer.prefix, rawFVer.postfix);
            FVersion test = new(args[1]);

            Console.WriteLine($"{args[0]} -> {test} == {src}");

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
