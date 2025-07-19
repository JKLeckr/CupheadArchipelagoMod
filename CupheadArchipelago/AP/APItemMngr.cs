/// Copyright 2025 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

using System;
using CupheadArchipelago.Interfaces;
using CupheadArchipelago.Mapping;

namespace CupheadArchipelago.AP {
    public class APItemMngr {
        public static bool ApplyItem(APItemData item) {
            string itemName = APClient.GetItemName(item.id);
            Logging.Log($"[APItemMngr] Applying item {itemName} ({item.id})...");
            return ApplyItem(item.id, PlayerDataItfc.Default);
        }
        internal static bool ApplyItem(long itemId, IPlayerDataItfc pdMngr) {
            try {
                switch (ItemMap.GetItemType(itemId)) {
                    case APItemType.Weapon: {
                            Weapon weapon = ItemMap.GetWeapon(itemId);
                            pdMngr.Gift(weapon);
                            if (ItemMap.IsPlaneItem(itemId)) {
                                ResolvePlaneWeapons(itemId, pdMngr.Gift);
                            }
                            else {
                                uint weaponbits = GetWeaponBit(itemId);
                                APClient.APSessionGSPlayerData.AddWeaponBit(weapon, weaponbits);
                            }
                            if ((pdMngr.IsUnlocked(Weapon.plane_weapon_peashot) && pdMngr.IsUnlocked(Weapon.plane_weapon_bomb)) ||
                                (pdMngr.IsUnlocked(Weapon.plane_chalice_weapon_3way) && pdMngr.IsUnlocked(Weapon.plane_chalice_weapon_bomb))) {
                                pdMngr.DoPlaneSecondaryEquipTrigger();
                            }
                            break;
                        }
                    case APItemType.Charm: {
                            Charm charm = ItemMap.GetCharm(itemId);
                            pdMngr.Gift(charm);
                            break;
                        }
                    case APItemType.Super: {
                            pdMngr.Gift(ItemMap.GetSuper(itemId));
                            if (itemId == APItem.super_i && !IsChaliceSeparate(ItemGroups.Super)) {
                                pdMngr.Gift(Super.level_super_chalice_vert_beam);
                            }
                            else if (itemId == APItem.super_ii && !IsChaliceSeparate(ItemGroups.Super)) {
                                pdMngr.Gift(Super.level_super_chalice_shield);
                            }
                            else if (itemId == APItem.super_iii && !IsChaliceSeparate(ItemGroups.Super)) {
                                pdMngr.Gift(Super.level_super_chalice_iii);
                            }
                            break;
                        }
                    case APItemType.Ability: {
                            ApplyAbiltiy(itemId);
                            break;
                        }
                    case APItemType.Essential: {
                            if (itemId == APItem.coin) {
                                Logging.Log("AddCurrency");
                                AddCoins(1, pdMngr);
                            }
                            else if (itemId == APItem.coin2) {
                                Logging.Log("AddCurrency x2");
                                AddCoins(2, pdMngr);
                            }
                            else if (itemId == APItem.coin3) {
                                Logging.Log("AddCurrency x3");
                                AddCoins(3, pdMngr);
                            }
                            else if (itemId == APItem.contract) {
                                if (APClient.APSessionGSPlayerData.contracts < APClient.GetReceivedItemCount(APItem.contract)) {
                                    Logging.Log($"Adding Contract...");
                                    APClient.APSessionGSPlayerData.contracts++;
                                }
                                else Logging.Log("Contract is already applied. Skipping.");
                                Logging.Log($"Contracts: {APClient.APSessionGSPlayerData.contracts}");
                            }
                            else if (itemId == APItem.plane_ex) {
                                APClient.APSessionGSPlayerData.plane_ex = true;
                                if (!IsChaliceSeparate(ItemGroups.WeaponEx))
                                    APClient.APSessionGSPlayerData.dlc_cplane_ex = true;
                            }
                            else if (itemId == APItem.plane_super) {
                                APClient.APSessionGSPlayerData.plane_super = true;
                                if (!IsChaliceSeparate(ItemGroups.Super))
                                    APClient.APSessionGSPlayerData.dlc_cplane_super = true;
                            }
                            else if (itemId == APItem.dlc_cplane_super) {
                                APClient.APSessionGSPlayerData.dlc_cplane_super = true;
                            }
                            else if (itemId == APItem.dlc_boat) {
                                APClient.APSessionGSPlayerData.dlc_boat = true;
                            }
                            else if (itemId == APItem.healthupgrade) {
                                if (APClient.APSessionGSPlayerData.healthupgrades < APClient.GetReceivedItemCount(APItem.healthupgrade)) {
                                    Logging.Log($"Adding Health Upgrade...");
                                    APClient.APSessionGSPlayerData.healthupgrades++;
                                }
                                else Logging.Log("Health Upgrade is already applied. Skipping.");
                                Logging.Log($"Health Upgrades: {APClient.APSessionGSPlayerData.healthupgrades}");
                            }
                            else if (itemId == APItem.dlc_ingredient) {
                                if (APClient.APSessionGSPlayerData.dlc_ingredients < APClient.GetReceivedItemCount(APItem.dlc_ingredient)) {
                                    Logging.Log($"Adding Ingredient...");
                                    APClient.APSessionGSPlayerData.dlc_ingredients++;
                                }
                                else Logging.Log("Ingredient is already applied. Skipping.");
                                Logging.Log($"Ingredients: {APClient.APSessionGSPlayerData.dlc_ingredients}");
                            }
                            break;
                        }
                    case APItemType.Special: {
                            break;
                        }
                    case APItemType.Level: {
                            APLevelItemMngr.ApplyLevelItem(itemId);
                            break;
                        }
                    default: break;
                }
            }
            catch (Exception e) { Logging.LogError(e); return false; }
            APClient.UpdateGoalFlags();
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
            } else if (id==APItem.ability_dlc_p_cdash) {
                if (APClient.GetReceivedItemCount(id)>=2)
                    APClient.APSessionGSPlayerData.dlc_cparry = true;
                APClient.APSessionGSPlayerData.dlc_cdash = true;
            } else if (id==APItem.ability_dlc_cduck) {
                APClient.APSessionGSPlayerData.dlc_cduck = true;
            } else if (id==APItem.ability_dlc_cdoublejump) {
                APClient.APSessionGSPlayerData.dlc_cdoublejump = true;
            } else if (id==APItem.ability_dlc_cplane_parry) {
                APClient.APSessionGSPlayerData.dlc_cplane_parry = true;
            } else if (id==APItem.ability_dlc_cplane_shrink) {
                APClient.APSessionGSPlayerData.dlc_cplane_shrink = true;
            }
        }

        private static void AddCoins(int count, IPlayerDataItfc pdMngr) {
            int diff = APClient.GetReceivedCoinCount() - APClient.APSessionGSPlayerData.coins_collected;
            Logging.Log($"[AddCoins] Diff {diff}");
            int ncount = count;
            if (ncount != diff) {
                Logging.Log($"[AddCoins] Coins are out of sync. Adjusting...");
                ncount = diff;
            }
            if (ncount > 0) {
                Logging.Log($"[AddCoins] Adding {ncount} coin{(ncount!=1?"s":"")}...");
                pdMngr.AddCoins(ncount);
                APClient.APSessionGSPlayerData.coins_collected += ncount;
            } else {
                Logging.Log("[AddCoins] Coins are already applied. Skipping.");
            }
            Logging.Log($"Current coins: {pdMngr.GetCoins()}");
            Logging.Log($"Total coins: {APClient.APSessionGSPlayerData.coins_collected}");
        }

        private static bool IsChaliceSeparate(ItemGroups group, bool anybit = false) =>
            APSettings.IsItemGroupChaliceSeparate(group, anybit);

        private static uint GetWeaponBit(long itemId) {
            uint weaponbits = 0;
            if (ItemMap.IsItemWeaponEx(itemId)) {
                if (ItemMap.IsChaliceItem(itemId))
                    weaponbits |= (uint)WeaponParts.CEx;
                else {
                    weaponbits |= (uint)WeaponParts.Ex;
                    if (IsChaliceSeparate(ItemGroups.WeaponEx))
                        weaponbits |= (uint)WeaponParts.CEx;
                }
            }
            else {
                bool basebit = ItemMap.IsChaliceItem(itemId);
                bool chalicebit = ItemMap.IsChaliceItem(itemId) || !IsChaliceSeparate(ItemGroups.WeaponBasic, true);
                weaponbits |= (basebit ? (uint)WeaponParts.Basic : 0) | (chalicebit ? (uint)WeaponParts.CBasic : 0);
                if ((ItemMap.IsItemProgressiveWeapon(itemId) && (APClient.GetReceivedItemCount(itemId) > 1 ||
                    APSettings.StartWeapon.id == itemId)) || APSettings.WeaponMode == WeaponModes.Normal
                ) {
                    weaponbits |= (basebit ? (uint)WeaponParts.Ex : 0) | (chalicebit ? (uint)WeaponParts.CEx : 0);
                }
            }
            return weaponbits;
        }

        private static void ResolvePlaneWeapons(long itemId, Action<Weapon> giftW) {
            if (itemId == APItem.plane_gun) {
                if (!IsChaliceSeparate(ItemGroups.WeaponBasic)) {
                    giftW(Weapon.plane_chalice_weapon_3way);
                    if (APSettings.WeaponMode == WeaponModes.Normal)
                        APClient.APSessionGSPlayerData.dlc_cplane_ex = true;
                }
            }
            else if (itemId == APItem.plane_bombs) {
                if (!IsChaliceSeparate(ItemGroups.WeaponBasic)) {
                    giftW(Weapon.plane_chalice_weapon_bomb);
                    if (APSettings.WeaponMode == WeaponModes.Normal)
                        APClient.APSessionGSPlayerData.dlc_cplane_ex = true;
                }
            }
        }
    }
}
