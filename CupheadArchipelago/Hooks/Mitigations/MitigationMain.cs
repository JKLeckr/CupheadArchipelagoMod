/// Copyright 2025-2026 JKLeckr
/// SPDX-License-Identifier: GPL-3.0-or-later

namespace CupheadArchipelago.Hooks.Mitigations {
    internal class MitigationMain {
        internal static void Hook() {
            VSyncMitigationHook.Hook();
        }
    }
}
