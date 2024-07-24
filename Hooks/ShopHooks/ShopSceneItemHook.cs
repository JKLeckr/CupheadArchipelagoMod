/// Copyright 2024 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

using System.Collections.Generic;
using CupheadArchipelago.AP;

namespace CupheadArchipelago.Hooks.ShopHooks {
    internal class ShopSceneItemHook {
        internal static void Hook() {}

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
    }
}