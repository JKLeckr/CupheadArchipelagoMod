/// Copyright 2024 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

using CupheadArchipelago.AP;
using UnityEngine;

namespace CupheadArchipelago.Unity {
    public class APManager : MonoBehaviour {
        public static APManager Current { get; private set; } = null;

        public enum Type {
            Normal,
            Level
        }

        [SerializeField]
        private bool debug = false;
        [SerializeField]
        private float applyInterval = 1f;
        private bool init = false;
        private bool active = false;
        private Type type = Type.Normal;
        private float timer = 0f;
        
        public void Init(Type type = Type.Normal) {
            if (init) return;
            if (Current!=this) {
                if (Current!=null) Destroy(Current);
                Current = this;
            }
            Plugin.Log($"[APManager] Initialized as Current {type}");
            this.type = type;
            active = true;
            init = true;
        }
        public bool IsActive() => active;
        public void SetActive(bool active) => this.active = active;

        //void Awake() {}

        void Update() {
            if (init) {
                if (Current!=this) {
                    init = false;
                    Destroy(this);
                }
                else if (active) {
                    if (debug) Plugin.Log($"ReceiveQueue {APClient.ItemReceiveQueueCount()}");
                    APClient.ItemUpdate();
                    if (type == Type.Level) {
                        if (debug) Plugin.Log($"ItemLevelQueue {APClient.ItemApplyLevelQueueCount()}");
                        if (!APClient.ItemApplyLevelQueueIsEmpty()) {
                            if (debug) Plugin.Log($"ItemLevelQueue has item");
                            if (timer>=applyInterval) {
                                if (debug) Plugin.Log($"ItemLevelQueue is applying");
                                APClient.PopItemApplyLevelQueue();
                                AudioManager.Play("level_coin_pickup"); //TEMP
                                timer = 0f;
                            }
                        }
                    }
                    if (debug) Plugin.Log($"ItemQueue {APClient.ItemApplyQueueCount()}");
                    if (!APClient.ItemApplyQueueIsEmpty()) {
                        if (debug) Plugin.Log($"ItemQueue has item");
                        if (timer>=applyInterval) {
                            if (debug) Plugin.Log($"ItemQueue is applying");
                            APClient.PopItemApplyQueue();
                            AudioManager.Play("level_coin_pickup"); //TEMP
                            timer = 0f;
                        }
                    }
                    if (timer<applyInterval) timer += Time.deltaTime;
                }
            }
        }

        void OnDestroy() {
            Plugin.Log("[APManager] Destroyed");
            init = false;
            Current = null;
        }
    }
}