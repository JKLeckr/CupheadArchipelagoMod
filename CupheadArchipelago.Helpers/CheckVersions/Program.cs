/// Copyright 2025-2026 JKLeckr
/// SPDX-License-Identifier: GPL-3.0-or-later

using System;
using System.IO;
using FVer;
using CupheadArchipelago.Helpers.CsprojParser;
using CupheadArchipelago.Helpers.FVerParser;

namespace CupheadArchipelago.Helpers.CheckVersions {
    internal class Program {
        private const string CSPROJ_NAME = "CupheadArchipelago.csproj";

        private static int Main(string[] args) {
            if (args.Length != 2) {
                Console.WriteLine("FORMAT: CMD <SRC_DIR> <TEST_FVER>");
                return -1;
            }

            string modDir = args[0];

            if (!Path.Exists(modDir)) {
                Console.WriteLine($"Error: {modDir}: no such file or directory!");
                return -2;
            }

            string csProjPath = Path.Combine(modDir, CSPROJ_NAME);

            if (!File.Exists(csProjPath)) {
                Console.WriteLine($"Error: {csProjPath}: no such file or directory!");
                return -3;
            }

            RawFVer rawFVer;
            try {
                rawFVer = FVerParse.GetRawFVer(
                    CsprojExtractor.GetFullVersionString(csProjPath),
                    CsprojExtractor.GetVersionRelNumber(csProjPath)
                );
            }
            catch (Exception ex) {
                Console.WriteLine($"Error: {ex.Message}");
                return -100;
            }

            string src = new FVersion(rawFVer.baseline, rawFVer.revision, rawFVer.release, rawFVer.prefix, rawFVer.postfix);
            FVersion test = new(args[1]);

            Console.WriteLine($"{args[0]} -> {test} == {src}");

            test = new(test.Baseline, test.RevisionNumber, test.Release, test.Prefix, test.Postfix);

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
