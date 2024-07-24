/// Copyright 2024 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

using System;

namespace CupheadArchipelago {
    [Flags]
    public enum LoggingFlags {
        None = 0x0,
        PluginInfo = 0x1,
        Info = 0x2,
        Network = 0x4,
        Debug = 0x8,
        Transpiler = 0x16,
        All = PluginInfo|Info|Network|Debug|Transpiler
    }
}
