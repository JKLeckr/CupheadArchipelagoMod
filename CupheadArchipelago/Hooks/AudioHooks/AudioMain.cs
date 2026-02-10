/// Copyright 2025-2026 JKLeckr
/// SPDX-License-Identifier: GPL-3.0-or-later

namespace CupheadArchipelago.Hooks.AudioHooks {
    public class AudioMain {
        public static void Hook() {
            AudioManagerComponentHook.Hook();
        }
    }
}
