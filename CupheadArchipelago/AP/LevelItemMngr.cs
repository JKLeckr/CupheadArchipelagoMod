/// Copyright 2025 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

using CupheadArchipelago.Hooks.PlayerHooks;
using CupheadArchipelago.Unity;

namespace CupheadArchipelago.AP {
    internal class LevelItemMngr {
        internal static void ApplyLevelItem(long itemId) {
            PlayerStatsManager stats1 = PlayerStatsManagerHook.CurrentStatMngr1;
            PlayerStatsManager stats2 = PlayerStatsManagerHook.CurrentStatMngr2;
            PlayersStatsBossesHub bstats1 = Level.GetPlayerStats(stats1.basePlayer.id);
            PlayersStatsBossesHub bstats2 = (stats2 != null) ? Level.GetPlayerStats(stats2.basePlayer.id) : null;

            if (itemId == APItem.level_extrahealth) {
                if (Level.IsInBossesHub) {
                    bstats1.BonusHP++;
                    if (bstats2 != null) bstats2.BonusHP++;
                }
                stats1.SetHealth(stats1.Health + 1);
                stats2?.SetHealth(stats2.Health + 1);
            }
            else if (itemId == APItem.level_supercharge) {
                if (stats1.CanGainSuperMeter) {
                    Logging.Log("Can gain super");
                    PlayerStatsManagerHook.SetSuper(stats1, PlayerStatsManagerHook.DEFAULT_SUPER_FILL_AMOUNT);
                }
                if (stats2 != null && stats2.CanGainSuperMeter) PlayerStatsManagerHook.SetSuper(stats2, PlayerStatsManagerHook.DEFAULT_SUPER_FILL_AMOUNT);
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
                if (stats1.CanGainSuperMeter) PlayerStatsManagerHook.SetSuper(stats1, 0);
                if (stats2 != null && stats2.CanGainSuperMeter) PlayerStatsManagerHook.SetSuper(stats2, 0);
            }
            else if (itemId == APItem.level_trap_loadout) {
                //AudioManager.Play("level_menu_select");
                Stub("Loadout Trap");
            }
        }

        private static void Stub(string itemName) => Logging.LogWarning($"Item handling unimplemented: {itemName}");
    }
}
