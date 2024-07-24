/// Copyright 2024 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

using System.Collections.Generic;
using Newtonsoft.Json;
using CupheadArchipelago.Util;

namespace CupheadArchipelago.AP {
    public class APSlotData {
        public long version {get; private set;}
        public long id_version {get; private set;}
        public double world_version {get; private set;}
        public string[] levels {get; private set;}
        public LevelShuffleMap level_shuffle_map {get; private set;}
        public ShopSet[] shop_map {get; private set;}
        public bool use_dlc {get; private set;}
        public bool expert_mode {get; private set;}
        public APItem start_weapon {get; private set;}
        public bool freemove_isles {get; private set;}
        public GradeChecks boss_grade_checks {get; private set;}
        public GradeChecks rungun_grade_checks {get; private set;}
        public bool deathlink {get; private set;}

        public APSlotData(Dictionary<string, object> slotData) {
            version = GetAPSlotDataLong(slotData, "version");
            id_version = GetAPSlotDataLong(slotData, "id_version");
            world_version = GetAPSlotDataFloat(slotData, "world_version");
            //Plugin.Log($"levels: {GetAPSlotData(slotData, "levels")}");
            levels = GetAPSlotDataDeserialized<List<string>>(slotData, "levels").ToArray();
            level_shuffle_map = new LevelShuffleMap(GetAPSlotDataDeserialized<Dictionary<string, string>>(slotData, "level_shuffle_map"));
            shop_map = GetAPShopMap(slotData);
            //Plugin.Log($"shop_map: {shop_map}");
            use_dlc = GetAPSlotDataBool(slotData, "use_dlc");
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
            freemove_isles = GetAPSlotDataBool(slotData, "freemove_isles");
            boss_grade_checks = (GradeChecks)GetAPSlotDataLong(slotData, "boss_grade_checks");
            rungun_grade_checks = (GradeChecks)GetAPSlotDataLong(slotData, "rungun_grade_checks");
            deathlink = GetAPSlotDataBool(slotData, "deathlink");
        }

        public static long GetSlotDataVersion(Dictionary<string, object> slotData) => GetAPSlotDataLong(slotData, "version");
        private static bool GetAPSlotDataBool(Dictionary<string, object> slotData, string key) => Aux.IntAsBool(GetAPSlotDataLong(slotData, key));
        private static long GetAPSlotDataLong(Dictionary<string, object> slotData, string key) => (long)GetAPSlotData(slotData, key);
        private static double GetAPSlotDataFloat(Dictionary<string, object> slotData, string key) => (double)GetAPSlotData(slotData, key);
        private static string GetAPSlotDataString(Dictionary<string, object> slotData, string key) => GetAPSlotData(slotData, key).ToString();
        private static T GetAPSlotDataDeserialized<T>(Dictionary<string, object> slotData, string key) => JsonConvert.DeserializeObject<T>(GetAPSlotDataString(slotData, key));
        private static object GetAPSlotData(Dictionary<string, object> slotData, string key) {
            try { 
                return slotData[key];
            } catch (KeyNotFoundException) {
                throw new KeyNotFoundException($"GetAPSlotData: {key} is not a valid key!");
            }
        }
        private static ShopSet[] GetAPShopMap(Dictionary<string, object> slotData) {
            string shopMapKey = "shop_map";
            List<List<int>> map_raw = GetAPSlotDataDeserialized<List<List<int>>>(slotData, shopMapKey);
            ShopSet[] map = new ShopSet[map_raw.Count];
            for (int i=0;i<map.Length;i++) {
                map[i] = new ShopSet(map_raw[i][0], map_raw[i][1]);
            }
            return map;
        }
    }
}