/// Copyright 2025-2026 JKLeckr
/// SPDX-License-Identifier: GPL-3.0-or-later

using System;
using System.IO;
using Newtonsoft.Json;
using CupheadArchipelago.Helpers.GenManifest.GenManifestMain;

namespace CupheadArchipelago.Helpers.GenManifest.GenOrigManifest {
    internal class Program {
        private static readonly string[] modAuthors = ["JKLeckr"];
        private static readonly string modLicense = "GPL-3.0-or-later";
        private static readonly string[] modDeps = [];

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

                string versionNumber = Util.SemVersionFourPartToThreePart(mdata.modVersionSem, mdata.modVersionRel, mdata.modVersionPostfix);

                Manifest manifest = new(
                    mdata.modName,
                    mdata.modGuid,
                    mdata.modVersion,
                    mdata.modVersionSemFull,
                    modAuthors,
                    modLicense,
                    mdata.websiteUrl,
                    modDeps
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
            string mod_name,
            string mod_guid,
            string mod_version,
            string mod_sem_version,
            string[] mod_authors,
            string mod_license,
            string website_url,
            string[] mod_dependencies
        ) {
            public readonly uint version = 2;
            public string mod_name = mod_name;
            public string mod_guid = mod_guid;
            public string mod_version = mod_version;
            public string mod_sem_version = mod_sem_version;
            public string[] mod_authors = mod_authors;
            public string mod_license = mod_license;
            public string website_url = website_url;
            public string[] mod_dependencies = mod_dependencies;
        }
    }
}
