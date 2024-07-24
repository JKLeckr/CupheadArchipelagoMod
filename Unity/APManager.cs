/// Copyright 2024 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

using CupheadArchipelago.AP;
using System.Collections;
using UnityEngine;

namespace CupheadArchipelago.Unity {
    public class APManager : MonoBehaviour {
        public enum Type {
            Normal,
            Level
        }

        private bool init = false;
        private Type type = Type.Normal;
        
        public void Init(Type type = Type.Normal) {
            init = true;
        }

        void Update() {
            if (init) {
                if (type == Type.Level) {}
                else {}
            }
        }

        private IEnumerator ApplyItem_cr() {
            yield break;
        }
    }
}