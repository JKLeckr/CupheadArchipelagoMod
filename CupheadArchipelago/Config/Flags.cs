/// Copyright 2025 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

using System;

namespace CupheadArchipelago.Config {
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
}
