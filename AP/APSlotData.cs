/// Copyright 2025 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using CupheadArchipelago.Util;

namespace CupheadArchipelago.AP {
    public class APSlotData {
        internal const long AP_SLOTDATA_VERSION = 2;

        public long version {get; private set;}
        public APVersion world_version {get; private set;}
        public LevelShuffleMap level_shuffle_map {get; private set;}
        public ShopSet[] shop_map {get; private set;}
        public bool use_dlc {get; private set;}
        public GameModes mode {get; private set;}
        public bool expert_mode {get; private set;}
        public APItem start_weapon {get; private set;}
        public WeaponExModes randomize_weapon_ex {get; private set;}
        public int[] contract_requirements {get; private set;}
        public int dlc_ingredient_requirements {get; private set;}
        public int contract_goal_requirements {get; private set;}
        public int dlc_ingredient_goal_requirements {get; private set;}
        public bool freemove_isles {get; private set;}
        public bool randomize_abilities {get; private set;}
        public bool randomize_abilities_aim {get; private set;}
        public GradeChecks boss_grade_checks {get; private set;}
        public GradeChecks rungun_grade_checks {get; private set;}
        public int start_maxhealth {get; private set;}
        public DlcChaliceModes dlc_chalice {get; private set;}
        public DlcCurseModes dlc_curse_mode {get; private set;}
        public bool trap_loadout_anyweapon {get; private set;}
        public MusicGroups music_shuffle {get; private set;}
        public bool deathlink {get; private set;}

        public APSlotData(Dictionary<string, object> slotData) {
            version = GetAPSlotDataLong(slotData, "version");
            world_version = GetAPSlotDataVersion(slotData, "world_version");
            level_shuffle_map = new LevelShuffleMap(GetAPSlotDataDeserialized<Dictionary<long, long>>(slotData, "level_shuffle_map"));
            shop_map = GetAPShopMap(slotData);
            //Logging.Log($"shop_map: {shop_map}");
            use_dlc = GetAPSlotDataBool(slotData, "use_dlc");
            mode = (GameModes)GetAPSlotDataLong(slotData, "mode");
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
            randomize_weapon_ex = (WeaponExModes)GetAPSlotDataLong(slotData, "randomize_weapon_ex");
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
            dlc_chalice = (DlcChaliceModes)GetAPSlotDataLong(slotData, "dlc_chalice");
            dlc_curse_mode = (DlcCurseModes)GetAPSlotDataLong(slotData, "dlc_curse_mode");
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
        private static object GetOptionalAPSlotData(Dictionary<string, object> slotData, string key, object fallbackValue) {
            try {
                return GetAPSlotData(slotData, key);
            } catch (KeyNotFoundException) {
                Logging.LogWarning($"GetOptionalAPSlotData: \"{key}\" is not a valid key. The world you are connecting to probably does not support this feature. Using default value of \"{fallbackValue}\"");
                return fallbackValue;
            }
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
        private static APVersion GetAPSlotDataVersion(Dictionary<string, object> slotData, string key) {
            string vraw = GetAPSlotDataString(slotData, key);
            APVersion res;
            try {
                res = new APVersion(vraw);
            } catch (Exception) {
                Logging.LogWarning($"[APSlotData] Invalid version {vraw}");
                res = new APVersion();
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