/// Copyright 2024 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Models;

namespace CupheadArchipelago.AP {
    public class APItemInfo {
        public long Id { get => id; }
        public string Name { get => name; }
        public ItemFlags Flags { get => flags; }
        public string Location { get => location; }
        public string LocationGame { get => locationGame; }
        public string Player { get => player; }

        private readonly long id;
        private readonly string name;
        private readonly ItemFlags flags;
        private readonly string location;
        private readonly string locationGame;
        private readonly string player;

        public APItemInfo(ItemInfo item) {
            id = item.ItemId;
            name = item.ItemName ?? $"Item id:{item.ItemId}";
            flags = item.Flags;
            location = item.LocationName ?? $"Location id:{item.LocationId}";
            locationGame = item.LocationGame;
            player = item.Player.Alias;
        }
        public APItemInfo(long id, string name, ItemFlags flags, string location, string locationGame, string player) {
            this.id = id;
            this.name = name;
            this.flags = flags;
            this.location = location;
            this.locationGame = locationGame;
            this.player = player;
        }
    }
}