/// Copyright 2025-2026 JKLeckr
/// SPDX-License-Identifier: GPL-3.0-or-later

using System;
using System.IO;
using Newtonsoft.Json;
using CupheadArchipelago.Helpers.GenManifest.GenManifestMain;

namespace CupheadArchipelago.Helpers.GenManifest.GenThunderstoreManifest {
    internal class Program {
        private static readonly string[] modTSDeps = ["BepInEx-BepInExPack-5.4.2305"];

        private static int Main(string[] args) {
            if (args.Length < 1 || args.Length > 2) {
                Console.WriteLine("FORMAT: CMD <SRC_CSPROJ> [TGT_FILE]");
                return -1;
            }
            string srcFile = args[0];
            string? destFilePath = args.Length > 1 ? args[1] : null;

            if (!Path.Exists(srcFile)) {
                Console.WriteLine($"Error: {srcFile}: no such file or directory!");
                return -2;
            }

            ManifestData mdata;

            try {
                mdata = ManifestData.GatherManifestData(srcFile);

                string versionNumber = Util.SemVersionFourPartToThreePart(
                    mdata.modVersionSem,
                    mdata.modVersionRel,
                    mdata.modVersionPostfix
                );

                Manifest manifest = new(
                    mdata.modName,
                    versionNumber,
                    mdata.modVersion,
                    mdata.websiteUrl,
                    mdata.modDescription,
                    modTSDeps
                );

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

        private class Manifest(
            string name,
            string version_number,
            string version_name,
            string website_url,
            string description,
            string[] dependencies
        ) {
            public string name = name;
            public string version_number = version_number;
            public string version_name = version_name;
            public string website_url = website_url;
            public string description = description;
            public string[] dependencies = dependencies;
        }
    }
}
