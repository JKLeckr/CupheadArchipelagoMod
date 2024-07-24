/// Copyright 2024 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

using System.Collections.Generic;

namespace CupheadArchipelago.AP {
    public class APItemMngr {
        public enum ItemType {
            NoType = 0,
            Weapon = 1,
            Charm = 2,
            Super = 4,
            Ability = 8,
            Essential = 16,
            Special = 32,
            Level = 64,
        }

        public static class ItemMap {
            private static readonly Dictionary<long, ItemType> itemTypes = new() {
                {APItem.lv_extrahealth, ItemType.Level},
                {APItem.lv_superrecharge, ItemType.Level},
                {APItem.lv_trap_fingerjam, ItemType.Level},
                {APItem.lv_trap_inktrap, ItemType.Level},
                {APItem.lv_trap_slowfire, ItemType.Level},
                {APItem.lv_trap_superdrain, ItemType.Level},

                {APItem.coin, ItemType.Essential},
                {APItem.coin2, ItemType.Essential},
                {APItem.coin3, ItemType.Essential},
                {APItem.contract, ItemType.Essential},
                {APItem.dlc_boat, ItemType.Essential},
                {APItem.dlc_ingredient, ItemType.Essential},

                {APItem.plane_gun, ItemType.Weapon},
                {APItem.plane_bombs, ItemType.Weapon},

                {APItem.weapon_peashooter, ItemType.Weapon},
                {APItem.weapon_spread, ItemType.Weapon},
                {APItem.weapon_chaser, ItemType.Weapon},
                {APItem.weapon_lobber, ItemType.Weapon},
                {APItem.weapon_charge, ItemType.Weapon},
                {APItem.weapon_roundabout, ItemType.Weapon},
                {APItem.weapon_dlc_crackshot, ItemType.Weapon},
                {APItem.weapon_dlc_converge, ItemType.Weapon},
                {APItem.weapon_dlc_twistup, ItemType.Weapon},

                {APItem.charm_heart, ItemType.Charm},
                {APItem.charm_smokebomb, ItemType.Charm},
                {APItem.charm_psugar, ItemType.Charm},
                {APItem.charm_coffee, ItemType.Charm},
                {APItem.charm_twinheart, ItemType.Charm},
                {APItem.charm_whetstone, ItemType.Charm},
                //{APItem.dlc_cookie, ItemType.Charm}, // Not part of logic fn
                {APItem.charm_dlc_heartring, ItemType.Charm},
                {APItem.charm_dlc_broken_relic, ItemType.Charm},

                {APItem.super_i, ItemType.Super},
                {APItem.super_ii, ItemType.Super},
                {APItem.super_iii, ItemType.Super},

                {APItem.ability_dash, ItemType.Ability},
                {APItem.ability_duck, ItemType.Ability},
                {APItem.ability_parry, ItemType.Ability},
                {APItem.ability_plane_parry, ItemType.Ability},
                {APItem.ability_plane_shrink, ItemType.Ability},

                /* Aim Abilities to add later */
            };
            public static ItemType GetItemType(long item) {
                if (itemTypes.ContainsKey(item))
                    return itemTypes[item];
                else {
                    Plugin.LogWarning($"[APItemMngr] Item Id: {item} does not exist!");
                    return ItemType.NoType;
                }
            }

            private static readonly Dictionary<long, Weapon> idToWeapon = new() {
                {APItem.weapon_peashooter, Weapon.level_weapon_peashot},
                {APItem.weapon_spread, Weapon.level_weapon_spreadshot},
                {APItem.weapon_chaser, Weapon.level_weapon_homing},
                {APItem.weapon_lobber, Weapon.level_weapon_arc},
                {APItem.weapon_charge, Weapon.level_weapon_charge},
                {APItem.weapon_roundabout, Weapon.level_weapon_boomerang},
                {APItem.weapon_dlc_crackshot, Weapon.level_weapon_crackshot},
                {APItem.weapon_dlc_converge, Weapon.level_weapon_wide_shot},

                {APItem.plane_gun, Weapon.plane_weapon_peashot},
                {APItem.plane_bombs, Weapon.plane_weapon_bomb},

                /* More unimplemented ones to check out */
            };
            public static Weapon GetWeapon(long item) => idToWeapon[item];
            private static readonly Dictionary<long, Charm> idToCharm = new() {
                {APItem.charm_heart, Charm.charm_health_up_1},
                {APItem.charm_smokebomb, Charm.charm_smoke_dash},
                {APItem.charm_psugar, Charm.charm_parry_plus},
                {APItem.charm_coffee, Charm.charm_super_builder},
                {APItem.charm_twinheart, Charm.charm_health_up_2},
                {APItem.charm_whetstone, Charm.charm_parry_attack},
                {APItem.charm_dlc_heartring, Charm.charm_healer},
                {APItem.charm_dlc_broken_relic, Charm.charm_curse}, // Is this right?
            };
            public static Charm GetCharm(long item) => idToCharm[item];
            private static readonly Dictionary<long, Super> idToSuper = new() {
                {APItem.super_i, Super.level_super_beam},
                {APItem.super_ii, Super.level_super_invincible},
                {APItem.super_iii, Super.level_super_ghost},

                // Plane Supers??
            };
            public static Super GetSuper(long item) => idToSuper[item];
        }

        public static void ApplyItem(long itemId) {
            APItem item = APItem.FromId(itemId);
            Plugin.Log($"[APItemMngr] Applying item {item.Name}...");

            switch (ItemMap.GetItemType(item)) {
                case ItemType.Weapon: {
                    PlayerData.Data.Gift(PlayerId.PlayerOne, ItemMap.GetWeapon(item));
                    PlayerData.Data.Gift(PlayerId.PlayerTwo, ItemMap.GetWeapon(item));
                    if (DLCManager.DLCEnabled()) {
                        if (item==APItem.plane_gun) {
                            PlayerData.Data.Gift(PlayerId.PlayerOne, Weapon.plane_chalice_weapon_3way);
                            PlayerData.Data.Gift(PlayerId.PlayerTwo, Weapon.plane_chalice_weapon_3way);
                        }
                        else if (item==APItem.plane_bombs) {
                            PlayerData.Data.Gift(PlayerId.PlayerOne, Weapon.plane_chalice_weapon_bomb);
                            PlayerData.Data.Gift(PlayerId.PlayerTwo, Weapon.plane_chalice_weapon_bomb);
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
                    else if (item==APItem.contract) {Stub(item);}
                    else if (item==APItem.dlc_boat) {Stub(item);}
                    else if (item==APItem.dlc_ingredient) {Stub(item);}
                    break;
                }
                case ItemType.Special: {
                    break;
                }
                case ItemType.Level: {
                    AbstractPlayerController p1 = PlayerManager.GetPlayer(PlayerId.PlayerOne);
                    AbstractPlayerController p2 = PlayerManager.GetPlayer(PlayerId.PlayerTwo);
                    PlayerStatsManager stats1 = p1.stats;
                    PlayerStatsManager stats2 = p2.stats;
                    
                    if (item==APItem.lv_extrahealth) {
                        AudioManager.Play("pop_up");
                        stats1.SetHealth(stats1.Health + 1);
                        if (p2!=null) stats2.SetHealth(stats2.Health + 1);
                    }
                    else if (item==APItem.lv_superrecharge) {
                        stats1.DebugFillSuper();
                        if (p2!=null) stats2.DebugFillSuper();
                    }
                    else if (item==APItem.lv_trap_fingerjam) {Stub(item);}
                    else if (item==APItem.lv_trap_inktrap) {Stub(item);}
                    else if (item==APItem.lv_trap_slowfire) {Stub(item);}
                    else if (item==APItem.lv_trap_superdrain) {Stub(item);}
                    break;
                }
                default: break;
            }
        }

        private static void Stub(APItem item) => Plugin.LogWarning($"Item handling unimplemented: {item.Name}");
    }
}