/// Copyright 2025 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace CupheadArchipelago.AP {
    public class APData {
        internal const int AP_DATA_VERSION = 1;

        public static bool Loaded { get; private set; } = false;
        public static APData[] SData { get; private set; }
        public static APData CurrentSData { get => SData[global::PlayerData.CurrentSaveFileIndex]; }
        public static APData SessionSData { get => APClient.APSessionGSData; }

        [JsonIgnore]
        public int index {get; private set;} = 0;
        [JsonIgnore]
        public sbyte state {get; private set;} = 0;
        [JsonIgnore]
        internal bool dlock = false;
        [JsonProperty("version")]
        public long version {get; private set;} = AP_DATA_VERSION;
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
        public PlayerData playerData = new();
        [JsonProperty("doneChecks")]
        public List<long> doneChecks = [];
        [JsonProperty("receivedItems")]
        public List<APItemData> receivedItems = [];
        [JsonProperty("goalsCompleted")]
        private Goals goalsCompleted = Goals.None;
        [JsonProperty("override")]
        private int _override = 0;

        static APData() {
            SData = new APData[3];
            for (int i=0;i<SData.Length;i++) {
                SData[i] = new APData();
            }
        }

        public static void LoadData() {
            if (!SaveData.IsInitialized()) {
                Logging.LogError("[APData] Not initialized! Cannot load data!");
                return;
            }
            Logging.Log($"[APData] Loading Data from {SaveData.AP_SAVE_PATH}");
            for (int i=0;i<SaveData.AP_SAVE_FILE_KEYS.Length;i++) {
                string filename = Path.Combine(SaveData.AP_SAVE_PATH, SaveData.AP_SAVE_FILE_KEYS[i]+".sav");
                if (File.Exists(filename)) {
                    APData data = null;
                    sbyte state = 0;
                    try {
                        string sdata = File.ReadAllText(filename);
                        data = JsonConvert.DeserializeObject<APData>(sdata);
                        //Logging.Log($"Dump:\n{sdata}");
                        //Logging.Log($"Digest:\n{JsonConvert.SerializeObject(data)}");
                    }
                    catch (Exception e) {
                        Logging.LogError($"[APData] Unable to read AP Save Data for slot {i}: {e}");
                        state = -1;
                    }
                    if (data == null) {
                        Logging.LogError($"[APData] Data could not be unserialized for key: {SaveData.AP_SAVE_FILE_KEYS[i]}. Loading defaults.");
                        SData[i] = new APData {
                            state = state
                        };
                    }
                    else {
                        data.index = i;
                        SData[i] = data;
                        if (data._override != 0) {
                            Logging.LogWarning($"[APData] Slot {i}: There are overrides enabled ({data._override}). I hope you know what you are doing!");
                        } 
                        if (data.version != AP_DATA_VERSION && !data.IsEmpty() && data.IsOverridden(1)) {
                            Logging.LogWarning($"[APData] Slot {i}: Data version mismatch. {data.version} != {AP_DATA_VERSION}. Risk of data loss!");
                            data.state = 1;
                        }
                    }
                }
                else {
                    Logging.LogWarning($"[APData] No data. Saving default data for slot {i}");
                    Save(i);
                }
            }
            Loaded = true;
        }
        public static void Save(int index) {
            if (!SaveData.IsInitialized()) {
                Logging.LogError("[APData] Not initialized! Cannot save data!");
                return;
            }
            Logging.Log($"[APData] Saving slot {index}");
            APData data = SData[index];
            if (data.version != AP_DATA_VERSION && !data.IsOverridden(1)) {
                Logging.LogError($"[APData] Slot {index} Data version mismatch. {data.version} != {AP_DATA_VERSION}. Skipping.");
                return;
            }
            data.version = AP_DATA_VERSION;
            if (data.dlock) {
                Logging.LogWarning($"[APData] Slot {index} is locked, cannot save at this time.");
                return;
            }
            string filename = Path.Combine(SaveData.AP_SAVE_PATH, SaveData.AP_SAVE_FILE_KEYS[index]+".sav");
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
            if (!SaveData.IsInitialized()) {
                Logging.LogError("[APData] Not initialized! Cannot save data!");
                return;
            }
            for (int i=0;i<SaveData.AP_SAVE_FILE_KEYS.Length;i++) {
                Save(i);
            }
        }
        public static void SaveCurrent() => Save(global::PlayerData.CurrentSaveFileIndex);

        public static void ResetData(int index) {
            bool reset = Config.ResetAPConfigOnFileDelete();
            ResetData(index, reset, reset);
        }
        public static void ResetData(int index, bool disable, bool resetSettings) {
            Logging.Log("[APData] Resetting Data...");
            APData old_data = SData[index];
            SData[index] = new APData() {
                index = old_data.index
            };
            APData data = SData[index];
            if (!disable) data.enabled = old_data.enabled;
            if (!resetSettings) {
                data.address = old_data.address;
                data.port = old_data.port;
                data.player = old_data.player;
                data.password = old_data.password;
            }
            if (old_data.IsOverridden(16)) {
                data._override = old_data._override;
            }
            Save(index);
        }

        public static bool IsSlotEnabled(int index) {
            return Loaded && SData[index].enabled;
        }
        public static bool IsCurrentSlotEnabled() => IsSlotEnabled(global::PlayerData.CurrentSaveFileIndex);
        public static bool IsSlotLocked(int index) {
            return Loaded && SData[index].dlock;
        }
        public static bool IsCurrentSlotLocked() => IsSlotLocked(global::PlayerData.CurrentSaveFileIndex);

        public bool IsOverridden(int i) {
            return (_override & i) == i;
        }
        public bool IsAnyOverridden(int i) {
            return (_override & i) != 0;
        }

        public bool IsEmpty() => IsEmpty(SaveDataType.Auto);
        public bool IsEmpty(SaveDataType saveDataType) {
            bool cond;
            bool res;
            if (saveDataType == SaveDataType.Auto)
                cond = enabled || saveDataType != SaveDataType.Vanilla;
            else
                cond = saveDataType == SaveDataType.AP;
            if (cond) {
                res = !playerData.HasStartWeapon();
            } else {
                global::PlayerData data = global::PlayerData.GetDataForSlot(index);
                res = !data.GetMapData(Scenes.scene_map_world_1).sessionStarted && !data.IsTutorialCompleted && data.CountLevelsCompleted(Level.world1BossLevels) == 0;
            }
            //Logging.Log($"[APData] Slot {index}: {(res?"E":"Not e")}mpty");
            return res;
        }
        public static bool IsSlotEmpty(int index) => IsSlotEmpty(index, SaveDataType.Auto);
        public static bool IsSlotEmpty(int index, SaveDataType saveDataType) {
            if (global::PlayerData.Initialized) {
                APData sdata = SData[index];
                return sdata.IsEmpty(saveDataType);
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
                ChaliceEssential = 2,
                AllEssential = 3,
                Super = 4,
                ChaliceSuper = 8,
                AllSuper = 12,
                BasicAbilities = 16,
                ChaliceAbilities = 32,
                AllAbilities = 48,
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
            [JsonProperty("plane_ex")]
            public bool plane_ex = false;
            [JsonProperty("dlc_cplane_ex")]
            public bool dlc_cplane_ex = false;
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
            [JsonProperty("dlc_cdoublejump")]
            public bool dlc_cdoublejump = false;
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

            [JsonProperty("weaponupgrades")]
            private HashSet<Weapon> weaponupgrades = [];

            public void SetBoolValues(bool value, SetTarget setTarget) {
                if ((setTarget&SetTarget.BasicAbilities)>0) dash = value;
                if ((setTarget&SetTarget.BasicAbilities)>0) duck = value;
                if ((setTarget&SetTarget.BasicAbilities)>0) parry = value;
                if ((setTarget&SetTarget.BasicAbilities)>0) plane_parry = value;
                if ((setTarget&SetTarget.BasicAbilities)>0) plane_shrink = value;
                if ((setTarget&SetTarget.ChaliceAbilities)>0) dlc_cdash = value;
                if ((setTarget&SetTarget.ChaliceAbilities)>0) dlc_cduck = value;
                if ((setTarget&SetTarget.ChaliceAbilities)>0) dlc_cparry = value;
                if ((setTarget&SetTarget.ChaliceAbilities)>0) dlc_cdoublejump = value;
                if ((setTarget&SetTarget.ChaliceAbilities)>0) dlc_cplane_parry = value;
                if ((setTarget&SetTarget.ChaliceAbilities)>0) dlc_cplane_shrink = value;
                if ((setTarget&SetTarget.Super)>0) plane_super = value;
                if ((setTarget&SetTarget.ChaliceSuper)>0) dlc_cplane_super = value;
                if ((setTarget&SetTarget.Essential)>0) dlc_boat = value;
            }
            public void SetIntValues(int value, SetTarget setTarget) {
                if ((setTarget&SetTarget.Essential)>0) contracts = value;
                if ((setTarget&SetTarget.Essential)>0) dlc_ingredients = value;
            }

            public void AddWeaponUpgrade(Weapon weapon) {
                if (!weaponupgrades.Contains(weapon)) {
                    weaponupgrades.Add(weapon);
                }
            }
            public void AddWeaponUpgrades(IEnumerable<Weapon> weapons) {
                weaponupgrades.UnionWith(weapons);
            }
            public void ClearWeaponUpgrades() {
                weaponupgrades = new();
            }
            public bool GetWeaponUpgrade(Weapon weapon) {
                return weaponupgrades.Contains(weapon);
            }

            [JsonProperty("got_start_weapon")]
            private bool got_start_weapon = false;
            public bool HasStartWeapon() => got_start_weapon;
            public void GotStartWeapon() => got_start_weapon = true;
        }
    }
}