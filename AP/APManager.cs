/// Copyright 2024 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

using UnityEngine;

namespace CupheadArchipelago.AP {
    public class APManager : MonoBehaviour {
        public static APManager Current {get; private set;}

        void Awake() {
            if (Current != null) {
                Destroy(gameObject);
                return;
            }
        }

        void Update() {
            if (!PlayerData.inGame) {
                Plugin.Log("[APManager] Outside game. Dying off.");
            }
        }

        void OnDestroy() {
            if (Current == this) Current = null;
            Plugin.Log("[APManager] Destroy");
        }
    }
}