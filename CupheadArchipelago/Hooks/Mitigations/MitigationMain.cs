/// Copyright 2025 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

namespace CupheadArchipelago.Hooks.Mitigations {
    internal class MitigationMain {
        internal static void Hook() {
            VSyncMitigationHook.Hook();
        }
    }
}
