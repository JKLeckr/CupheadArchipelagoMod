/// Copyright 2024 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

using CupheadArchipelago.AP;
using CupheadArchipelago.Hooks.PlayerHooks;
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
        private float levelApplyInterval = 1f;
        [SerializeField]
        private float mapApplyInterval = 0.25f;
        private bool init = false;
        private bool active = false;
        private Type type = Type.Normal;
        private float timer = 0f;
        [SerializeField]
        private bool death = false;
        [SerializeField]
        private string deathCause = "";
        private bool deathExecuted = false;
        [SerializeField]
        private float fastFire = 0f;
        [SerializeField]
        private float fingerJam = 0f;
        [SerializeField]
        private float slowFire = 0f;
        
        public void Init(Type type = Type.Normal) {
            if (init) return;
            if (Current!=this) {
                if (Current!=null) Destroy(Current);
                Current = this;
            }
            Logging.Log($"[APManager] Initialized as Current {type}");
            this.type = type;
            init = true;
        }
        public bool IsActive() => active;
        public void SetActive(bool active) => this.active = active;
        
        public bool IsDeathTriggered() => death;
        public void TriggerDeath(string deathCause = null) {
            if (type != Type.Level) return;
            if (IsDeathTriggered()) {
                Logging.LogWarning("[APManager] Death already triggered!");
                return;
            }
            death = true;
            this.deathCause = deathCause ?? "";
        }

        public bool IsFastFired() => fastFire > 0;
        public void FastFire(float addTime = 5) {
            if (fastFire<0) fastFire = 0;
            fastFire += addTime;
        }
        public bool IsFingerJammed() => fingerJam > 0;
        public void FingerJam(float addTime = 5) {
            if (fingerJam<0) fingerJam = 0;
            fingerJam += addTime;
        }
        public bool IsSlowFired() => slowFire > 0;
        public void SlowFire(float addTime = 8) {
            if (slowFire<0) slowFire = 0;
            slowFire += addTime;
        }

        void Update() {
            if (init) {
                if (Current!=this) {
                    init = false;
                    Destroy(this);
                }
                else if (active && PauseManager.state != PauseManager.State.Paused) {
                    if (debug) Logging.Log($"ReceiveQueue {APClient.ItemReceiveQueueCount()}");
                    if (type == Type.Level && death && !deathExecuted) {
                        Logging.Log($"[APManager] Killing Players. Cause: \"{deathCause}\"");
                        PlayerStatsManagerHook.KillPlayer(PlayerId.Any);
                        deathExecuted = true;
                        active = false;
                        return;
                    }
                    if (fastFire>0) {
                        fastFire -= Time.deltaTime;
                        if (fastFire<0) fastFire = 0;
                    }
                    if (fingerJam>0) {
                        fingerJam -= Time.deltaTime;
                        if (fingerJam<0) fingerJam = 0;
                    }
                    if (slowFire>0) {
                        slowFire -= Time.deltaTime;
                        if (slowFire<0) slowFire = 0;
                    }
                    APClient.ItemUpdate();
                    float applyInterval = type == Type.Level ? levelApplyInterval : mapApplyInterval;
                    if (type == Type.Level) {
                        if (debug) Logging.Log($"ItemLevelQueue {APClient.ItemApplyLevelQueueCount()}");
                        if (!APClient.ItemApplyLevelQueueIsEmpty()) {
                            if (debug) Logging.Log($"ItemLevelQueue has item");
                            if (timer>=applyInterval) {
                                if (debug) Logging.Log($"ItemLevelQueue is applying");
                                APClient.PopItemApplyLevelQueue();
                                AudioManager.Play("level_coin_pickup"); //TEMP
                                timer = 0f;
                            }
                        }
                    }
                    if (debug) Logging.Log($"ItemQueue {APClient.ItemApplyQueueCount()}");
                    if (!APClient.ItemApplyQueueIsEmpty()) {
                        if (debug) Logging.Log($"ItemQueue has item");
                        if (timer>=applyInterval) {
                            if (debug) Logging.Log($"ItemQueue is applying");
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
            Logging.Log("[APManager] Destroyed");
            init = false;
            Current = null;
        }
    }
}