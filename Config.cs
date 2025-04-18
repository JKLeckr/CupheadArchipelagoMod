/// Copyright 2025 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

using System;
using CupheadArchipelago.AP;

namespace CupheadArchipelago {
    [Flags]
    public enum Cutscenes {
        None = 0,
        Intro = 1,
        KettleIntro = 2,
        //FullIntro = 3,
        DieHouseCutscenes = 4,
        KingdiceIntro = 16, //TODO Add
        FinalIntro = 32, //TODO Add
        EndCutscene = 64,
        DLCIntro = 256,
        DLCSaltbakerIntro = 512,
        //DLCFullIntro = 768,
        DLCFinalBossIntro = 4096, //TODO Add
        DLCEndCutscene = 8192, //TODO Add
        //All = 32767,
    }

    [Flags]
    public enum APStatsFunctions {
        None = 0,
        ConnectionIndicator = 1,
    }

    public class Config {
        private static Config current;

        private bool configDebugAsInfo;
        private Cutscenes configSkipCutscenes;
        private bool configSkipCutscenesAPOnly;
        private bool configFileDeleteClearsAP;
        private APStatsFunctions configAPStatusFunctions;

        //public static Cutscenes SkipCutscenes { get => current.configSkipCutscenes; }
        //public static bool SkipCutscenesAPOnly { get => current.configSkipCutscenesAPOnly; }

        internal static void Init(
            bool configDebugAsInfo,
            Cutscenes configSkipCutscenes,
            bool configSkipCutscenesAPOnly,
            bool configFileDeleteClearsAP,
            APStatsFunctions configAPStatusFunctions
        ) {
            current = new() {
                configDebugAsInfo = configDebugAsInfo,
                configSkipCutscenes = configSkipCutscenes,
                configSkipCutscenesAPOnly = configSkipCutscenesAPOnly,
                configFileDeleteClearsAP = configFileDeleteClearsAP,
                configAPStatusFunctions = configAPStatusFunctions,
            };
        }

        public static bool IsDebugLogsInfo() => current.configDebugAsInfo;
        public static bool IsSkippingCutscene(Cutscenes cutscene) {
            return IsSkippingCutscene(cutscene, APData.IsCurrentSlotEnabled());
        }
        public static bool IsSkippingCutscene(Cutscenes cutscene, bool apEnabledCondition) {
            if (!current.configSkipCutscenesAPOnly || apEnabledCondition)
                return (current.configSkipCutscenes & cutscene) == cutscene;
            else return false;
        }
        public static bool ResetAPConfigOnFileDelete() {
            return current.configFileDeleteClearsAP;
        }
        public static bool IsAPStatsFunctionEnabled(APStatsFunctions function) {
            return (current.configAPStatusFunctions & function) == function;
        }
    }
}
