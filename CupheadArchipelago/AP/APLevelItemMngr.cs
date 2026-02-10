/// Copyright 2025-2026 JKLeckr
/// SPDX-License-Identifier: GPL-3.0-or-later

using CupheadArchipelago.Unity;

namespace CupheadArchipelago.AP {
    internal class APLevelItemMngr {
        internal static void ApplyLevelItem(long itemId) {
            if (APClient.GetAppliedItemCount(itemId) >= APClient.GetReceivedItemCount(itemId)) {
                Logging.Log($"[APLevelItemMngr] Item is already applied. Skipping applying item.");
                return;
            }

            // TODO: Add a way to handle if filler items did not apply at all (bools)

            if (itemId == APItem.level_extrahealth) {
                PlayerStatsInterface.AddHealth(PlayerId.PlayerOne, 1);
                if (PlayerManager.Multiplayer) {
                    PlayerStatsInterface.AddHealth(PlayerId.PlayerTwo, 1);
                }
            }
            else if (itemId == APItem.level_supercharge) {
                PlayerStatsInterface.FillSuper(PlayerId.PlayerOne);
                if (PlayerManager.Multiplayer) {
                    PlayerStatsInterface.FillSuper(PlayerId.PlayerTwo);
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
                PlayerStatsInterface.SetSuper(PlayerId.PlayerOne, 0);
                if (PlayerManager.Multiplayer) {
                    PlayerStatsInterface.SetSuper(PlayerId.PlayerTwo, 0);
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
