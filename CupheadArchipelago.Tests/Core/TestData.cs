/// Copyright 2025-2026 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

using System.IO;
using System.Reflection;
using CupheadArchipelago.Config;

namespace CupheadArchipelago.Tests.Core {
    internal class TestData {
        private static readonly string EXE_DIR = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
        internal static string TestDir = Path.Combine(Path.Combine(EXE_DIR, ".."), "TestData");

        internal static void Setup() {
            Directory.CreateDirectory(TestDir);
            Clean();
            SaveData.Init("save_test_data_slot_", TestDir);
        }

        internal static void Clean() {
            DirectoryInfo di = new(TestDir);
            foreach(DirectoryInfo sdi in di.GetDirectories()) {
                sdi.Delete(true);
            }
            foreach(FileInfo fi in di.GetFiles()) {
                fi.Delete();
            }
        }
    }
}
