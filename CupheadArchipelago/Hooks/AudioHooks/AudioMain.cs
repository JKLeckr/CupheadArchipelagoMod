/// Copyright 2025 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

namespace CupheadArchipelago.Hooks.AudioHooks {
    public class AudioMain {
        public static void Hook() {
            AudioManagerComponentHook.Hook();
        }
    }
}
