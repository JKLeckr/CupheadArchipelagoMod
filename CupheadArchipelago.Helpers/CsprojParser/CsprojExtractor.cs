/// Copyright 2025-2026 JKLeckr
/// SPDX-License-Identifier: GPL-3.0-or-later

using System;
using System.Xml.Linq;

namespace CupheadArchipelago.Helpers.CsprojParser {
    public class CsprojExtractor {
        public static XDocument LoadXmlDocument(string csprojPath) {
            return XDocument.Load(csprojPath);
        }

        public static string? ExtractCsprojProperty(XDocument csprojDoc, string propertyName) {
            try {
                XNamespace ns = csprojDoc?.Root?.Name.Namespace ?? "";
                XElement? property = csprojDoc?.Root?
                    .Element(ns + "PropertyGroup")?
                    .Element(ns + propertyName);

                return property?.Value.Trim();
            }
            catch {
                return null;
            }
        }

        public static string GetFullVersionString(XDocument csprojDoc) {
            string prefix = ExtractCsprojProperty(csprojDoc, "VersionPrefix") ?? throw new NullReferenceException("VersionPrefix is null");
            string suffix = ExtractCsprojProperty(csprojDoc, "VersionSuffix") ?? "";

            return prefix + (suffix.Length > 0 ? "-" : "") + suffix;
        }

        public static ushort GetVersionRelNumber(XDocument csprojDoc) {
            try {
                string str = ExtractCsprojProperty(csprojDoc, "VersionRelNumber") ?? "";
                return ushort.Parse(str);
            } catch {
                return 0;
            }
        }
    }
}
