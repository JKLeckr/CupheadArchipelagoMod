/// Copyright 2024 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Archipelago.MultiClient.Net.Models;
using BepInEx.Logging;

namespace CupheadArchipelago.AP {
    public class APData {
        private const int AP_WORLD_VERSION = 0;
        private static readonly string[] AP_SAVE_FILE_KEYS = new string[3]
        {
            "cuphead_player_data_v1_ap_slot_0_apdata",
            "cuphead_player_data_v1_ap_slot_1_apdata",
            "cuphead_player_data_v1_ap_slot_2_apdata"
        };
        private static readonly string AP_SAVE_PATH = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Cuphead");

        public static bool Initialized { get; private set; } = false;
        public static APData[] SData { get; private set; }
        public static APData CurrentSData { get => SData[PlayerData.CurrentSaveFileIndex]; }
        public static APData SessionSData { get => APClient.APSessionGSData; }

        public int version {get; private set;} = AP_WORLD_VERSION;
        public bool enabled = false;
        public string address = "archipelago.gg";
        public int port = 38281;
        public string slot = "Player";
        public string password = "";
        public string seed = "";
        public Dictionary<string, object> slotData = null;
        public DataPackage dataPackage = null;
        public List<long> doneChecks = new List<long>();
        public long ReceivedItemIndex {get => receivedItems.Count;}
        public Queue<NetworkItem> receivedItemApplyQueue = new Queue<NetworkItem>();
        public Queue<NetworkItem> receivedLevelItemApplyQueue = new Queue<NetworkItem>();
        public List<NetworkItem> receivedItems = new List<NetworkItem>();
        public List<NetworkItem> appliedReceivedItems = new List<NetworkItem>();

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
                        data = JsonUtility.FromJson<APData>(sdata);
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
            string filename = Path.Combine(AP_SAVE_PATH, AP_SAVE_FILE_KEYS[index]+".sav");
            APData data = SData[index];
            string sdata = JsonUtility.ToJson(SData[index]);
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
        public static void SaveCurrent() => Save(PlayerData.CurrentSaveFileIndex);

        public static void ResetData(int index, bool disable = true, bool resetSettings = false) {
            APData old_data = SData[index];
            SData[index] = new APData();
            APData data = SData[index];
            //if (!disable) data.enabled = old_data.enabled; //NOTE: This is a temp disable for testing purposes
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
        public static bool IsCurrentSlotEnabled() => IsSlotEnabled(PlayerData.CurrentSaveFileIndex);

        private static bool SlotDataIsEmpty(int index) {
            if (PlayerData.Initialized) {
                PlayerData slotData = PlayerData.GetDataForSlot(index);
                return !slotData.GetMapData(Scenes.scene_map_world_1).sessionStarted && 
                !slotData.IsTutorialCompleted && 
                slotData.CountLevelsCompleted(Level.world1BossLevels) == 0;
            }
            else {
                Plugin.Log("[APData] PlayerData is not initialized!", LogLevel.Warning);
                return true;
            }
        }
    }
}