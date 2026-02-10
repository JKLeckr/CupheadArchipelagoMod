/// Copyright 2025-2026 JKLeckr
/// SPDX-License-Identifier: GPL-3.0-or-later

using System.Collections.Generic;
using CupheadArchipelago.Tests.Core;
using CupheadArchipelago.AP;
using CupheadArchipelago.Mapping;
using CupheadArchipelago.Tests.TestClasses;
using NUnit.Framework;

namespace CupheadArchipelago.Tests {
    public sealed class UnitTest4_Items {
        private static readonly Dictionary<string, APItem> apItems;

        static UnitTest4_Items() {
            var apItemFields = TReflection.GetFieldsFromClass(typeof(APItem), typeof(APItem));
            apItems = [];
            foreach (var field in apItemFields) {
                apItems[field.Name] = (APItem)field.GetValue(null);
            }
        }

        [SetUp]
        public void Setup() { }

        public sealed class Items_ItemMap {
            private static readonly HashSet<APItem> skip = [
                APItem.level_generic,
            ];

            [SetUp]
            public void Setup() { }

            [Test]
            public void ItemMap_All_Items_Have_Type() {
                bool fail = false;

                foreach (string itemname in apItems.Keys) {
                    APItem item = apItems[itemname];
                    if (!skip.Contains(item) && ItemMap.GetItemType(item) == APItemType.None) {
                        fail = true;
                        Logging.Log($"Item {itemname} failed.");
                    }
                }

                Assert.That(fail, Is.False);
            }

            public sealed class Items_ItemMap_Weapons {
                private static readonly HashSet<APItem> weapons;

                static Items_ItemMap_Weapons() {
                    weapons = [];
                    foreach (APItem item in apItems.Values) {
                        if (ItemMap.GetItemType(item) == APItemType.Weapon) {
                            weapons.Add(item);
                        }
                    }
                }

                [Test]
                public void ItemMap_Mapped_Weapons_Are_Defined() {
                    bool fail = false;

                    foreach (APItem weapon in weapons) {
                        try {
                            ItemMap.GetWeapon(weapon);
                        }
                        catch (KeyNotFoundException) {
                            fail = true;
                            Logging.Log($"Item {weapon} failed.");
                        }
                    }

                    Assert.That(fail, Is.False);
                }
            }

            public sealed class Items_ItemMap_Charms {
                private static readonly HashSet<APItem> charms;

                static Items_ItemMap_Charms() {
                    charms = [];
                    foreach (APItem item in apItems.Values) {
                        if (ItemMap.GetItemType(item) == APItemType.Charm) {
                            charms.Add(item);
                        }
                    }
                }

                [SetUp]
                public void Setup() { }

                [Test]
                public void ItemMap_Mapped_Charms_Are_Defined() {
                    bool fail = false;

                    foreach (APItem charm in charms) {
                        try {
                            ItemMap.GetCharm(charm);
                        }
                        catch (KeyNotFoundException) {
                            fail = true;
                            Logging.Log($"Item {charm} failed.");
                        }
                    }

                    Assert.That(fail, Is.False);
                }
            }
        }

        public sealed class Items_ItemMngr {
            private static readonly HashSet<long> skipItems = [];

            private static readonly TPlayerDataItfc playerDataMngr = new();

            [SetUp]
            public void Setup() {
                TestData.Clean();
                APData.LoadData(false);
                TPlayerData.Init();
                APSettings.Init();
                TUtil.InitAPClient();
            }

            // TODO: finish this test
            [Test]
            public void APItemMngr_WeaponsApplyCorrectly() {
                bool fail = false;
                foreach (string itemname in apItems.Keys) {
                    APItem item = apItems[itemname];
                    if (ItemMap.GetItemType(item) == APItemType.Weapon) {
                        Assert.That(ApplyItem(item), Is.True);
                    }
                    else {
                        Logging.Log($"Skipping {itemname}.");
                    }
                }

                Assert.That(fail, Is.False);
            }

            private static bool ApplyItem(APItem item) =>
                APItemMngr.ApplyItem(item, playerDataMngr);
        }
    }
}
