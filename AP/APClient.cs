/// Copyright 2024 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

using System;
using System.Threading;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Archipelago.MultiClient.Net;
using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Packets;
using Archipelago.MultiClient.Net.Models;
using Archipelago.MultiClient.Net.Helpers;
using Archipelago.MultiClient.Net.MessageLog.Messages;
using BepInEx.Logging;
using Archipelago.MultiClient.Net.Exceptions;
using Archipelago.MultiClient.Net.BounceFeatures.DeathLink;
using CupheadArchipelago.Unity;

namespace CupheadArchipelago.AP {
    public class APClient {
        private static ArchipelagoSession session;
        public static bool Enabled { get; private set; } = false;
        public static bool Connected { get => session?.Socket.Connected ?? false; }
        public static bool Ready { get => SessionStatus == STATUS_READY && scoutMapStatus==1; }
        public static APData APSessionGSData { get => GetAPSessionData(); }
        public static APData.PlayerData APSessionGSPlayerData { get => APSessionGSData.playerData; }
        public static int APSessionPlayerTeam { get; private set; }
        public static int APSessionPlayerSlot { get; private set; } = 0;
        public static string APSessionPlayerName { get; private set; } = "";
        public static int APSessionGSDataSlot { get; private set; } = -1;
        public static ConnectedPacket ConnectionInfo { get; private set; }
        public static bool IsTryingSessionConnect { get => SessionStatus > 1; }
        public static int SessionStatus { get; private set; } = 0;
        public static APSlotData SlotData { get; private set; }
        private static Dictionary<long, ScoutedItemInfo> locMap = new();
        private static Dictionary<long, APItemInfo> itemMap = new();
        public static PlayerInfo APSessionPlayerInfo { get; private set; } = null;
        private static List<APItemData> ReceivedItems { get => APSessionGSData.receivedItems; }
        private static long ReceivedItemsIndex { get => ReceivedItems.Count + receivedItemsQueue.Count; }
        private static List<long> DoneChecks { get => APSessionGSData.doneChecks; }
        private static HashSet<long> doneChecksUnique;
        private static HashSet<APItemData> receivedItemsUnique;
        private static bool receivedItemsQueueLock = false;
        private static Queue<APItemData> receivedItemsQueue = new();
        private static Queue<int> itemApplyQueue = new();
        private static Queue<int> itemApplyLevelQueue = new();
        private static int itemApplyIndex = 0;
        private static bool complete = false;
        private static int scoutMapStatus = 0;
        private static long currentReceivedItemIndex = 0;
        private static bool sending = false;
        private static DeathLinkService deathLinkService = null;
        private static readonly byte debug = 0;
        private static readonly Version AP_VERSION = new Version(0,5,0,0);
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
                    Reset(false);
                }
            }
            bool res = false;

            APData data = APData.SData[index];
            session = ArchipelagoSessionFactory.CreateSession(data.address, data.port);
            APSessionPlayerName = data.player;
            APSessionGSDataSlot = index;
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
            APData data = APData.SData[APSessionGSDataSlot];
            Plugin.Log($"[APClient] Connecting to {data.address}:{data.port} as {data.player}...");
            LoginResult result;
            try {
                string passwd = data.password.Length>0?data.password:null;
                result = session.TryConnectAndLogin("Cuphead", data.player, ItemsHandlingFlags.AllItems, AP_VERSION, null, null, passwd); //FIXME: Use better Item Handling Later
            } catch (Exception e) {
                result = new LoginFailure(e.GetBaseException().Message);
            }

            if (result.Successful)
            {
                Plugin.Log($"[APClient] Connected to {data.address}:{data.port} as {data.player}");
                SessionStatus = 3;
                Plugin.Log($"[APClient] Getting AP Data...");

                try {
                    LoginSuccessful loginData = (LoginSuccessful)result;
                    APSessionPlayerTeam = loginData.Team;
                    APSessionPlayerSlot = loginData.Slot;
                    if (APSessionGSData.seed.Length==0)
                        APSessionGSData.seed = session.RoomState.Seed;
                    SlotData = new APSlotData(loginData.SlotData);
                    APSessionPlayerInfo = session.Players.GetPlayerInfo(APSessionPlayerTeam, APSessionPlayerSlot);
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
                //Plugin.Log($"Seed: {seed}");
                //Plugin.Log($"File {APSessionGSDataSlot} seed: {APSessionGSData.seed}");
                if (APSessionGSData.seed != seed) {
                    if (APSessionGSData.seed != "") {
                        Plugin.LogError("[APClient] Seed mismatch! Are you connecting to a different multiworld?");
                        CloseArchipelagoSession(resetOnFail);
                        SessionStatus = -4;
                        return false;
                    }
                    APSessionGSData.seed = seed;
                }

                try {
                    // Probably handle this better later
                    Plugin.Log($"[APClient] Checking settings...");
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
                    APSettings.QuestPacifist = SlotData.pacifist_quest;
                    APSettings.QuestProfessional = SlotData.agrade_quest;
                    APSettings.QuestJuggler = true;
                    APSettings.DeathLink = SlotData.deathlink;
                    
                    Plugin.Log($"[APClient] Setting up game...");
                    doneChecksUnique = new HashSet<long>(DoneChecks);
                    if (!APSettings.RandomizeAbilities)
                        APSessionGSData.playerData.SetBoolValues(true, APData.PlayerData.SetTarget.All);
                    if (APSettings.DeathLink) {
                        Plugin.Log($"[APClient] Setting up DeathLink...");
                        deathLinkService = session.CreateDeathLinkService();
                        deathLinkService.OnDeathLinkReceived += OnDeathLinkReceived;
                    }

                    Plugin.Log($"[APClient] Setting up items...");
                    if (APSessionGSData.dlock) {
                        Plugin.Log($"[APClient] Waiting for AP save data unlock...");
                        while (APSessionGSData.dlock) {
                            if (SessionStatus<=0) {
                                Plugin.Log($"[APClient] Cancelled.");
                                return false;
                            }
                            Thread.Sleep(100);
                        }
                    }
                    APSessionGSData.dlock = true;
                    receivedItemsUnique = new HashSet<APItemData>(new APItemDataComparer(false));
                    for (int i=0; i<ReceivedItems.Count;i++) {
                        APItemData item = ReceivedItems[i];
                        if (item.State==0) {
                            QueueItem(item, i);
                        }
                        else if (itemApplyIndex < item.State) {
                            itemApplyIndex = item.State;
                        }
                        if (item.Location>=0) receivedItemsUnique.Add(item);
                    }
                    APSessionGSData.dlock = false;

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
                string errorMessage = $"[APClient] Failed to Connect to {data.address}:{data.port} as {data.player}:";
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
            if (SessionStatus<=0) SessionStatus = 0;
            else Plugin.LogWarning("[APClient] Cannot Reset an active session. Close the session first! ");
        }
        private static void Reset(bool resetSessionStatus=true) {
            session = null;
            if (resetSessionStatus && SessionStatus>0) SessionStatus = 0;
            APSessionPlayerName = "";
            APSessionPlayerSlot = -1;
            APSessionPlayerTeam = -1;
            APSessionGSDataSlot = -1;
            ConnectionInfo = null;
            APSessionPlayerInfo = null;
            currentReceivedItemIndex = 0;
            doneChecksUnique = null;
            receivedItemsUnique = null;
            scoutMapStatus = 0;
            locMap.Clear();
            itemMap.Clear();
            deathLinkService = null;
            receivedItemsQueueLock = false;
            receivedItemsQueue = new();
            itemApplyQueue = new();
            itemApplyLevelQueue = new();
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
            string locName = locMap.ContainsKey(loc) ? locMap[loc].LocationName : $"#{loc}";
            Plugin.Log($"[APClient] Adding check \"{locName}\"...");
            //Plugin.Log(doneChecksUnique.Count);
            //Plugin.Log(DoneChecks.Count);
            if (!doneChecksUnique.Contains(loc)) {
                doneChecksUnique.Add(loc);
                DoneChecks.Add(loc);
                //APData.SaveCurrent();
                if (sendChecks) SendChecks();
            } else {
                Plugin.Log($"[APClient] \"{locName}\" is already checked.");
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
                    for (int i=DoneChecks.Count;i<checkedLocations.Count;i++) {
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
        public static APItemInfo GetItemInfo(long item) {
            if (itemMap.ContainsKey(item))
                return itemMap[item];
            else {
                Plugin.LogWarning($"[APClient] GetItemInfo: No information on item id {item}. Item must not exist in this world!");
                return null;
            }
        }
        public static string GetItemName(long item) => GetItemInfo(item)?.Name ?? $"APItem {item}";
        public static void SendChecks() {
            if (DoneChecks.Count<1) return;
            long[] locs = DoneChecks.ToArray();
            if (session.Socket.Connected) {
                Plugin.Log($"[APClient] Sending Checks...");
                if (!sending) {
                    sending = true;
                    ThreadPool.QueueUserWorkItem(_ => SendChecksThread(locs));
                }
                else
                    Plugin.LogWarning("[APClient] Already sending checks.");
                UpdateGoal();
                Plugin.Log($"[APClient] Done");
            }
            else {
                Plugin.Log($"[APClient] Disconnected. Cannot send check. Will retry after connecting.");
                ReconnectArchipelagoSession();
            }
        }
        public static void UpdateGoal() {
            if (IsAPGoalComplete()) {
                if (!complete) {
                    StatusUpdatePacket statusUpdate = new StatusUpdatePacket() { Status = ArchipelagoClientState.ClientGoal };
                    session.Socket.SendPacketAsync(statusUpdate, _ => {complete = true;});
                }
            }
        }
        private static bool SendChecksThread(long[] locs) {
            bool state = false;
            try {
                session.Locations.CompleteLocationChecks(locs);
                state = true;
            } catch (ArchipelagoSocketClosedException e) {
                Plugin.LogWarning($"[APClient] Failed to send checks! {e.Message}");
            }
            LoggingFlags loggingFlags = LoggingFlags.Network | (state?LoggingFlags.Info:LoggingFlags.Warning);
            if (Plugin.IsLoggingFlagsEnabled(loggingFlags)) {
                string locsstr = "[";
                for (int i=0;i<locs.Length;i++) {
                    if (i>0) locsstr += ","; 
                    locsstr += locMap.ContainsKey(locs[i])?locMap[locs[i]].LocationName:locs[i];
                    if (i==locs.Length-1) locsstr += "]";
                }
                Plugin.Log($"[APClient] Location(s) {locsstr} send {(state?"success":"fail")}", loggingFlags, state?LogLevel.Info:LogLevel.Warning);
            }
            sending = false;
            return state;
        }

        public static void GoalComplete(Goal goal) {
            APSessionGSData.AddGoals(goal);
            UpdateGoal();
        }
        public static bool IsAPGoalComplete() {
            if (APSettings.UseDLC) return APSessionGSData.IsGoalsCompleted(Goal.Devil | Goal.Saltbaker);
            else return APSessionGSData.IsGoalsCompleted(Goal.Devil);
        }

        private static void OnMessageReceived(LogMessage message) {
            Plugin.Log($"[Archipelago] {message}");
        }
        private static void OnError(Exception e, string message) {
            Plugin.Log($"[APClient] {message}: {e}", LogLevel.Error);
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
            Plugin.Log($"[APClient] Current Item Index: {currentReceivedItemIndex}; Saved Item Index: {ReceivedItemsIndex}");
            try {
                ItemInfo item = helper.PeekItem();
                if (!itemMap.ContainsKey(item.ItemId)) {
                    itemMap.Add(item.ItemId, new APItemInfo(item.ItemId, item.ItemName, item.Flags));
                }
                long itemId = item.ItemId;
                string itemName = GetItemName(itemId);
                if (currentReceivedItemIndex>ReceivedItemsIndex) {
                    currentReceivedItemIndex=ReceivedItemsIndex;
                    Plugin.LogWarning("[APClient] currentReceivedItemIndex is greater than ReceivedItemIndex!");
                }
                else if (currentReceivedItemIndex==ReceivedItemsIndex && item.ItemId!=APSettings.StartWeapon) {
                    Plugin.Log($"[APClient] Recieving {itemName}...");
                    APItemData nitem = new APItemData(item);
                    if (!receivedItemsQueueLock) {
                        receivedItemsQueueLock = true;
                        receivedItemsQueue.Enqueue(nitem);
                        currentReceivedItemIndex++;
                        receivedItemsQueueLock = false;
                    } else {
                        Plugin.Log("[APClient] Item Queue is locked. Will try again next time.");
                    }
                } else {
                    Plugin.Log($"Skipping {itemName}");
                    currentReceivedItemIndex++;
                }
                helper.DequeueItem();
            } catch (Exception e) {
                Plugin.LogError($"[APClient] Error receiving item: {e.Message}");
                return;
            }
        }
        public static bool AreItemsUpToDate() => currentReceivedItemIndex==ReceivedItemsIndex;
        public static void ItemUpdate() {
            if (!receivedItemsQueueLock && !APSessionGSData.dlock) {
                if (receivedItemsQueue.Count>0) {
                    receivedItemsQueueLock = true;
                    APSessionGSData.dlock = true;
                    APItemData item = receivedItemsQueue.Peek();
                    ReceiveItem(item);
                    receivedItemsQueue.Dequeue();
                    APSessionGSData.dlock = false;
                    receivedItemsQueueLock = false;
                }
            }
        }
        internal static void ReceiveItem(APItemData item) {
            if (!receivedItemsUnique.Contains(item)) {
                ReceivedItems.Add(item);
                if (item.Location>=0) receivedItemsUnique.Add(item);
                Plugin.Log($"[APClient] Received {GetItemName(item.Id)} from {item.Player}");
                QueueItem(item);
            }
            else {
                Plugin.LogWarning($"[APClient] Item {GetItemName(item.Id)} from {item.Player} already exists! Skipping!");
            }
        }
        private static void QueueItem(APItemData item) => QueueItem(item, ReceivedItems.Count-1);
        private static void QueueItem(APItemData item, int itemIndex) {
            if (ItemMap.GetItemType(item.Id)==ItemType.Level) {
                QueueItem(itemApplyLevelQueue, itemIndex);
            }
            else {
                QueueItem(itemApplyQueue, itemIndex);
            }
        }
        private static void QueueItem(Queue<int> itemQueue, int itemIndex) {
            itemQueue.Enqueue(itemIndex);
            Plugin.Log("[APClient] Queue Push");
            LogQueueItemCounts();
        }
        internal static void LogQueueItemCounts() {
            Plugin.Log($"[APClient] Current ItemQueue Counts: {itemApplyQueue.Count}, {itemApplyLevelQueue.Count}"); //, LoggingFlags.Debug
        }
        public static APItemData GetReceivedItem(int index) {
            if (index >= 0 && index < ReceivedItems.Count) { 
                return ReceivedItems[index];
            } else {
                throw new IndexOutOfRangeException($"[APClient] Index Out of Range! i:{index} C:{ReceivedItems.Count}");
            }
        }
        public static bool ItemReceiveQueueIsEmpty() => receivedItemsQueue.Count==0;
        public static bool ItemApplyQueueIsEmpty() => itemApplyQueue.Count==0;
        public static bool ItemApplyLevelQueueIsEmpty() => itemApplyLevelQueue.Count==0;
        public static int ItemReceiveQueueCount() => receivedItemsQueue.Count;
        public static int ItemApplyQueueCount() => itemApplyQueue.Count;
        public static int ItemApplyLevelQueueCount() => itemApplyLevelQueue.Count;
        public static APItemData PopItemApplyQueue() => PopItemQueue(itemApplyQueue);
        public static APItemData PopItemApplyLevelQueue() => PopItemQueue(itemApplyLevelQueue);
        private static APItemData PopItemQueue(Queue<int> itemQueue) {
            int index = itemQueue.Peek();
            if (index >= 0 && index < ReceivedItems.Count) { 
                APItemData item = ReceivedItems[index];
                bool success = APItemMngr.ApplyItem(item);
                if (!success) return item;
                item.State = ++itemApplyIndex;
                Plugin.Log("[APClient] Queue Pop");
                itemQueue.Dequeue();
                Plugin.Log($"[APClient] Current ItemQueue Counts: {itemApplyQueue.Count}, {itemApplyLevelQueue.Count}");
                return item;
            } else {
                throw new IndexOutOfRangeException($"[APClient] Index Out of Range! i:{index} C:{ReceivedItems.Count}");
            }
        }

        private static void OnDeathLinkReceived(DeathLink deathLink) {
            Plugin.Log($"[APClient] DeathLink: {deathLink.Cause}");
            Plugin.Log($"[APClient] Death received from {deathLink.Source}");
            if (APManager.Current!=null) {
                Plugin.Log($"[APClient] Commencing...");
                APManager.Current.TriggerDeath(deathLink.Cause);
                Plugin.Log($"[APClient] Enjoy your death!");
            }
            else {
                Plugin.Log($"[APClient] Death avoided because level conditions were not met.");
            }
        }
        public static bool IsDeathLinkActive() => deathLinkService != null;
        public static void SendDeathLink(string cause=null, DeathLinkCauseType causeType=DeathLinkCauseType.Normal) {
            if (!IsDeathLinkActive()) return;
            Plugin.Log("[APClient] Sharing your death...");
            string player = APSessionPlayerInfo.Alias;
            string deathTxt = "walloped";
            string chessDeathTxt = "beaten";
            string causeMessage = causeType switch
            {
                DeathLinkCauseType.Boss => player + " got " + deathTxt + (cause != null ? " by " + cause : "" + "!"),
                DeathLinkCauseType.Mausoleum => player + " failed to protect the Chalice" + (cause != null ? " at " + cause : "" + "!"),
                DeathLinkCauseType.Tutorial => player + " died in a tutorial level!",
                DeathLinkCauseType.ChessCastle => player + " was " + chessDeathTxt + " at The King's Leap" + (cause != null ? " by " + cause : "" + "!"),
                DeathLinkCauseType.Graveyard => player + " got taken for a ride in the graveyard!",
                _ => player + " was " + deathTxt + (cause != null ? " at " + cause : "" + "!"),
            };
            Plugin.Log($"[APClient] Your message: \"{causeMessage}\"");
            deathLinkService.SendDeathLink(new DeathLink(APSessionPlayerName, causeMessage));
            Plugin.Log("[APClient] Shared. They are enjoying your death probably...");
        }

        private static void OnPacketReceived(ArchipelagoPacketBase packet) {
            Plugin.Log(string.Format("Packet got: {0}", packet.PacketType));
            switch (packet.PacketType) {
                /*case ArchipelagoPacketType.DataPackage: {
                    DataPackagePacket datapackagepkt = (DataPackagePacket)packet;
                    APSessionGSData.dataPackage = datapackagepkt.DataPackage;
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
            if (session!=null&&APSessionGSDataSlot>=0) {
                return APData.SData[APSessionGSDataSlot];
            }
            else {
                Plugin.Log("[APClient] Cannot get APSessionData", LogLevel.Error);
                return null;
            }
        }
    }
}
