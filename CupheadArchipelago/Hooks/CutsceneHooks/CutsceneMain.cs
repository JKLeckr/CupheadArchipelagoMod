/// Copyright 2025-2026 JKLeckr
/// SPDX-License-Identifier: GPL-3.0-or-later

namespace CupheadArchipelago.Hooks.CutsceneHooks {
    public class CutsceneMain {
        public static void Hook() {
            KingDiceCutsceneHook.Hook();
            CreditsScreenHook.Hook();
            DLCCreditsComicCutsceneHook.Hook();
            DLCCreditsCutsceneHook.Hook();
        }
    }
}
