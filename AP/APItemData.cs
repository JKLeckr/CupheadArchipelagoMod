/// Copyright 2024 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

using System.Collections.Generic;
using Newtonsoft.Json;
using Archipelago.MultiClient.Net.Models;

namespace CupheadArchipelago.AP {
    public class APItemData {
        [JsonProperty("id")]
        public long Id { get; private set; }
        [JsonProperty("location")]
        public long Location { get; private set; }
        [JsonProperty("player")]
        public string Player { get; private set; }
        [JsonProperty("state")]
        public int State { get; internal set; }

        [JsonConstructor]
        public APItemData(long id, long location, string player, int state = 0) {
            Id = id;
            Location = location;
            Player = player;
            State = state;
        }
        public APItemData(ItemInfo item) {
            Id = item.ItemId;
            Location = item.LocationId;
            Player = item.Player.Name;
            State = 0;
        }

        public override string ToString() {
            return Id + ": " + Location + " - \"" + Player + "\"";
        }
        public override bool Equals(object obj) {
            if (obj is APItemData item) {
                return Id == item.Id && Location == item.Location && Player == item.Player;
            } else return false;
        }
        public override int GetHashCode() => GetHashCode(true);
        public int GetHashCode(bool useId) {
            unchecked {
                int hash = 17;
                if (useId) hash *= 29 + Id.GetHashCode();
                hash *= 29 + Location.GetHashCode();
                if (Player == null) {
                    Logging.LogWarning("[APItemData] GetHashCode: Player is null!");
                }
                hash *= 29 + (Player?.GetHashCode() ?? 0);
                return hash;
            }
        }
    }

    internal class APItemDataComparer(bool useId = true) : IEqualityComparer<APItemData> {
        private readonly bool useId = useId;

        public bool Equals(APItemData a, APItemData b) {
            return (!useId || a.Id == b.Id) && a.Location == b.Location && a.Player == b.Player;
        }
        public int GetHashCode(APItemData item) => item.GetHashCode(useId);
    }
}