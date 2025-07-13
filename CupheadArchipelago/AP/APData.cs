/// Copyright 2025 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.IO;
using CupheadArchipelago.Config;
using Newtonsoft.Json;

namespace CupheadArchipelago.AP {
    public class APData {
        internal const int AP_DATA_VERSION = 2;

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
        [JsonIgnore]
        private long ltime = 0;
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
        [JsonProperty("ftime")]
        private long ftime = 0;
        [JsonProperty("override")]
        private int _override = 0;

        static APData() {
            SData = new APData[3];
            Init();
        }

        internal static void Init() {
            for (int i=0;i<SData.Length;i++) {
                SData[i] = new APData() {
                    index = i
                };
            }
        }

        public static void LoadData() => LoadData(true);
        public static void LoadData(bool showPath) {
            if (!SaveData.IsInitialized()) {
                Logging.LogError("[APData] Not initialized! Cannot load data!");
                return;
            }
            if (showPath)
                Logging.Log($"[APData] Loading APData from \"{SaveData.APSavePath}\"...");
            else
                Logging.Log("[APData] Loading APData...");
            for (int i=0;i<SaveData.AP_SAVE_FILE_KEYS.Length;i++) {
                Load(i);
            }
            Loaded = true;
        }
        private static void Load(int index) {
            string filename = SaveData.AP_SAVE_FILE_KEYS[index]+".sav";
            string filepath = Path.Combine(SaveData.APSavePath, filename);
            if (File.Exists(filepath)) {
                APData data = null;
                sbyte state = 0;
                try {
                    string sdata = File.ReadAllText(filepath);
                    data = JsonConvert.DeserializeObject<APData>(sdata);
                    data.ltime = DateTime.UtcNow.Ticks;
                    //Logging.Log($"Dump:\n{sdata}");
                    //Logging.Log($"Digest:\n{JsonConvert.SerializeObject(data)}");
                }
                catch (Exception e) {
                    Logging.LogError($"[APData] Unable to read AP Save Data for slot {index}: {e}");
                    state = -1;
                }
                if (data == null) {
                    Logging.LogError($"[APData] Data could not be unserialized for key: {filename}. Loading defaults.");
                    SData[index] = new APData {
                        state = state
                    };
                }
                else {
                    data.index = index;
                    SData[index] = data;
                    if (data._override != 0) {
                        Logging.LogWarning($"[APData] Slot {index}: There are overrides enabled ({data._override}). I hope you know what you are doing!");
                    } 
                    if (data.version != AP_DATA_VERSION && !data.IsEmpty() && data.IsOverridden(Overrides.DataVersionOverride)) {
                        Logging.LogWarning($"[APData] Slot {index}: Data version mismatch. {data.version} != {AP_DATA_VERSION}. Risk of data loss!");
                        data.state = 1;
                    }
                    if (data.IsOverridden(Overrides.CleanupReceivedItemsOverride)) {
                        data._override &= ~(int)Overrides.CleanupReceivedItemsOverride;
                        Logging.LogWarning($"[APData] Slot {index}: Cleaning up received items...");
                        int counter = 0;
                        for (int j=0;j<data.receivedItems.Count;j++) {
                            if ((data.receivedItems[index]?.id ?? -1) == -1) {
                                data.receivedItems.RemoveAt(index);
                                counter++;
                            }
                        }
                        Logging.LogWarning($"[APData] Slot {index}: Removed {counter} items.");
                    }
                }
            }
            else {
                Logging.LogWarning($"[APData] No data. Saving default data for slot {index}");
                Save(index, false);
                SData[index].ltime = DateTime.UtcNow.Ticks;
            }
        }
        public static void Save(int index) {
            SData[index].Save(global::PlayerData.inGame);
        }
        internal static void Save(int index, bool writeTime) {
            SData[index].Save(writeTime);
        }
        private void Save(bool writeTime) {
            if (!SaveData.IsInitialized()) {
                Logging.LogError("[APData] SaveData Not initialized! Cannot save data!");
                return;
            }
            Logging.Log($"[APData] Saving slot {index}");
            if (version != AP_DATA_VERSION && !IsOverridden(Overrides.DataVersionOverride)) {
                Logging.LogError($"[APData] Slot {index} Data version mismatch. {version} != {AP_DATA_VERSION}. Skipping.");
                return;
            }
            version = AP_DATA_VERSION;
            if (dlock) {
                Logging.LogWarning($"[APData] Slot {index} is locked, cannot save at this time.");
                return;
            }
            string filename = Path.Combine(SaveData.APSavePath, SaveData.AP_SAVE_FILE_KEYS[index]+".sav");
            if (writeTime) {
                long diff = DateTime.UtcNow.Ticks - ltime;
                if (diff > 0) ftime += diff;
                else Logging.LogWarning("[APData] File time elapsed is negative!");
                ltime = DateTime.UtcNow.Ticks;
            }
            try {
                string sdata = JsonConvert.SerializeObject(this);
                File.WriteAllText(filename, sdata);
            }
            catch (Exception e) {
                Logging.LogError($"[APData] Error while saving AP Save Data for {index}: {e}");
                return;
            }
        }
        public static void SaveAll() => SaveAll(global::PlayerData.inGame);
        public static void SaveAll(bool writeTime) {
            if (!SaveData.IsInitialized()) {
                Logging.LogError("[APData] Not initialized! Cannot save data!");
                return;
            }
            for (int i=0;i<SaveData.AP_SAVE_FILE_KEYS.Length;i++) {
                Save(i, writeTime);
            }
        }
        public static void SaveCurrent() => Save(global::PlayerData.CurrentSaveFileIndex);

        public static void ResetAllData() {
            bool reset = MConf.ResetAPConfigOnFileDelete();
            for (int i=0;i<SData.Length;i++) ResetData(i, reset, reset);
        }
        public static void ResetData(int index) {
            bool reset = MConf.ResetAPConfigOnFileDelete();
            ResetData(index, reset, reset);
        }
        public static void ResetData(int index, bool disable, bool resetSettings) {
            ResetData(index, disable, resetSettings, !SData[index].IsOverridden(Overrides.OverrideResetOverride));
        }
        public static void ResetData(int index, bool disable, bool resetSettings, bool resetOverrides) {
            Logging.Log("[APData] Resetting Data...");
            APData old_data = SData[index];
            APData data = new() {
                index = old_data.index
            };
            if (!disable) data.enabled = old_data.enabled;
            if (!resetSettings) {
                data.address = old_data.address;
                data.port = old_data.port;
                data.player = old_data.player;
                data.password = old_data.password;
            }
            if (!resetOverrides) {
                data._override = old_data._override;
            }
            SData[index] = data;
            Save(index);
        }
        internal long GetFTime() => ftime;
        internal void ResetLTime() {
            ltime = DateTime.UtcNow.Ticks;
        }

        public static bool IsSlotEnabled(int index) {
            return Loaded && SData[index].enabled;
        }
        public static bool IsCurrentSlotEnabled() => IsSlotEnabled(global::PlayerData.CurrentSaveFileIndex);
        public static bool IsSlotLocked(int index) {
            return Loaded && SData[index].dlock;
        }
        public static bool IsCurrentSlotLocked() => IsSlotLocked(global::PlayerData.CurrentSaveFileIndex);

        public bool IsOverridden(Overrides o) => IsOverridden((int)o);
        public bool IsAnyOverridden(Overrides o) => IsAnyOverridden((int)o);
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
        public bool AreGoalsCompleted(Goals goals) => (goals & goalsCompleted) >= goals;

        public class PlayerData {
            [Flags]
            public enum SetTarget {
                None = 0,
                Essential = 1,
                ChaliceEssential = 2,
                AllEssential = 3,
                Super = 4,
                ChaliceSuper = 8,
                AllSuper = Super | ChaliceSuper,
                Abilities = 16,
                ChaliceAbilities = 32,
                AllAbilities = Abilities | ChaliceAbilities,
                Aim = 64,
                ChaliceAim = 128,
                AllAim = Aim | ChaliceAim,
                All = int.MaxValue,
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
            [JsonProperty("weapons")]
            private Dictionary<Weapon, uint> weapons = [];

            public void SetBoolValues(bool value, SetTarget setTarget) {
                if ((setTarget&SetTarget.Abilities)>0) dash = value;
                if ((setTarget&SetTarget.Abilities)>0) duck = value;
                if ((setTarget&SetTarget.Abilities)>0) parry = value;
                if ((setTarget&SetTarget.Abilities)>0) plane_parry = value;
                if ((setTarget&SetTarget.Abilities)>0) plane_shrink = value;
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

            internal void ClearWeaponUpgrades() {
                weapons = [];
            }
            public void AddWeaponBit(Weapon weapon, uint bit) {
                if (!weapons.ContainsKey(weapon)) {
                    Logging.Log($"Adding {weapon}");
                    weapons.Add(weapon, 0);
                }
                if ((weapons[weapon] & bit)==0) Logging.Log($"Adding {weapon} bit {bit}");
                weapons[weapon] |= bit;
            }
            public void AddWeaponsBit(IEnumerable<Weapon> weapons, uint bit) {
                foreach (Weapon w in weapons) {
                    AddWeaponBit(w, bit);
                }
            }
            public bool WeaponHasBit(Weapon weapon, uint bit) {
                return weapons.ContainsKey(weapon) ? (weapons[weapon] & bit) == bit : false;
            }

            [JsonProperty("got_start_weapon")]
            private bool got_start_weapon = false;
            public bool HasStartWeapon() => got_start_weapon;
            public void GotStartWeapon() => got_start_weapon = true;
        }
    }
}
