/// Copyright 2025-2026 JKLeckr
/// SPDX-License-Identifier: GPL-3.0-or-later

namespace CupheadArchipelago.Hooks {
    public class Main {
        public static void HookMain() {
            StartScreenHook.Hook();

            Mitigations.MitigationMain.Hook();

            RuntimeSceneAssetDatabaseHook.Hook();
            AssetHooks.AssetMain.Hook();
            SceneLoaderHook.Hook();
            CupheadHook.Hook();
            DLCManagerHook.Hook();
            PlayerDataHook.Hook();
            AbstractPauseGUIHook.Hook();
            WinScreenHook.Hook();

            AudioHooks.AudioMain.Hook();
            MenuHooks.MenuMain.Hook();
            PlayerHooks.PlayerMain.Hook();
            MapHooks.MapMain.Hook();
            LevelHooks.LevelMain.Hook();
            ShopHooks.ShopMain.Hook();
            CutsceneHooks.CutsceneMain.Hook();
        }

        public static void HookSaveKeyUpdater(string saveKeyName) {
            SaveKeyUpdaterHook.SetSaveKeyBaseName(saveKeyName);
            SaveKeyUpdaterHook.Hook();
        }
    }
}
