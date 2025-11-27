/// Copyright 2025-2026 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

using System;
using CupheadArchipelago.Config;
using UnityEngine;

namespace CupheadArchipelago.Unity {
    internal class APStatusMain : MonoBehaviour {
        bool initted = false;

        void Awake() {
            if (initted) return;
            if (MConf.IsAPStatsFunctionEnabled(APStatsFunctions.ConnectionIndicator)) {
                CreateConnectionIndicator();
            }
            initted = true;
        }

        private void CreateConnectionIndicator() {}
    }
}
