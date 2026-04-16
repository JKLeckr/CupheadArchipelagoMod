/// Copyright 2025-2026 JKLeckr
/// SPDX-License-Identifier: GPL-3.0-or-later

using CupheadArchipelago.AP;
using UnityEngine;

namespace CupheadArchipelago.Unity {
    internal class APMain : MonoBehaviour {
        private static GameObject current = null;

        internal static void Create() {
            if (current == null) {
                current = new GameObject("APMain", typeof(APMain));
            }
        }

        void Awake() {
            DontDestroyOnLoad(gameObject);
        }

        void OnDestroy() {
            APClient.CloseArchipelagoSession(false);
        }
    }
}
