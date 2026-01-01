/// Copyright 2025-2026 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

namespace CupheadArchipelago.Helpers.FVerParser {
    public class RawFVer(int baseline, int revision, int release, string prefix, string postfix) {
        public readonly int baseline = baseline;
        public readonly int revision = revision;
        public readonly int release = release;
        public readonly string prefix = prefix;
        public readonly string postfix = postfix;
    }
}
