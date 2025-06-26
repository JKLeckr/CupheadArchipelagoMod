/// Copyright 2025 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

using UnityEngine;

namespace CupheadArchipelago.Resources {
    internal class StaticAssets {
        internal static bool Initted { get; private set; } = false;

        internal static Shader standardSprite;

        internal static void Init() {
            standardSprite = Shader.Find("Sprites/Default");
            Initted = true;
        }
    }
}
