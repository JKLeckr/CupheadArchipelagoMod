/// Copyright 2025-2026 JKLeckr
/// SPDX-License-Identifier: GPL-3.0-or-later

using System;

namespace CupheadArchipelago {
    [Flags]
    public enum LoggingFlags : byte {
        None = 0,
        PluginInfo = 1,
        Info = 2,
        Message = 4,
        Warning = 8,
        Network = 16,
        Debug = 32,
        //Transpiler = 64,
        //All = byte.MaxValue,
    }
}
