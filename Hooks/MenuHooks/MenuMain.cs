/// Copyright 2024 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

namespace CupheadArchipelago.Hooks.MenuHooks {
    public class MenuMain {
        public static void Hook() {
            SlotSelectScreenHook.Hook();
            SlotSelectScreenSlotHook.Hook();
        }
    }
}