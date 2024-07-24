/// Copyright 2024 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

namespace CupheadArchipelago.AP {
    public class APSettings {
        public static bool UseDLC { get; internal set; } = false;
        public static bool Hard { get; internal set; } = false;
        public static bool FreemoveIsles { get; internal set; } = false;
        public static GradeChecks BossGradeChecks { get; internal set; } = GradeChecks.Disabled;
        public static GradeChecks RungunGradeChecks { get; internal set; } = GradeChecks.Disabled;
        public static bool DeathLink { get; internal set; } = false;
    }
}