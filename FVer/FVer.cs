/// Copyright 2025 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

/// FVer version 01a

using System;

namespace FVer {
    public sealed class FVersion : IComparable<FVersion> {
        public readonly int Baseline;
        public string Revision => IntToRevision(_revision);
        public int RevisionNumber => _revision;
        public readonly int Release;
        public readonly string Prefix;
        public readonly string Postfix;

        private readonly int _revision;

        public FVersion(int baseline, string revision, int release, string prefix = null, string postfix = null) {
            if (baseline < 0 || release < 0)
                throw new ArgumentOutOfRangeException("Version numbers must not be negative");
            if (string.IsNullOrEmpty(revision))
                throw new ArgumentNullException("Revision cannot be null");

            _revision = RevisionToInt(revision);

            Baseline = baseline;
            Release = release;
            Prefix = prefix ?? "";
            Postfix = postfix ?? "";
        }
        public FVersion(int baseline, int revision, int release, string prefix = null, string postfix = null) {
            if (baseline < 0 || revision < 0 || release < 0)
                throw new ArgumentOutOfRangeException("Version numbers must not be negative");

            _revision = revision;
            Baseline = baseline;
            Release = release;
            Prefix = prefix ?? "";
            Postfix = postfix ?? "";
        }

        public FVersion(string version) {
            if (string.IsNullOrEmpty(version) || version.Trim().Length == 0)
                throw new ArgumentNullException(nameof(version));

            string postfix = "";
            int dashIndex = version.IndexOf('-');
            if (dashIndex >= 0) {
                postfix = version.Substring(dashIndex+1);
                version = version.Substring(0, dashIndex);
            }
            
            string prefix = "";
            int digitIndex = -1;
            for (int i=0;i<version.Length;i++) {
                if (char.IsDigit(version[i])) {
                    digitIndex = i;
                    break;
                }
            }

            if (digitIndex > 0) {
                prefix = version.Substring(0, digitIndex);
            }
            else if (digitIndex < 0) {
                throw new FormatException("No baseline digits found in version string.");
            }

            version = version.Substring(digitIndex);

            int index = 0;
            while (index < version.Length && char.IsDigit(version[index])) {
                index++;
            }
            if (index < 2) {
                throw new FormatException("Baseline must contain at least two digits.");
            }

            int baseline = int.Parse(version.Substring(0, index));

            int revStart = index;
            int revLen = 0;
            while (revStart + revLen < version.Length && char.IsLetter(version[revStart + revLen])) {
                revLen++;
            }
            if (revLen == 0) {
                throw new FormatException("No alphabetical revision segment found.");
            }

            string rev = version.Substring(revStart, revLen);

            int rel = 0;
            int nextIndex = revStart + revLen;
            if (nextIndex < version.Length && version[nextIndex] == '.') {
                nextIndex++;
                int relStart = nextIndex;
                while (nextIndex < version.Length && char.IsDigit(version[nextIndex])) {
                    nextIndex++;
                }
                
                if (relStart == nextIndex) {
                    throw new FormatException("Expected release number after '.'");
                }

                rel = int.Parse(version.Substring(relStart, nextIndex));
            }

            if (nextIndex < version.Length) {
                throw new FormatException(
                    $"Unexpected trailing characters in version string after release number: '{version.Substring(nextIndex)}'"
                );
            }

            Baseline = baseline;
            _revision = RevisionToInt(rev);
            Release = rel;
            Prefix = prefix;
            Postfix = postfix;
        }

        private static int RevisionToInt(string rev) {
            int val = 0;
            foreach (char c in rev) {
                if (c < 'a' || c > 'z')
                    throw new FormatException("Invalid revision character");

                int digit = c - 'a' + 1;
                if (val > (int.MaxValue - digit) / 26)
                    throw new OverflowException("Revision string is too large to fit in Int32");
                
                val = val * 26 + digit;
            }
            return val - 1;
        }
        private static string IntToRevision(int i) {
            if (i < 0) throw new ArgumentOutOfRangeException(nameof(i));
            int num = i + 1;

            char[] buf = new char[16];
            int index = buf.Length;

            while (num > 0) {
                num--;
                buf[--index] = (char)('a' + (num % 26));
                num /= 26;
            }

            return new string(buf, index, buf.Length - index);
        }

        public static FVersion Zero() {
            return new FVersion(0, 0, 0, null, null);
        }

        public int CompareTo(FVersion other) {
            if (other == null) return 1;

            int res = string.IsNullOrEmpty(Prefix).CompareTo(string.IsNullOrEmpty(other.Prefix));
            if (res != 0) return res;

            res = Baseline.CompareTo(other.Baseline);
            if (res != 0) return res;

            res = RevisionToInt(Revision).CompareTo(RevisionToInt(other.Revision));
            if (res != 0) return res;

            res = Release.CompareTo(other.Release);
            if (res != 0) return res;

            res = string.IsNullOrEmpty(Postfix).CompareTo(string.IsNullOrEmpty(other.Postfix));
            if (res != 0) return res;

            return ComparePostfix(Postfix, other.Postfix);
        }
        private static int ComparePostfix(string a, string b) {
            string[] aParts = a.Split('.');
            string[] bParts = b.Split('.');

            int len = Math.Max(a.Length, b.Length);
            for (int i=0;i<len;i++) {
                bool aOut = i >= aParts.Length;
                bool bOut = i >= bParts.Length;

                if (aOut && bOut) return 0;
                if (aOut) return -1;
                if (bOut) return 1;

                string aSeg = aParts[i];
                string bSeg = bParts[i];

                bool aIsNum = int.TryParse(aSeg, out int aNum);
                bool bIsNum = int.TryParse(bSeg, out int bNum);

                if (aIsNum && bIsNum) {
                    int numComp = aNum.CompareTo(bNum);
                    if (numComp != 0) return numComp;
                }
                else if (aIsNum) {
                    return -1;
                }
                else if (bIsNum) {
                    return 1;
                }
                else {
                    int textComp = string.Compare(aSeg, bSeg, StringComparison.Ordinal);
                    if (textComp != 0) return 0;
                }
            }

            return 0;
        }

        public override bool Equals(object obj) =>
            obj is FVersion other && CompareTo(other) == 0;

        public override int GetHashCode() {
            unchecked {
                int hash = 17;
                hash *= 29 + Baseline.GetHashCode();
                hash *= 29 + _revision.GetHashCode();
                hash *= 29 + Release.GetHashCode();
                hash *= 29 + (Prefix?.GetHashCode() ?? 0);
                hash *= 29 + (Postfix?.GetHashCode() ?? 0);
                return hash;
            }
        }

        public override string ToString() {
            string res = $"{Prefix ?? ""}{Baseline:D2}{Revision}";
            if (Release > 0)
                res += $".{Release}";
            if (!string.IsNullOrEmpty(Postfix))
                res += $"-{Postfix}";
            return res;
        }

        public static bool operator ==(FVersion a, FVersion b) => a.Equals(b);
        public static bool operator !=(FVersion a, FVersion b) => !(a == b);
        public static bool operator <(FVersion a, FVersion b) => a.CompareTo(b) < 0;
        public static bool operator >(FVersion a, FVersion b) => a.CompareTo(b) > 0;
        public static bool operator <=(FVersion a, FVersion b) => a.CompareTo(b) <= 0;
        public static bool operator >=(FVersion a, FVersion b) => a.CompareTo(b) >= 0;

        public static implicit operator string(FVersion v) => v.ToString();
    }
}
