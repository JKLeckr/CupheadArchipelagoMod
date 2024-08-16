/// Copyright 2024 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

using System;

namespace CupheadArchipelago.AP {
    [Flags]
    public enum Goals {
        None = 0,
        Devil = 1,
        Saltbaker = 2,
        DevilAndSaltbaker = 3,
        All = int.MaxValue,
    }
}