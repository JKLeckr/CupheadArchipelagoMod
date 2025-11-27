/// Copyright 2025-2026 JKLeckr
/// SPDX-License-Identifier: GPL-3.0-or-later

using CupheadArchipelago.Unity;
using UnityEngine;

namespace CupheadArchipelago.AP {
    internal class APStatus {
        private GameObject _instance = null;

        internal APStatus() {
            GameObject obj = new("APStatus");
            Object.DontDestroyOnLoad(obj);
            obj.transform.SetPosition(0,25,0);
            obj.AddComponent<APStatusMain>();
            obj.SetActive(true);
        }

        private void Destroy() {
            Object.Destroy(_instance);
            _instance = null;
        }

        ~APStatus() {
            if (_instance!=null) Destroy();
        }
    }
}
