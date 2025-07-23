/// Copyright 2025 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

using CupheadArchipelago.Unity;

namespace CupheadArchipelago.AP {
    internal class APLevelItemMngr {
        internal static void ApplyLevelItem(long itemId) {
            if (APClient.GetAppliedItemCount(itemId) >= APClient.GetReceivedItemCount(itemId)) {
                Logging.Log($"[APLevelItemMngr] Item is already applied. Skipping applying item.");
                return;
            }

            if (itemId == APItem.level_extrahealth) {
                PlayerStatsManagerInterface.AddHealth(PlayerId.PlayerOne, 1);
                if (PlayerManager.Multiplayer) {
                    PlayerStatsManagerInterface.AddHealth(PlayerId.PlayerTwo, 1);
                }
            }
            else if (itemId == APItem.level_supercharge) {
                Logging.Log("Setting Player 1 Super");
                PlayerStatsManagerInterface.FillSuper(PlayerId.PlayerOne);
                if (PlayerManager.Multiplayer) {
                    Logging.Log("Setting Player 2 Super");
                    PlayerStatsManagerInterface.FillSuper(PlayerId.PlayerTwo);
                }
            }
            else if (itemId == APItem.level_fastfire) {
                //AudioManager.Play("pop_up");
                APManager.Current.SlowFire();
            }
            else if (itemId == APItem.level_trap_fingerjam) {
                //AudioManager.Play("level_menu_select");
                APManager.Current.FingerJam();
            }
            else if (itemId == APItem.level_trap_slowfire) {
                //AudioManager.Play("level_menu_select");
                APManager.Current.SlowFire();
            }
            else if (itemId == APItem.level_trap_superdrain) {
                //AudioManager.Play("level_menu_select");
                PlayerStatsManagerInterface.SetSuper(PlayerId.PlayerOne, 0);
                if (PlayerManager.Multiplayer) {
                    PlayerStatsManagerInterface.SetSuper(PlayerId.PlayerTwo, 0);
                }
            }
            else if (itemId == APItem.level_trap_loadout) {
                //AudioManager.Play("level_menu_select");
                Stub("Loadout Trap");
            }
            else {
                Logging.LogError($"[APLevelItemMngr] Cannot apply item {itemId}! Unregistered Level Item.");
                return;
            }
            APClient.AddAppliedItem(itemId, 1);
        }

        private static void Stub(string itemName) => Logging.LogWarning($"Item handling unimplemented: {itemName}");
    }
}
