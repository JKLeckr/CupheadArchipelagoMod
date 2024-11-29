/// Copyright 2024 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

namespace CupheadArchipelago.AP {
    public enum DeathLinkCauseType {
        Normal = 0,
        Boss = 1,
        Mausoleum = 2,
        Tutorial = 3,
        ChessCastle = 4,
        Graveyard = 5,
    }

    public enum GameMode {
        BeatDevil = 0,
        CollectContracts = 1,
        DlcBeatSaltbaker = 2,
        DlcBeatBoth = 3,
        DlcCollectIngradients = 4,
        DlcCollectBoth = 5,
    }
    
    public enum GradeChecks {
        Disabled = 0,
        AMinusGrade = 1,
        AGrade = 2,
        APlusGrade = 3,
        SGrade = 4,
        Pacifist = 5,
    }

    public enum ItemType {
        NoType = 0,
        Weapon = 1,
        Charm = 2,
        Super = 4,
        Ability = 8,
        Essential = 16,
        Special = 32,
        Level = 64,
    }
}