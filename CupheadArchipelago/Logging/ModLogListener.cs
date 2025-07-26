/// Copyright 2025 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

using System;
using System.IO;
using System.Threading;
using BepInEx;
using BepInEx.Logging;
using BepInEx.Unity.Logging;

namespace CupheadArchipelago {
    public class ModLogListener : ILogListener, IDisposable {
        public string LogSourceName { get; protected set;}

        public LogLevel LogLevelFilter { get; private set; }

        public TextWriter LogWriter { get; protected set; }

        public Timer FlushTimer { get; protected set; }

        public bool WriteFromUnityLog { get; set; }

        public ModLogListener(string logFile, string logPath, string logSourceName, LogLevel displayedLogLevel = LogLevel.All, bool includeUnityLog = true) {
            LogSourceName = logSourceName;
            WriteFromUnityLog = includeUnityLog;
            LogLevelFilter = displayedLogLevel;

            if (!Utility.TryOpenFileStream(Path.Combine(logPath, logFile), FileMode.Create, out FileStream fileStream, FileAccess.Write)) {
                Logging.LogError($"Could not open \"{logFile}\" for writing. Not logging.");
                return;
            }

            Logging.Log($"Logging to {logFile}");

            LogWriter = TextWriter.Synchronized(new StreamWriter(fileStream, Utility.UTF8NoBom));
            FlushTimer = new Timer(delegate {
                LogWriter?.Flush();
            }, null, 2000, 2000);
        }

        public void LogEvent(object sender, LogEventArgs eventArgs) {
            if (((WriteFromUnityLog && eventArgs.Source is UnityLogSource) || eventArgs.Source.SourceName == LogSourceName) && (eventArgs.Level & LogLevelFilter) != 0) {
                LogWriter.WriteLine(eventArgs.ToString());
            }
        }

        public void Dispose() {
            FlushTimer?.Dispose();
            LogWriter?.Flush();
            LogWriter?.Dispose();
        }

        ~ModLogListener() {
            Dispose();
        }
    }
}
