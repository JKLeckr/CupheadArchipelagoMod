/// Copyright 2024 JKLeckr
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
                case ItemType.Weapon: {
                    Weapon weapon = ItemMap.GetWeapon(itemId);
                    PlayerData.Data.Gift(PlayerId.PlayerOne, weapon);
                    PlayerData.Data.Gift(PlayerId.PlayerTwo, weapon);
                    if (itemId==APItem.plane_gun) {
                        PlayerData.Data.Gift(PlayerId.PlayerOne, Weapon.plane_chalice_weapon_3way);
                        PlayerData.Data.Gift(PlayerId.PlayerTwo, Weapon.plane_chalice_weapon_3way);
                    }
                    else if (itemId==APItem.plane_bombs) {
                        PlayerData.Data.Gift(PlayerId.PlayerOne, Weapon.plane_chalice_weapon_bomb);
                        PlayerData.Data.Gift(PlayerId.PlayerTwo, Weapon.plane_chalice_weapon_bomb);
                    }
                    if (PlayerData.Data.IsUnlocked(PlayerId.PlayerOne, Weapon.plane_weapon_peashot) && PlayerData.Data.IsUnlocked(PlayerId.PlayerOne, Weapon.plane_weapon_bomb)) {
                        PlayerData.Data.Loadouts.GetPlayerLoadout(PlayerId.PlayerOne).HasEquippedSecondarySHMUPWeapon = true;
                        PlayerData.Data.Loadouts.GetPlayerLoadout(PlayerId.PlayerTwo).HasEquippedSecondarySHMUPWeapon = true;
                    }
                    break;
                }
                case ItemType.Charm: {
                    PlayerData.Data.Gift(PlayerId.PlayerOne, ItemMap.GetCharm(itemId));
                    PlayerData.Data.Gift(PlayerId.PlayerTwo, ItemMap.GetCharm(itemId));
                    break;
                }
                case ItemType.Super: {
                    PlayerData.Data.Gift(PlayerId.PlayerOne, ItemMap.GetSuper(itemId));
                    PlayerData.Data.Gift(PlayerId.PlayerTwo, ItemMap.GetSuper(itemId));
                    if (itemId==APItem.super_i) {
                        PlayerData.Data.Gift(PlayerId.PlayerOne, Super.level_super_chalice_vert_beam);
                        PlayerData.Data.Gift(PlayerId.PlayerTwo, Super.level_super_chalice_vert_beam);
                    }
                    else if (itemId==APItem.super_ii) {
                        PlayerData.Data.Gift(PlayerId.PlayerOne, Super.level_super_chalice_shield);
                        PlayerData.Data.Gift(PlayerId.PlayerTwo, Super.level_super_chalice_shield);
                    }
                    else if (itemId==APItem.super_iii) {
                        PlayerData.Data.Gift(PlayerId.PlayerOne, Super.level_super_chalice_iii);
                        PlayerData.Data.Gift(PlayerId.PlayerTwo, Super.level_super_chalice_iii);
                    }
                    break;
                }
                case ItemType.Ability: {
                    if (itemId==APItem.ability_dash)
                        APClient.APSessionGSPlayerData.dash = true;
                    else if (itemId==APItem.ability_duck)
                        APClient.APSessionGSPlayerData.duck = true;
                    else if (itemId==APItem.ability_parry)
                        APClient.APSessionGSPlayerData.parry = true;
                    else if (itemId==APItem.ability_plane_parry)
                        APClient.APSessionGSPlayerData.plane_parry = true;
                    else if (itemId==APItem.ability_plane_shrink)
                        APClient.APSessionGSPlayerData.plane_shrink = true;
                    break;
                }
                case ItemType.Essential: {
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
                        if ((APSettings.Mode & GameMode.CollectContracts) > 0) {
                            Logging.Log($"Contracts Goal: {APClient.APSessionGSPlayerData.contracts}");
                            if (APClient.APSessionGSPlayerData.contracts >= APSettings.ContractsGoal) {
                                APClient.GoalComplete(Goals.Contracts);
                            }
                        }
                    }
                    else if (itemId==APItem.plane_super)
                        APClient.APSessionGSPlayerData.plane_super=true;
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
                        if ((APSettings.Mode & GameMode.CollectContracts) > 0) {
                            Logging.Log($"Ingredients Goal: {APClient.APSessionGSPlayerData.dlc_ingredients}");
                            if (APClient.APSessionGSPlayerData.dlc_ingredients >= APSettings.DLCIngredientsGoal) {
                                APClient.GoalComplete(Goals.Ingredients);
                            }
                        }
                    }
                    break;
                }
                case ItemType.Special: {
                    break;
                }
                case ItemType.Level: {
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
                    else if (itemId==APItem.level_trap_reverse) {
                        AudioManager.Play("level_menu_select");
                        PlayerStatsManagerHook.ReverseControls();
                    }
                    break;
                }
                default: break;
            }
            } catch (Exception e) {Logging.LogError(e); return false;}
            return true;
        }

        internal static void AddCoins(int count=1) {
            PlayerData.Data.AddCurrency(PlayerId.PlayerOne, count);
            PlayerData.Data.AddCurrency(PlayerId.PlayerTwo, count);
            Logging.Log($"Current coins: {PlayerData.Data.GetCurrency(PlayerId.PlayerOne)}");
            APClient.APSessionGSPlayerData.coins_collected += count;
            Logging.Log($"Total coins: {APClient.APSessionGSPlayerData.coins_collected}");
        }

        private static void Stub(string itemName) => Logging.LogWarning($"Item handling unimplemented: {itemName}");
    }
}