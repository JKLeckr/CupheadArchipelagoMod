/// Copyright 2025 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

using System.Collections.Generic;
using CupheadArchipelago.AP;
using NUnit.Framework;

namespace CupheadArchipelago.Tests {
    public sealed class UnitTest3_Items {
        private static readonly Dictionary<string, APItem> apItems;

        static UnitTest3_Items() {
            var apItemFields = TReflection.GetFieldsFromClass(typeof(APItem), typeof(APItem));
            apItems = [];
            foreach (var field in apItemFields) {
                apItems[field.Name] = (APItem)field.GetValue(null);
            }
        }

        [SetUp]
        public void Setup() {}

        public sealed class APItemMap {
            private static readonly HashSet<APItem> skip = [
                APItem.level_generic,
            ];

            [SetUp]
            public void Setup() {}

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

            public sealed class APItemMap_Weapons {
                private static readonly HashSet<APItem> weapons;

                static APItemMap_Weapons() {
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

            public sealed class APItemMap_Charms {
                private static readonly HashSet<APItem> charms;

                static APItemMap_Charms() {
                    charms = [];
                    foreach (APItem item in apItems.Values) {
                        if (ItemMap.GetItemType(item) == APItemType.Charm) {
                            charms.Add(item);
                        }
                    }
                }

                [SetUp]
                public void Setup() {}

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
    }
}
