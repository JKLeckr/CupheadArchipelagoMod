/// Copyright 2025 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

using System;

namespace CupheadArchipelago.AP {
    public struct APVersion : IComparable<APVersion> {
        public readonly int major;
        public readonly int minor;
        public readonly int patch;
        public readonly string pre;

        public APVersion() {
            major = 0;
            minor = 0;
            patch = 0;
            pre = "";
        }

        public APVersion(int major, int minor, int patch, string pre = null) {
            if (major < 0 || minor < 0 || patch < 0)
                throw new ArgumentOutOfRangeException("Version numbers must not be negative");

            this.major = major;
            this.minor = minor;
            this.patch = patch;
            this.pre = pre;
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

            this.major = major;
            this.minor = minor;
            this.patch = patch;
            pre = (mainParts.Length>1) ? mainParts[1] : "";
        }

        public int CompareTo(APVersion other) {
            if (other == null) return 1;

            int res = major.CompareTo(other.major);
            if (res != 0) return res;

            res = minor.CompareTo(other.minor);
            if (res != 0) return res;

            res = patch.CompareTo(other.patch);
            if (res != 0) return res;

            if (pre == null && other.pre != null) return 1;
            if (pre != null && other.pre == null) return -1;
            if (pre != null && other.pre != null) {
                res = StringComparer.Ordinal.Compare(pre, other.pre);
                if (res != 0) return res;
            }

            return 0;
        }

        public override bool Equals(object obj) =>
            obj is APVersion other && CompareTo(other) == 0;

        public override int GetHashCode() {
            unchecked {
                int hash = 17;
                hash *= 29 + major.GetHashCode();
                hash *= 29 + minor.GetHashCode();
                hash *= 29 + patch.GetHashCode();
                hash *= 29 + (pre?.GetHashCode() ?? 0);
                return hash;
            }
        }

        public override string ToString() {
            var version = $"{major}.{minor}.{patch}";
            if (!string.IsNullOrEmpty(pre))
                version += $"-{pre}";

            return version;
        }

        public static bool operator ==(APVersion a, APVersion b) => a.Equals(b);
        public static bool operator !=(APVersion a, APVersion b) => !(a == b);
        public static bool operator <(APVersion a, APVersion b) => a.CompareTo(b) < 0;
        public static bool operator >(APVersion a, APVersion b) => a.CompareTo(b) > 0;
        public static bool operator <=(APVersion a, APVersion b) => a.CompareTo(b) <= 0;
        public static bool operator >=(APVersion a, APVersion b) => a.CompareTo(b) >= 0;
}

}
