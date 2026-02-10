/// Copyright 2025-2026 JKLeckr
/// SPDX-License-Identifier: GPL-3.0-or-later

namespace CupheadArchipelago.Hooks.MenuHooks {
    public class MenuMain {
        public static void Hook() {
            SlotSelectScreenHook.Hook();
            SlotSelectScreenSlotHook.Hook();
        }
    }
}
