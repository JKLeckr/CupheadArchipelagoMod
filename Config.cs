/// Copyright 2025 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

using CupheadArchipelago.AP;

namespace CupheadArchipelago {
    public class Config {
        private static Config current;

        private Cutscenes configSkipCutscenes;
        private bool configSkipCutscenesAPOnly;
        private bool configFileDeleteClearsAP;
        private APStatsFunctions configAPStatusFunctions;

        //public static Cutscenes SkipCutscenes { get => current.configSkipCutscenes; }
        //public static bool SkipCutscenesAPOnly { get => current.configSkipCutscenesAPOnly; }

        internal static void Init(
            Cutscenes configSkipCutscenes,
            bool configSkipCutscenesAPOnly,
            bool configFileDeleteClearsAP,
            APStatsFunctions configAPStatusFunctions
        ) {
            current = new() {
                configSkipCutscenes = configSkipCutscenes,
                configSkipCutscenesAPOnly = configSkipCutscenesAPOnly,
                configFileDeleteClearsAP = configFileDeleteClearsAP,
                configAPStatusFunctions = configAPStatusFunctions,
            };
        }

        public static bool IsSkippingCutscene(Cutscenes cutscene) {
            return IsSkippingCutscene(cutscene, APData.IsCurrentSlotEnabled());
        }
        public static bool IsSkippingCutscene(Cutscenes cutscene, bool apEnabledCondition) {
            if (!current.configSkipCutscenesAPOnly || apEnabledCondition)
                return (current.configSkipCutscenes & cutscene) > 0;
            else return false;
        }
        public static bool DeleteAPConfigOnFileDelete() {
            return current.configFileDeleteClearsAP;
        }
        public static bool IsAPStatsFunctionEnabled(APStatsFunctions function) {
            return (current.configAPStatusFunctions & function) > 0;
        }
    }
}
