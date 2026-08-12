/// Copyright 2025-2026 JKLeckr
/// SPDX-License-Identifier: GPL-3.0-or-later

using System;
using System.Xml.Linq;

namespace CupheadArchipelago.Helpers.CsprojParser {
    public class CsprojExtractor {
        public static string? ExtractCsprojProperty(string csprojPath, string propertyName) {
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

        public static string GetFullVersionString(string csprojPath) {
            string prefix = ExtractCsprojProperty(csprojPath, "VersionPrefix") ?? throw new NullReferenceException("VersionPrefix is null");
            string suffix = ExtractCsprojProperty(csprojPath, "VersionSuffix") ?? "";

            return prefix + (suffix.Length > 0 ? "-" : "") + suffix;
        }

        public static ushort GetVersionRelNumber(string csprojPath) {
            try {
                string str = ExtractCsprojProperty(csprojPath, "VersionRelNumber") ?? "";
                return ushort.Parse(str);
            } catch {
                return 0;
            }
        }
    }
}
