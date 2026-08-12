/// Copyright 2025-2026 JKLeckr
/// SPDX-License-Identifier: GPL-3.0-or-later

using System;
using System.IO;
using Newtonsoft.Json;
using FVer;
using CupheadArchipelago.Helpers.CsprojParser;
using CupheadArchipelago.Helpers.FVerParser;

namespace CupheadArchipelago.Helpers.GenManifest {
    internal class Program {
        private const string CSPROJ_NAME = "CupheadArchipelago.csproj";

        private static int Main(string[] args) {
            if (args.Length < 1 || args.Length > 2) {
                Console.WriteLine("FORMAT: CMD <SRC_DIR> [TGT_FILE]");
                return -1;
            }
            string modDir = args[0];
            string? destFilePath = args.Length > 1 ? args[1] : null;

            if (!Path.Exists(modDir)) {
                Console.WriteLine($"Error: {modDir}: no such file or directory!");
                return -2;
            }

            string csProjPath = Path.Combine(modDir, CSPROJ_NAME);

            if (!File.Exists(csProjPath)) {
                Console.WriteLine($"Error: {csProjPath}: no such file or directory!");
                return -3;
            }

            try {
                string modName = CsprojExtractor.ExtractCsprojProperty(csProjPath, "AssemblyName") ?? Path.GetFileNameWithoutExtension(csProjPath);
                RawFVer rawVer = FVerParse.GetRawFVer(
                    CsprojExtractor.GetFullVersionString(csProjPath),
                    CsprojExtractor.GetVersionRelNumber(csProjPath)
                );
                string modVersion = new FVersion(rawVer.baseline, rawVer.revision, rawVer.release, rawVer.prefix, rawVer.postfix);

                string modGuid = CsprojExtractor.ExtractCsprojProperty(csProjPath, "GUID") ?? throw new NullReferenceException("GUID cannot be null!");

                Manifest manifest = new(modName, modGuid, modVersion, []);

                string json = JsonConvert.SerializeObject(manifest, Formatting.Indented) + '\n';

                if (destFilePath != null) {
                    File.WriteAllText(destFilePath, json);
                    Console.WriteLine($"Written to {destFilePath}");
                }
                else {
                    Console.WriteLine(json);
                }
            }
            catch (Exception ex) {
                Console.WriteLine($"Error: {ex.Message}");
                return -100;
            }

            return 0;
        }

        private class Manifest(string mod_name, string mod_guid, string mod_version, string[] mod_dependencies) {
            public readonly uint version = 1;
            public string mod_name = mod_name;
            public string mod_guid = mod_guid;
            public string mod_version = mod_version;
            public string[] mod_dependencies = mod_dependencies;
        }
    }
}
