/// Copyright 2025 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

namespace CupheadArchipelago {
    public class Config {
        private static Config current;

        public Cutscenes ConfigSkipCutscenes { get; private set; }

        internal static void Init(Cutscenes configSkipCutscenes) {
            current = new() {
                ConfigSkipCutscenes = configSkipCutscenes
            };
        }

        public static bool IsSkippingCutscene(Cutscenes cutscene) => (current.ConfigSkipCutscenes&cutscene)>0;
    }
}
