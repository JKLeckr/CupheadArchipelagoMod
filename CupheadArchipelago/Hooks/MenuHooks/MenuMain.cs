/// Copyright 2025-2026 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

namespace CupheadArchipelago.Hooks.MenuHooks {
    public class MenuMain {
        public static void Hook() {
            SlotSelectScreenHook.Hook();
            SlotSelectScreenSlotHook.Hook();
        }
    }
}
