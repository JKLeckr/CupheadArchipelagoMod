/// Copyright 2024 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

using Newtonsoft.Json;
using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Models;

namespace CupheadArchipelago.AP {
    public class APItemInfo {
        [JsonProperty("id")]
        public long Id { get; private set; }
        [JsonProperty("name")]
        public string Name { get; private set; }
        [JsonProperty("flags")]
        public ItemFlags Flags { get; private set; }
        [JsonProperty("location")]
        public string Location { get; private set; }
        [JsonProperty("locationGame")]
        public string LocationGame { get; private set; }
        [JsonProperty("player")]
        public string Player { get; private set; }
        [JsonProperty("status")]
        public int State { get; internal set; }

        [JsonConstructor]
        public APItemInfo(long id, string name, ItemFlags flags, string location, string locationGame, string player, int state = 0) {
            Id = id;
            Name = name;
            Flags = flags;
            Location = location;
            LocationGame = locationGame;
            Player = player;
            State = state;
        }
        public APItemInfo(ItemInfo item) {
            Id = item.ItemId;
            Name = item.ItemName ?? $"Item id:{item.ItemId}";
            Flags = item.Flags;
            Location = item.LocationName ?? $"Location id:{item.LocationId}";
            LocationGame = item.LocationGame;
            Player = item.Player.Alias;
            State = 0;
        }
    }
}