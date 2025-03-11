/// Copyright 2025 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

using System.Collections.Generic;

namespace CupheadArchipelago.AP {
    public static class ItemMap {
        private static readonly Dictionary<long, ItemTypes> itemTypes = new() {
            {APItem.level_generic, ItemTypes.NoType},
            {APItem.level_extrahealth, ItemTypes.Level},
            {APItem.level_superrecharge, ItemTypes.Level},
            {APItem.level_fastfire, ItemTypes.Level},
            {APItem.level_4, ItemTypes.Level},
            {APItem.level_trap_fingerjam, ItemTypes.Level},
            {APItem.level_trap_slowfire, ItemTypes.Level},
            {APItem.level_trap_superdrain, ItemTypes.Level},
            {APItem.level_trap_loadout, ItemTypes.Level},
            {APItem.coin, ItemTypes.Essential},
            {APItem.coin2, ItemTypes.Essential},
            {APItem.coin3, ItemTypes.Essential},
            {APItem.contract, ItemTypes.Essential},
            {APItem.plane_super, ItemTypes.Essential},
            {APItem.dlc_boat, ItemTypes.Essential},
            {APItem.dlc_ingredient, ItemTypes.Essential},
            {APItem.dlc_cplane_super, ItemTypes.Essential},

            {APItem.plane_gun, ItemTypes.Weapon},
            {APItem.plane_bombs, ItemTypes.Weapon},
            {APItem.dlc_cplane_gun, ItemTypes.Weapon},
            {APItem.dlc_cplane_bombs, ItemTypes.Weapon},

            {APItem.weapon_peashooter, ItemTypes.Weapon},
            {APItem.weapon_spread, ItemTypes.Weapon},
            {APItem.weapon_chaser, ItemTypes.Weapon},
            {APItem.weapon_lobber, ItemTypes.Weapon},
            {APItem.weapon_charge, ItemTypes.Weapon},
            {APItem.weapon_roundabout, ItemTypes.Weapon},
            {APItem.weapon_dlc_crackshot, ItemTypes.Weapon},
            {APItem.weapon_dlc_converge, ItemTypes.Weapon},
            {APItem.weapon_dlc_twistup, ItemTypes.Weapon},

            {APItem.charm_heart, ItemTypes.Charm},
            {APItem.charm_smokebomb, ItemTypes.Charm},
            {APItem.charm_psugar, ItemTypes.Charm},
            {APItem.charm_coffee, ItemTypes.Charm},
            {APItem.charm_twinheart, ItemTypes.Charm},
            {APItem.charm_whetstone, ItemTypes.Charm},
            {APItem.charm_dlc_cookie, ItemTypes.Charm},
            {APItem.charm_dlc_heartring, ItemTypes.Charm},
            {APItem.charm_dlc_broken_relic, ItemTypes.Charm},

            {APItem.super_i, ItemTypes.Super},
            {APItem.super_ii, ItemTypes.Super},
            {APItem.super_iii, ItemTypes.Super},
            {APItem.super_dlc_c_i, ItemTypes.Super},
            {APItem.super_dlc_c_ii, ItemTypes.Super},
            {APItem.super_dlc_c_iii, ItemTypes.Super},

            {APItem.ability_dash, ItemTypes.Ability},
            {APItem.ability_duck, ItemTypes.Ability},
            {APItem.ability_parry, ItemTypes.Ability},
            {APItem.ability_plane_parry, ItemTypes.Ability},
            {APItem.ability_plane_shrink, ItemTypes.Ability},
            {APItem.ability_dlc_cdash, ItemTypes.Ability},
            {APItem.ability_dlc_cduck, ItemTypes.Ability},
            {APItem.ability_dlc_cparry, ItemTypes.Ability},
            {APItem.ability_dlc_cplane_parry, ItemTypes.Ability},
            {APItem.ability_dlc_cplane_shrink, ItemTypes.Ability},

            {APItem.ability_aim_left, ItemTypes.Ability},
            {APItem.ability_aim_right, ItemTypes.Ability},
            {APItem.ability_aim_up, ItemTypes.Ability},
            {APItem.ability_aim_down, ItemTypes.Ability},
            {APItem.ability_aim_upleft, ItemTypes.Ability},
            {APItem.ability_aim_upright, ItemTypes.Ability},
            {APItem.ability_aim_downleft, ItemTypes.Ability},
            {APItem.ability_aim_downright, ItemTypes.Ability},
        };
        public static ItemTypes GetItemType(long item) {
            if (itemTypes.ContainsKey(item))
                return itemTypes[item];
            else {
                Logging.LogWarning($"[APItemMngr] Item Id: {item} does not exist!");
                return ItemTypes.NoType;
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

            {APItem.plane_gun, Weapon.plane_weapon_peashot},
            {APItem.plane_bombs, Weapon.plane_weapon_bomb},

            {APItem.dlc_cplane_gun, Weapon.plane_chalice_weapon_3way},
            {APItem.dlc_cplane_bombs, Weapon.plane_chalice_weapon_bomb},
            
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