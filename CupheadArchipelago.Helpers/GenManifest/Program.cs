/// Copyright 2025 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Newtonsoft.Json;
using System.IO;
using FVer;
using CupheadArchipelago.Helpers.FVerParser;

namespace GenManifest {
    internal class Program {
        private const string CSPROJ_NAME = "CupheadArchipelago.csproj";
        private const string SRC_MAIN_NAME = "Plugin.cs";

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
            string csMainPath = Path.Combine(modDir, SRC_MAIN_NAME);

            if (!File.Exists(csProjPath)) {
                Console.WriteLine($"Error: {csProjPath}: no such file or directory!");
                return -3;
            }
            if (!File.Exists(csMainPath)) {
                Console.WriteLine($"Error: {csMainPath}: no such file or directory!");
                return -4;
            }

            try {
                string modName = ExtractCsprojProperty(csProjPath, "AssemblyName") ?? Path.GetFileNameWithoutExtension(csProjPath);
                RawFVer rawVer = FVerParse.GetRawFVer(
                    ExtractCsprojProperty(csProjPath, "Version") ?? throw new NullReferenceException("Version cannot be null!")
                );
                string modVersion = new FVersion(rawVer.baseline, rawVer.revision, rawVer.release, rawVer.prefix, rawVer.postfix);

                string modGuid = "";

                Dictionary<string, string> constants = ExtractConstantsFromCs(csMainPath);
                foreach (KeyValuePair<string, string> kvp in constants) {
                    //Console.WriteLine($"{kvp.Key}: {kvp.Value}");
                    if (kvp.Key == "MOD_GUID")
                        modGuid = kvp.Value;
                }

                if (modGuid == "") throw new Exception("MOD_GUID cannot be null!");

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
            }

            return 0;
        }

        static string? ExtractCsprojProperty(string csprojPath, string propertyName) {
            try {
                XDocument doc = XDocument.Load(csprojPath);
                XNamespace ns = doc.Root?.Name.Namespace ?? "";
                XElement? property = doc.Root?
                    .Element(ns + "PropertyGroup")?
                    .Element(ns + propertyName);

                return property?.Value.Trim();
            }
            catch {
                return null;
            }
        }

        static Dictionary<string, string> ExtractConstantsFromCs(string csFilePath)
        {
            Dictionary<string, string> constants = [];
            string pattern = @"protected\s+const\s+string\s+(\w+)\s*=\s*""([^""]*)"";";
            string code = File.ReadAllText(csFilePath);

            foreach (Match match in Regex.Matches(code, pattern)) {
                if (match.Groups.Count == 3) {
                    string name = match.Groups[1].Value;
                    string value = match.Groups[2].Value;
                    constants[name] = value;
                }
            }

            return constants;
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
