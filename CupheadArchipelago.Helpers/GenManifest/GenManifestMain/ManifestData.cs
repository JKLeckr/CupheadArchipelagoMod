/// Copyright 2025-2026 JKLeckr
/// SPDX-License-Identifier: GPL-3.0-or-later

using System;
using System.IO;
using System.Xml.Linq;
using FVer;
using CupheadArchipelago.Helpers.CsprojParser;
using CupheadArchipelago.Helpers.FVerParser;

namespace CupheadArchipelago.Helpers.GenManifest.GenManifestMain {
    public class ManifestData {
        public readonly string modName;
        public readonly string modGuid;
        public readonly string websiteUrl;
        public readonly string modVersion;
        public readonly string modVersionSem;
        public readonly string modVersionSemFull;
        public readonly ushort modVersionRel;
        public readonly string modVersionPostfix;
        public readonly string modDescription;
        public readonly string[] modDependencies;

        public ManifestData(
            string modName,
            string modGuid,
            string websiteUrl,
            string modVersion,
            string modVersionSem,
            ushort modVersionRel,
            string modVersionPostfix,
            string modDescription,
            string[] modDependencies
        ) {
            this.modName = modName;
            this.modGuid = modGuid;
            this.websiteUrl = websiteUrl;
            this.modVersion = modVersion;
            this.modVersionSem = modVersionSem;
            this.modVersionRel = modVersionRel;
            this.modVersionPostfix = modVersionPostfix;
            modVersionSemFull = modVersionSem + (modVersionPostfix.Length > 0 ? $"-{modVersionPostfix}" : "");
            this.modDescription = modDescription;
            this.modDependencies = modDependencies;
        }

        private ManifestData(
            string modName,
            string modGuid,
            string websiteUrl,
            string modVersion,
            string modVersionSem,
            string modVersionSemFull,
            ushort modVersionRel,
            string modVersionPostfix,
            string modDescription,
            string[] modDependencies
        ) {
            this.modName = modName;
            this.modGuid = modGuid;
            this.websiteUrl = websiteUrl;
            this.modVersion = modVersion;
            this.modVersionSem = modVersionSem;
            this.modVersionRel = modVersionRel;
            this.modVersionPostfix = modVersionPostfix;
            this.modVersionSemFull = modVersionSemFull;
            this.modDescription = modDescription;
            this.modDependencies = modDependencies;
        }

        public static ManifestData GatherManifestData(string srcFile) {
            XDocument doc = CsprojExtractor.LoadXmlDocument(srcFile);

            string modName = CsprojExtractor.ExtractCsprojProperty(doc, "AssemblyName") ?? Path.GetFileNameWithoutExtension(srcFile);
            string modGuid = CsprojExtractor.ExtractCsprojProperty(doc, "GUID") ?? throw new NullReferenceException("GUID cannot be null!");

            string url = CsprojExtractor.ExtractCsprojProperty(doc, "RepositoryUrl") ?? "";

            string description = CsprojExtractor.ExtractCsprojProperty(doc, "Description") ?? "";

            string[] modDependencies = [];

            string fullVersion = CsprojExtractor.GetFullVersionString(doc);
            ushort versionRelNumber = CsprojExtractor.GetVersionRelNumber(doc);
            string[] versionPFixes = fullVersion.Split('-');
            string modVersionPrefix = versionPFixes[0];
            string modVersionSuffix = versionPFixes.Length > 1 ? versionPFixes[1] : "";

            RawFVer rawVer = FVerParse.GetRawFVer(fullVersion, versionRelNumber);
            string modFVersion = new FVersion(rawVer.baseline, rawVer.revision, rawVer.release, rawVer.prefix, rawVer.postfix);

            return new(
                modName,
                modGuid,
                url,
                modFVersion,
                modVersionPrefix,
                fullVersion,
                versionRelNumber,
                modVersionSuffix,
                description,
                modDependencies
            );
        }
    }
}
