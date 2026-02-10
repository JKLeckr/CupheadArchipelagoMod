/// Copyright 2025-2026 JKLeckr
/// SPDX-License-Identifier: GPL-3.0-or-later

using System.Collections.Generic;
using CupheadArchipelago.AP;
using CupheadArchipelago.Mapping;

namespace CupheadArchipelago.Hooks.ShopHooks {
    internal class ShopHookBase {
        private static readonly Weapon[] weapons = [
            Weapon.level_weapon_homing,
            Weapon.level_weapon_spreadshot,
            Weapon.level_weapon_boomerang,
            Weapon.level_weapon_bouncer,
            Weapon.level_weapon_charge,
            Weapon.level_weapon_wide_shot,
            Weapon.level_weapon_crackshot,
            Weapon.level_weapon_upshot,
        ];
        internal static readonly Charm[] charms = [
            Charm.charm_health_up_1,
            Charm.charm_smoke_dash,
            Charm.charm_parry_plus,
            Charm.charm_super_builder,
            Charm.charm_parry_attack,
            Charm.charm_health_up_2,
            Charm.charm_healer,
            Charm.charm_curse,
        ];
        private static readonly Dictionary<Weapon, int> weaponOrder = [];
        private static readonly Dictionary<Charm, int> charmOrder = [];

        static ShopHookBase() {
            for (int i = 0; i < weapons.Length; i++) {
                weaponOrder.Add(weapons[i], i);
            }
            for (int i = 0; i < charms.Length; i++) {
                charmOrder.Add(charms[i], i);
            }
        }

        internal static int GetWeaponOrderIndex(Weapon weapon) => weaponOrder[weapon];
        internal static int GetCharmOrderIndex(Charm charm) => charmOrder[charm];

        internal static bool IsAPCharmChecked(Charm charm) {
            long loc = ShopMap.GetAPCharmLocation(charm);
            bool res = APClient.IsLocationChecked(loc);
            Logging.Log($"{APClient.GetCheck(loc).LocationName}: {loc}");
            return res;
        }
        internal static bool IsAPWeaponChecked(Weapon weapon) {
            long loc = ShopMap.GetAPWeaponLocation(weapon);
            bool res = APClient.IsLocationChecked(loc);
            Logging.Log($"{APClient.GetCheck(loc).LocationName}: {loc}");
            return res;
        }
        internal static bool IsAPLocationChecked(ShopSceneItem item) =>
            ShopSceneItemHook.IsAPLocationChecked(item);

        internal static bool APIsAllItemsBought() {
            foreach (Weapon weapon in weapons) {
                if (!IsAPWeaponChecked(weapon)) return false;
            }
            foreach (Charm charm in charms) {
                if (!IsAPCharmChecked(charm)) return false;
            }
            return true;
        }
    }
}
