/// Copyright 2024 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

using System;

namespace CupheadArchipelago {
    [Flags]
    public enum LoggingFlags {
        None = 0x0,
        PluginInfo = 0x1,
        Info = 0x2,
        Warning = 0x4,
        Network = 0x8,
        Debug = 0x16,
        Transpiler = 0x32,
        All = PluginInfo|Info|Warning|Network|Debug|Transpiler
    }
}
