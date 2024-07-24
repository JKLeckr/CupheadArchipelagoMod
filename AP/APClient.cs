/// Copyright 2024 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

using System;
using System.Threading;
using System.Collections.Generic;
using Archipelago.MultiClient.Net;
using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Packets;
using Archipelago.MultiClient.Net.Models;
using Archipelago.MultiClient.Net.Helpers;
using Archipelago.MultiClient.Net.MessageLog.Messages;
using BepInEx.Logging;
using CupheadArchipelago.Auxiliary;

namespace CupheadArchipelago.AP {
    public class APClient {
        public static ArchipelagoSession APSession {get; private set;}
        public static bool Enabled {get; private set;} = false;
        public static bool Connected {get => SessionStatus>=STATUS_CONNECTED;}
        public static APData APSessionGSData {get => GetAPSessionData();}
        public static APData.PlayerData APSessionGSPlayerData {get => APSessionGSData.playerData;}
        public static string APSessionSlotName {get; private set;} = "";
        public static int APSessionDataSlotNum {get; private set;} = -1;
        public static ConnectedPacket ConnectionInfo {get; private set;}
        public static bool IsTryingSessionConnect {get => SessionStatus > 1;}
        public static int SessionStatus { get; private set; } = 0;
        private static Dictionary<string, object> SlotData {get => APSessionGSData.slotData;}
        private static Queue<NetworkItem> ItemReceiveQueue {get => APSessionGSData.receivedItemApplyQueue;}
        private static Queue<NetworkItem> ItemReceiveLevelQueue {get => APSessionGSData.receivedLevelItemApplyQueue;}
        private static List<NetworkItem> ReceivedItems {get => APSessionGSData.receivedItems;}
        private static long ReceivedItemsIndex {get => APSessionGSData.ReceivedItemIndex;}
        private static List<long> DoneChecks { get => APSessionGSData.doneChecks; }
        private static HashSet<long> doneChecksUnique;
        private static bool complete = false;
        private static long currentReceivedItemIndex = 0;
        private static readonly Version AP_VERSION = new Version(0,4,4,0);
        private const int STATUS_CONNECTED = 1;
        private const int RECONNECT_MAX_RETRIES = 3;
        private const int RECONNECT_RETRY_WAIT = 5000;

        public static bool CreateAndStartArchipelagoSession(int index) {
            if (IsTryingSessionConnect) {
                Plugin.Log($"[APClient] Already Trying to Connect. Aborting.", LogLevel.Error);
                return false;
            }
            SessionStatus = 2;
            if (APSession!=null) {
                if (APSession.Socket.Connected) {
                    Plugin.Log($"[APClient] Already Connected. Disconnecting...", LogLevel.Warning);
                    APSession.Socket.Disconnect();
                }
            }
            bool res = false;

            APData data = APData.SData[index];
            APSession = ArchipelagoSessionFactory.CreateSession(data.address, data.port);
            APSessionSlotName = data.slot;
            APSessionDataSlotNum = index;
            APSession.MessageLog.OnMessageReceived += OnMessageReceived;
            APSession.Socket.ErrorReceived += OnError;
            APSession.Socket.SocketClosed += OnSocketClosed;
            APSession.Items.ItemReceived += OnItemReceived;
            APSession.Socket.PacketReceived += OnPacketReceived;

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
                result = APSession.TryConnectAndLogin("Cuphead", data.slot, ItemsHandlingFlags.AllItems, AP_VERSION, null, null, passwd); //FIXME: Use better Item Handling Later
            } catch (Exception e) {
                result = new LoginFailure(e.GetBaseException().Message);
            }

            if (result.Successful)
            {
                Plugin.Log($"[APClient] Connected to {data.address} as {data.slot}");

                Plugin.Log($"[APClient] Getting AP Data...");

                LoginSuccessful loginData = (LoginSuccessful)result;
                APSessionGSData.slotData = loginData.SlotData;
                APSessionGSData.seed = APSession.RoomState.Seed;

                SessionStatus = 4;
                Plugin.Log($"[APClient] Checking seed...");
                string seed = APSession.RoomState.Seed;
                if (APData.CurrentSData.seed != seed) {
                    if (APData.CurrentSData.seed != "") {
                        Plugin.LogError("[APClient] Seed mismatch! Are you connecting to a different multiworld?");
                        CloseArchipelagoSession(resetOnFail);
                        SessionStatus = -2;
                        return false;
                    }
                    APData.CurrentSData.seed = seed;
                }

                Plugin.Log($"[APClient] Checking settings...");
                try {
                    bool use_dlc = GetAPSlotDataBool("use_dlc");
                    // Probably handle this better later
                    if (DLCManager.DLCEnabled()!=use_dlc) {
                        Plugin.LogError($"[APClient] Content Mismatch! Client: {DLCManager.DLCEnabled()}, Server: {use_dlc}");
                        if (DLCManager.DLCEnabled())
                            Plugin.LogError($"[APClient] Note: You can disable the DLC if you have to.");
                        CloseArchipelagoSession(resetOnFail);
                        SessionStatus = -3;
                        return false;
                    }

                    SessionStatus = 5;

                    Plugin.Log($"[APClient] Applying settings...");
                    APSettings.UseDLC = use_dlc;
                    APSettings.Hard = GetAPSlotDataBool("expert_mode");
                    APSettings.FreemoveIsles = GetAPSlotDataBool("freemove_isles");
                    APSettings.BossGradeChecks = (GradeChecks)GetAPSlotDataLong("boss_grade_checks");
                    APSettings.RungunGradeChecks = (GradeChecks)GetAPSlotDataLong("rungun_grade_checks");
                    APSettings.DeathLink = GetAPSlotDataBool("deathlink"); 
                    
                    Plugin.Log($"[APClient] Setting up game...");
                    doneChecksUnique = new HashSet<long>(APData.SData[APSessionDataSlotNum].doneChecks);
                    if (true) APSessionGSData.playerData.SetBoolValues(true, APData.PlayerData.SetTarget.All); // Implement ability workings later

                    //TODO: Add randomize client-side stuff
                } catch (Exception e) {
                    Plugin.LogError($"[APClient] Exception: {e.Message}");
                    CloseArchipelagoSession(resetOnFail);
                    SessionStatus = -4;
                    return false;
                }

                LogQueueItemCounts();

                SessionStatus = 6;

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
            if (APSession!=null) {
                if (APSession.Socket.Connected) {
                    Plugin.Log($"[APClient] Disconnecting APSession...");
                    APSession.Socket.Disconnect();
                    res = true;
                }
            }
            if (reset) Reset();
            return res;
        }
        private static void Reset() {
            APSession = null;
            SessionStatus = 0;
            APSessionSlotName = "";
            APSessionDataSlotNum = -1;
            ConnectionInfo = null;
            doneChecksUnique = null;
        }

        public static bool IsLocationChecked(long loc) => doneChecksUnique.Contains(loc);
        public static void Check(long loc, bool sendChecks = true) {
            Plugin.Log(string.Format("[APClient] Adding check \"{0}\"...", APLocation.IdToName(loc)));
            //Plugin.Log(doneChecksUnique.Count);
            //Plugin.Log(DoneChecks.Count);
            if (!doneChecksUnique.Contains(loc)) {
                doneChecksUnique.Add(loc);
                DoneChecks.Add(loc);
                //APData.SaveCurrent();
                if (sendChecks) SendChecks();
            }
        }
        public static void SendChecks() {
            if (DoneChecks.Count<1) return;
            long[] locs = DoneChecks.ToArray();
            if (APSession.Socket.Connected) {
                APSession.Locations.CompleteLocationChecksAsync((bool state) => OnLocationSendComplete(state, locs), locs);
                if (IsAPGoalComplete()) {
                    if (!complete) {
                        StatusUpdatePacket statusUpdate = new StatusUpdatePacket() { Status = ArchipelagoClientState.ClientGoal };
                        APSession.Socket.SendPacketAsync(statusUpdate, _ => {complete = true;});
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
                locsstr += APLocation.IdExists(locs[i])?APLocation.IdToName(locs[i]):locs[i];
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
            NetworkItem item = helper.PeekItem();
            string itemName = APItem.IdExists(item.Item)?APItem.IdToName(item.Item):item.Item.ToString();
            if (currentReceivedItemIndex>ReceivedItemsIndex) {
                currentReceivedItemIndex=ReceivedItemsIndex;
                Plugin.LogWarning("[APClient] currentReceivedItemIndex is greater than ReceivedItemIndex!");
            }
            if (currentReceivedItemIndex==ReceivedItemsIndex) {
                Plugin.Log($"Recieved {itemName} from {item.Player}");
                ReceiveItem(item);
            } else {
                Plugin.Log($"Skipping {itemName}");
                currentReceivedItemIndex++;
            }
            helper.DequeueItem();
        }
        private static void ReceiveItem(NetworkItem item) {
            if (ItemMap.GetItemType(item.Item)==ItemType.Level) {
                QueueItem(item, true);
            }
            else {
                QueueItem(item, false);
            }
        }
        private static void QueueItem(NetworkItem item, bool levelItem) {
            if (levelItem) ItemReceiveLevelQueue.Enqueue(item);
            else ItemReceiveQueue.Enqueue(item);
            Plugin.Log("[APClient] Queue Push");
            ReceivedItems.Add(item);
            LogQueueItemCounts();
            currentReceivedItemIndex++;
        }
        internal static void LogQueueItemCounts() {
            Plugin.Log($"[APClient] Current ItemQueue Counts: {ItemReceiveQueue.Count}, {ItemReceiveLevelQueue.Count}", LoggingFlags.Debug);
        }
        public static bool ItemReceiveQueueIsEmpty() => ItemReceiveQueue.Count==0;
        public static bool ItemReceiveLevelQueueIsEmpty() => ItemReceiveLevelQueue.Count==0;
        public static int ItemReceiveQueueCount() => ItemReceiveQueue.Count;
        public static int ItemReceiveLevelQueueCount() => ItemReceiveLevelQueue.Count;
        public static NetworkItem PopItemReceiveQueue() => PopItemQueue(ItemReceiveQueue);
        public static NetworkItem PopItemReceiveLevelQueue() => PopItemQueue(ItemReceiveLevelQueue);
        private static NetworkItem PopItemQueue(Queue<NetworkItem> itemQueue) {
            NetworkItem item = itemQueue.Peek();
            APItemMngr.ApplyItem(item.Item);
            APSessionGSData.appliedReceivedItems.Add(item);
            Plugin.Log("[APClient] Queue Pop");
            itemQueue.Dequeue();
            Plugin.Log($"[APClient] Current ItemQueue Counts: {ItemReceiveQueue.Count}, {ItemReceiveLevelQueue.Count}", LoggingFlags.Debug);
            return item;
        }

        private static void OnPacketReceived(ArchipelagoPacketBase packet) {
            switch (packet.PacketType) {
                /*case ArchipelagoPacketType.DataPackage: {
                    DataPackagePacket datapackagepkt = (DataPackagePacket)packet;
                    APData.CurrentSData.dataPackage = datapackagepkt.DataPackage;
                    break;
                }*/
                default: Plugin.Log(string.Format("Packet got: {0}", packet.PacketType)); break;
            }
        }

        private static APData GetAPSessionData() {
            if (APSession!=null&&APSessionDataSlotNum>=0) {
                return APData.SData[APSessionDataSlotNum];
            }
            else {
                Plugin.Log("[APClient] Cannot get APSessionData", LogLevel.Error);
                return null;
            }
        }

        private static bool GetAPSlotDataBool(string key) => Aux.IntAsBool(GetAPSlotDataLong(key));
        private static long GetAPSlotDataLong(string key) => (long)GetAPSlotData(key);
        private static string GetAPSlotDataString(string key) => (string)GetAPSlotData(key);
        private static object GetAPSlotData(string key) {
            try { 
                return APSessionGSData.slotData[key];
            } catch (KeyNotFoundException) {
                throw new KeyNotFoundException($"GetAPSlotData: {key} is not a valid key!");
            }
        }
    }
}
