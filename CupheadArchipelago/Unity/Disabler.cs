/// Copyright 2025 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

using System.Collections;
using CupheadArchipelago.AP;
using UnityEngine;

namespace CupheadArchipelago.Unity {
    public class Disabler : MonoBehaviour {
        bool initted = false;
        MonoBehaviour toDisable;
        
        public void Init(MonoBehaviour toDisable) {
            this.toDisable = toDisable;
            initted = true;
        }

        void Start() {
            StartCoroutine(Disable_cr());
        }

        IEnumerator Disable_cr() {
            while (!initted) yield return null;
            yield return null;
            if (toDisable != null) toDisable.enabled = false;
            Destroy(this);
            yield break;
        }
    }
}
