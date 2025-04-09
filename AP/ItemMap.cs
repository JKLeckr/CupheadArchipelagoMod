/// Copyright 2025 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

using System.Collections.Generic;

namespace CupheadArchipelago.AP {
    public static class ItemMap {
        private static readonly Dictionary<long, APItemType> itemTypes = new() {
            {APItem.level_generic, APItemType.NoType},
            {APItem.level_extrahealth, APItemType.Level},
            {APItem.level_supercharge, APItemType.Level},
            {APItem.level_fastfire, APItemType.Level},
            {APItem.level_4, APItemType.Level},
            {APItem.level_trap_fingerjam, APItemType.Level},
            {APItem.level_trap_slowfire, APItemType.Level},
            {APItem.level_trap_superdrain, APItemType.Level},
            {APItem.level_trap_loadout, APItemType.Level},

            {APItem.coin, APItemType.Essential},
            {APItem.coin2, APItemType.Essential},
            {APItem.coin3, APItemType.Essential},
            {APItem.contract, APItemType.Essential},
            {APItem.plane_super, APItemType.Essential},
            {APItem.healthupgrade, APItemType.Essential},
            {APItem.dlc_boat, APItemType.Essential},
            {APItem.dlc_ingredient, APItemType.Essential},
            {APItem.dlc_cplane_super, APItemType.Essential},

            {APItem.plane_gun, APItemType.Weapon},
            {APItem.plane_bombs, APItemType.Weapon},
            {APItem.dlc_cplane_gun, APItemType.Weapon},
            {APItem.dlc_cplane_bombs, APItemType.Weapon},

            {APItem.weapon_peashooter, APItemType.Weapon},
            {APItem.weapon_spread, APItemType.Weapon},
            {APItem.weapon_chaser, APItemType.Weapon},
            {APItem.weapon_lobber, APItemType.Weapon},
            {APItem.weapon_charge, APItemType.Weapon},
            {APItem.weapon_roundabout, APItemType.Weapon},
            {APItem.weapon_dlc_crackshot, APItemType.Weapon},
            {APItem.weapon_dlc_converge, APItemType.Weapon},
            {APItem.weapon_dlc_twistup, APItemType.Weapon},

            {APItem.p_weapon_peashooter, APItemType.Weapon},
            {APItem.p_weapon_spread, APItemType.Weapon},
            {APItem.p_weapon_chaser, APItemType.Weapon},
            {APItem.p_weapon_lobber, APItemType.Weapon},
            {APItem.p_weapon_charge, APItemType.Weapon},
            {APItem.p_weapon_roundabout, APItemType.Weapon},
            {APItem.p_weapon_dlc_crackshot, APItemType.Weapon},
            {APItem.p_weapon_dlc_converge, APItemType.Weapon},
            {APItem.p_weapon_dlc_twistup, APItemType.Weapon},

            {APItem.charm_heart, APItemType.Charm},
            {APItem.charm_smokebomb, APItemType.Charm},
            {APItem.charm_psugar, APItemType.Charm},
            {APItem.charm_coffee, APItemType.Charm},
            {APItem.charm_twinheart, APItemType.Charm},
            {APItem.charm_whetstone, APItemType.Charm},
            {APItem.charm_dlc_cookie, APItemType.Charm},
            {APItem.charm_dlc_heartring, APItemType.Charm},
            {APItem.charm_dlc_broken_relic, APItemType.Charm},

            {APItem.super_i, APItemType.Super},
            {APItem.super_ii, APItemType.Super},
            {APItem.super_iii, APItemType.Super},
            {APItem.super_dlc_c_i, APItemType.Super},
            {APItem.super_dlc_c_ii, APItemType.Super},
            {APItem.super_dlc_c_iii, APItemType.Super},

            {APItem.ability_dash, APItemType.Ability},
            {APItem.ability_duck, APItemType.Ability},
            {APItem.ability_parry, APItemType.Ability},
            {APItem.ability_plane_parry, APItemType.Ability},
            {APItem.ability_plane_shrink, APItemType.Ability},
            {APItem.ability_dlc_p_cdash, APItemType.Ability},
            {APItem.ability_dlc_cduck, APItemType.Ability},
            {APItem.ability_dlc_cdoublejump, APItemType.Ability},
            {APItem.ability_dlc_cplane_parry, APItemType.Ability},
            {APItem.ability_dlc_cplane_shrink, APItemType.Ability},

            {APItem.ability_aim_left, APItemType.Ability},
            {APItem.ability_aim_right, APItemType.Ability},
            {APItem.ability_aim_up, APItemType.Ability},
            {APItem.ability_aim_down, APItemType.Ability},
            {APItem.ability_aim_upleft, APItemType.Ability},
            {APItem.ability_aim_upright, APItemType.Ability},
            {APItem.ability_aim_downleft, APItemType.Ability},
            {APItem.ability_aim_downright, APItemType.Ability},
        };
        public static APItemType GetItemType(long item) {
            if (itemTypes.ContainsKey(item))
                return itemTypes[item];
            else {
<<<<<<< HEAD
                Logging.LogWarning($"[APItemMngr] Item Id {item} has an unknown type!");
                return ItemType.NoType;
=======
                Logging.LogWarning($"[APItemMngr] Item Id: {item} does not exist!");
                return APItemType.NoType;
>>>>>>> c4dce37 (Code organization and cleanup)
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
            Weapon.level_weapon_upshot,
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
