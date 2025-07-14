/// Copyright 2025 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using CupheadArchipelago.Mapping;
using CupheadArchipelago.Util;
using Newtonsoft.Json;
using FVer;

namespace CupheadArchipelago.AP {
    internal class APSlotData {
        internal const long AP_SLOTDATA_VERSION = 4;
        internal const long AP_SLOTDATA_MIN_VERSION = 4;

        internal readonly long version;
        internal readonly FVersion world_version;
        internal readonly LevelMap level_map;
        internal readonly ShopSet[] shop_map;
        internal readonly bool use_dlc;
        internal readonly GameModes mode;
        internal readonly bool expert_mode;
        internal readonly APItem start_weapon;
        internal readonly WeaponModes weapon_mode;
        internal readonly int[] contract_requirements;
        internal readonly int dlc_ingredient_requirements;
        internal readonly int contract_goal_requirements;
        internal readonly int dlc_ingredient_goal_requirements;
        internal readonly bool freemove_isles;
        internal readonly bool randomize_abilities;
        internal readonly bool randomize_abilities_aim;
        internal readonly GradeChecks boss_grade_checks;
        internal readonly GradeChecks rungun_grade_checks;
        internal readonly int start_maxhealth;
        internal readonly int start_maxhealth_p2;
        internal readonly DlcChaliceModes dlc_chalice;
        internal readonly DlcChaliceCheckModes dlc_boss_chalice_checks;
        internal readonly DlcChaliceCheckModes dlc_rungun_chalice_checks;
        internal readonly DlcCurseModes dlc_curse_mode;
        internal readonly bool trap_loadout_anyweapon;
        internal readonly MusicGroups music_shuffle;
        internal readonly bool ducklock_platdrop;
        internal readonly bool deathlink;

        internal APSlotData(Dictionary<string, object> slotData) {
            version = GetAPSlotDataValue<long>(slotData, "version");
            world_version = GetAPSlotDataVersion(slotData, "world_version");
            level_map = new LevelMap(GetAPSlotDataDeserializedValue<Dictionary<long, long>>(slotData, "level_map"));
            shop_map = GetAPShopMap(slotData);
            //Logging.Log($"shop_map: {shop_map}");
            use_dlc = GetAPSlotDataValue<bool>(slotData, "use_dlc");
            mode = GetAPSlotDataValue<GameModes>(slotData, "mode");
            expert_mode = GetAPSlotDataValue<bool>(slotData, "expert_mode");
            start_weapon = GetAPSlotDataValue<sbyte>(slotData, "start_weapon") switch {
                1 => APItem.weapon_spread,
                2 => APItem.weapon_chaser,
                3 => APItem.weapon_lobber,
                4 => APItem.weapon_charge,
                5 => APItem.weapon_roundabout,
                6 => APItem.weapon_dlc_crackshot,
                7 => APItem.weapon_dlc_converge,
                8 => APItem.weapon_dlc_twistup,
                _ => APItem.weapon_peashooter,
            };
            Logging.Log($"start_weapon: {start_weapon.id}");
            weapon_mode = GetAPSlotDataValue<WeaponModes>(slotData, "weapon_mode");
            contract_requirements = GetAPSlotDataDeserializedValue<List<int>>(slotData, "contract_requirements").ToArray();
            dlc_ingredient_requirements = GetAPSlotDataValue<int>(slotData, "dlc_ingredient_requirements");
            contract_goal_requirements = GetAPSlotDataValue<int>(slotData, "contract_goal_requirements");
            dlc_ingredient_goal_requirements = GetAPSlotDataValue<int>(slotData, "dlc_ingredient_goal_requirements");
            freemove_isles = GetAPSlotDataValue<bool>(slotData, "freemove_isles");
            randomize_abilities = GetAPSlotDataValue<bool>(slotData, "randomize_abilities");
            randomize_abilities_aim = false; //GetAPSlotDataValue<bool>(slotData, "randomize_abilities_aim");
            boss_grade_checks = GetAPSlotDataValue<GradeChecks>(slotData, "boss_grade_checks");
            rungun_grade_checks = GetAPSlotDataValue<GradeChecks>(slotData, "rungun_grade_checks");
            start_maxhealth = GetAPSlotDataValue<int>(slotData, "start_maxhealth");
            start_maxhealth_p2 = GetOptionalAPSlotDataValue(slotData, "start_maxhealth_p2", start_maxhealth);
            dlc_chalice = GetAPSlotDataValue<DlcChaliceModes>(slotData, "dlc_chalice");
            dlc_boss_chalice_checks = GetAPSlotDataValue<DlcChaliceCheckModes>(slotData, "dlc_boss_chalice_checks");
            dlc_rungun_chalice_checks = GetAPSlotDataValue<DlcChaliceCheckModes>(slotData, "dlc_rungun_chalice_checks");
            dlc_curse_mode = GetAPSlotDataValue<DlcCurseModes>(slotData, "dlc_curse_mode");
            trap_loadout_anyweapon = GetAPSlotDataValue<bool>(slotData, "trap_loadout_anyweapon");
            music_shuffle = 0; //GetOptionalAPSlotDataValue<MusicGroups>(slotData, "music_rando", 0);
            ducklock_platdrop = GetOptionalAPSlotDataValue(slotData, "ducklock_platdrop", false);
            deathlink = GetAPSlotDataValue<bool>(slotData, "deathlink");
        }

        internal static long GetSlotDataVersion(Dictionary<string, object> slotData) => GetAPSlotDataValue<long>(slotData, "version");
        internal static string GetAPWorldVersionString(Dictionary<string, object> slotData) => GetAPSlotDataValue<string>(slotData, "world_version");
        
        private static T GetAPSlotDataDeserializedValue<T>(Dictionary<string, object> slotData, string key) {
            try {
                string raw = GetAPSlotDataValue<string>(slotData, key);
                //Logging.Log($"[APSlotData]: Deserializing: \"{raw}\"");
                return JsonConvert.DeserializeObject<T>(raw);
            } catch (Exception e) {
                throw new ArgumentException($"GetAPSlotDataDeserializedValue: Invalid SlotData data for: \"{key}\"! Exception: {e}");
            }
        }
        private static T GetOptionalAPSlotDataDeserializedValue<T>(Dictionary<string, object> slotData, string key, T fallbackValue) {
            try {
                string raw = GetAPSlotDataValue<string>(slotData, key);
                //Logging.Log($"[APSlotData]: Deserializing: \"{raw}\"");
                return JsonConvert.DeserializeObject<T>(raw);
            } catch (Exception e) {
                Logging.LogWarning($"GetOptionalAPSlotDataDeserializedValue: Invalid SlotData data for: \"{key}\"! Exception: {e}");
                Logging.LogWarning($"Using fallback value of \"{fallbackValue}\" for \"{key}\"");
                return fallbackValue;
            }
        }
        private static T GetAPSlotDataValue<T>(Dictionary<string, object> slotData, string key) {
            if (slotData.ContainsKey(key)) {
                try {
                    return Converter.ConvertTo<T>(slotData[key]);
                } catch (InvalidCastException e) {
                    throw new InvalidCastException($"GetAPSlotData: Could not get value from \"{key}\". Exception: {e.Message}");
                }
            } else {
                throw new KeyNotFoundException($"GetAPSlotDataValue: \"{key}\" is not a valid key!");
            }
        }
        private static T GetOptionalAPSlotDataValue<T>(Dictionary<string, object> slotData, string key, T fallbackValue) {
            if (slotData.ContainsKey(key)) {
                try {
                    return Converter.ConvertTo<T>(slotData[key]);
                } catch (InvalidCastException e) {
                    Logging.LogWarning($"GetOptionalAPSlotDataValue: Could not get value from \"{key}\". Exception: {e.Message}");
                }
            } else {
                Logging.LogWarning($"GetOptionalAPSlotData: \"{key}\" is not a valid key. The world you are connecting to probably does not support this feature.");
            }
            Logging.LogWarning($"Using fallback value of \"{fallbackValue}\" for \"{key}\"");
            return fallbackValue;
        }
        private static FVersion GetAPSlotDataVersion(Dictionary<string, object> slotData, string key) {
            string vraw = GetAPSlotDataValue<string>(slotData, key);
            FVersion res;
            try {
                res = new FVersion(vraw);
            } catch (Exception e) {
                Logging.LogWarning($"[APSlotData] Invalid version {vraw}. Exception: {e}");
                res = new FVersion(0, 0, 0, "unsupported");
            }
            return res;
        }
        private static ShopSet[] GetAPShopMap(Dictionary<string, object> slotData) {
            string shopMapKey = "shop_map";
            try {
                List<List<int>> map_raw = GetAPSlotDataDeserializedValue<List<List<int>>>(slotData, shopMapKey);
                ShopSet[] map = new ShopSet[map_raw.Count];
                for (int i=0;i<map.Length;i++) {
                    map[i] = new ShopSet(map_raw[i][0], map_raw[i][1]);
                }
                return map;
            } catch (Exception e) {
                throw new ArgumentException($"Invalid ShopMap data! Exception: {e}");
            }
        }
    }
}
