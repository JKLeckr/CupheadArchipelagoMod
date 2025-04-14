/// Copyright 2025 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

using System.Collections.Generic;
using System.Linq;

namespace CupheadArchipelago.AP {
    public static class ItemMap {
        private static readonly Dictionary<long, ItemType> itemTypes = new() {
            {APItem.level_generic, ItemType.NoType},
            {APItem.level_extrahealth, ItemType.Level},
            {APItem.level_superrecharge, ItemType.Level},
            {APItem.level_fastfire, ItemType.Level},
            {APItem.level_4, ItemType.Level},
            {APItem.level_trap_fingerjam, ItemType.Level},
            {APItem.level_trap_slowfire, ItemType.Level},
            {APItem.level_trap_superdrain, ItemType.Level},
            {APItem.level_trap_loadout, ItemType.Level},
            {APItem.coin, ItemType.Essential},
            {APItem.coin2, ItemType.Essential},
            {APItem.coin3, ItemType.Essential},
            {APItem.contract, ItemType.Essential},
            {APItem.plane_super, ItemType.Essential},
            {APItem.dlc_boat, ItemType.Essential},
            {APItem.dlc_ingredient, ItemType.Essential},
            {APItem.dlc_cplane_super, ItemType.Essential},

            {APItem.plane_gun, ItemType.Weapon},
            {APItem.plane_bombs, ItemType.Weapon},
            {APItem.dlc_cplane_gun, ItemType.Weapon},
            {APItem.dlc_cplane_bombs, ItemType.Weapon},

            {APItem.weapon_peashooter, ItemType.Weapon},
            {APItem.weapon_spread, ItemType.Weapon},
            {APItem.weapon_chaser, ItemType.Weapon},
            {APItem.weapon_lobber, ItemType.Weapon},
            {APItem.weapon_charge, ItemType.Weapon},
            {APItem.weapon_roundabout, ItemType.Weapon},
            {APItem.weapon_dlc_crackshot, ItemType.Weapon},
            {APItem.weapon_dlc_converge, ItemType.Weapon},
            {APItem.weapon_dlc_twistup, ItemType.Weapon},

            {APItem.p_weapon_peashooter, ItemType.Weapon},
            {APItem.p_weapon_spread, ItemType.Weapon},
            {APItem.p_weapon_chaser, ItemType.Weapon},
            {APItem.p_weapon_lobber, ItemType.Weapon},
            {APItem.p_weapon_charge, ItemType.Weapon},
            {APItem.p_weapon_roundabout, ItemType.Weapon},
            {APItem.p_weapon_dlc_crackshot, ItemType.Weapon},
            {APItem.p_weapon_dlc_converge, ItemType.Weapon},
            {APItem.p_weapon_dlc_twistup, ItemType.Weapon},

            {APItem.charm_heart, ItemType.Charm},
            {APItem.charm_smokebomb, ItemType.Charm},
            {APItem.charm_psugar, ItemType.Charm},
            {APItem.charm_coffee, ItemType.Charm},
            {APItem.charm_twinheart, ItemType.Charm},
            {APItem.charm_whetstone, ItemType.Charm},
            {APItem.charm_dlc_cookie, ItemType.Charm},
            {APItem.charm_dlc_heartring, ItemType.Charm},
            {APItem.charm_dlc_broken_relic, ItemType.Charm},

            {APItem.super_i, ItemType.Super},
            {APItem.super_ii, ItemType.Super},
            {APItem.super_iii, ItemType.Super},
            {APItem.super_dlc_c_i, ItemType.Super},
            {APItem.super_dlc_c_ii, ItemType.Super},
            {APItem.super_dlc_c_iii, ItemType.Super},

            {APItem.ability_dash, ItemType.Ability},
            {APItem.ability_duck, ItemType.Ability},
            {APItem.ability_parry, ItemType.Ability},
            {APItem.ability_plane_parry, ItemType.Ability},
            {APItem.ability_plane_shrink, ItemType.Ability},
            {APItem.ability_dlc_p_cdash, ItemType.Ability},
            {APItem.ability_dlc_cduck, ItemType.Ability},
            {APItem.ability_dlc_cdoublejump, ItemType.Ability},
            {APItem.ability_dlc_cplane_parry, ItemType.Ability},
            {APItem.ability_dlc_cplane_shrink, ItemType.Ability},

            {APItem.ability_aim_left, ItemType.Ability},
            {APItem.ability_aim_right, ItemType.Ability},
            {APItem.ability_aim_up, ItemType.Ability},
            {APItem.ability_aim_down, ItemType.Ability},
            {APItem.ability_aim_upleft, ItemType.Ability},
            {APItem.ability_aim_upright, ItemType.Ability},
            {APItem.ability_aim_downleft, ItemType.Ability},
            {APItem.ability_aim_downright, ItemType.Ability},
        };
        public static ItemType GetItemType(long item) {
            if (itemTypes.ContainsKey(item))
                return itemTypes[item];
            else {
                Logging.LogWarning($"[APItemMngr] Item Id: {item} does not exist!");
                return ItemType.NoType;
            }
        }

        private static readonly Dictionary<long, Weapon> idToWeapon = new() {
            {APItem.weapon_peashooter, Weapon.level_weapon_peashot},
            {APItem.weapon_spread, Weapon.level_weapon_spreadshot},
            {APItem.weapon_chaser, Weapon.level_weapon_homing},
            {APItem.weapon_lobber, Weapon.level_weapon_bouncer},
            {APItem.weapon_charge, Weapon.level_weapon_charge},
            {APItem.weapon_roundabout, Weapon.level_weapon_boomerang},
            {APItem.weapon_dlc_crackshot, Weapon.level_weapon_crackshot},
            {APItem.weapon_dlc_converge, Weapon.level_weapon_wide_shot},
            {APItem.weapon_dlc_twistup, Weapon.level_weapon_upshot},

            {APItem.p_weapon_peashooter, Weapon.level_weapon_peashot},
            {APItem.p_weapon_spread, Weapon.level_weapon_spreadshot},
            {APItem.p_weapon_chaser, Weapon.level_weapon_homing},
            {APItem.p_weapon_lobber, Weapon.level_weapon_bouncer},
            {APItem.p_weapon_charge, Weapon.level_weapon_charge},
            {APItem.p_weapon_roundabout, Weapon.level_weapon_boomerang},
            {APItem.p_weapon_dlc_crackshot, Weapon.level_weapon_crackshot},
            {APItem.p_weapon_dlc_converge, Weapon.level_weapon_wide_shot},
            {APItem.p_weapon_dlc_twistup, Weapon.level_weapon_upshot},

            {APItem.plane_gun, Weapon.plane_weapon_peashot},
            {APItem.plane_bombs, Weapon.plane_weapon_bomb},

            {APItem.dlc_cplane_gun, Weapon.plane_chalice_weapon_3way},
            {APItem.dlc_cplane_bombs, Weapon.plane_chalice_weapon_bomb},
            
            /* More unimplemented ones to check out */
        };
        private static readonly HashSet<Weapon> upgradableWeapons = [
            Weapon.level_weapon_peashot,
            Weapon.level_weapon_spreadshot,
            Weapon.level_weapon_homing,
            Weapon.level_weapon_bouncer,
            Weapon.level_weapon_charge,
            Weapon.level_weapon_boomerang,
            Weapon.level_weapon_crackshot,
            Weapon.level_weapon_wide_shot,
        ];
        public static Weapon GetWeapon(long item) => idToWeapon[item];
        public static IEnumerable<Weapon> GetUpgradableWeapons() {
            return upgradableWeapons;
        }
        public static bool IsWeaponUpgradable(Weapon weapon) => upgradableWeapons.Contains(weapon);
        public static bool IsItemUpgradableWeapon(long itemId) => 
            idToWeapon.ContainsKey(itemId) && upgradableWeapons.Contains(idToWeapon[itemId]);
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

            {APItem.super_dlc_c_i, Super.level_super_chalice_vert_beam},
            {APItem.super_dlc_c_ii, Super.level_super_chalice_shield},
            {APItem.super_dlc_c_iii, Super.level_super_chalice_iii},
        };
        public static Super GetSuper(long item) => idToSuper[item];

        private static readonly HashSet<long> planeItems = new() {
            APItem.plane_gun,
            APItem.plane_bombs,
            APItem.plane_super,
            APItem.ability_plane_parry,
            APItem.ability_plane_shrink,
        };
        public static bool IsPlaneItem(long item) => planeItems.Contains(item);
    }
}
