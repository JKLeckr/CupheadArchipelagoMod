/// Copyright 2025 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

namespace CupheadArchipelago.AP {
    public enum SaveDataType {
        Auto = 0,
        Vanilla = 1,
        AP = 2,
    }

    public enum DeathLinkCauseType {
        Normal = 0,
        Boss = 1,
        Mausoleum = 2,
        Tutorial = 3,
        ChessCastle = 4,
        Graveyard = 5,
    }

    public enum GameModes {
        BeatDevil = 0,
        CollectContracts = 1,
        BuyOutShop = 2,
        DlcBeatSaltbaker = 3,
        DlcBeatBoth = 4,
        DlcCollectIngredients = 5,
        DlcCollectBoth = 6,
    }

    public enum WeaponModes : byte {
        Normal = 1,
        Progressive = 2,
        ProgressiveNoStart = 3,
    }

    public enum GradeChecks {
        Disabled = 0,
        AMinusGrade = 1,
        AGrade = 2,
        APlusGrade = 3,
        SGrade = 4,
        Pacifist = 5,
    }

    public enum APItemType {
        None = 0,
        Weapon = 1,
        Charm = 2,
        Super = 4,
        Ability = 8,
        Essential = 16,
        Special = 32,
        Level = 64,
        SpecialLevel = 96,
    }

    public enum DlcChaliceModes {
        Disabled = 0,
        Start = 1,
        Vanilla = 2,
        Randomized = 3,
    }

    public enum ChaliceCheckMode {
        Disabled = 0,
        Enabled = 1,
        Separate = 2,
    }

    public enum DlcCurseModes {
        Off = 0,
        Normal = 1,
        Reverse = 2,
        AlwaysOn = 3,
        AlwaysOnR = 4,
        AlwaysOn_1 = 5,
        AlwaysOn_2 = 6,
        AlwaysOn_3 = 7,
        AlwaysOn_4 = 8,
    }
}
