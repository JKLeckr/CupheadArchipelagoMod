/// Copyright 2025 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

using CupheadArchipelago.AP;

namespace CupheadArchipelago {
    public class Config {
        private static Config current;

        public Cutscenes ConfigSkipCutscenes { get; private set; }
        public bool ConfigSkipCutscenesAPOnly { get; private set; }

        internal static void Init(Cutscenes configSkipCutscenes, bool configSkipCutscenesAPOnly) {
            current = new() {
                ConfigSkipCutscenes = configSkipCutscenes,
                ConfigSkipCutscenesAPOnly = configSkipCutscenesAPOnly,
            };
        }

        public static bool IsSkippingCutscene(Cutscenes cutscene) {
            return IsSkippingCutscene(cutscene, APData.IsCurrentSlotEnabled());
        }
        public static bool IsSkippingCutscene(Cutscenes cutscene, bool condition) {
            if (!current.ConfigSkipCutscenesAPOnly || condition)
                return (current.ConfigSkipCutscenes&cutscene)>0;
            else return false;
        }
    }
}
