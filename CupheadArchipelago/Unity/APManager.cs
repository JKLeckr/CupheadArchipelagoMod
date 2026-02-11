/// Copyright 2025-2026 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

using CupheadArchipelago.AP;
using UnityEngine;

namespace CupheadArchipelago.Unity {
    public class APManager : MonoBehaviour {
        public static APManager Current { get; private set; } = null;

        public enum MngrType {
            Normal,
            Level,
            SpecialLevel,
        }

        [SerializeField]
        private bool debug = false;
        [SerializeField]
        private float levelApplyInterval = 1f;
        [SerializeField]
        private float mapApplyInterval = 0.025f;
        private bool init = false;
        private bool active = false;
        private MngrType type = MngrType.Normal;
        private float timer = 0f;
        [SerializeField]
        private bool deathLink = false;
        [SerializeField]
        private bool death = false;
        [SerializeField]
        private string deathMessage = "";
        private bool deathExecuted = false;
        [SerializeField]
        private float fastFire = 0f;
        [SerializeField]
        private float fingerJam = 0f;
        [SerializeField]
        private float slowFire = 0f;

        public void Init(MngrType type) => Init(type, type == MngrType.Level);
        public void Init(MngrType type, bool deathLink) {
            if (init) return;
            if (Current!=this) {
                if (Current!=null) Destroy(Current);
                Current = this;
            }
            Logging.Log($"[APManager] Initialized as Current {type}");
            this.type = type;
            this.deathLink = deathLink;
            init = true;
        }
        public bool IsActive() => active;
        public void SetActive(bool active) => this.active = active;

        public bool IsDeathTriggered() => death;
        public void TriggerDeath(string message = "Self") {
            if (type != MngrType.Level) return;
            if (IsDeathTriggered()) {
                Logging.LogWarning("[APManager] Death already triggered!");
                return;
            }
            death = true;
            this.deathMessage = message ?? "Unknown";
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
                    if (type == MngrType.Level && deathLink && death && !deathExecuted) {
                        Logging.Log($"[APManager] Killing Players.");
                        PlayerStatsInterface.KillPlayer(PlayerId.Any);
                        Logging.Log($"[APManager] {deathMessage}");
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
                    float applyInterval = type switch {
                        MngrType.Level or MngrType.SpecialLevel => levelApplyInterval,
                        _ => mapApplyInterval,
                    };
                    if (type == MngrType.Level || type == MngrType.SpecialLevel) {
                        if (debug) Logging.Log($"ItemSpecialLevelQueue {APClient.ItemApplySpecialLevelQueueCount()}");
                        if (!APClient.ItemApplySpecialLevelQueueIsEmpty()) {
                            if (debug) Logging.Log($"ItemSpecialLevelQueue has item");
                            long itemId = APClient.PeekItemApplySpecialLevelQueue().id;
                            if (APClient.GetAppliedItemCount(itemId) >= APClient.GetReceivedItemCount(itemId)) {
                                APClient.PopItemApplySpecialLevelQueue(false);
                            }
                            else if (timer >= applyInterval) {
                                if (debug) Logging.Log($"ItemSpecialLevelQueue is applying");
                                APClient.PopItemApplySpecialLevelQueue();
                                AudioManager.Play("level_coin_pickup"); //TEMP
                                timer = 0f;
                            }
                        }
                    }
                    if (type == MngrType.Level) {
                        if (debug) Logging.Log($"ItemLevelQueue {APClient.ItemApplyLevelQueueCount()}");
                        if (!APClient.ItemApplyLevelQueueIsEmpty()) {
                            if (debug) Logging.Log($"ItemLevelQueue has item");
                            long itemId = APClient.PeekItemApplyLevelQueue().id;
                            if (APClient.GetAppliedItemCount(itemId) >= APClient.GetReceivedItemCount(itemId)) {
                                APClient.PopItemApplyLevelQueue(false);
                            }
                            else if (timer >= applyInterval) {
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
                        long itemId = APClient.PeekItemApplyQueue().id;
                        if (APClient.GetAppliedItemCount(itemId) >= APClient.GetReceivedItemCount(itemId)) {
                            APClient.PopItemApplyQueue(false);
                        }
                        else if (timer >= applyInterval) {
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
            if (ReferenceEquals(Current, this)) Current = null;
        }
    }
}
