/// Copyright 2025 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using BepInEx;

namespace CupheadArchipelago {
    internal class AssetMngr {
        private static readonly Dictionary<string, string> assetBundles = new() {
            {"TitleCards_W1", "atlas_titlecards_w1"},
            {"TitleCards_W2", "atlas_titlecards_w2"},
            {"TitleCards_W3", "atlas_titlecards_w3"},
            {"TitleCards_WDLC", "atlas_titlecards_wdlc"},
        };
        private static readonly Dictionary<string, HashSet<Levels>> assetScenes = new() { };
        private static readonly HashSet<string> universalAssets = [];

        internal static void Init(BaseUnityPlugin pinstance) { }
        internal static void Unload() { }

        internal static int TestDefs() {
            return 0;
        }
    }
}
