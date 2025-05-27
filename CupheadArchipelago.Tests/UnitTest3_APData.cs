/// Copyright 2025 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

using System;
using System.IO;
using System.Threading;
using CupheadArchipelago.Tests.Core;
using CupheadArchipelago.AP;
using NUnit.Framework;

namespace CupheadArchipelago.Tests {
    public sealed class UnitTest3_APData {
        [SetUp]
        public void Setup() {}

        private static string GetSlotFileName(int index) =>
            SaveData.AP_SAVE_FILE_KEYS[index]+".sav";
        private static string GetSlotFilePath(int index) =>
            Path.Combine(TestData.TestDir, GetSlotFileName(index));

        public sealed class APData_Empty {
            [SetUp]
            public void Setup() {
                TestData.Clean();
                APData.LoadData(false);
            }

            [Test]
            public void TestFiles_Created() {
                for (int i = 0; i < 3; i++) {
                    string filepath = GetSlotFilePath(i);
                    Assert.That(File.Exists(filepath), Is.True);
                }
            }
        }

        public sealed class APData_Existing {
            [SetUp]
            public void Setup() {
                TestData.Clean();
                APData.Init();
                APData.SData[0].player = "testee";
                APData.SData[2].player = "testee";
                APData.SaveAll(false);
            }

            [Test]
            public void TestFiles_Load_WithDataIntact() {
                File.Delete(GetSlotFilePath(1));
                APData.LoadData(false);
                Assert.Multiple(() => {
                    Assert.That(APData.SData[0].player, Is.EqualTo("testee"));
                    Assert.That(APData.SData[1].player, Is.EqualTo("Player"));
                    Assert.That(APData.SData[2].player, Is.EqualTo("testee"));
                });
            }

            [Test]
            public void TestFile_Loses_Data_OnLoad() {
                APData.SData[0].player = "T2TS";
                APData.LoadData(false);
                Assert.That(APData.SData[0].player, Is.EqualTo("testee"));
            }
        }

        public sealed class APData_Timestamp {
            [SetUp]
            public void Setup() {
                TestData.Clean();
                APData.Init();
                APData.SaveAll(false);
                APData.LoadData(false);
            }

            [Test]
            public void TestTimestamp1_IsZero() {
                Assert.That(APData.SData[0].GetFTime(), Is.Zero);
            }

            [Test]
            public void TestTimestamp2_IsOneSecond() {
                APData.LoadData(false);
                Console.WriteLine("Twiddling thumbs for 1 second");
                Thread.Sleep(1000);
                APData.Save(0, true);
                TimeSpan spanA = new(APData.SData[0].GetFTime());
                long testTimeA = ((long)Math.Floor((decimal)spanA.TotalMilliseconds)) / 100;
                Assert.That(testTimeA, Is.EqualTo(10));
            }

            [Test]
            public void TestTimestamp3_Accumulation() {
                APData.LoadData(false);
                Console.WriteLine("Twiddling thumbs for .5 seconds");
                Thread.Sleep(500);
                APData.Save(0, true);
                TimeSpan spanA = new(APData.SData[0].GetFTime());
                long testTimeA = ((long)Math.Floor((decimal)spanA.TotalMilliseconds)) / 10;
                Console.WriteLine($"Test time A: {testTimeA}");
                Assert.That(testTimeA, Is.InRange(50, 60));
                Console.WriteLine("Twiddling thumbs for another .5 seconds");
                Thread.Sleep(500);
                APData.Save(0, true);
                TimeSpan spanB = new(APData.SData[0].GetFTime());
                long testTimeB = ((long)Math.Floor((decimal)spanB.TotalMilliseconds)) / 10;
                Console.WriteLine($"Test time B: {testTimeB}");
                Assert.That(testTimeB, Is.InRange(100, 120));
                APData.LoadData(false);
                TimeSpan spanC = new(APData.SData[0].GetFTime());
                long testTimeC = ((long)Math.Floor((decimal)spanC.TotalMilliseconds)) / 10;
                Console.WriteLine($"Test time C: {testTimeC}");
                Assert.That(testTimeC, Is.EqualTo(testTimeB));
                Console.WriteLine("Twiddling thumbs for .2 seconds");
                Thread.Sleep(200);
                APData.Save(0, true);
                TimeSpan spanD = new(APData.SData[0].GetFTime());
                long testTimeD = ((long)Math.Floor((decimal)spanD.TotalMilliseconds)) / 10;
                Console.WriteLine($"Test time D: {testTimeD}");
                Assert.That(testTimeD, Is.InRange(120, 140));
            }
        }
    }
}
