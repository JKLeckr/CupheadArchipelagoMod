/// Copyright 2025 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

using System;
using System.IO;
using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Configuration;
using BepInEx.Logging;
using FVer;
using Newtonsoft.Json;

namespace CupheadArchipelago {
    [BepInPlugin(MOD_GUID, MOD_NAME, MOD_VERSION)]
    [BepInDependency(DEP_SAVECONFIG_MOD_GUID, BepInDependency.DependencyFlags.SoftDependency)]
    [BepInProcess("Cuphead.exe")]
    public class Plugin : BaseUnityPlugin {
        internal const string DEP_SAVECONFIG_MOD_GUID = "com.JKLeckr.CupheadSaveConfig";

        protected const string MOD_NAME = "CupheadArchipelago"; //PluginInfo.PLUGIN_NAME
        protected const string MOD_GUID = "com.JKLeckr.CupheadArchipelago";
        protected const string MOD_VERSION = PluginInfo.PLUGIN_VERSION;
        protected static readonly string MOD_FRIENDLY_VERSION = GetFVer(MOD_VERSION);

        private const long CONFIG_VERSION = 1;

        public static string Name => MOD_NAME;
        public static string Version => MOD_VERSION;
        public static string FullVersion => $"{MOD_FRIENDLY_VERSION} ({MOD_VERSION})";
        public static int State { get; private set; } = 0;
        
        private static readonly string verPath = Path.Combine(Path.Combine(Paths.PluginPath, MOD_NAME), "configver");
        private long configVer;
        private ConfigEntry<bool> configEnabled;
        private ConfigEntry<bool> configModLogs;
        private ConfigEntry<int> configModFileMax;
        private ConfigEntry<LoggingFlags> configLoggingFlags;
        private ConfigEntry<LicenseLogModes> configLogLicense;
        private ConfigEntry<string> configSaveKeyName;
        private ConfigEntry<Cutscenes> configSkipCutscenes;
        private ConfigEntry<bool> configSkipCutscenesAPOnly;
        private ConfigEntry<bool> configFileDeleteResetsAPData;
        private ConfigEntry<APStatsFunctions> configAPStatusFunctions;
        private ConfigEntry<bool> configDebugAsInfo;

        private void Awake() {
            SetupConfigVersion(this);
            configEnabled = Config.Bind("Main", "Enabled", true, "Mod Master Switch");
            configModLogs = Config.Bind("Logging", "ModLogFiles", true, "Writes mod logs to files. They are not overwritten on startup unlike the main BepInEx log (unless configured not to).");
            configModFileMax = Config.Bind("Logging", "ModLogFileMax", 10, "The maxmum amount of mod logs that can be in the mod logs folder at once. Oldest gets deleted when max is reached.");
            configLoggingFlags = Config.Bind("Logging", "LoggingFlags", LoggingFlags.PluginInfo | LoggingFlags.Info | LoggingFlags.Message | LoggingFlags.Warning | LoggingFlags.Network, "Set mod logging verbosity.");
            configLogLicense = Config.Bind("Logging", "LogLicense", LicenseLogModes.Off, "Log the copyright notice and license on load.\nFirstParty prints only the notice for this mod itself.\nAll includes third party notices for the libraries used. (Careful! Will flood the terminal and log!)");
            configDebugAsInfo = Config.Bind("Debug", "DebugAsInfo", true, "Logs mod debug messages to loglevel info instead of debug.");
            configSaveKeyName = Config.Bind("SaveConfig", "SaveKeyName", "cuphead_player_data_v1_ap_slot_",
                "Set save data prefix.\nPlease note that using Vanilla save files can cause data loss. It is recommended not to use Vanilla saves! (Vanilla: \"cuphead_player_data_v1_slot_\")");
            configSkipCutscenes = Config.Bind("Game", "SkipCutscenes", Cutscenes.Intro | Cutscenes.DLCIntro, "Skip the specified cutscenes in an AP game.");
            configSkipCutscenesAPOnly = Config.Bind("Game", "SkipCutscenesAPOnly", true, "Skip cutscenes only if playing an Archipelago game.");
            configFileDeleteResetsAPData = Config.Bind("AP", "FileDeleteResetsAPData", true, "When the save file is deleted, the Archipelago settings gets reset.");
            configAPStatusFunctions = Config.Bind("AP", "APStatusFunctions", APStatsFunctions.ConnectionIndicator, "Enable specific Archipelago Status HUD Functionalities. (Currently does nothing)");
            //configAPOverrides = Config.Bind("AP", "Overrides", true, "Overrides specific non-functional server-side settings.");
            CupheadArchipelago.Config.Init(
                configDebugAsInfo.Value,
                configSkipCutscenes.Value,
                configSkipCutscenesAPOnly.Value,
                configFileDeleteResetsAPData.Value,
                configAPStatusFunctions.Value
            );

            if (configEnabled.Value) {
                if (configModLogs.Value) SetupLogging(this);
                Logging.Log($"CupheadArchipelago {FullVersion} by JKLeckr");
                
                /*Logging.Log("[Log] Info", LoggingFlags.Debug);
                Logging.LogWarning("[Log] Warning", LoggingFlags.Debug);
                Logging.LogError("[Log] Error", LoggingFlags.Debug);
                Logging.LogFatal("[Log] Fatal", LoggingFlags.Debug);
                Logging.LogMessage("[Log] Message", LoggingFlags.Debug);
                Logging.LogDebug("[Log] Debug", LoggingFlags.Debug);*/

                if (configVer != CONFIG_VERSION) {
                    Logging.LogWarning($"Config version changed ({configVer} -> {CONFIG_VERSION})! You may want to check the config.");
                    configVer = CONFIG_VERSION;
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
                    Logging.Log(lictext);
                }

                if (!IsPluginLoaded(DEP_SAVECONFIG_MOD_GUID)) {
                    try {
                        Hooks.Main.HookSaveKeyUpdater(configSaveKeyName.Value);
                        Logging.Log($"Using Save Key: {configSaveKeyName.Value}");
                    } catch (Exception e) {
                        Fail(e, -2);
                    }
                } else Logging.Log($"[CupheadArchipelago] Plugin {DEP_SAVECONFIG_MOD_GUID} is loaded, skipping SaveConfig", LoggingFlags.PluginInfo);
                try {
                    SaveData.Init(configSaveKeyName.Value);
                    Hooks.Main.HookMain();
                } catch (Exception e) {
                    Fail(e, -1);
                }
                State = 1;
                Logging.Log($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!", LoggingFlags.PluginInfo);
            }
            else {
                Logging.Log($"Plugin {PluginInfo.PLUGIN_GUID} is loaded, but disabled!", LoggingFlags.PluginInfo); 
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

        // TEMP. This will be changed when entering main branch version.
        private static string GetFVer(string ver) {
            string[] versionParts = ver.Split(['.'], 4);
            if (int.Parse(versionParts[0]) > 0) {
                throw new Exception("Version parsing system needs to be changed for main version!");
            }
            int pre = int.Parse(versionParts[1]);
            string pres = pre switch {
                1 => "preview",
                2 => "alpha",
                3 => "beta",
                4 => "rc",
                _ => "branch"
            };
            int baseline = int.Parse(versionParts[2]) + 1;
            int rev = int.Parse(versionParts[3]);
            FVersion fver = new(baseline, rev, 0, pres, null);
            return fver;
        }

        private static void SetupConfigVersion(Plugin instance) {
            if (File.Exists(verPath)) {
                try {
                    string raw = File.ReadAllText(verPath);
                    instance.configVer = (long)JsonConvert.DeserializeObject(raw);
                    instance.Logger.Log(LogLevel.Info, $"Config version {instance.configVer}");
                    return;
                } catch (Exception) {
                    instance.Logger.Log(LogLevel.Info, "Config version not found.");
                }
            }
            instance.configVer = CONFIG_VERSION;
            SaveConfigVersion(instance);
            instance.Logger.Log(LogLevel.Info, $"Config version {instance.configVer}");
        }
        private static void SaveConfigVersion(Plugin instance) {
            try {
                File.WriteAllText(verPath, JsonConvert.SerializeObject(instance.configVer));
            } catch (Exception) {
                instance.Logger.Log(LogLevel.Info, "Config version could not be written.");
            }
        }

        private static void SetupLogging(Plugin instance) {
            Logging.Init(instance.Logger, instance.configLoggingFlags.Value);
            if (instance.configModLogs.Value) {
                Logging.Log("Setting up mod logging...");
                try {
                    LogFiles.Setup("CupheadAPLog", MOD_NAME, instance.configModFileMax.Value);
                } catch (Exception e) {
                    Logging.LogError($"Mod logging set up failure: {e.Message}");
                    return;
                }
                ModLogListener listener = new(LogFiles.LogFile, LogFiles.LogDirPath, instance.Logger.SourceName);
                BepInEx.Logging.Logger.Listeners.Add(listener);
                Logging.Log("Mod logging started");
            }
        }

        private static void Fail(Exception e, int failCode) {
            Logging.LogError("An exception occured while loading.");
            Logging.LogFatal($"Plugin {PluginInfo.PLUGIN_GUID} failed to load!");
            State = failCode;
            Logging.LogFatal("Throwing Exception...");
            throw e;
        }
    }
}
