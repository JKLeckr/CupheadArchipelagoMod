/// Copyright 2025-2026 JKLeckr
/// SPDX-License-Identifier: GPL-3.0-or-later

using CupheadArchipelago.AP;
using UnityEngine;

namespace CupheadArchipelago.Unity {
    internal class APMain : MonoBehaviour {
        private static APMain current = null;

        private byte state = 0;
        public bool Initted { get => state == 1; }

        internal static void Create() {
            if (current == null) {
                GameObject obj = new("APMain");
                obj.AddComponent<APMain>();
                obj.SetActive(true);
                Logging.Log("APMain active");
            }
            else {
                Logging.LogWarning("APMain already exists!");
            }
        }

        internal static bool Exists() => current?.Initted == true;

        void Awake() {
            Logging.Log("APMain created");
            if (current == null) {
                current = this;
            }
            if (current == this) {
                DontDestroyOnLoad(gameObject);
                state = 1;
                Logging.Log("APMain Initialized");
            }
            else {
                Logging.LogError("APMain initialized incorrectly!");
                state = 2;
                Destroy(this);
            }
        }

        void OnDestroy() {
            if (current == this) {
                Logging.Log("APMain Destroyed");
                if (state <= 0) {
                    Logging.LogError("APMain Destroyed Prematurely");
                }
                Logging.Log("Closing existing sessions");
                APClient.CloseArchipelagoSession(false);
                current = null;
            }
        }
    }
}
