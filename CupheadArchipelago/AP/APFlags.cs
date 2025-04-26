/// Copyright 2025 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

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
        Abilities = 32,
        AimAbilities = 64,
        All = 255,
    }
}
