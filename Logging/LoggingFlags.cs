/// Copyright 2024 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

using System;

namespace CupheadArchipelago {
    [Flags]
    public enum LoggingFlags {
        None = 0x0,
        PluginInfo = 0x1,
        Info = 0x2,
        Message = 0x4,
        Warning = 0x8,
        Network = 0x16,
        Debug = 0x32,
        Transpiler = 0x64,
        All = PluginInfo|Info|Message|Warning|Network|Debug|Transpiler
    }
}
