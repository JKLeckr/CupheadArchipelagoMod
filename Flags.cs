/// Copyright 2025 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

using System;

namespace CupheadArchipelago {
    [Flags]
    public enum Cutscenes {
        None = 0,
        Intro = 1,
        KettleIntro = 2,
        //FullIntro = 3,
        DieHouseCutscenes = 4,
        EndCutscene = 64,
        DLCIntro = 256, //TODO Add
        DLCSaltbakerIntro = 512, //TODO Add
        //DLCFullIntro = 768,
        DLCEndCutscene = 8192, //TODO Add
        //All = 32767,
    }

    [Flags]
    public enum APStatsFunctions {
        None = 0,
        ConnectionIndicator = 1,
    }
}
