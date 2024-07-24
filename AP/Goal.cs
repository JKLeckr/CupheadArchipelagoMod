/// Copyright 2024 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

using System;

namespace CupheadArchipelago.AP {
    [Flags]
    public enum Goal {
        None = 0,
        Devil = 1,
        Saltbaker = 2,
        All = int.MaxValue,
    }
}