/// Copyright 2025-2026 JKLeckr
/// SPDX-License-Identifier: GPL-3.0-or-later

using System;

namespace CupheadArchipelago.Helpers.FVerParser {
    public class FVerParse {
        // TEMP. This will be changed when entering main branch version.
        public static RawFVer GetRawFVer(string ver, ushort rel = 0) {
            string[] versionParts = ver.Split(['.', '-'], 5);
            if (int.Parse(versionParts[0]) > 0) {
                throw new Exception("Version parsing system needs to be changed for main version!");
            }
            int pre = int.Parse(versionParts[1]);
            string pres = pre switch {
                1 => "preview",
                2 => "alpha",
                3 => "beta",
                4 => "rc",
                _ => "unknown"
            };
            int baseline = int.Parse(versionParts[2]) + 1;
            int rev = int.Parse(versionParts[3]);
            string postfix = versionParts.Length > 4 ? versionParts[4] : "";
            RawFVer fver = new(baseline, rev, rel, pres, postfix);
            return fver;
        }
    }
}
