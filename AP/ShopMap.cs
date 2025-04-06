/// Copyright 2025 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

using System.Collections.Generic;

namespace CupheadArchipelago.AP {
    public class ShopMap {
        private static readonly Dictionary<Weapon, APLocation> weaponLocations = new() {
            {Weapon.level_weapon_homing, APLocation.shop_weapon1},
            {Weapon.level_weapon_spreadshot, APLocation.shop_weapon2},
            {Weapon.level_weapon_boomerang, APLocation.shop_weapon3},
            {Weapon.level_weapon_bouncer, APLocation.shop_weapon4},
            {Weapon.level_weapon_charge, APLocation.shop_weapon5},
            {Weapon.level_weapon_wide_shot, APLocation.shop_dlc_weapon6},
            {Weapon.level_weapon_crackshot, APLocation.shop_dlc_weapon7},
            {Weapon.level_weapon_upshot, APLocation.shop_dlc_weapon8},
        };
        private static readonly Dictionary<Charm, APLocation> charmLocations = new() {
            {Charm.charm_health_up_1, APLocation.shop_charm1},
            {Charm.charm_smoke_dash, APLocation.shop_charm2},
            {Charm.charm_parry_plus, APLocation.shop_charm3},
            {Charm.charm_super_builder, APLocation.shop_charm4},
            {Charm.charm_parry_attack, APLocation.shop_charm5},
            {Charm.charm_health_up_2, APLocation.shop_charm6},
            {Charm.charm_healer, APLocation.shop_dlc_charm7},
            {Charm.charm_curse, APLocation.shop_dlc_charm8},
        };

        private static ShopSet[] shopMap;

        internal static void SetShopMap(ShopSet[] shopMap) => ShopMap.shopMap = shopMap;
        public static ShopSet[] GetShopMap() => shopMap;

        public static long GetAPWeaponLocation(Weapon weapon) {
            if (weaponLocations.ContainsKey(weapon)) {
                return weaponLocations[weapon];
            }
            throw new KeyNotFoundException($"[GetAPWeaponLocation] Unknown item: {weapon}");
        }
        public static long GetAPCharmLocation(Charm charm) {
            if (charmLocations.ContainsKey(charm)) {
                return charmLocations[charm];
            }
            throw new KeyNotFoundException($"[GetAPWeaponLocation] Unknown item: {charm}");
        }
        public static long GetAPLocation(ShopSceneItem item) {
            long loc = -6;
            switch(item.itemType) {
                case ItemType.Weapon:
                    loc = GetAPWeaponLocation(item.weapon);
                    break;
                case ItemType.Charm: 
                    loc = GetAPCharmLocation(item.charm);
                    break;
                default:
                    Logging.LogWarning($"[ShopMap][GetAPLocation] Cannot get item. Invalid Type {item.itemType}");
                    break;
            }
            return loc;
        }
    }

    public readonly struct ShopSet {
        public ShopSet(int weapons, int charms) {
            Weapons = weapons;
            Charms = charms;
        }

        public int Weapons { get; }
        public int Charms { get; }
    }
}
