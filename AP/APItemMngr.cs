/// Copyright 2025 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

using System;
using CupheadArchipelago.Hooks.PlayerHooks;
using CupheadArchipelago.Unity;

namespace CupheadArchipelago.AP {
    public class APItemMngr {
        public static bool ApplyItem(APItemData item) {
            long itemId = item.Id;
            string itemName = APClient.GetItemName(item.Id);
            Logging.Log($"[APItemMngr] Applying item {itemName} ({item.Id})...");

            ItemMap.GetItemType(itemId);

            try{
            switch (ItemMap.GetItemType(itemId)) {
                case ItemTypes.Weapon: {
                    Weapon weapon = ItemMap.GetWeapon(itemId);
                    Gift(weapon);
                    if (itemId==APItem.plane_gun && !IsChaliceSeparate(ItemGroups.Essential)) {
                        Gift(Weapon.plane_chalice_weapon_3way);
                    }
                    else if (itemId==APItem.plane_bombs && !IsChaliceSeparate(ItemGroups.Essential)) {
                        Gift(Weapon.plane_chalice_weapon_bomb);
                    }
                    if (APSettings.RandomizeWeaponEX == WeaponExModes.Off) {
                        if (itemId==APItem.plane_gun || itemId==APItem.plane_bombs) {
                            APClient.APSessionGSPlayerData.plane_ex = true;
                        }
                        if (itemId==APItem.dlc_cplane_gun || itemId==APItem.dlc_cplane_bombs) {
                            APClient.APSessionGSPlayerData.dlc_cplane_ex = true;
                        }
                    }
                    if ((PlayerData.Data.IsUnlocked(PlayerId.PlayerOne, Weapon.plane_weapon_peashot) && PlayerData.Data.IsUnlocked(PlayerId.PlayerOne, Weapon.plane_weapon_bomb)) ||
                        (PlayerData.Data.IsUnlocked(PlayerId.PlayerOne, Weapon.plane_chalice_weapon_3way) && PlayerData.Data.IsUnlocked(PlayerId.PlayerOne, Weapon.plane_chalice_weapon_bomb))) {
                            PlayerData.Data.Loadouts.GetPlayerLoadout(PlayerId.PlayerOne).HasEquippedSecondarySHMUPWeapon = true;
                            PlayerData.Data.Loadouts.GetPlayerLoadout(PlayerId.PlayerTwo).HasEquippedSecondarySHMUPWeapon = true;
                    }
                    break;
                }
                case ItemTypes.Charm: {
                    Charm charm = ItemMap.GetCharm(itemId);
                    Gift(charm);
                    break;
                }
                case ItemTypes.Super: {
                    Gift(ItemMap.GetSuper(itemId));
                    if (itemId==APItem.super_i && !IsChaliceSeparate(ItemGroups.Super)) {
                        Gift(Super.level_super_chalice_vert_beam);
                    }
                    else if (itemId==APItem.super_ii && !IsChaliceSeparate(ItemGroups.Super)) {
                        Gift(Super.level_super_chalice_shield);
                    }
                    else if (itemId==APItem.super_iii && !IsChaliceSeparate(ItemGroups.Super)) {
                        Gift(Super.level_super_chalice_iii);
                    }
                    break;
                }
                case ItemTypes.Ability: {
                    ApplyAbiltiy(itemId);
                    break;
                }
                case ItemTypes.Essential: {
                    if (itemId==APItem.coin) {
                        Logging.Log("AddCurrency");
                        AddCoins(1);
                    }
                    else if (itemId==APItem.coin2) {
                        Logging.Log("AddCurrency x2");
                        AddCoins(2);
                    }
                    else if (itemId==APItem.coin3) {
                        Logging.Log("AddCurrency x3");
                        AddCoins(3);
                    }
                    else if (itemId==APItem.contract) {
                        APClient.APSessionGSPlayerData.contracts++;
                        Logging.Log($"Contracts: {APClient.APSessionGSPlayerData.contracts}");
                        if ((APSettings.Mode & GameModes.CollectContracts) > 0) {
                            Logging.Log($"Contracts Goal: {APClient.APSessionGSPlayerData.contracts}");
                            if (APClient.APSessionGSPlayerData.contracts >= APSettings.ContractsGoal) {
                                APClient.GoalComplete(Goals.Contracts);
                            }
                        }
                    }
                    else if (itemId==APItem.plane_ex) {
                        APClient.APSessionGSPlayerData.plane_ex = true;
                        if (!IsChaliceSeparate(ItemGroups.Essential))
                            APClient.APSessionGSPlayerData.dlc_cplane_ex = true;
                    }
                    else if (itemId==APItem.plane_super) {
                        APClient.APSessionGSPlayerData.plane_super=true;
                        if (!IsChaliceSeparate(ItemGroups.Essential))
                            APClient.APSessionGSPlayerData.dlc_cplane_super = true;
                    }
                    else if (itemId==APItem.dlc_cplane_super) {
                        APClient.APSessionGSPlayerData.dlc_cplane_super=true;
                    }
                    else if (itemId==APItem.dlc_boat) {
                        APClient.APSessionGSPlayerData.dlc_boat=true;
                    }
                    else if (itemId==APItem.healthupgrade) {
                        APClient.APSessionGSPlayerData.healthupgrades++;
                        Logging.Log($"Health Upgrades: {APClient.APSessionGSPlayerData.healthupgrades}");
                    }
                    else if (itemId==APItem.dlc_ingredient) {
                        APClient.APSessionGSPlayerData.dlc_ingredients++;
                        Logging.Log($"Ingredients: {APClient.APSessionGSPlayerData.dlc_ingredients}");
                        if ((APSettings.Mode & GameModes.CollectContracts) > 0) {
                            Logging.Log($"Ingredients Goal: {APClient.APSessionGSPlayerData.dlc_ingredients}");
                            if (APClient.APSessionGSPlayerData.dlc_ingredients >= APSettings.DLCIngredientsGoal) {
                                APClient.GoalComplete(Goals.Ingredients);
                            }
                        }
                    }
                    break;
                }
                case ItemTypes.Special: {
                    break;
                }
                case ItemTypes.Level: {
                    PlayerStatsManager stats1 = PlayerStatsManagerHook.CurrentStatMngr1;
                    PlayerStatsManager stats2 = PlayerStatsManagerHook.CurrentStatMngr2;
                    PlayersStatsBossesHub bstats1 = Level.GetPlayerStats(stats1.basePlayer.id);
                    PlayersStatsBossesHub bstats2 = (stats2!=null) ? Level.GetPlayerStats(stats2.basePlayer.id) : null;
                    
                    if (itemId==APItem.level_extrahealth) {
                        if (Level.IsInBossesHub) {
                            bstats1.BonusHP++;
                            if (bstats2!=null) bstats2.BonusHP++;
                        }
                        stats1.SetHealth(stats1.Health + 1);
                        stats2?.SetHealth(stats2.Health + 1);
                    }
                    else if (itemId==APItem.level_superrecharge) {
                        if (stats1.CanGainSuperMeter) {
                            Logging.Log("Can gain super");
                            PlayerStatsManagerHook.SetSuper(stats1, PlayerStatsManagerHook.DEFAULT_SUPER_FILL_AMOUNT);
                        }
                        if (stats2!=null&&stats2.CanGainSuperMeter) PlayerStatsManagerHook.SetSuper(stats2, PlayerStatsManagerHook.DEFAULT_SUPER_FILL_AMOUNT);
                    }
                    else if (itemId==APItem.level_fastfire) {
                        AudioManager.Play("pop_up");
                        APManager.Current.SlowFire();
                    }
                    else if (itemId==APItem.level_trap_fingerjam) {
                        AudioManager.Play("level_menu_select");
                        APManager.Current.FingerJam();
                    }
                    else if (itemId==APItem.level_trap_slowfire) {
                        AudioManager.Play("level_menu_select");
                        APManager.Current.SlowFire();
                    }
                    else if (itemId==APItem.level_trap_superdrain) {
                        AudioManager.Play("level_menu_select");
                        if (stats1.CanGainSuperMeter) PlayerStatsManagerHook.SetSuper(stats1, 0);
                        if (stats2!=null&&stats2.CanGainSuperMeter) PlayerStatsManagerHook.SetSuper(stats2, 0);
                    }
                    else if (itemId==APItem.level_trap_loadout) {
                        AudioManager.Play("level_menu_select");
                        Stub("Loadout Trap");
                    }
                    break;
                }
                default: break;
            }
            } catch (Exception e) {Logging.LogError(e); return false;}
            return true;
        }

        private static void ApplyAbiltiy(long id) {
            if (id==APItem.ability_dash) {
                APClient.APSessionGSPlayerData.dash = true;
                if (!IsChaliceSeparate(ItemGroups.Abilities))
                    APClient.APSessionGSPlayerData.dlc_cdash = true;
            } else if (id==APItem.ability_duck) {
                APClient.APSessionGSPlayerData.duck = true;
                if (!IsChaliceSeparate(ItemGroups.Abilities))
                    APClient.APSessionGSPlayerData.dlc_cduck = true;
            } else if (id==APItem.ability_parry) {
                APClient.APSessionGSPlayerData.parry = true;
                if (!IsChaliceSeparate(ItemGroups.Abilities))
                    APClient.APSessionGSPlayerData.dlc_cparry = true;
            } else if (id==APItem.ability_plane_parry) {
                APClient.APSessionGSPlayerData.plane_parry = true;
                if (!IsChaliceSeparate(ItemGroups.Abilities))
                    APClient.APSessionGSPlayerData.dlc_cplane_parry = true;
            } else if (id==APItem.ability_plane_shrink) {
                APClient.APSessionGSPlayerData.plane_shrink = true;
                if (!IsChaliceSeparate(ItemGroups.Abilities))
                    APClient.APSessionGSPlayerData.dlc_cplane_shrink = true;
            } else if (id==APItem.ability_dlc_cdash) {
                APClient.APSessionGSPlayerData.dlc_cdash = true;
            } else if (id==APItem.ability_dlc_cduck) {
                APClient.APSessionGSPlayerData.dlc_cduck = true;
            } else if (id==APItem.ability_dlc_cparry) {
                APClient.APSessionGSPlayerData.dlc_cparry = true;
            } else if (id==APItem.ability_dlc_cplane_parry) {
                APClient.APSessionGSPlayerData.dlc_cplane_parry = true;
            } else if (id==APItem.ability_dlc_cplane_shrink) {
                APClient.APSessionGSPlayerData.dlc_cplane_shrink = true;
            }
        }

        private static void AddCoins(int count=1) {
            PlayerData.Data.AddCurrency(PlayerId.PlayerOne, count);
            PlayerData.Data.AddCurrency(PlayerId.PlayerTwo, count);
            Logging.Log($"Current coins: {PlayerData.Data.GetCurrency(PlayerId.PlayerOne)}");
            APClient.APSessionGSPlayerData.coins_collected += count;
            Logging.Log($"Total coins: {APClient.APSessionGSPlayerData.coins_collected}");
        }

        private static bool IsChaliceSeparate(ItemGroups group) {
            return (APSettings.DLCChaliceItemsSeparate & group) > 0;
        }

        private static void Gift(Weapon weapon) {
            if (PlayerData.Data.IsUnlocked(PlayerId.PlayerOne, weapon))
                PlayerData.Data.Gift(PlayerId.PlayerOne, weapon);
            if (PlayerData.Data.IsUnlocked(PlayerId.PlayerTwo, weapon))
                PlayerData.Data.Gift(PlayerId.PlayerTwo, weapon);
        }
        private static void Gift(Charm charm) {
            if (PlayerData.Data.IsUnlocked(PlayerId.PlayerOne, charm))
                PlayerData.Data.Gift(PlayerId.PlayerOne, charm);
            if (PlayerData.Data.IsUnlocked(PlayerId.PlayerTwo, charm))
                PlayerData.Data.Gift(PlayerId.PlayerTwo, charm);
        }
        private static void Gift(Super super) {
            if (PlayerData.Data.IsUnlocked(PlayerId.PlayerOne, super))
                PlayerData.Data.Gift(PlayerId.PlayerOne, super);
            if (PlayerData.Data.IsUnlocked(PlayerId.PlayerTwo, super))
                PlayerData.Data.Gift(PlayerId.PlayerTwo, super);
        }

        private static void Stub(string itemName) => Logging.LogWarning($"Item handling unimplemented: {itemName}");
    }
}