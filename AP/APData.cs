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
        private static readonly string AP_SAVE_PATH_WIN = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)+"\\Cuphead\\";
        private static readonly string AP_SAVE_PATH_MAC = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)+"/Cuphead/";
        private static readonly string AP_SAVE_PATH;

        public static bool Initialized { get; private set; }
        public static APData[] SData { get; private set; }
        public static APData CurrentSData { get => SData[PlayerData.CurrentSaveFileIndex]; }
        public static APData SessionSData { get => APClient.APSessionSData; }
        
        /* Overrides */
        private static readonly bool? hard = null;
        private static readonly bool? freemoveIsles = false;
        private static readonly int? bossGradeChecks = 0;
        private static readonly int? rungunGradeChecks = 0;

        public bool UseDLC {get => (int)(long)slotData["use_dlc"]>0;}
        public bool Hard {
            get => (hard.HasValue)?(bool)hard:(long)slotData["expert_mode"]>0;
        }
        public bool FreemoveIsles {
            get => (freemoveIsles.HasValue)?(bool)freemoveIsles:(long)slotData["freemove_isles"]>0;
        }
        public int BossGradeChecks {
            get => (bossGradeChecks.HasValue)?(int)bossGradeChecks:(int)(long)slotData["boss_grade_checks"];
        }
        public int RungunGradeChecks {
            get => (rungunGradeChecks.HasValue)?(int)rungunGradeChecks:(int)(long)slotData["rungun_grade_checks"];
        }

        public int version {get; private set;}
        public bool enabled;
        public string address;
        public string slot;
        public string password;
        public string seed;
        public DataPackage data;
        public Dictionary<string, object> slotData;
        public string DataSum {get => (data!=null&&slot!=null)?data.Games[slot].Checksum:"0";}

        static APData() {
            Initialized = false;
            AP_SAVE_PATH = Environment.OSVersion.Platform==PlatformID.MacOSX?AP_SAVE_PATH_MAC:AP_SAVE_PATH_WIN;
        }
        public APData(bool enabled = false,
                        string address = null,
                        string slot = null,
                        string password = null,
                        string seed = null
        ) {
            this.version = AP_WORLD_VERSION;
            this.enabled = enabled;
            this.address = address;
            this.slot = slot;
            this.password = password;
            this.seed = seed;
            this.data = null;
            this.slotData = null;
        }

        public static void Init() {
            SData = new APData[3];
            for (int i=0;i<SData.Length;i++) {
                SData[i] = new APData(false,"","","");
            }
        }

        public static void LoadData(bool overrideSave = false) {
            Plugin.Log($"[APData] Loading Data");
            for (int i=0;i<AP_SAVE_FILE_KEYS.Length;i++) {
                string filename = AP_SAVE_PATH + AP_SAVE_FILE_KEYS[i]+".sav";
                if (File.Exists(filename)) {
                    APData data = null;
                    try {
                        string sdata = File.ReadAllText(filename);
                        data = JsonUtility.FromJson<APData>(sdata);
                    }
                    catch (Exception e) {
                        Plugin.Log($"[APData] Unable to read AP Save Data for {i}: " + e.StackTrace, LogLevel.Error);
                    }
                    if (data == null)
                        Plugin.Log((object) ("[APData] Data could not be unserialized for key: " + AP_SAVE_FILE_KEYS[i]), LogLevel.Error);
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
            string filename = AP_SAVE_PATH + AP_SAVE_FILE_KEYS[index]+".sav";
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

        public static void ResetData(int index, bool disable = true, bool resetSettings = false) {
            APData data = SData[index];
            //if (disable) data.enabled = false; //NOTE: This is a temp disable for testing purposes
            if (resetSettings) {
                data.address = null;
                data.slot = null;
                data.password = null;
            }
            data.seed = null;
            data.data = null;
            Save(index);
        }

        public static bool IsSlotEnabled(int index) {
            return APData.Initialized&&APData.SData[index].enabled;
        }
        public static bool IsCurrentSlotEnabled() {
            return APData.Initialized&&APData.SData[PlayerData.CurrentSaveFileIndex].enabled;
        }

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