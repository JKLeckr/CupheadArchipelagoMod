/// Copyright 2025-2026 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

using System.Collections.Generic;
using CupheadArchipelago.Resources;
using NUnit.Framework;

namespace CupheadArchipelago.Tests {
    public sealed class UnitTest2_Resources {
        [SetUp]
        public void Setup() { }

        public sealed class Resources_AssetReg {
            [SetUp]
            public void Setup() { }

            [Test]
            public void TestReg_AssetBundles_Have_Resource() {
                IEnumerable<string> bundles = AssetDefs.GetAllRegisteredBundles();
                HashSet<string> resources = [.. ResourceDefs.GetRegisteredResources()];
                bool fail = false;

                foreach (string bundle in bundles) {
                    if (!resources.Contains(bundle)) {
                        fail = true;
                        Logging.Log($"Asset Bundle {bundle} has no registered resource.");
                    }
                }

                Assert.That(fail, Is.False);
            }

            [Test]
            public void TestReg_Assets_Are_Defined() {
                HashSet<string> bundleAssets = [];
                HashSet<string> assets = [.. AssetDefs.GetAllRegisteredAssets()];
                bool fail = false;

                foreach (string bundle in AssetDefs.GetAllRegisteredBundles()) {
                    bundleAssets.UnionWith(AssetDefs.GetAssetsInBundle(bundle));
                }

                foreach (string asset in bundleAssets) {
                    if (!assets.Contains(asset)) {
                        fail = true;
                        Logging.Log($"Asset {asset} is not defined.");
                    }
                }

                Assert.That(fail, Is.False);
            }

            [Test]
            public void TestReg_No_Dups() {
                IEnumerable<string> bundles = AssetDefs.GetAllRegisteredBundles();
                HashSet<string> registered = [];
                bool fail = false;

                foreach (string bundle in bundles) {
                    foreach (string asset in AssetDefs.GetAssetsInBundle(bundle)) {
                        if (registered.Contains(asset)) {
                            fail = true;
                            Logging.Log($"Asset {asset} has a duplicate.");
                        }
                    }
                }

                Assert.That(fail, Is.False);
            }

            [Test]
            public void TestReg_Assets_Have_Type() {
                IEnumerable<string> assets = AssetDefs.GetAllRegisteredAssets();
                bool fail = false;

                foreach (string asset in assets) {
                    if (AssetDefs.GetAssetType(asset) == RAssetType.Object) {
                        fail = true;
                        Logging.Log($"Asset {asset} failed.");
                    }
                }

                Assert.That(fail, Is.False);
            }

            [Test]
            public void TestReg_PersistentAssets_Are_Valid() {
                bool fail = false;

                foreach (string pAsset in AssetDefs.GetPersisentAssets()) {
                    if (AssetDefs.GetBundleFromAsset(pAsset) == null) {
                        fail = true;
                        Logging.Log($"Persistent Asset {pAsset} failed.");
                    }
                }

                Assert.That(fail, Is.False);
            }

            [Test]
            public void TestReg_Scene_Assets_Are_Registered() {
                HashSet<string> registeredAssets = [.. AssetDefs.GetAllRegisteredAssets()];
                bool fail = false;

                foreach (string scene in SceneAssetMap.GetRegisteredScenes()) {
                    foreach (string asset in SceneAssetMap.GetSceneAssets(scene)) {
                        if (!registeredAssets.Contains(asset)) {
                            fail = true;
                            Logging.Log($"Asset {asset} failed.");
                        }
                    }
                }

                Assert.That(fail, Is.False);
            }
        }
    }
}
