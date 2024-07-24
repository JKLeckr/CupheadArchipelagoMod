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
        private static readonly string[] AP_SAVE_FILE_KEYS = new string[3]
        {
            "cuphead_player_data_v1_ap_slot_0_apdata",
            "cuphead_player_data_v1_ap_slot_1_apdata",
            "cuphead_player_data_v1_ap_slot_2_apdata"
        };
        private static readonly string AP_SAVE_PATH = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Cuphead");

        public static bool Initialized { get; private set; } = false;
        public static APData[] SData { get; private set; }
        public static APData CurrentSData { get => SData[global::PlayerData.CurrentSaveFileIndex]; }
        public static APData SessionSData { get => APClient.APSessionGSData; }

        public long version {get; private set;} = AP_DATA_VERSION;
        public bool enabled = false;
        public string address = "archipelago.gg";
        public int port = 38281;
        public string slot = "Player";
        public string password = "";
        public string seed = "";
        public PlayerData playerData = new PlayerData();
        public DataPackage dataPackage = new DataPackage();
        public List<long> doneChecks = new List<long>();
        public long receivedItemIndex {get => receivedItems.Count;}
        public Queue<NetworkItem> receivedItemApplyQueue = new Queue<NetworkItem>();
        public Queue<NetworkItem> receivedLevelItemApplyQueue = new Queue<NetworkItem>();
        public List<NetworkItem> receivedItems = new List<NetworkItem>();
        public List<NetworkItem> appliedItems = new List<NetworkItem>();
        private Goal goalsCompleted = Goal.None;

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
                    try {
                        string sdata = File.ReadAllText(filename);
                        data = JsonConvert.DeserializeObject<APData>(sdata);
                    }
                    catch (Exception e) {
                        Plugin.Log($"[APData] Unable to read AP Save Data for {i}: " + e.StackTrace, LogLevel.Error);
                    }
                    if (data == null) {
                        Plugin.Log("[APData] Data could not be unserialized for key: " + AP_SAVE_FILE_KEYS[i] + ". Loading defaults.", LogLevel.Error);
                        SData[i] = new APData();
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
            Plugin.Log($"Saving slot {index}");
            APData data = SData[index];
            if (data.version != AP_DATA_VERSION) {
                Plugin.LogError($"[APData] Slot {index} Data version mismatch. {data.version} != {AP_DATA_VERSION}. Skipping.");
                return;
            }
            string filename = Path.Combine(AP_SAVE_PATH, AP_SAVE_FILE_KEYS[index]+".sav");
            string sdata = JsonConvert.SerializeObject(data);
            try {
                File.WriteAllText(filename, sdata);
            }
            catch (Exception e) {
                Plugin.Log($"Unable to save AP Save Data for {index}: " + e.StackTrace, LogLevel.Error);
            }
        }
        public static void SaveAll() {
            for (int i=0;i<AP_SAVE_FILE_KEYS.Length;i++) {
                Save(i);
            }
        }
        public static void SaveCurrent() => Save(global::PlayerData.CurrentSaveFileIndex);

        public static void ResetData(int index, bool disable = false, bool resetSettings = false) { // TODO: disable is false by default for testing purposes
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
            
            public int contracts = 0;
            public int dlc_ingredients = 0;

            public bool dash = false;
            public bool duck = false;
            public bool parry = false;
            public bool plane_parry = false;
            public bool plane_shrink = false;
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

            private bool got_start_weapon = false;
            public bool HasStartWeapon() => got_start_weapon;
            public void GotStartWeapon() => got_start_weapon = true;
        }
    }
}