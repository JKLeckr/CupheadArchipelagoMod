/// Copyright 2024 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

using CupheadArchipelago.AP;
using UnityEngine;

namespace CupheadArchipelago.Unity {
    public class ControlBoard : MonoBehaviour {
        public static ControlBoard Current { get; private set; } = null;
        
        public bool invincible = false;

        void Awake() {
            if (Current == null) {
                Current = this;
            }
            else {
                Logging.Log("Destroying old ControlBoard");
                ControlBoard old = Current;
                Current = this;
                Destroy(old.gameObject);
            }
            DontDestroyOnLoad(Current.gameObject);
            Logging.Log("ControlBoard initialized");
        }

        void Update() {
            if (invincible != PlayerStatsManager.DebugInvincible)
                PlayerStatsManager.DebugToggleInvincible();
        }

        void OnEnable() {
		    Debug.Log("ControlBoard active");
	    }
	    void OnDisable() {
		    Debug.Log("ControlBoard inactive");
	    }
    }
}
