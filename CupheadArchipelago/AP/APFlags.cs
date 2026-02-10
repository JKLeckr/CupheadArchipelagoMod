/// Copyright 2025-2026 JKLeckr
/// SPDX-License-Identifier: GPL-3.0-or-later

using System;

namespace CupheadArchipelago.AP {
    [Flags]
    public enum Goals {
        None = 0,
        Devil = 1,
        Saltbaker = 2,
        DevilAndSaltbaker = 3,
        Contracts = 4,
        Ingredients = 8,
        ContractsAndIngredients = 12,
        ShopBuyout = 16,
        All = int.MaxValue,
    }

    [Flags]
    public enum LevelTypes : byte {
        None = 0,
        Boss = 1,
        RunGun = 2,
        DicePalace = 4,
        Final = 8,
        ChessCastle = 16,
        All = byte.MaxValue
    }

    [Flags]
    public enum MusicGroups {
        None = 0,
        LevelMusic = 1,
        MapMusic = 2,
        LevelAndMapMusic = 3,
        All = 255
    }

    [Flags]
    public enum ItemGroups {
        None = 0,
        Essential = 1,
        Super = 2,
        CoreItems = Essential | Super,
        WeaponBasic = 4,
        WeaponEx = 8,
        Weapons = 12,
        Abilities = 32,
        AimAbilities = 64,
        All = 255,
    }

    [Flags]
    public enum WeaponParts : uint {
        None = 0,
        Basic = 1,
        Ex = 2,
        CBasic = 4,
        CEx = 8,
        AllBase = Basic | Ex,
        AllC = CBasic | CEx,
        AllBasic = Basic | CBasic,
        AllEx = Ex | CEx,
        All = Basic | Ex | CBasic | CEx
    }

    [Flags]
    public enum AimDirections : uint {
        None = 0,
        Left = 256,
        Right = 512,
        Up = 1024,
        Down = 2048,
        UpLeft = 4096,
        UpRight = 8192,
        DownLeft = 16384,
        DownRight = 32768,
        All = Left | Right | Up | Down | UpLeft | UpRight | DownLeft | DownRight
    }

    [Flags]
    internal enum Overrides {
        None = 0,
        DataVersionOverride = 1,
        SeedMismatchOverride = 2,
        DlcOverride = 4,
        OverrideResetOverride = 16,
        OverrideEnableDeathLink = 32,
        DamageOverrideP1 = 64,
        DamageOverrideP2 = 128,
        NukeOverride = 256,
        ClearReceivedItemsOverride = 512,
    }
}
