/// Copyright 2024 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

using CupheadArchipelago.Hooks;

namespace CupheadArchipelago.AP {
    public class APItemMngr {
        public static void ApplyItem(long itemId) {
            APItem item = APItem.FromId(itemId);
            Plugin.Log($"[APItemMngr] Applying item {item.Name}...");

            switch (ItemMap.GetItemType(item)) {
                case ItemType.Weapon: {
                    PlayerData.Data.Gift(PlayerId.PlayerOne, ItemMap.GetWeapon(item));
                    PlayerData.Data.Gift(PlayerId.PlayerTwo, ItemMap.GetWeapon(item));
                    if (APSettings.UseDLC) {
                        if (item==APItem.plane_gun) {
                            PlayerData.Data.Gift(PlayerId.PlayerOne, Weapon.plane_chalice_weapon_3way);
                            PlayerData.Data.Gift(PlayerId.PlayerTwo, Weapon.plane_chalice_weapon_3way);
                        }
                        else if (item==APItem.plane_bombs) {
                            PlayerData.Data.Gift(PlayerId.PlayerOne, Weapon.plane_chalice_weapon_bomb);
                            PlayerData.Data.Gift(PlayerId.PlayerTwo, Weapon.plane_chalice_weapon_bomb);
                            PlayerData.Data.Loadouts.GetPlayerLoadout(PlayerId.PlayerOne).HasEquippedSecondarySHMUPWeapon = true;
                            PlayerData.Data.Loadouts.GetPlayerLoadout(PlayerId.PlayerTwo).HasEquippedSecondarySHMUPWeapon = true;
                        }
                    }
                    break;
                }
                case ItemType.Charm: {
                    PlayerData.Data.Gift(PlayerId.PlayerOne, ItemMap.GetCharm(item));
                    PlayerData.Data.Gift(PlayerId.PlayerTwo, ItemMap.GetCharm(item));
                    break;
                }
                case ItemType.Super: {
                    PlayerData.Data.Gift(PlayerId.PlayerOne, ItemMap.GetSuper(item));
                    PlayerData.Data.Gift(PlayerId.PlayerTwo, ItemMap.GetSuper(item));
                    if (item==APItem.super_i) {
                        PlayerData.Data.Gift(PlayerId.PlayerOne, Super.level_super_chalice_vert_beam);
                        PlayerData.Data.Gift(PlayerId.PlayerTwo, Super.level_super_chalice_vert_beam);
                    }
                    else if (item==APItem.super_i) {
                        PlayerData.Data.Gift(PlayerId.PlayerOne, Super.level_super_chalice_shield);
                        PlayerData.Data.Gift(PlayerId.PlayerTwo, Super.level_super_chalice_shield);
                    }
                    else if (item==APItem.super_i) {
                        PlayerData.Data.Gift(PlayerId.PlayerOne, Super.level_super_chalice_iii);
                        PlayerData.Data.Gift(PlayerId.PlayerTwo, Super.level_super_chalice_iii);
                    }
                    break;
                }
                case ItemType.Ability: {
                    if (item==APItem.ability_dash)
                        APClient.APSessionGSPlayerData.dash = true;
                    else if (item==APItem.ability_duck)
                        APClient.APSessionGSPlayerData.duck = true;
                    else if (item==APItem.ability_parry)
                        APClient.APSessionGSPlayerData.parry = true;
                    else if (item==APItem.ability_plane_parry)
                        APClient.APSessionGSPlayerData.plane_parry = true;
                    else if (item==APItem.ability_plane_shrink)
                        APClient.APSessionGSPlayerData.plane_shrink = true;
                    break;
                }
                case ItemType.Essential: {
                    if (item==APItem.coin) {
                        PlayerData.Data.AddCurrency(PlayerId.PlayerOne, 1);
                        PlayerData.Data.AddCurrency(PlayerId.PlayerTwo, 1);
                    }
                    else if (item==APItem.coin2) {
                        PlayerData.Data.AddCurrency(PlayerId.PlayerOne, 2);
                        PlayerData.Data.AddCurrency(PlayerId.PlayerTwo, 2);
                    }
                    else if (item==APItem.coin3) {
                        PlayerData.Data.AddCurrency(PlayerId.PlayerOne, 3);
                        PlayerData.Data.AddCurrency(PlayerId.PlayerTwo, 3);
                    }
                    else if (item==APItem.contract) {
                        APClient.APSessionGSPlayerData.contracts++;
                    }
                    else if (item==APItem.dlc_boat) {
                        APClient.APSessionGSPlayerData.dlc_boat=true;
                    }
                    else if (item==APItem.dlc_ingredient) {
                        APClient.APSessionGSPlayerData.dlc_ingredients++;
                    }
                    break;
                }
                case ItemType.Special: {
                    break;
                }
                case ItemType.Level: {
                    AbstractPlayerController p1 = PlayerManager.GetPlayer(PlayerId.PlayerOne);
                    AbstractPlayerController p2 = PlayerManager.GetPlayer(PlayerId.PlayerTwo);
                    PlayerStatsManager stats1 = p1.stats;
                    PlayerStatsManager stats2 = p2?.stats;
                    
                    if (item==APItem.lv_extrahealth) {
                        AudioManager.Play("pop_up");
                        stats1.SetHealth(stats1.Health + 1);
                        stats2?.SetHealth(stats2.Health + 1);
                    }
                    else if (item==APItem.lv_superrecharge) {
                        if (stats1.CanGainSuperMeter) PlayerStatsManagerHook.SetSuper(stats1, PlayerStatsManagerHook.DEFAULT_SUPER_FILL_AMOUNT);
                        if (p2!=null&&stats2.CanGainSuperMeter) PlayerStatsManagerHook.SetSuper(stats2, PlayerStatsManagerHook.DEFAULT_SUPER_FILL_AMOUNT);
                    }
                    else if (item==APItem.lv_trap_fingerjam) {Stub(item);}
                    else if (item==APItem.lv_trap_inktrap) {Stub(item);}
                    else if (item==APItem.lv_trap_slowfire) {Stub(item);}
                    else if (item==APItem.lv_trap_superdrain) {
                        if (stats1.CanGainSuperMeter) PlayerStatsManagerHook.SetSuper(stats1, 0);
                        if (p2!=null&&stats2.CanGainSuperMeter) PlayerStatsManagerHook.SetSuper(stats2, 0);
                    }
                    break;
                }
                default: break;
            }
        }

        private static void Stub(APItem item) => Plugin.LogWarning($"Item handling unimplemented: {item.Name}");
    }
}