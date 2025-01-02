/// Copyright 2024 JKLeckr
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
        EndCutscene = 16,
        DLCIntro = 32,
        DLCSaltbakerIntro = 64,
        //DLCFullIntro = 96,
        DLCEndCutscene = 128,
    }
}
