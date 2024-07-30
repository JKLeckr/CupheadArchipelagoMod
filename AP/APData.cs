/// Copyright 2024 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.IO;
using Archipelago.MultiClient.Net.Models;
using Newtonsoft.Json;
using BepInEx.Logging;

namespace CupheadArchipelago.AP {
    public class APData {
        internal const int AP_DATA_VERSION = 0;
        private static readonly string[] AP_SAVE_FILE_KEYS = [
            "cuphead_player_data_v1_ap_slot_0_apdata",
            "cuphead_player_data_v1_ap_slot_1_apdata",
            "cuphead_player_data_v1_ap_slot_2_apdata"
        ];
        private static readonly string AP_SAVE_PATH = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Cuphead");

        public static bool Initialized { get; private set; } = false;
        public static APData[] SData { get; private set; }
        public static APData CurrentSData { get => SData[global::PlayerData.CurrentSaveFileIndex]; }
        public static APData SessionSData { get => APClient.APSessionGSData; }

        [JsonProperty("version")]
        public long version {get; private set;} = AP_DATA_VERSION;
        [JsonIgnore]
        public int error {get; private set;} = 0;
        [JsonProperty("enabled")]
        public bool enabled = false;
        [JsonProperty("address")]
        public string address = "archipelago.gg";
        [JsonProperty("port")]
        public int port = 38281;
        [JsonProperty("slot")]
        public string slot = "Player";
        [JsonProperty("password")]
        public string password = "";
        [JsonProperty("seed")]
        public string seed = "";
        [JsonProperty("playerData")]
        public PlayerData playerData = new PlayerData();
        [JsonIgnore]
        public DataPackage dataPackage = new DataPackage();
        [JsonProperty("doneChecks")]
        public List<long> doneChecks = new List<long>();
        [JsonProperty("receivedItems")]
        public List<APItemInfo> receivedItems = new();
        [JsonProperty("goalsCompleted")]
        private Goal goalsCompleted = Goal.None;
        [JsonIgnore]
        internal bool dlock = false;

        static APData() {
            SData = new APData[3];
            for (int i=0;i<SData.Length;i++) {
                SData[i] = new APData();
            }
        }

        public static void LoadData() {
            Plugin.Log($"[APData] Loading Data from {AP_SAVE_PATH}");
            for (int i=0;i<AP_SAVE_FILE_KEYS.Length;i++) {
                string filename = Path.Combine(AP_SAVE_PATH, AP_SAVE_FILE_KEYS[i]+".sav");
                if (File.Exists(filename)) {
                    APData data = null;
                    int error = 0;
                    try {
                        string sdata = File.ReadAllText(filename);
                        data = JsonConvert.DeserializeObject<APData>(sdata);
                    }
                    catch (Exception e) {
                        Plugin.LogError($"[APData] Unable to read AP Save Data for slot {i}: {e}");
                        error = 1;
                    }
                    if (data == null) {
                        Plugin.LogError($"[APData] Data could not be unserialized for key: {AP_SAVE_FILE_KEYS[i]}. Loading defaults.");
                        SData[i] = new APData {
                            error = error
                        };
                    }
                    else {
                        SData[i] = data;
                        if (data.version != AP_DATA_VERSION) {
                            Plugin.LogWarning($"[APData] Data version mismatch. {data.version} != {AP_DATA_VERSION}. Risk of data loss!");
                        }
                    }
                }
                else {
                    Plugin.Log($"[APData] No data. Saving default data for slot {i}", LogLevel.Warning);
                    Save(i);
                }
            }
            Initialized = true;
        }
        public static void Save(int index) {
            Plugin.Log($"[APData] Saving slot {index}");
            APData data = SData[index];
            if (data.version != AP_DATA_VERSION) {
                Plugin.LogError($"[APData] Slot {index} Data version mismatch. {data.version} != {AP_DATA_VERSION}. Skipping.");
                return;
            }
            if (data.dlock) {
                Plugin.LogWarning($"[APData] Slot {index} is locked, cannot save at this time.");
                return;
            }
            string filename = Path.Combine(AP_SAVE_PATH, AP_SAVE_FILE_KEYS[index]+".sav");
            try {
                string sdata = JsonConvert.SerializeObject(data);
                File.WriteAllText(filename, sdata);
            }
            catch (Exception e) {
                Plugin.LogError($"[APData] Error while saving AP Save Data for {index}: {e}");
                return;
            }
        }
        public static void SaveAll() {
            for (int i=0;i<AP_SAVE_FILE_KEYS.Length;i++) {
                Save(i);
            }
        }
        public static void SaveCurrent() => Save(global::PlayerData.CurrentSaveFileIndex);

        public static void ResetData(int index, bool disable = false, bool resetSettings = false) { // TODO: disable is false by default for testing purposes
            Plugin.Log("[APData] Resetting Data...");
            APData old_data = SData[index];
            SData[index] = new APData();
            APData data = SData[index];
            if (!disable) data.enabled = old_data.enabled;
            if (!resetSettings) {
                data.address = old_data.address;
                data.slot = old_data.slot;
                data.password = old_data.password;
            }
            Save(index);
        }

        public static bool IsSlotEnabled(int index) {
            return Initialized&&SData[index].enabled;
        }
        public static bool IsCurrentSlotEnabled() => IsSlotEnabled(global::PlayerData.CurrentSaveFileIndex);
        public static bool IsSlotLocked(int index) {
            return Initialized&&SData[index].dlock;
        }
        public static bool IsCurrentSlotLocked() => IsSlotLocked(global::PlayerData.CurrentSaveFileIndex);

        public static bool IsSlotEmpty(int index) {
            if (global::PlayerData.Initialized) {
                return SData[index].playerData.HasStartWeapon();
            }
            else {
                Plugin.Log("[APData] PlayerData is not initialized!", LogLevel.Warning);
                return true;
            }
        }

        public void AddGoals(Goal goals) => goalsCompleted |= goals;
        public void RemoveGoals(Goal goals) => goalsCompleted &= ~goals;
        public void ResetGoals() => goalsCompleted = Goal.None;
        public bool IsGoalsCompleted(Goal goals) => (goals & goalsCompleted) >= goals;

        public class PlayerData {
            [Flags]
            public enum SetTarget {
                None = 0,
                Essential = 1,
                BasicAbilities = 2,
                All = int.MaxValue,
            }
            
            [JsonProperty("contracts")]
            public int contracts = 0;
            [JsonProperty("dlc_ingredients")]
            public int dlc_ingredients = 0;

            [JsonProperty("dash")]
            public bool dash = false;
            [JsonProperty("duck")]
            public bool duck = false;
            [JsonProperty("parry")]
            public bool parry = false;
            [JsonProperty("plane_parry")]
            public bool plane_parry = false;
            [JsonProperty("plane_shrink")]
            public bool plane_shrink = false;
            [JsonProperty("dlc_boat")]
            public bool dlc_boat = false;

            public void SetBoolValues(bool value, SetTarget setTarget) {
                if ((setTarget&SetTarget.BasicAbilities)>0) dash = value;
                if ((setTarget&SetTarget.BasicAbilities)>0) duck = value;
                if ((setTarget&SetTarget.BasicAbilities)>0) parry = value;
                if ((setTarget&SetTarget.BasicAbilities)>0) plane_parry = value;
                if ((setTarget&SetTarget.BasicAbilities)>0) plane_shrink = value;
                if ((setTarget&SetTarget.Essential)>0) dlc_boat = value;
            }
            public void SetIntValues(int value, SetTarget setTarget) {
                if ((setTarget&SetTarget.Essential)>0) contracts = value;
                if ((setTarget&SetTarget.Essential)>0) dlc_ingredients = value;
            }

            [JsonProperty("got_start_weapon")]
            private bool got_start_weapon = false;
            public bool HasStartWeapon() => got_start_weapon;
            public void GotStartWeapon() => got_start_weapon = true;
        }
    }
}