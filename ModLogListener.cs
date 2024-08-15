/// Copyright 2024 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

using System;
using System.IO;
using System.Threading;
using BepInEx;
using BepInEx.Logging;

namespace CupheadArchipelago {
    public class ModLogListener : ILogListener, IDisposable {
        public string LogSourceName { get; protected set;}

        public LogLevel DisplayedLogLevel { get; set; }

        public TextWriter LogWriter { get; protected set; }

        public Timer FlushTimer { get; protected set; }

        public bool WriteFromUnityLog { get; set; }

        public ModLogListener(string logName, string logSourceName, LogLevel displayedLogLevel = LogLevel.All, bool includeUnityLog = true) {
            LogSourceName = logSourceName;
            WriteFromUnityLog = includeUnityLog;
            DisplayedLogLevel = displayedLogLevel;

            string logPath = Path.Combine(Paths.BepInExRootPath, "logs");
            Directory.CreateDirectory(logPath);
            string modLogPath = Path.Combine(logPath, Plugin.Name);
            Directory.CreateDirectory(modLogPath);
            
            int num = 0;
            string filename;
            do {
                filename = $"{logName}.{num++}.log";
            } while (File.Exists(Path.Combine(modLogPath, filename)));
            Plugin.Log($"Logging to {filename}");
            
            int startnum = num;
            
            FileStream fileStream;
            while (!Utility.TryOpenFileStream(Path.Combine(modLogPath, filename), FileMode.Create, out fileStream, FileAccess.Write)) {
                if (num >= startnum+5) {
                    Plugin.LogError("5 attempts of opening files for mod logging failed. Not logging.");
                    return;
                }

                Plugin.LogWarning($"Could not open \"{filename}\" for writing.");
                filename = $"{logName}.{num++}.log";
                Plugin.Log($"Trying {filename}...");
            }

            LogWriter = TextWriter.Synchronized(new StreamWriter(fileStream, Utility.UTF8NoBom));
            FlushTimer = new Timer(delegate {
                LogWriter?.Flush();
            }, null, 2000, 2000);
        }

        public void LogEvent(object sender, LogEventArgs eventArgs) {
            if (((WriteFromUnityLog && eventArgs.Source is UnityLogSource) || eventArgs.Source.SourceName == LogSourceName) && (eventArgs.Level & DisplayedLogLevel) != 0) {
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
