/// Copyright 2025-2026 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

using CupheadArchipelago.AP;

namespace CupheadArchipelago.Tests.Core {
    internal class TUtil {
        internal static void InitAPClient() {
            APClient.SetupOffline();
            APClient.ResetOffline();
            APClient.SetupOffline();
        }
    }
}
