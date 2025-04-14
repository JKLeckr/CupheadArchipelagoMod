/// Copyright 2025 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using CupheadArchipelago.Util;
using FVer;

namespace CupheadArchipelago.AP {
    public class APSlotData {
        internal const long AP_SLOTDATA_VERSION = 2;

        public readonly long version;
        public readonly FVersion world_version;
        public readonly LevelShuffleMap level_shuffle_map;
        public readonly ShopSet[] shop_map;
        public readonly bool use_dlc;
        public readonly GameMode mode;
        public readonly bool expert_mode;
        public readonly APItem start_weapon;
        public readonly WeaponExMode randomize_weapon_ex;
        public readonly int[] contract_requirements;
        public readonly int dlc_ingredient_requirements;
        public readonly int contract_goal_requirements;
        public readonly int dlc_ingredient_goal_requirements;
        public readonly bool freemove_isles;
        public readonly bool randomize_abilities;
        public readonly bool randomize_abilities_aim;
        public readonly GradeChecks boss_grade_checks;
        public readonly GradeChecks rungun_grade_checks;
        public readonly int start_maxhealth;
        public readonly DlcChaliceMode dlc_chalice;
        public readonly DlcCurseMode dlc_curse_mode;
        public readonly bool trap_loadout_anyweapon;
        public readonly MusicGroups music_shuffle;
        public readonly bool deathlink;

        public APSlotData(Dictionary<string, object> slotData) {
            version = GetAPSlotDataLong(slotData, "version");
            world_version = GetAPSlotDataVersion(slotData, "world_version");
            level_shuffle_map = new LevelShuffleMap(GetAPSlotDataDeserialized<Dictionary<long, long>>(slotData, "level_shuffle_map"));
            shop_map = GetAPShopMap(slotData);
            //Logging.Log($"shop_map: {shop_map}");
            use_dlc = GetAPSlotDataBool(slotData, "use_dlc");
            mode = (GameMode)GetAPSlotDataLong(slotData, "mode");
            expert_mode = GetAPSlotDataBool(slotData, "expert_mode");
            start_weapon = GetAPSlotDataLong(slotData, "start_weapon") switch {
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
            randomize_weapon_ex = (WeaponExMode)GetAPSlotDataLong(slotData, "randomize_weapon_ex");
            contract_requirements = GetAPSlotDataDeserialized<List<int>>(slotData, "contract_requirements").ToArray();
            dlc_ingredient_requirements = (int)GetAPSlotDataLong(slotData, "dlc_ingredient_requirements");
            contract_goal_requirements = (int)GetAPSlotDataLong(slotData, "contract_goal_requirements");
            dlc_ingredient_goal_requirements = (int)GetAPSlotDataLong(slotData, "dlc_ingredient_goal_requirements");
            freemove_isles = GetAPSlotDataBool(slotData, "freemove_isles");
            randomize_abilities = GetAPSlotDataBool(slotData, "randomize_abilities");
            randomize_abilities_aim = false; //GetAPSlotDataBool(slotData, "randomize_abilities_aim");
            boss_grade_checks = (GradeChecks)GetAPSlotDataLong(slotData, "boss_grade_checks");
            rungun_grade_checks = (GradeChecks)GetAPSlotDataLong(slotData, "rungun_grade_checks");
            start_maxhealth = (int)GetAPSlotDataLong(slotData, "start_maxhealth");
            dlc_chalice = (DlcChaliceMode)GetAPSlotDataLong(slotData, "dlc_chalice");
            dlc_curse_mode = (DlcCurseMode)GetAPSlotDataLong(slotData, "dlc_curse_mode");
            trap_loadout_anyweapon = GetAPSlotDataBool(slotData, "trap_loadout_anyweapon");
            music_shuffle = 0; //(MusicGroups)(long)GetOptionalAPSlotData(slotData, "music_rando", 0);
            deathlink = GetAPSlotDataBool(slotData, "deathlink");
        }

        public static long GetSlotDataVersion(Dictionary<string, object> slotData) => GetAPSlotDataLong(slotData, "version");
        public static string GetAPWorldVersionString(Dictionary<string, object> slotData) => GetAPSlotDataString(slotData, "world_version");
        private static bool GetAPSlotDataBool(Dictionary<string, object> slotData, string key) => Aux.IntAsBool(GetAPSlotDataLong(slotData, key));
        private static long GetAPSlotDataLong(Dictionary<string, object> slotData, string key) => (long)GetAPSlotData(slotData, key);
        private static double GetAPSlotDataFloat(Dictionary<string, object> slotData, string key) => (double)GetAPSlotData(slotData, key);
        private static string GetAPSlotDataString(Dictionary<string, object> slotData, string key) => GetAPSlotData(slotData, key).ToString();
        private static T GetAPSlotDataDeserialized<T>(Dictionary<string, object> slotData, string key) {
            try {
                string raw = GetAPSlotDataString(slotData, key);
                //Logging.Log($"[APSlotData]: Deserializing: \"{raw}\"");
                return JsonConvert.DeserializeObject<T>(raw);
            } catch (Exception e) {
                throw new ArgumentException($"Invalid SlotData data for: \"{key}\"! Exception: {e}");
            }
        }
        private static T GetOptionalAPSlotData<T>(Dictionary<string, object> slotData, string key, T fallbackValue) {
            if (slotData.ContainsKey(key)) {
                try {
                    return (T)GetAPSlotData(slotData, key);
                } catch (InvalidCastException e) {
                    Logging.LogWarning($"GetOptionalAPSlotData: Could not get value from \"{key}\". Exception: {e.Message}");
                }
            } else {
                Logging.LogWarning($"GetOptionalAPSlotData: \"{key}\" is not a valid key. The world you are connecting to probably does not support this feature.");
            }
            Logging.LogWarning($"Using fallback value of \"{fallbackValue}\" for \"{key}\"");
            return fallbackValue;
        }
        private static object GetAPSlotData(Dictionary<string, object> slotData, string key) {
            try { 
                return slotData[key];
            } catch (KeyNotFoundException) {
                throw new KeyNotFoundException($"GetAPSlotData: \"{key}\" is not a valid key!");
            } catch (Exception e) {
                throw new Exception($"GetAPSlotData: Failed to get \"{key}\" from slot data! Exception: {e}");
            }
        }
        private static FVersion GetAPSlotDataVersion(Dictionary<string, object> slotData, string key) {
            string vraw = GetAPSlotDataString(slotData, key);
            string[] mainParts = vraw.Split(['-'], 2);
            string[] versionParts = mainParts[0].Split(['.'], 4);
            FVersion res;
            try {
                if (mainParts.Length > 1 && versionParts.Length >= 3) {
                    Logging.Log($"[APSlotData] Got legacy version format. Converting.");
                    res = new APVersion(vraw).AsFVersion();
                }
                else res = new FVersion(vraw);
            } catch (Exception e) {
                Logging.LogWarning($"[APSlotData] Invalid version {vraw}. Exception: {e}");
                res = FVersion.Zero();
            }
            return res;
        }
        private static ShopSet[] GetAPShopMap(Dictionary<string, object> slotData) {
            string shopMapKey = "shop_map";
            try {
                List<List<int>> map_raw = GetAPSlotDataDeserialized<List<List<int>>>(slotData, shopMapKey);
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
