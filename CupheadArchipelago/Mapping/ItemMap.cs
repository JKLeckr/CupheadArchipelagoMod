/// Copyright 2025 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

using System.Collections.Generic;
using CupheadArchipelago.AP;

namespace CupheadArchipelago.Mapping {
    public static class ItemMap {
        private static readonly Dictionary<long, APItemType> itemTypes = new() {
            {APItem.level_generic, APItemType.None},
            {APItem.level_extrahealth, APItemType.Level},
            {APItem.level_supercharge, APItemType.Level},
            {APItem.level_fastfire, APItemType.Level},
            {APItem.level_4, APItemType.Level},
            {APItem.level_trap_fingerjam, APItemType.Level},
            {APItem.level_trap_slowfire, APItemType.Level},
            {APItem.level_trap_superdrain, APItemType.Level},
            {APItem.level_trap_loadout, APItemType.Level},
            {APItem.level_trap_screen, APItemType.Level},

            {APItem.coin, APItemType.Essential},
            {APItem.coin2, APItemType.Essential},
            {APItem.coin3, APItemType.Essential},
            {APItem.contract, APItemType.Essential},
            {APItem.plane_super, APItemType.Essential},
            {APItem.healthupgrade, APItemType.Essential},
            {APItem.plane_ex, APItemType.Essential},
            {APItem.dlc_boat, APItemType.Essential},
            {APItem.dlc_ingredient, APItemType.Essential},
            {APItem.dlc_cplane_super, APItemType.Essential},
            {APItem.dlc_cplane_ex, APItemType.Essential},

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

            {APItem.weapon_peashooter_ex, APItemType.Weapon},
            {APItem.weapon_spread_ex, APItemType.Weapon},
            {APItem.weapon_chaser_ex, APItemType.Weapon},
            {APItem.weapon_lobber_ex, APItemType.Weapon},
            {APItem.weapon_charge_ex, APItemType.Weapon},
            {APItem.weapon_roundabout_ex, APItemType.Weapon},
            {APItem.weapon_dlc_crackshot_ex, APItemType.Weapon},
            {APItem.weapon_dlc_converge_ex, APItemType.Weapon},
            {APItem.weapon_dlc_twistup_ex, APItemType.Weapon},

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

            {APItem.ability_dlc_c_aim_left, APItemType.Ability},
            {APItem.ability_dlc_c_aim_right, APItemType.Ability},
            {APItem.ability_dlc_c_aim_up, APItemType.Ability},
            {APItem.ability_dlc_c_aim_down, APItemType.Ability},
            {APItem.ability_dlc_c_aim_upleft, APItemType.Ability},
            {APItem.ability_dlc_c_aim_upright, APItemType.Ability},
            {APItem.ability_dlc_c_aim_downleft, APItemType.Ability},
            {APItem.ability_dlc_c_aim_downright, APItemType.Ability},
        };
        public static APItemType GetItemType(long item) {
            if (itemTypes.ContainsKey(item))
                return itemTypes[item];
            else {
                Logging.LogWarning($"[APItemMngr] Item Id {item} has an unknown type!");
                return APItemType.None;
            }
        }
        public static APItemType GetItemType(this APItem item) => GetItemType(item.id);

        public static bool IsItemFiller(long item) {
            APItemType type = GetItemType(item);
            return type == APItemType.None || type == APItemType.Level;
        }
        public static bool IsItemFiller(this APItem item) => IsItemFiller(item.id);

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

            {APItem.weapon_peashooter_ex, Weapon.level_weapon_peashot},
            {APItem.weapon_spread_ex, Weapon.level_weapon_spreadshot},
            {APItem.weapon_chaser_ex, Weapon.level_weapon_homing},
            {APItem.weapon_lobber_ex, Weapon.level_weapon_bouncer},
            {APItem.weapon_charge_ex, Weapon.level_weapon_charge},
            {APItem.weapon_roundabout_ex, Weapon.level_weapon_boomerang},
            {APItem.weapon_dlc_crackshot_ex, Weapon.level_weapon_crackshot},
            {APItem.weapon_dlc_converge_ex, Weapon.level_weapon_wide_shot},
            {APItem.weapon_dlc_twistup_ex, Weapon.level_weapon_upshot},

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
        private static readonly HashSet<Weapon> modularWeapons = [
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
        private static readonly HashSet<long> weaponExItems = [
            APItem.weapon_peashooter_ex,
            APItem.weapon_spread_ex,
            APItem.weapon_chaser_ex,
            APItem.weapon_lobber_ex,
            APItem.weapon_charge_ex,
            APItem.weapon_roundabout_ex,
            APItem.weapon_dlc_crackshot_ex,
            APItem.weapon_dlc_converge_ex,
            APItem.weapon_dlc_twistup_ex,
        ];
        private static readonly HashSet<long> weaponProgressiveItems = [
            APItem.p_weapon_peashooter,
            APItem.p_weapon_spread,
            APItem.p_weapon_chaser,
            APItem.p_weapon_lobber,
            APItem.p_weapon_charge,
            APItem.p_weapon_roundabout,
            APItem.p_weapon_dlc_crackshot,
            APItem.p_weapon_dlc_converge,
            APItem.p_weapon_dlc_twistup,
        ];
        public static Weapon GetWeapon(long item) => idToWeapon[item];
        public static IEnumerable<Weapon> GetModularWeapons() {
            return modularWeapons;
        }
        public static bool IsWeaponModular(Weapon weapon) => modularWeapons.Contains(weapon);
        public static bool IsItemModularWeapon(long itemId) =>
            idToWeapon.ContainsKey(itemId) && modularWeapons.Contains(idToWeapon[itemId]);
        public static bool IsItemModularWeapon(this APItem item) => IsItemModularWeapon(item.id);
        public static bool IsItemWeaponEx(APItem item) => weaponExItems.Contains(item);
        public static bool IsItemWeaponEx(long itemId) => APItem.IdExists(itemId) && IsItemWeaponEx(APItem.FromId(itemId));
        public static bool IsItemProgressiveWeapon(APItem item) => weaponProgressiveItems.Contains(item);
        public static bool IsItemProgressiveWeapon(long itemId) => APItem.IdExists(itemId) && IsItemProgressiveWeapon(APItem.FromId(itemId));
        private static readonly Dictionary<long, Charm> idToCharm = new() {
            {APItem.charm_heart, Charm.charm_health_up_1},
            {APItem.charm_smokebomb, Charm.charm_smoke_dash},
            {APItem.charm_psugar, Charm.charm_parry_plus},
            {APItem.charm_coffee, Charm.charm_super_builder},
            {APItem.charm_twinheart, Charm.charm_health_up_2},
            {APItem.charm_whetstone, Charm.charm_parry_attack},
            {APItem.charm_dlc_heartring, Charm.charm_healer},
            {APItem.charm_dlc_cookie, Charm.charm_chalice},
            {APItem.charm_dlc_broken_relic, Charm.charm_curse},
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

        private static readonly HashSet<long> planeItems = [
            APItem.plane_gun,
            APItem.plane_bombs,
            APItem.plane_super,
            APItem.plane_ex,
            APItem.ability_plane_parry,
            APItem.ability_plane_shrink,

            APItem.dlc_cplane_gun,
            APItem.dlc_cplane_bombs,
            APItem.dlc_cplane_super,
            APItem.dlc_cplane_ex,
            APItem.ability_dlc_cplane_parry,
            APItem.ability_dlc_cplane_shrink,
        ];
        public static bool IsPlaneItem(long item) => planeItems.Contains(item);
        public static bool IsPlaneItem(this APItem item) => planeItems.Contains(item);

        private static readonly HashSet<long> chaliceItems = [
            APItem.dlc_cplane_super,
            APItem.dlc_cplane_ex,
            
            APItem.dlc_cplane_gun,

            APItem.dlc_cplane_bombs,
            APItem.super_dlc_c_i,
            APItem.super_dlc_c_ii,
            APItem.super_dlc_c_iii,
            
            APItem.ability_dlc_p_cdash,
            APItem.ability_dlc_cduck,
            APItem.ability_dlc_cdoublejump,
            APItem.ability_dlc_cplane_parry,
            APItem.ability_dlc_cplane_shrink,
            
            APItem.ability_dlc_c_aim_left,
            APItem.ability_dlc_c_aim_right,
            APItem.ability_dlc_c_aim_up,
            APItem.ability_dlc_c_aim_down,
            APItem.ability_dlc_c_aim_upleft,
            APItem.ability_dlc_c_aim_upright,
            APItem.ability_dlc_c_aim_downleft,
            APItem.ability_dlc_c_aim_downright,
        ];
        public static bool IsChaliceItem(long item) => chaliceItems.Contains(item);
        public static bool IsChaliceItem(this APItem item) => chaliceItems.Contains(item);
    }
}
