/// Copyright 2025-2026 JKLeckr
/// SPDX-License-Identifier: GPL-3.0-or-later

using System;
using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Models;
using Archipelago.MultiClient.Net.Packets;
using CupheadArchipelago.Mapping;
using CupheadArchipelago.Mapping.Bits;

namespace CupheadArchipelago.AP {
    internal class APDataStorage {
        internal static void WriteCurrentLevel(Levels lv) {
            try {
                int lv_modifier = 10000000;
                OperationSpecification dop = new() {
                    OperationType = OperationType.Default,
                    Value = -1
                };
                OperationSpecification rop = new() {
                    OperationType = OperationType.Replace,
                    Value =
                        LevelMap.LevelExists(lv) ?
                        LevelMap.GetLevelId(lv) :
                        ((int)lv < lv_modifier ? ((int)lv + lv_modifier) : (int)lv)
                };
                SetPacket pk = new() {
                    Key = $"Slot:{APClient.APSessionPlayerSlot}:current_level",
                    DefaultValue = -1,
                    WantReply = false,
                    Operations = [dop, rop]
                };
                APClient.SendPacketAsync(pk, (res) => {
                    if (res) Logging.Log($"Successfully wrote 'current_level' to DataStorage.");
                    else Logging.LogWarning($"Failed to write 'current_level' to DataStorage.");
                });
            }
            catch (Exception e) {
                Logging.LogWarning($"Failed to write 'current_level' to DataStorage: {e.Message}");
            }
        }

        internal static void WriteCurrentMap(Scenes scene) {
            try {
                OperationSpecification dop = new() {
                    OperationType = OperationType.Default,
                    Value = 0
                };
                OperationSpecification rop = new() {
                    OperationType = OperationType.Replace,
                    Value = MapBits.IsBittableMap(scene) ? MapBits.GetBitId(scene) : -1
                };
                SetPacket pk = new() {
                    Key = $"Slot:{APClient.APSessionPlayerSlot}:current_map",
                    DefaultValue = 0,
                    WantReply = false,
                    Operations = [dop, rop]
                };
                APClient.SendPacketAsync(pk, (res) => {
                    if (res) Logging.Log($"Successfully wrote 'current_map' to DataStorage.");
                    else Logging.LogWarning($"Failed to write 'current_map' to DataStorage.");
                });
            }
            catch (Exception e) {
                Logging.LogWarning($"Failed to write 'current_map' to DataStorage: {e.Message}");
            }
        }

        internal static void WriteDeathLinkGraceCount(int count) {
            try {
                OperationSpecification dop = new() {
                    OperationType = OperationType.Default,
                    Value = 0
                };
                OperationSpecification rop = new() {
                    OperationType = OperationType.Replace,
                    Value = count
                };
                SetPacket pk = new() {
                    Key = $"Slot:{APClient.APSessionPlayerSlot}:deathlink_grace_count",
                    DefaultValue = 0,
                    WantReply = false,
                    Operations = [dop, rop]
                };
                APClient.SendPacketAsync(pk, (res) => {
                    if (res) Logging.Log($"Successfully wrote 'deathlink_grace_count' to DataStorage.");
                    else Logging.LogWarning($"Failed to write 'deathlink_grace_count' to DataStorage.");
                });
            }
            catch (Exception e) {
                Logging.LogWarning($"Failed to write 'deathlink_grace_count' to DataStorage: {e.Message}");
            }
        }

        internal static void WriteMapsVisited(int mapbits) {
            try {
                OperationSpecification dop = new() {
                    OperationType = OperationType.Default,
                    Value = 0
                };
                OperationSpecification oop = new() {
                    OperationType = OperationType.Or,
                    Value = mapbits
                };
                SetPacket pk = new() {
                    Key = $"Slot:{APClient.APSessionPlayerSlot}:maps_visited",
                    DefaultValue = 0,
                    WantReply = false,
                    Operations = [dop, oop]
                };
                APClient.SendPacketAsync(pk, (res) => {
                    if (res) Logging.Log($"Successfully wrote 'maps_visited' to DataStorage.");
                    else Logging.LogWarning($"Failed to write 'maps_visited' to DataStorage.");
                });
            }
            catch (Exception e) {
                Logging.LogWarning($"Failed to write 'maps_visited' to DataStorage: {e.Message}");
            }
        }
    }
}
