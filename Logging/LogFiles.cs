/// Copyright 2024 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

using System;
using System.IO;
using System.Linq;
using BepInEx;

namespace CupheadArchipelago {
    public class LogFiles {
        public static string LogDirPath { get; private set; }
        public static string LogName { get; private set; }
        public static string LogFile { get; private set; }
        public static int LogFileMax { get; private set; }

        private const string LOG_FILE_EXTENSION = ".log";

        public static void Setup(string logName, string logDirName, int fileMax) {
            LogName = logName;
            LogFileMax = fileMax;
            if (fileMax == 0) {
                Logging.Log("Mod log file max set to 0. Not logging.");
                return;
            }
            string logPath = Path.Combine(Paths.BepInExRootPath, "logs");
            Directory.CreateDirectory(logPath);
            string modLogPath = Path.Combine(logPath, logDirName);
            Directory.CreateDirectory(modLogPath);
            LogDirPath = modLogPath;
            SetupLogName(logName, modLogPath, fileMax);
        }

        private static void SetupLogName(string logName, string logDir, int fileMax) {
            Logging.Log(logDir);
            var logFiles = 
                Directory.GetFiles(logDir, $"{logName}.*{LOG_FILE_EXTENSION}")
                            .Select(Path.GetFileName)
                            .Select(name => new {
                                FileName = name,
                                Number = GetLogFileNumber(name, logName)
                            })
                            .Where(x => x.Number.HasValue)
                            .OrderBy(x => x.Number.Value)
                            .ToList();

            while (fileMax > 0 && logFiles.Count >= fileMax) {
                var oldest = logFiles.First();
                File.Delete(Path.Combine(logDir, oldest.FileName));
                logFiles.RemoveAt(0);
            }

            uint next = logFiles.Any() ? logFiles.Last().Number.Value + 1 : 0;
            LogFile = $"{logName}.{next}{LOG_FILE_EXTENSION}";
        }

        private static uint? GetLogFileNumber(string fileName, string logName) {
            if (fileName.StartsWith(logName, StringComparison.Ordinal) && fileName.EndsWith(LOG_FILE_EXTENSION, StringComparison.Ordinal)) {
                string logNumberRaw = fileName.Substring(logName.Length + 1, fileName.Length - logName.Length - LOG_FILE_EXTENSION.Length - 1);
                if (uint.TryParse(logNumberRaw, out uint res)) {
                    return res;
                }
            }
            return null;
        }
    }
}
