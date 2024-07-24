/// Copyright 2024 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

using System;
using System.Threading;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Archipelago.MultiClient.Net;
using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Packets;
using Archipelago.MultiClient.Net.Models;
using Archipelago.MultiClient.Net.Helpers;
using Archipelago.MultiClient.Net.MessageLog.Messages;
using BepInEx.Logging;
using System.Linq;

namespace CupheadArchipelago.AP {
    public class APClient {
        private static ArchipelagoSession session;
        public static bool Enabled { get; private set; } = false;
        public static bool Connected { get => session?.Socket.Connected ?? false; }
        public static bool Ready { get => SessionStatus == STATUS_READY && scoutMapStatus==1; }
        public static APData APSessionGSData { get => GetAPSessionData(); }
        public static APData.PlayerData APSessionGSPlayerData { get => APSessionGSData.playerData; }
        public static string APSessionSlotName { get; private set; } = "";
        public static int APSessionDataSlotNum { get; private set; } = -1;
        public static ConnectedPacket ConnectionInfo { get; private set; }
        public static bool IsTryingSessionConnect { get => SessionStatus > 1; }
        public static int SessionStatus { get; private set; } = 0;
        public static APSlotData SlotData { get; private set; }
        private static Dictionary<long, ScoutedItemInfo> locMap = new();
        private static Queue<ItemInfo> ItemReceiveQueue { get => APSessionGSData.receivedItemApplyQueue; }
        private static Queue<ItemInfo> ItemReceiveLevelQueue { get => APSessionGSData.receivedLevelItemApplyQueue; }
        private static List<ItemInfo> ReceivedItems { get => APSessionGSData.receivedItems; }
        private static long ReceivedItemsIndex { get => APSessionGSData.receivedItemIndex; }
        private static List<long> DoneChecks { get => APSessionGSData.doneChecks; }
        private static HashSet<long> doneChecksUnique;
        private static bool complete = false;
        private static int scoutMapStatus = 0;
        private static long currentReceivedItemIndex = 0;
        private static readonly byte debug = 0;
        private static readonly Version AP_VERSION = new Version(0,4,6,0);
        internal const long AP_SLOTDATA_VERSION = 0;
        internal const long AP_ID_VERSION = 0;
        private const int STATUS_READY = 1;
        private const int RECONNECT_MAX_RETRIES = 3;
        private const int RECONNECT_RETRY_WAIT = 5000;

        public static bool CreateAndStartArchipelagoSession(int index) {
            if (IsTryingSessionConnect) {
                Plugin.Log($"[APClient] Already Trying to Connect. Aborting.", LogLevel.Error);
                return false;
            }
            SessionStatus = 2;
            if (session!=null) {
                if (session.Socket.Connected) {
                    Plugin.Log($"[APClient] Already Connected. Disconnecting...", LogLevel.Warning);
                    session.Socket.Disconnect();
                }
            }
            bool res = false;

            APData data = APData.SData[index];
            session = ArchipelagoSessionFactory.CreateSession(data.address, data.port);
            APSessionSlotName = data.slot;
            APSessionDataSlotNum = index;
            session.MessageLog.OnMessageReceived += OnMessageReceived;
            session.Socket.ErrorReceived += OnError;
            session.Socket.SocketClosed += OnSocketClosed;
            session.Items.ItemReceived += OnItemReceived;
            session.Socket.PacketReceived += OnPacketReceived;
            session.Socket.SocketOpened += () => {Plugin.Log("Socket Opened");};

            res = ConnectArchipelagoSession();

            return res;
        }

        private static bool ConnectArchipelagoSession(bool resetOnFail = true) {
            if (SessionStatus > 2) {
                Plugin.LogWarning("[APClient] Already Trying to Connect. Aborting.");
                return false;
            }
            SessionStatus = 3;
            APData data = APData.SData[APSessionDataSlotNum];
            Plugin.Log($"[APClient] Connecting to {data.address} as {data.slot}...");
            LoginResult result;
            try {
                string passwd = data.password.Length>0?data.password:null;
                result = session.TryConnectAndLogin("Cuphead", data.slot, ItemsHandlingFlags.AllItems, AP_VERSION, null, null, passwd); //FIXME: Use better Item Handling Later
            } catch (Exception e) {
                result = new LoginFailure(e.GetBaseException().Message);
            }

            if (result.Successful)
            {
                Plugin.Log($"[APClient] Connected to {data.address} as {data.slot}");

                Plugin.Log($"[APClient] Getting AP Data...");

                try {
                    LoginSuccessful loginData = (LoginSuccessful)result;
                    if (APSessionGSData.seed.Length==0)
                        APSessionGSData.seed = session.RoomState.Seed;
                    SlotData = new APSlotData(loginData.SlotData);
                } catch (Exception e) {
                    Plugin.LogError($"[APClient] Exception: {e.Message}");
                    Plugin.LogError(e.ToString());
                    //Plugin.Log(e.ToString(), LoggingFlags.Debug, LogLevel.Error);
                    CloseArchipelagoSession(resetOnFail);
                    SessionStatus = -2;
                    return false;
                }

                //Plugin.Log($"[APClient] APWorld version {SlotData.world_version}");

                SessionStatus = 4;
                Plugin.Log($"[APClient] Checking SlotData version...");
                if (SlotData.version != AP_SLOTDATA_VERSION) {
                    Plugin.LogError($"[APClient] SlotData version mismatch: C:{AP_SLOTDATA_VERSION} != S:{SlotData.version}! Incompatible client!");
                    CloseArchipelagoSession(resetOnFail);
                    SessionStatus = -3;
                    return false;
                }
                Plugin.Log($"[APClient] Checking ID version...");
                if (SlotData.id_version != AP_SLOTDATA_VERSION) {
                    Plugin.LogError($"[APClient] SlotData version mismatch: C:{AP_SLOTDATA_VERSION} != S:{SlotData.version}! Incompatible client!");
                    CloseArchipelagoSession(resetOnFail);
                    SessionStatus = -3;
                    return false;
                }
                Plugin.Log($"[APClient] Checking seed...");
                string seed = session.RoomState.Seed;
                if (APData.CurrentSData.seed != seed) {
                    if (APData.CurrentSData.seed != "") {
                        Plugin.LogError("[APClient] Seed mismatch! Are you connecting to a different multiworld?");
                        CloseArchipelagoSession(resetOnFail);
                        SessionStatus = -4;
                        return false;
                    }
                    APData.CurrentSData.seed = seed;
                }

                Plugin.Log($"[APClient] Checking settings...");
                try {
                    // Probably handle this better later
                    if (DLCManager.DLCEnabled()!=SlotData.use_dlc) {
                        Plugin.LogError($"[APClient] Content Mismatch! Client: {DLCManager.DLCEnabled()}, Server: {SlotData.use_dlc}");
                        if (DLCManager.DLCEnabled())
                            Plugin.LogError($"[APClient] Note: You can disable the DLC if you have to.");
                        CloseArchipelagoSession(resetOnFail);
                        SessionStatus = -5;
                        return false;
                    }

                    SessionStatus = 5;

                    Plugin.Log($"[APClient] Applying settings...");
                    APSettings.UseDLC = SlotData.use_dlc;
                    APSettings.Hard = SlotData.expert_mode;
                    APSettings.StartWeapon = SlotData.start_weapon;
                    APSettings.FreemoveIsles = SlotData.freemove_isles;
                    APSettings.BossGradeChecks = SlotData.boss_grade_checks;
                    APSettings.RungunGradeChecks = SlotData.rungun_grade_checks;
                    APSettings.DeathLink = SlotData.deathlink;
                    
                    Plugin.Log($"[APClient] Setting up game...");
                    doneChecksUnique = new HashSet<long>(DoneChecks);
                    if (true) APSessionGSData.playerData.SetBoolValues(true, APData.PlayerData.SetTarget.All); // Implement ability workings later

                    Plugin.Log($"[APClient] Catching up...");
                    CatchUpChecks();

                    //TODO: Add randomize client-side stuff
                } catch (Exception e) {
                    Plugin.LogError($"[APClient] Exception: {e.Message}");
                    Plugin.Log(e.ToString(), LoggingFlags.Debug, LogLevel.Error);
                    CloseArchipelagoSession(resetOnFail);
                    SessionStatus = -6;
                    return false;
                }

                LogQueueItemCounts();

                SessionStatus = 6;

                if (scoutMapStatus==0) Plugin.Log($"[APClient] Waiting for location scout...");
                while (scoutMapStatus==0) {
                    Thread.Sleep(100);
                }
                if (scoutMapStatus<0) {
                    Plugin.LogError($"[APClient] Scout failed!");
                    SessionStatus = -7;
                    return false;
                }

                Enabled = true;
                SessionStatus = 1;

                Plugin.Log($"[APClient] Done!");
                return true;
            }
            else {
                LoginFailure failure = (LoginFailure)result;
                string errorMessage = $"[APClient] Failed to Connect to {data.address} as {data.slot}:";
                foreach (string error in failure.Errors)
                {
                    errorMessage += $"\n    {error}";
                }
                foreach (ConnectionRefusedError error in failure.ErrorCodes)
                {
                    errorMessage += $"\n    {error}";
                }

                Plugin.LogError(errorMessage);

                if (resetOnFail) Reset();
                SessionStatus = -1;
                return false;
            }
        }
        public static void ReconnectArchipelagoSession() {
            /* TODO: Have a return result from the success of the connection instead of passively failing */
            ThreadPool.QueueUserWorkItem(_ => {
                int chances = RECONNECT_MAX_RETRIES;
                while (chances<0||chances>0) {
                    Plugin.Log($"[APClient] Waiting {RECONNECT_RETRY_WAIT}...");
                    Thread.Sleep(RECONNECT_RETRY_WAIT);
                    Plugin.Log("[APClient] Reconnecting...");
                    bool result = ConnectArchipelagoSession(false);
                    if (result) return;
                    chances--;
                }
                Plugin.LogError("[APClient] Failed to reconnect!");
            });
        }
        public static bool CloseArchipelagoSession(bool reset = true) {
            bool res = false;
            Enabled = false;
            if (session!=null) {
                if (session.Socket.Connected) {
                    Plugin.Log($"[APClient] Disconnecting APSession...");
                    session.Socket.Disconnect();
                    SessionStatus = 0;
                    res = true;
                }
            }
            if (reset) Reset();
            return res;
        }
        public static void ResetSessionError() {
            if (SessionStatus<0) SessionStatus = 0;
            else Plugin.LogWarning("[APClient] Cannot Reset an active session. Close the session first! ");
        }
        private static void Reset() {
            session = null;
            if (SessionStatus>0) SessionStatus = 0;
            APSessionSlotName = "";
            APSessionDataSlotNum = -1;
            ConnectionInfo = null;
            doneChecksUnique = null;
            scoutMapStatus = 0;
            locMap.Clear();
        }

        public static bool IsLocationChecked(long loc) => doneChecksUnique.Contains(loc);
        public static bool IsAnyLocationChecked(long[] locs) {
            foreach (long loc in locs) {
                if (IsLocationChecked(loc))
                    return true;
            }
            return false;
        }
        public static bool IsLocationsChecked(long[] locs) {
            if (locs.Length<1) return false;
            foreach (long loc in locs) {
                if (IsLocationChecked(loc))
                    return false;
            }
            return true;
        }
        public static bool Check(long loc, bool sendChecks = true) {
            if (!locMap.ContainsKey(loc)) {
                Plugin.LogError($"[APClient] Location {loc} is missing. Skipping.");
                return false;
            }
            string locName = !locMap.ContainsKey(loc) ? locMap[loc].LocationName : $"#{loc}";
            Plugin.Log($"[APClient] Adding check \"{locName}\"...");
            //Plugin.Log(doneChecksUnique.Count);
            //Plugin.Log(DoneChecks.Count);
            if (!doneChecksUnique.Contains(loc)) {
                doneChecksUnique.Add(loc);
                DoneChecks.Add(loc);
                //APData.SaveCurrent();
                if (sendChecks) SendChecks();
            } else {
                Plugin.LogWarning($"[APClient] \"{locName}\" is already Checked!");
            }
            return true;
        }
        public static void Check(long[] locs, bool sendChecks = true) {
            foreach (long loc in locs) {
                Check(loc, false);
            }
            if (sendChecks) SendChecks();
        }
        public static void CatchUpChecks() {
            if (session.Socket.Connected) {
                ReadOnlyCollection<long> checkedLocations = session.Locations.AllLocationsChecked;
                if (DoneChecks.Count<checkedLocations.Count) {
                    for (int i=DoneChecks.Count-1;i<checkedLocations.Count;i++) {
                        Check(checkedLocations[i], false);
                    }
                }
            }
        }
        public static ScoutedItemInfo GetCheck(long loc) {
            if (locMap.ContainsKey(loc))
                return locMap[loc];
            else
                throw new KeyNotFoundException($"[APClient] GetCheck: Invalid location id {loc}.");
        }
        public static void SendChecks() {
            if (DoneChecks.Count<1) return;
            long[] locs = DoneChecks.ToArray();
            if (session.Socket.Connected) {
                session.Locations.CompleteLocationChecksAsync((bool state) => OnLocationSendComplete(state, locs), locs);
                if (IsAPGoalComplete()) {
                    if (!complete) {
                        StatusUpdatePacket statusUpdate = new StatusUpdatePacket() { Status = ArchipelagoClientState.ClientGoal };
                        session.Socket.SendPacketAsync(statusUpdate, _ => {complete = true;});
                    }
                }
            }
            else {
                Plugin.Log($"[APClient] Disconnected. Cannot send check. Will retry after connecting.");
                ReconnectArchipelagoSession();
            }
        }
        private static void OnLocationSendComplete(bool state, long[] locs) {
            string locsstr = "[";
            for (int i=0;i<locs.Length;i++) {
                if (i>0) locsstr += ","; 
                locsstr += locMap.ContainsKey(locs[i])?locMap[locs[i]].LocationName:locs[i];
                if (i==locs.Length-1) locsstr += "]";
            }
            Plugin.Log($"[APClient] Location(s) {locsstr} send {(state?"success":"fail")}", LoggingFlags.Network, state?LogLevel.Info:LogLevel.Warning);
        }

        public static void GoalComplete(Goal goal) {
            APSessionGSData.AddGoals(goal);
        }
        public static bool IsAPGoalComplete() {
            if (APSettings.UseDLC) return APSessionGSData.IsGoalsCompleted(Goal.Devil | Goal.Saltbaker);
            else return APSessionGSData.IsGoalsCompleted(Goal.Devil);
        }

        private static void OnMessageReceived(LogMessage message) {
            Plugin.Log($"[Archipelago] {message}");
        }
        private static void OnError(Exception e, string message) {
            Plugin.Log($"[APClient] {message}", LogLevel.Error);
        }
        private static void OnSocketClosed(string reason) {
            Plugin.Log("[APClient] Disconnected.");
            Plugin.Log($"[APClient] Disconnect Reason: {reason}", LoggingFlags.Network);
            if (Enabled) {
                ReconnectArchipelagoSession();
            }
        }
        private static void OnItemReceived(ReceivedItemsHelper helper) {
            Plugin.Log("[APClient] OnItemReceived");
            ItemInfo item = helper.PeekItem();
            long itemId = item.ItemId;
            string itemName = item.ItemName ?? $"APItem {itemId}";
            if (currentReceivedItemIndex>ReceivedItemsIndex) {
                currentReceivedItemIndex=ReceivedItemsIndex;
                Plugin.LogWarning("[APClient] currentReceivedItemIndex is greater than ReceivedItemIndex!");
            }
            else if (currentReceivedItemIndex==ReceivedItemsIndex && item.ItemId!=APSettings.StartWeapon) {
                Plugin.Log($"Recieved {itemName} from {item.Player}");
                ReceiveItem(item);
                currentReceivedItemIndex++;
            } else {
                Plugin.Log($"Skipping {itemName}");
                currentReceivedItemIndex++;
            }
            helper.DequeueItem();
        }
        public static bool AreItemsUpToDate() => currentReceivedItemIndex==ReceivedItemsIndex;
        private static void ReceiveItem(ItemInfo item) {
            if (ItemMap.GetItemType(item.ItemId)==ItemType.Level) {
                QueueItem(item, true);
            }
            else {
                QueueItem(item, false);
            }
        }
        private static void QueueItem(ItemInfo item, bool isLevelItem) {
            if (isLevelItem) ItemReceiveLevelQueue.Enqueue(item);
            else ItemReceiveQueue.Enqueue(item);
            Plugin.Log("[APClient] Queue Push");
            ReceivedItems.Add(item);
            LogQueueItemCounts();
        }
        internal static void LogQueueItemCounts() {
            Plugin.Log($"[APClient] Current ItemQueue Counts: {ItemReceiveQueue.Count}, {ItemReceiveLevelQueue.Count}", LoggingFlags.Debug);
        }
        public static bool ItemReceiveQueueIsEmpty() => ItemReceiveQueue.Count==0;
        public static bool ItemReceiveLevelQueueIsEmpty() => ItemReceiveLevelQueue.Count==0;
        public static int ItemReceiveQueueCount() => ItemReceiveQueue.Count;
        public static int ItemReceiveLevelQueueCount() => ItemReceiveLevelQueue.Count;
        public static ItemInfo PopItemReceiveQueue() => PopItemQueue(ItemReceiveQueue);
        public static ItemInfo PopItemReceiveLevelQueue() => PopItemQueue(ItemReceiveLevelQueue);
        private static ItemInfo PopItemQueue(Queue<ItemInfo> itemQueue) {
            ItemInfo item = itemQueue.Peek();
            APItemMngr.ApplyItem(item);
            APSessionGSData.appliedItems.Add(item);
            Plugin.Log("[APClient] Queue Pop");
            itemQueue.Dequeue();
            Plugin.Log($"[APClient] Current ItemQueue Counts: {ItemReceiveQueue.Count}, {ItemReceiveLevelQueue.Count}", LoggingFlags.Debug);
            return item;
        }

        private static void OnPacketReceived(ArchipelagoPacketBase packet) {
            Plugin.Log(string.Format("Packet got: {0}", packet.PacketType));
            switch (packet.PacketType) {
                /*case ArchipelagoPacketType.DataPackage: {
                    DataPackagePacket datapackagepkt = (DataPackagePacket)packet;
                    APData.CurrentSData.dataPackage = datapackagepkt.DataPackage;
                    break;
                }*/
                case ArchipelagoPacketType.Connected: {
                    if (scoutMapStatus!=1) {
                        Plugin.Log($"[APClient] Getting location data...");
                        session.Locations.ScoutLocationsAsync((Dictionary<long, ScoutedItemInfo> si) => {
                            Plugin.Log($" [APClient] Processing {si.Count} locations...");
                            locMap.Clear();
                            bool err = false;
                            try {
                                foreach (ScoutedItemInfo item in si.Values) {
                                    if (SessionStatus<0) {
                                        Plugin.LogError($" [APClient] Aborted due to session error.");
                                        locMap.Clear();
                                        return;
                                    }
                                    long loc = item.LocationId;
                                    string locName = item.LocationName;

                                    bool loc_cond = APLocation.IdExists(loc);
                                    if (!loc_cond) {
                                        err = true;
                                        Plugin.LogError($" [APClient] Setup: Unknown Location: {locName}:{loc}");
                                    }
                                
                                    Plugin.Log($"Adding: {loc} {item.ItemId}", LoggingFlags.Debug);
                                    locMap.Add(loc, item);
                                }
                            } catch (Exception e) {
                                err = true;
                                Plugin.LogError($" [APClient] Exception: {e.Message}");
                            }
                            if (locMap.Count<1) {
                                scoutMapStatus = -1;
                                Plugin.LogError(" [APClient] scoutMap is empty!");
                                return;
                            }
                            if (err) {
                                scoutMapStatus = -2;
                                Plugin.LogError(" [APClient] Errors occured during processing.");
                                return;
                            }
                            Plugin.Log($" [APClient] Processed location data.");
                            if ((debug&4)>0) {
                                Plugin.Log(" -- Location data dump: --");
                                foreach (KeyValuePair<long, ScoutedItemInfo> entry in locMap) {
                                    Plugin.Log($"  {entry.Key}: {entry.Value.ItemId}: {entry.Value.LocationId}: {entry.Value.ItemName}: {entry.Value.LocationName}");
                                }
                                Plugin.Log(" -- End Location data dump --");
                            }
                            scoutMapStatus = 1;
                        }, session.Locations.AllLocations.ToArray());
                    } else scoutMapStatus = 1;
                    break;
                }
                default: break;
            }
        }

        private static APData GetAPSessionData() {
            if (session!=null&&APSessionDataSlotNum>=0) {
                return APData.SData[APSessionDataSlotNum];
            }
            else {
                Plugin.Log("[APClient] Cannot get APSessionData", LogLevel.Error);
                return null;
            }
        }
    }
}
