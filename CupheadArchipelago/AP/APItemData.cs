/// Copyright 2025 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

using System.Collections.Generic;
using Newtonsoft.Json;
using Archipelago.MultiClient.Net.Models;

namespace CupheadArchipelago.AP {
    public class APItemData {
        [JsonProperty("id")]
        public readonly long id;
        [JsonProperty("location")]
        public readonly long location;
        [JsonProperty("player")]
        public readonly string player;
        [JsonProperty("state")]
        public int State { get; internal set; }

        [JsonConstructor]
        public APItemData(long id, long location, string player, int state = 0) {
            this.id = id;
            this.location = location;
            this.player = player;
            State = state;
        }
        public APItemData(ItemInfo item) {
            id = item.ItemId;
            location = item.LocationId;
            player = item.Player.Name;
            State = 0;
        }

        public override string ToString() {
            return id + ": " + location + " - \"" + player + "\"";
        }
        public override bool Equals(object obj) {
            if (obj is APItemData item) {
                return id == item.id && location == item.location && player == item.player;
            } else return false;
        }
        public override int GetHashCode() => GetHashCode(true);
        public int GetHashCode(bool useId) {
            unchecked {
                int hash = 17;
                if (useId) hash *= 29 + id.GetHashCode();
                hash *= 29 + location.GetHashCode();
                if (player == null) {
                    Logging.LogWarning("[APItemData] GetHashCode: Player is null!");
                }
                hash *= 29 + (player?.GetHashCode() ?? 0);
                return hash;
            }
        }
    }

    internal class APItemDataComparer(bool useId = true) : IEqualityComparer<APItemData> {
        private readonly bool useId = useId;

        public bool Equals(APItemData a, APItemData b) {
            return (!useId || a.id == b.id) && a.location == b.location && a.player == b.player;
        }
        public int GetHashCode(APItemData item) => item.GetHashCode(useId);
    }
}
