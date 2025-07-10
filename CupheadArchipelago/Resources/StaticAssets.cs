/// Copyright 2025 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

using UnityEngine;

namespace CupheadArchipelago.Resources {
    internal class StaticAssets {
        internal static bool Initted { get; private set; } = false;

        internal static Shader StandardSpriteShader { get; private set; }
        internal static Shader DebugShader { get; private set; }
        internal static Shader OverlayShader { get; private set; }

        internal static void Init() {
            StandardSpriteShader = Shader.Find("Sprites/Default");
            DebugShader = AssetMngr.GetLoadedAsset<Shader>("s_Debug");
            OverlayShader = AssetMngr.GetLoadedAsset<Shader>("s_TexOverlay");
            Initted = true;
        }
    }
}
