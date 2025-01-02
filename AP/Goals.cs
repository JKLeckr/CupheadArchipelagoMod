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
}