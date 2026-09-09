/// Copyright 2025-2026 JKLeckr
/// SPDX-License-Identifier: GPL-3.0-or-later

using System;

namespace CupheadArchipelago.Helpers.GenManifest.GenManifestMain {
    public class Util {
        public static string SemVersionFourPartToThreePart(string semVersion, ushort semVersionRel, string postfix) {
            string[] vparts = semVersion.Split('.');

            if (vparts.Length != 4) {
                throw new ArgumentException("semVersion must have 4 parts");
            }

            ushort part3 = ushort.Parse(vparts[2]);
            byte part4 = byte.Parse(vparts[3]);

            if (part3 > 999) {
                throw new Exception("semVersion part 3 cannot exceed 999");
            }
            if (part4 > 99) {
                throw new Exception("semVersion part 4 cannot exceed 99");
            }
            if (semVersionRel > 99) {
                throw new Exception("semVersionRel cannot exceed 99");
            }

            byte format = 1;
            int npart3 =
                (format * 10000000) +
                (part3 * 10000) +
                (part4 * 100) +
                semVersionRel + (postfix.Length == 0 ? 10 : 0);

            return $"{vparts[0]}.{vparts[1]}.{npart3}";
        }
    }
}
