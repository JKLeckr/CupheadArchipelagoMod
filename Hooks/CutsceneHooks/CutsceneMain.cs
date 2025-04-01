/// Copyright 2025 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

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
