/// Copyright 2025 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.IO;
using Archipelago.MultiClient.Net.Models;
using Newtonsoft.Json;

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
        [JsonProperty("player")]
        public string player = "Player";
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
        public List<APItemData> receivedItems = new();
        [JsonProperty("goalsCompleted")]
        private Goals goalsCompleted = Goals.None;
        [JsonIgnore]
        internal bool dlock = false;

        static APData() {
            SData = new APData[3];
            for (int i=0;i<SData.Length;i++) {
                SData[i] = new APData();
            }
        }

        public static void LoadData() {
            Logging.Log($"[APData] Loading Data from {AP_SAVE_PATH}");
            for (int i=0;i<AP_SAVE_FILE_KEYS.Length;i++) {
                string filename = Path.Combine(AP_SAVE_PATH, AP_SAVE_FILE_KEYS[i]+".sav");
                if (File.Exists(filename)) {
                    APData data = null;
                    int error = 0;
                    try {
                        string sdata = File.ReadAllText(filename);
                        data = JsonConvert.DeserializeObject<APData>(sdata);
                        //Logging.Log($"Dump:\n{sdata}");
                        //Logging.Log($"Digest:\n{JsonConvert.SerializeObject(data)}");
                    }
                    catch (Exception e) {
                        Logging.LogError($"[APData] Unable to read AP Save Data for slot {i}: {e}");
                        error = 1;
                    }
                    if (data == null) {
                        Logging.LogError($"[APData] Data could not be unserialized for key: {AP_SAVE_FILE_KEYS[i]}. Loading defaults.");
                        SData[i] = new APData {
                            error = error
                        };
                    }
                    else {
                        SData[i] = data;
                        if (data.version != AP_DATA_VERSION) {
                            Logging.LogWarning($"[APData] Data version mismatch. {data.version} != {AP_DATA_VERSION}. Risk of data loss!");
                        }
                    }
                }
                else {
                    Logging.LogWarning($"[APData] No data. Saving default data for slot {i}");
                    Save(i);
                }
            }
            Initialized = true;
        }
        public static void Save(int index) {
            Logging.Log($"[APData] Saving slot {index}");
            APData data = SData[index];
            if (data.version != AP_DATA_VERSION) {
                Logging.LogError($"[APData] Slot {index} Data version mismatch. {data.version} != {AP_DATA_VERSION}. Skipping.");
                return;
            }
            if (data.dlock) {
                Logging.LogWarning($"[APData] Slot {index} is locked, cannot save at this time.");
                return;
            }
            string filename = Path.Combine(AP_SAVE_PATH, AP_SAVE_FILE_KEYS[index]+".sav");
            try {
                string sdata = JsonConvert.SerializeObject(data);
                File.WriteAllText(filename, sdata);
            }
            catch (Exception e) {
                Logging.LogError($"[APData] Error while saving AP Save Data for {index}: {e}");
                return;
            }
        }
        public static void SaveAll() {
            for (int i=0;i<AP_SAVE_FILE_KEYS.Length;i++) {
                Save(i);
            }
        }
        public static void SaveCurrent() => Save(global::PlayerData.CurrentSaveFileIndex);

        public static void ResetData(int index) {
            bool reset = Config.DeleteAPConfigOnFileDelete();
            ResetData(index, reset, reset);
        }
        public static void ResetData(int index, bool disable, bool resetSettings) {
            Logging.Log("[APData] Resetting Data...");
            APData old_data = SData[index];
            SData[index] = new APData();
            APData data = SData[index];
            if (!disable) data.enabled = old_data.enabled;
            if (!resetSettings) {
                data.address = old_data.address;
                data.port = old_data.port;
                data.player = old_data.player;
                data.password = old_data.password;
            }
            Save(index);
        }

        public static bool IsSlotEnabled(int index) {
            return Initialized&&SData[index].enabled;
        }
        public static bool IsCurrentSlotEnabled() => IsSlotEnabled(global::PlayerData.CurrentSaveFileIndex);
        public static bool IsSlotLocked(int index) {
            return Initialized && SData[index].dlock;
        }
        public static bool IsCurrentSlotLocked() => IsSlotLocked(global::PlayerData.CurrentSaveFileIndex);

        public static bool IsSlotEmpty(int index, bool checkVanillaIfAPDisabled=false) {
            if (global::PlayerData.Initialized) {
                APData sdata = SData[index];
                if (sdata.enabled || !checkVanillaIfAPDisabled) {
                    return !sdata.playerData.HasStartWeapon();
                } else {
                    global::PlayerData data = global::PlayerData.GetDataForSlot(index);
                    return !data.GetMapData(Scenes.scene_map_world_1).sessionStarted && !data.IsTutorialCompleted && data.CountLevelsCompleted(Level.world1BossLevels) == 0;
                }
            }
            else {
                Logging.LogWarning("[APData] PlayerData is not initialized!");
                return true;
            }
        }

        public void AddGoals(Goals goals) => goalsCompleted |= goals;
        public void RemoveGoals(Goals goals) => goalsCompleted &= ~goals;
        public void ResetGoals() => goalsCompleted = Goals.None;
        public bool IsGoalsCompleted(Goals goals) => (goals & goalsCompleted) >= goals;

        public class PlayerData {
            [Flags]
            public enum SetTarget {
                None = 0,
                Essential = 1,
                BasicAbilities = 2,
                All = int.MaxValue,
            }
            [Flags]
            public enum AimDirections {
                None = 0,
                Left = 1,
                Right = 2,
                Up = 4,
                Down = 8,
                UpLeft = 16,
                UpRight = 32,
                DownLeft = 64,
                DownRight = 128,
                All = 255
            }
            
            [JsonProperty("contracts")]
            public int contracts = 0;
            [JsonProperty("plane_super")]
            public bool plane_super = false;
            [JsonProperty("dlc_cplane_super")]
            public bool dlc_cplane_super = false;
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
            [JsonProperty("dlc_cdash")]
            public bool dlc_cdash = false;
            [JsonProperty("dlc_cduck")]
            public bool dlc_cduck = false;
            [JsonProperty("dlc_cparry")]
            public bool dlc_cparry = false;
            [JsonProperty("dlc_cplane_parry")]
            public bool dlc_cplane_parry = false;
            [JsonProperty("dlc_cplane_shrink")]
            public bool dlc_cplane_shrink = false;
            
            [JsonProperty("healthupgrades")]
            public int healthupgrades = 0;
            [JsonProperty("dlc_boat")]
            public bool dlc_boat = false;
            [JsonProperty("stat_coins_collected")]
            public int coins_collected = 0;
            [JsonProperty("aim_directions")]
            public AimDirections aim_directions = 0;

            public void SetBoolValues(bool value, SetTarget setTarget) {
                if ((setTarget&SetTarget.BasicAbilities)>0) dash = value;
                if ((setTarget&SetTarget.BasicAbilities)>0) duck = value;
                if ((setTarget&SetTarget.BasicAbilities)>0) parry = value;
                if ((setTarget&SetTarget.BasicAbilities)>0) plane_parry = value;
                if ((setTarget&SetTarget.BasicAbilities)>0) plane_shrink = value;
                if ((setTarget&SetTarget.BasicAbilities)>0) dlc_cdash = value;
                if ((setTarget&SetTarget.BasicAbilities)>0) dlc_cduck = value;
                if ((setTarget&SetTarget.BasicAbilities)>0) dlc_cparry = value;
                if ((setTarget&SetTarget.BasicAbilities)>0) dlc_cplane_parry = value;
                if ((setTarget&SetTarget.BasicAbilities)>0) dlc_cplane_shrink = value;
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