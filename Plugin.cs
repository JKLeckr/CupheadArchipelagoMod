/// Copyright 2024 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

﻿using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Logging;
using BepInEx.Configuration;

namespace CupheadArchipelago {
    [BepInPlugin("com.JKLeckr.CupheadArchipelago", "CupheadArchipelago", PluginInfo.PLUGIN_VERSION)]
    [BepInDependency(DEP_CUPHEAD_SAVE_CONFIG_GUID, BepInDependency.DependencyFlags.SoftDependency)]
    [BepInProcess("Cuphead.exe")]
    public class Plugin : BaseUnityPlugin {
        public const string VERSION = "20230928";
        internal const string DEP_CUPHEAD_SAVE_CONFIG_GUID = "com.JKLeckr.CupheadSaveConfig";

        public static Plugin Instance { get; private set; }
        public static bool ConfigSkipIntro => Instance.configSkipIntro.Value;

        private ConfigEntry<bool> configEnabled;
        private ConfigEntry<LoggingFlags> configLogging;
        private ConfigEntry<string> configSaveKeyName;
        private ConfigEntry<bool> configSkipIntro;

        private void Awake() {
            Instance = this;
            configEnabled = Config.Bind("Main", "Enabled", true);
            configLogging = Config.Bind("Main", "Logging", LoggingFlags.PluginInfo|LoggingFlags.Info, "Set mod logging verbosity.");
            configSaveKeyName = Config.Bind("SaveConfig", "SaveKeyName", "cuphead_player_data_v1_ap_slot_",
                "Set save data prefix.\nPlease note that using Vanilla save files can cause data loss. It is recommended not to use a Vanilla save (Default: \"cuphead_player_data_v1_ap_slot_\", Vanilla: \"cuphead_player_data_v1_slot_\")");
            configSkipIntro = Config.Bind("Game", "SkipIntro", true, "Skip the intro when starting a new ap game. (Default: true)");

            if (configEnabled.Value) {
                if (!IsPluginLoaded(DEP_CUPHEAD_SAVE_CONFIG_GUID)) {
                    Hooks.SaveKeyUpdaterHook.SetSaveKeyBaseName(configSaveKeyName.Value);
                    Hooks.SaveKeyUpdaterHook.Hook();
                    Log("Using Save Key: " + configSaveKeyName.Value);
                } else Log($"[CupheadArchipelago] Plugin {DEP_CUPHEAD_SAVE_CONFIG_GUID} is loaded, skipping SaveConfig", LoggingFlags.PluginInfo);
                Hook();
                Log($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!", LoggingFlags.PluginInfo);
            }
            else {
                Log($"Plugin {PluginInfo.PLUGIN_GUID} is loaded, but disabled!", LoggingFlags.PluginInfo); 
            }
        }

        private bool IsPluginLoaded(string plugin) => FindPlugin(plugin)>=0;
        private int FindPlugin(string plugin) {
            int index = 0;

            foreach (var p in Chainloader.PluginInfos) {
                BepInPlugin metadata = p.Value.Metadata;
                if (metadata.GUID.Equals(plugin)) {
                    return index;
                }
                index++;
            }       

            return -1;
        }

        private void Hook() {
            Hooks.CupheadHook.Hook();
            Hooks.OnlineInterfaceHook.Hook();
            Hooks.StartScreenHook.Hook();
            Hooks.SlotSelectScreenHook.Hook();
            Hooks.SlotSelectScreenSlotHook.Hook();
            Hooks.PlayerDataHook.Hook();
            Hooks.MapHook.Hook();
            Hooks.MapLevelDependentObstacleHook.Hook();
            Hooks.MapCoinHook.Hook();
            Hooks.MapNPCAppletravellerHook.Hook();
            Hooks.MapDifficultySelectStartUIHook.Hook();
            Hooks.LevelHook.Hook();
            Hooks.PlatformingLevelHook.Hook();
            Hooks.LevelCoinHook.Hook();
        }

        public static void Log(object data, params object[] args) {
            Log(data, LogLevel.Info, args);
        }
        public static void Log(object data, LogLevel logLevel, params object[] args) {
            Log(data, (logLevel==LogLevel.Fatal||logLevel==LogLevel.Error)?LoggingFlags.None:LoggingFlags.Info, logLevel, args);
        }
        public static void Log(object data, LoggingFlags requiredFlags, params object[] args) {
            Log(data, requiredFlags, LogLevel.Info, args);
        }
        public static void Log(object data, LoggingFlags requiredFlags, LogLevel logLevel, params object[] args) {
            if (IsLoggingFlagsEnabled(requiredFlags)) Instance.Logger.Log(logLevel, string.Format(data.ToString(), args));
        }
        public static void LogWarning(object data, LoggingFlags requiredFlags, params object[] args) {
            Log(data, LoggingFlags.Warning | requiredFlags, LogLevel.Warning, args);
        }
        public static void LogWarning(object data, params object[] args) => LogWarning(data, LoggingFlags.None, args);
        public static void LogError(object data, params object[] args) => Log(data, LoggingFlags.None, LogLevel.Error, args);
        public static void LogFatal(object data, params object[] args) => Log(data, LoggingFlags.None, LogLevel.Fatal, args);
        public static bool IsLoggingFlagsEnabled(LoggingFlags flags) {
            return (((int)flags)&(int)Instance.configLogging.Value)==(int)flags;
        }
    }
}
