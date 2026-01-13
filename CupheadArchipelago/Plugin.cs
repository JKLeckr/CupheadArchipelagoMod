/// Copyright 2025-2026 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

using System;
using System.IO;
using CupheadArchipelago.Config;
using CupheadArchipelago.Helpers.FVerParser;
using FVer;
using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Configuration;
using BepInEx.Logging;
using Newtonsoft.Json;

namespace CupheadArchipelago {
    [BepInPlugin(MOD_GUID, MOD_NAME, MOD_BASE_VERSION)]
    [BepInDependency(DEP_SAVECONFIG_MOD_GUID, BepInDependency.DependencyFlags.SoftDependency)]
    [BepInProcess("Cuphead.exe")]
    public class Plugin : BaseUnityPlugin {
        internal const string DEP_SAVECONFIG_MOD_GUID = "com.JKLeckr.CupheadSaveConfig";

        protected const string MOD_NAME = "CupheadArchipelago"; //PluginInfo.PLUGIN_NAME
        protected const string MOD_GUID = "com.JKLeckr.CupheadArchipelago";
        protected const string MOD_BASE_VERSION = PluginInfo.PLUGIN_VERSION;
        protected const string MOD_VERSION_POSTFIX = "";
        protected const string MOD_VERSION = $"{MOD_BASE_VERSION}{MOD_VERSION_POSTFIX}";
        protected static readonly string MOD_FRIENDLY_VERSION = GetFVer(MOD_BASE_VERSION);

        private const long CONFIG_VERSION = 1;

        public static string Name => MOD_NAME;
        public static string Version => MOD_VERSION;
        public static string FullVersion => $"{MOD_FRIENDLY_VERSION} ({MOD_VERSION})";
        public static int State { get; private set; } = 0;

        internal static Plugin Current { get; private set; } = null;

        private static readonly string verPath = Path.Combine(Path.Combine(Paths.PluginPath, MOD_NAME), "configver");
        private long configVer;
        private ConfigEntry<bool> configEnabled;

        private MConf config;

        private void Awake() {
            if (Current != null) throw new Exception("Plugin is already loaded!");
            Current = this;
            SetupConfigVersion();
            configEnabled = Config.Bind("Main", "Enabled", true, "Mod Master Switch");
            if (configEnabled.Value) {
                config = new(Config);
                SetupLogging();
                Logging.Log("----------------------------------------");
                Logging.Log($"CupheadArchipelago {FullVersion}");
                Logging.Log("Created by JKLeckr");
                Logging.Log("----------------------------------------");

                Logging.Log($"Game Build Version {UnityEngine.Application.version}");
                Logging.Log($"DLC is {(DLCManager.DLCEnabled() ? "Enabled" : "Disabled")}");

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

                if (config.LogLicense > 0) {
                    ModLicense license = new();
                    string lictext = $"License:\n--- LICENSE ---\n{license.PLUGIN_NOTICE}\n";
                    if (config.LogLicense == LicenseLogModes.All) {
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
                        Hooks.Main.HookSaveKeyUpdater(config.SaveKeyName);
                        Logging.Log($"Using Save Key: {config.SaveKeyName}");
                    }
                    catch (Exception e) {
                        Fail(e, -2);
                    }
                }
                else Logging.Log($"[CupheadArchipelago] Plugin {DEP_SAVECONFIG_MOD_GUID} is loaded, skipping SaveConfig", LoggingFlags.PluginInfo);
                try {
                    SaveData.Init(config.SaveKeyName);
                    Hooks.Main.HookMain();
                    Resources.ResourceLoader.LoadResources();
                }
                catch (Exception e) {
                    Fail(e, -1);
                }
                State = 1;
                Logging.Log($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!", LoggingFlags.PluginInfo);
            }
            else {
                Logging.Log($"Plugin {PluginInfo.PLUGIN_GUID} is loaded, but disabled!", LoggingFlags.PluginInfo);
            }
        }

        internal MConf GetConfig() => config;

        private bool IsPluginLoaded(string plugin) => FindPlugin(plugin) >= 0;
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
            RawFVer rawFVer = FVerParse.GetRawFVer(ver);
            FVersion fver = new(rawFVer.baseline, rawFVer.revision, rawFVer.release, rawFVer.prefix, rawFVer.postfix);
            return fver;
        }

        private void SetupConfigVersion() {
            if (File.Exists(verPath)) {
                try {
                    string raw = File.ReadAllText(verPath);
                    configVer = (long)JsonConvert.DeserializeObject(raw);
                    Logger.Log(LogLevel.Info, $"Config version {configVer}");
                    return;
                } catch (Exception) {
                    Logger.Log(LogLevel.Info, "Config version not found.");
                }
            }
            configVer = CONFIG_VERSION;
            SaveConfigVersion();
            Logger.Log(LogLevel.Info, $"Config version {configVer}");
        }
        private void SaveConfigVersion() {
            try {
                File.WriteAllText(verPath, JsonConvert.SerializeObject(configVer));
            } catch (Exception) {
                Logger.Log(LogLevel.Info, "Config version could not be written.");
            }
        }

        private void SetupLogging() {
            Logging.Init(Logger, config.LoggingFlags);
            if (config.ModLogs) {
                Logging.Log("Setting up mod logging...");
                try {
                    LogFiles.Setup("CupheadAPLog", MOD_NAME, config.ModFileMax);
                } catch (Exception e) {
                    Logging.LogError($"Mod logging set up failure: {e.Message}");
                    return;
                }
                ModLogListener listener = new(LogFiles.LogFile, LogFiles.LogDirPath, Logger.SourceName);
                BepInEx.Logging.Logger.Listeners.Add(listener);
                Logging.Log("Mod logging started");
            }
        }

        private void Fail(Exception e, int failCode) {
            Logging.LogError("An exception occured while loading.");
            Logging.LogFatal($"Plugin {PluginInfo.PLUGIN_GUID} failed to load! (Code: {failCode})");
            State = failCode;
            Logging.LogFatal("Throwing Exception...");
            throw e;
        }
    }
}
