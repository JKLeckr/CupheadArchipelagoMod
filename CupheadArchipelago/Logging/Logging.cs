/// Copyright 2025 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

using System;
using BepInEx.Logging;

namespace CupheadArchipelago {
    public class Logging {
        private static bool init = false;
        private static ManualLogSource logSource;
        private static Action<LogLevel, object> logAction;
        private static LoggingFlags loggingFlags;
        private static LoggingFlags permLoggingFlags;
        
        internal static void Init(ManualLogSource logSource, LoggingFlags loggingFlags) {
            if (init) Console.WriteLine("Reinitializing Logging...");
            Logging.logSource = logSource ?? throw new ArgumentNullException("logSource cannot be null.");
            logAction = logSource.Log;
            Logging.loggingFlags = loggingFlags;
            permLoggingFlags = loggingFlags;
            init = true;
        }
        internal static void Init(Action<LogLevel, object> logAction, LoggingFlags loggingFlags) {
            if (init) Console.WriteLine("Reinitializing Logging...");
            logSource = null;
            Logging.logAction = logAction;
            Logging.loggingFlags = loggingFlags;
            permLoggingFlags = loggingFlags;
            init = true;
        }

        internal static bool IsLoggingInitialized() => init;

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
            if (!init) {
                throw new Exception("Logging not initialized.");
            }
            if (IsLoggingFlagsEnabled(requiredFlags)) {
                logAction(logLevel, data);
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
        public static void LogError(object data, LoggingFlags requiredFlags) {
            Log(data, requiredFlags, LogLevel.Error);
        }
        public static void LogFatal(object data) => LogFatal(data, LoggingFlags.None);
        public static void LogFatal(object data, LoggingFlags requiredFlags) {
            Log(data, requiredFlags, LogLevel.Fatal);
        }
        public static void LogDebug(object data) => LogDebug(data, LoggingFlags.Debug);
        public static void LogDebug(object data, LoggingFlags requiredFlags) {
            Log(data, requiredFlags, Config.IsDebugLogsInfo() ? LogLevel.Info : LogLevel.Debug);
        }

        public static bool IsLoggingFlagsEnabled(LoggingFlags flags) {
            return (((int)flags)&((int)loggingFlags))==(int)flags;
        }
        public static bool IsDebugEnabled() => IsLoggingFlagsEnabled(LoggingFlags.Debug);
        internal static void SetLoggingFlags(LoggingFlags flags) => loggingFlags = flags;
        internal static void AddLoggingFlags(LoggingFlags flags) => loggingFlags |= flags;
        internal static void RemoveLoggingFlags(LoggingFlags flags) => loggingFlags &= ~flags;
        internal static void ResetLoggingFlags() => loggingFlags = permLoggingFlags;

        public static string GetLogSourceName() => logSource?.SourceName;
    }
}
