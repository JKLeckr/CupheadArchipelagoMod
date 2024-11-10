/// Copyright 2024 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

using System;

namespace CupheadArchipelago.AP {
    public class APVersion : IComparable<APVersion> {
        public int Major { get; private set; }
        public int Minor { get; private set; }
        public int Patch { get; private set; }
        public string Pre { get; private set; }

        public APVersion() {
            Major = 0;
            Minor = 0;
            Patch = 0;
            Pre = "";
        }

        public APVersion(int major, int minor, int patch, string pre = null) {
            if (major < 0 || minor < 0 || patch < 0)
                throw new ArgumentOutOfRangeException("Version numbers must not be negative");

            Major = major;
            Minor = minor;
            Patch = patch;
            Pre = pre;
        }

        public APVersion(string version) {
            if (string.IsNullOrEmpty(version))
                throw new ArgumentNullException(nameof(version));
            
            string[] mainParts = version.Split(['-'], 2);
            string[] versionParts = mainParts[0].Split(['.'], 4);

            if (versionParts.Length > 3)
                mainParts[1].Insert(0, versionParts[3]+"-");
            
            int major = 0;
            int minor = 0;
            int patch = 0;

            try {
                int vlen = versionParts.Length;
                if (vlen > 0) major = int.Parse(versionParts[0]);
                if (vlen > 1) minor = int.Parse(versionParts[1]);
                if (vlen > 2) patch = int.Parse(versionParts[2]);
            } catch (Exception e) {
                throw new FormatException("Invalid version format", e);
            }

            Major = major;
            Minor = minor;
            Patch = patch;
            Pre = (mainParts.Length>1) ? mainParts[1] : "";
        }

        public int CompareTo(APVersion other) {
            if (other == null) return 1;

            int res = Major.CompareTo(other.Major);
            if (res != 0) return res;

            res = Minor.CompareTo(other.Minor);
            if (res != 0) return res;

            res = Patch.CompareTo(other.Patch);
            if (res != 0) return res;

            if (Pre == null && other.Pre != null) return 1;
            if (Pre != null && other.Pre == null) return -1;
            if (Pre != null && other.Pre != null) {
                res = StringComparer.Ordinal.Compare(Pre, other.Pre);
                if (res != 0) return res;
            }

            return 0;
        }

        public override bool Equals(object obj) =>
            obj is APVersion other && CompareTo(other) == 0;

        public override int GetHashCode() {
            unchecked {
                int hash = 17;
                hash *= 29 + Major.GetHashCode();
                hash *= 29 + Minor.GetHashCode();
                hash *= 29 + Patch.GetHashCode();
                hash *= 29 + (Pre?.GetHashCode() ?? 0);
                return hash;
            }
        }

        public override string ToString() {
            var version = $"{Major}.{Minor}.{Patch}";
            if (!string.IsNullOrEmpty(Pre))
                version += $"-{Pre}";

            return version;
        }

        public static bool operator ==(APVersion a, APVersion b) {
            if (a is null) return b is null;
            return a.Equals(b);
        }
        public static bool operator !=(APVersion a, APVersion b) => !(a == b);
        public static bool operator <(APVersion a, APVersion b) => a.CompareTo(b) < 0;
        public static bool operator >(APVersion a, APVersion b) => a.CompareTo(b) > 0;
        public static bool operator <=(APVersion a, APVersion b) => a.CompareTo(b) <= 0;
        public static bool operator >=(APVersion a, APVersion b) => a.CompareTo(b) >= 0;
}

}
