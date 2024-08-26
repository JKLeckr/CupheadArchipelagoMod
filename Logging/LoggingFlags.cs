/// Copyright 2024 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

using System;

namespace CupheadArchipelago {
    [Flags]
    public enum LoggingFlags {
        None = 0,
        PluginInfo = 1,
        Info = 2,
        Message = 4,
        Warning = 8,
        Network = 16,
        Debug = 32,
        Transpiler = 64,
    }
}
