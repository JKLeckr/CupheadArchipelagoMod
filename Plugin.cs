/// Copyright 2024 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

using System;
using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Logging;
using BepInEx.Configuration;

namespace CupheadArchipelago {
    [BepInPlugin("com.JKLeckr.CupheadArchipelago", "CupheadArchipelago", PluginInfo.PLUGIN_VERSION)]
    [BepInDependency(DEP_SAVECONFIG_MOD_GUID, BepInDependency.DependencyFlags.SoftDependency)]
    [BepInProcess("Cuphead.exe")]
    public class Plugin : BaseUnityPlugin {
        internal const string DEP_SAVECONFIG_MOD_GUID = "com.JKLeckr.CupheadSaveConfig";
        private const long CONFIG_VERSION = 1;

        private static Plugin Instance { get; set;}
        public static bool ConfigSkipIntro => Instance.configSkipIntro.Value;
        public static string Name => PluginInfo.PLUGIN_NAME;
        public static string Version => PluginInfo.PLUGIN_VERSION;
        public static int State { get; private set; } = 0;

        private ConfigEntry<long> configVersion;
        private ConfigEntry<bool> configEnabled;
        private ConfigEntry<bool> configModLogs;
        private ConfigEntry<LoggingFlags> configLoggingFlags;
        private ConfigEntry<LicenseLogModes> configLogLicense;
        private ConfigEntry<string> configSaveKeyName;
        private ConfigEntry<bool> configSkipIntro;

        private void Awake() {
            Instance = this;
            configVersion = Config.Bind("Main", "version", CONFIG_VERSION);
            configEnabled = Config.Bind("Main", "Enabled", true);
            configModLogs = Config.Bind("Logging", "ModLogFiles", true, "Writes mod logs to files. They are not overwritten on startup unlike the main BepInEx log (unless configured not to).");
            configLoggingFlags = Config.Bind("Logging", "LoggingFlags", LoggingFlags.PluginInfo|LoggingFlags.Info|LoggingFlags.Message|LoggingFlags.Warning, "Set mod logging verbosity.");
            configLogLicense = Config.Bind("Logging", "LogLicense", LicenseLogModes.Off, "Log the copyright notice and license on load.\nFirstParty prints only the notice for this mod itself.\nAll includes third party notices for the libraries used. (Careful! Will flood the terminal and log!)");
            configSaveKeyName = Config.Bind("SaveConfig", "SaveKeyName", "cuphead_player_data_v1_ap_slot_",
                "Set save data prefix.\nPlease note that using Vanilla save files can cause data loss. It is recommended not to use Vanilla saves (Default: \"cuphead_player_data_v1_ap_slot_\", Vanilla: \"cuphead_player_data_v1_slot_\")");
            configSkipIntro = Config.Bind("Game", "SkipIntro", true, "Skip the intro when starting a new ap game. (Default: true)");

            if (configEnabled.Value) {
                if (configModLogs.Value) SetupLogging();
                Log($"CupheadArchipelago {Version} by JKLeckr");
                
                Log("[Log] Info", LoggingFlags.Debug);
                LogWarning("[Log] Warning", LoggingFlags.Debug);
                LogError("[Log] Error", LoggingFlags.Debug);
                LogFatal("[Log] Fatal", LoggingFlags.Debug);
                LogMessage("[Log] Message", LoggingFlags.Debug);
                LogDebug("[Log] Debug", LoggingFlags.Debug);

                if (configVersion.Value != CONFIG_VERSION) {
                    LogWarning("Config version changed! You may want to check the config.");
                    configVersion.Value = CONFIG_VERSION;
                }

                if (configLogLicense.Value>0) {
                    ModLicense license = new();
                    string lictext = $"License:\n--- LICENSE ---\n{license.PLUGIN_NOTICE}\n";
                    if (configLogLicense.Value == LicenseLogModes.All) {
                        ModLicenseThirdParty license3rdParty = new();
                        lictext += $"\n -- Third Party --\n{license3rdParty.PLUGIN_LIB_FULL_NOTICE}\n\n -- End Third Party --";
                    }
                    else {
                        lictext += $"\n{license.PLUGIN_LIB_NOTICE}"; 
                    }
                    lictext += "\n\n--- END LICENSE";
                    Log(lictext);
                }

                if (!IsPluginLoaded(DEP_SAVECONFIG_MOD_GUID)) {
                    try {
                        Hooks.Main.HookSaveKeyUpdater(configSaveKeyName.Value);
                        Log($"Using Save Key: {configSaveKeyName.Value}");
                    } catch (Exception e) {
                        LogError("An exception occured while loading.");
                        LogFatal($"Plugin {PluginInfo.PLUGIN_GUID} failed to load!");
                        State = -2;
                        LogFatal("Throwing Exception...");
                        throw e;
                    }
                } else Log($"[CupheadArchipelago] Plugin {DEP_SAVECONFIG_MOD_GUID} is loaded, skipping SaveConfig", LoggingFlags.PluginInfo);
                try {
                    Hooks.Main.HookMain();
                } catch (Exception e) {
                    LogError("An exception occured while loading.");
                    LogFatal($"Plugin {PluginInfo.PLUGIN_GUID} failed to load!");
                    State = -1;
                    LogFatal("Throwing Exception...");
                    throw e;
                }
                State = 1;
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

        private static void SetupLogging() {
            ModLogListener listener = new("CupheadAPLog", Instance.Logger.SourceName);
            BepInEx.Logging.Logger.Listeners.Add(listener);
        }

        public static void Log(object data) {
            Log(data, LogLevel.Info);
        }
        public static void Log(object data, LogLevel logLevel) {
            Log(data, (logLevel==LogLevel.Fatal||logLevel==LogLevel.Error)?LoggingFlags.None:LoggingFlags.Info, logLevel);
        }
        public static void Log(object data, LoggingFlags requiredFlags) {
            Log(data, requiredFlags, LogLevel.Info);
        }
        public static void Log(object data, LoggingFlags requiredFlags, LogLevel logLevel) {
            if (IsLoggingFlagsEnabled(requiredFlags)) {
                Instance.Logger.Log(logLevel, data);
            }
        }
        public static void LogMessage(object data) => LogMessage(data, LoggingFlags.Message);
        public static void LogMessage(object data, LoggingFlags requiredFlags) {
            Log(data, requiredFlags, LogLevel.Message);
        }
        public static void LogWarning(object data) => LogWarning(data, LoggingFlags.Warning);
        public static void LogWarning(object data, LoggingFlags requiredFlags) {
            Log(data, requiredFlags, LogLevel.Warning);
        }
        public static void LogError(object data) => LogError(data, LoggingFlags.None);
        private static void LogError(object data, LoggingFlags requiredFlags) {
            Log(data, requiredFlags, LogLevel.Error);
        }
        public static void LogFatal(object data) => LogFatal(data, LoggingFlags.None);
        private static void LogFatal(object data, LoggingFlags requiredFlags) {
            Log(data, requiredFlags, LogLevel.Fatal);
        }
        public static void LogDebug(object data) => LogDebug(data, LoggingFlags.Debug);
        public static void LogDebug(object data, LoggingFlags requiredFlags) {
            Log(data, requiredFlags, LogLevel.Debug);
        }
        public static bool IsLoggingFlagsEnabled(LoggingFlags flags) {
            return (((int)flags)&((int)Instance.configLoggingFlags.Value))==(int)flags;
        }
        public static bool IsDebug() => IsLoggingFlagsEnabled(LoggingFlags.Debug);
    }
}
