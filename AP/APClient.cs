/// Copyright 2025 JKLeckr
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
        public static APSlotData SlotData { get; private set; } = null;
        private static Dictionary<long, ScoutedItemInfo> locMap = new();
        private static Dictionary<long, APItemInfo> itemMap = new();
        public static PlayerInfo APSessionPlayerInfo { get; private set; } = null;
        private static List<APItemData> ReceivedItems { get => APSessionGSData.receivedItems; }
        private static long ReceivedItemsIndex { get => ReceivedItems.Count; }
        private static List<long> DoneChecks { get => APSessionGSData.doneChecks; }
        private static HashSet<long> doneChecksUnique;
        private static HashSet<APItemData> receivedItemsUnique;
        private static HashSet<APItemData> receivedItemsUniqueB; //FIXME: For debugging purposes. Remove later
        private static Dictionary<long, int> receivedItemCounts = new();
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
        private static readonly Version AP_VERSION = new Version(0,5,1,0);
        internal const long AP_ID_VERSION = 0;
        private const int STATUS_READY = 1;
        private const int RECONNECT_MAX_RETRIES = 3;
        private const int RECONNECT_RETRY_WAIT = 5000;

        public static bool CreateAndStartArchipelagoSession(int index) {
            if (IsTryingSessionConnect) {
                Logging.LogError($"[APClient] Already Trying to Connect. Aborting.");
                return false;
            }
            SessionStatus = 2;
            if (session!=null) {
                if (session.Socket.Connected) {
                    Logging.LogWarning($"[APClient] Already Connected. Disconnecting...");
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
            session.Socket.SocketOpened += () => {Logging.Log("Socket Opened");};

            res = ConnectArchipelagoSession();

            return res;
        }

        private static bool ConnectArchipelagoSession(bool resetOnFail = true) {
            if (SessionStatus > 2) {
                Logging.LogWarning("[APClient] Already Trying to Connect. Aborting.");
                return false;
            }
            APData data = APData.SData[APSessionGSDataSlot];
            Logging.Log($"[APClient] Connecting to {data.address}:{data.port} as {data.player}...");
            LoginResult result;
            try {
                string passwd = data.password.Length>0?data.password:null;
                result = session.TryConnectAndLogin("Cuphead", data.player, ItemsHandlingFlags.AllItems, AP_VERSION, null, null, passwd); //FIXME: Use better Item Handling Later
            } catch (Exception e) {
                result = new LoginFailure(e.GetBaseException().Message);
            }

            if (result.Successful)
            {
                Logging.Log($"[APClient] Connected to {data.address}:{data.port} as {data.player}");
                SessionStatus = 3;

                LoginSuccessful loginData = (LoginSuccessful)result;

                Logging.Log($"[APClient] Checking SlotData...");
                try {
                    long slotDataVersion = APSlotData.GetSlotDataVersion(loginData.SlotData);
                    if (slotDataVersion != APSlotData.AP_SLOTDATA_VERSION) {
                        Logging.LogError($"[APClient] SlotData version mismatch: C:{APSlotData.AP_SLOTDATA_VERSION} != S:{slotDataVersion}! Incompatible client!");
                        CloseArchipelagoSession(resetOnFail);
                        SessionStatus = -3;
                        return false;
                    }
                } catch (Exception e) {
                    Logging.LogError($"[APClient] Malformed SlotData! Exception: {e.Message}");
                    //Logging.LogError(e.ToString(), LoggingFlags.Debug);
                    CloseArchipelagoSession(resetOnFail);
                    SessionStatus = -2;
                    return false;
                }

                try {
                    string worldVersion = APSlotData.GetAPWorldVersionString(loginData.SlotData);
                    Logging.Log($"[APClient] APWorld version {worldVersion}");
                } catch (Exception e) {
                    Logging.LogWarning($"[APClient] Cannot get APWorld Version! Exception: {e.Message}");
                }

                SessionStatus = 4;

                Logging.Log($"[APClient] Getting AP Data...");
                try {
                    APSessionPlayerTeam = loginData.Team;
                    APSessionPlayerSlot = loginData.Slot;
                    if (APSessionGSData.seed.Length==0)
                        APSessionGSData.seed = session.RoomState.Seed;
                    SlotData = new APSlotData(loginData.SlotData);
                    APSessionPlayerInfo = session.Players.GetPlayerInfo(APSessionPlayerTeam, APSessionPlayerSlot);
                } catch (Exception e) {
                    Logging.LogError($"[APClient] Exception: {e.Message}");
                    Logging.LogError(e.ToString());
                    //Logging.LogError(e.ToString(), LoggingFlags.Debug);
                    CloseArchipelagoSession(resetOnFail);
                    SessionStatus = -4;
                    return false;
                }

                SessionStatus = 5;
                
                Logging.Log($"[APClient] Checking seed...");
                string seed = session.RoomState.Seed;
                //Logging.Log($"File {APSessionGSDataSlot} seed: {APSessionGSData.seed}");
                if (APSessionGSData.seed != seed) {
                    if (APSessionGSData.seed != "" || APSessionGSData.IsOverridden(2)) {
                        Logging.LogError("[APClient] Seed mismatch! Are you connecting to a different multiworld?");
                        CloseArchipelagoSession(resetOnFail);
                        SessionStatus = -5;
                        return false;
                    }
                    APSessionGSData.seed = seed;
                }

                Logging.Log($"Seed: {seed}");

                SessionStatus = 6;

                if (scoutMapStatus==0) Logging.Log($"[APClient] Waiting for location scout...");
                while (scoutMapStatus==0) {
                    Thread.Sleep(100);
                }
                if (scoutMapStatus<0) {
                    Logging.LogError($"[APClient] Scout failed!");
                    SessionStatus = -6;
                    return false;
                }

                SessionStatus = 7;

                try {
                    Logging.Log($"[APClient] Checking settings...");
                    if (SlotData.use_dlc && !DLCManager.DLCEnabled()) {
                        Logging.LogError($"[APClient] Content Mismatch! Server requires DLC, but running on a non-DLC client!");
                        CloseArchipelagoSession(resetOnFail);
                        SessionStatus = -7;
                        return false;
                    }

                    SessionStatus = 8;

                    Logging.Log($"[APClient] Applying settings...");
                    APSettings.Init();
                    APSettings.UseDLC = SlotData.use_dlc;
                    APSettings.Mode = SlotData.mode;
                    APSettings.Hard = SlotData.expert_mode;
                    APSettings.StartWeapon = SlotData.start_weapon;
                    APSettings.RandomizeWeaponEX = SlotData.randomize_weapon_ex;
                    APSettings.FreemoveIsles = SlotData.freemove_isles;
                    APSettings.RandomizeAbilities = SlotData.randomize_abilities;
                    APSettings.BossSecretChecks = LocationExists(APLocation.level_boss_veggies_secret);
                    APSettings.BossGradeChecks = SlotData.boss_grade_checks;
                    APSettings.DicePalaceBossSanity = LocationExists(APLocation.level_dicepalace_boss_booze);
                    APSettings.RungunGradeChecks = SlotData.rungun_grade_checks;
                    APSettings.QuestPacifist = LocationExists(APLocation.quest_pacifist);
                    APSettings.QuestProfessional = LocationExists(APLocation.quest_silverworth);
                    APSettings.QuestJuggler = LocationExists(APLocation.quest_4parries);
                    APSettings.StartMaxHealth = SlotData.start_maxhealth;
                    APSettings.RequiredContracts = SlotData.contract_requirements;
                    APSettings.DLCRequiredIngredients = SlotData.dlc_ingredient_requirements;
                    APSettings.ContractsGoal = SlotData.contract_goal_requirements;
                    APSettings.DLCIngredientsGoal = SlotData.dlc_ingredient_goal_requirements;
                    APSettings.DLCChaliceMode = SlotData.dlc_chalice;
                    APSettings.DLCBossChaliceChecks = LocationExists(APLocation.level_boss_veggies_dlc_chaliced);
                    APSettings.DLCRunGunChaliceChecks = LocationExists(APLocation.level_rungun_forest_dlc_chaliced);
                    APSettings.DLCCurseMode = SlotData.dlc_curse_mode;
                    APSettings.ShuffleMusic = SlotData.music_shuffle;
                    APSettings.DeathLink = SlotData.deathlink;
                    
                    Logging.Log($"[APClient] Setting up game...");
                    doneChecksUnique = new(DoneChecks);
                    if (APSettings.RandomizeWeaponEX == WeaponExMode.AllButStart) {
                        Weapon weapon = ItemMap.GetWeapon(APSettings.StartWeapon.Id);
                        APSessionGSPlayerData.AddWeaponUpgrade(weapon);
                    }
                    else if (APSettings.RandomizeWeaponEX == WeaponExMode.Off) {
                        APSessionGSPlayerData.AddWeaponUpgrades(ItemMap.GetUpgradableWeapons());
                    }
                    if (!APSettings.RandomizeAbilities)
                        APSessionGSPlayerData.SetBoolValues(true, APData.PlayerData.SetTarget.AllAbilities);
                    else {
                        if ((APSettings.DLCChaliceItemsSeparate&ItemGroups.Super)==0)
                            APSessionGSPlayerData.SetBoolValues(true, APData.PlayerData.SetTarget.ChaliceSuper);
                        if ((APSettings.DLCChaliceItemsSeparate&ItemGroups.Abilities)==0)
                            APSessionGSPlayerData.SetBoolValues(true, APData.PlayerData.SetTarget.ChaliceAbilities);
                    }
                    if (!APSettings.RandomizeAimAbilities)
                        APSessionGSPlayerData.aim_directions = APData.PlayerData.AimDirections.All;
                    if (APSettings.DeathLink) {
                        Logging.Log($"[APClient] Setting up DeathLink...");
                        deathLinkService = session.CreateDeathLinkService();
                        deathLinkService.EnableDeathLink();
                        deathLinkService.OnDeathLinkReceived += OnDeathLinkReceived;
                    }

                    Logging.Log($"[APClient] Setting up items...");
                    if (APSessionGSData.dlock) {
                        Logging.Log($"[APClient] Waiting for AP save data unlock...");
                        while (APSessionGSData.dlock) {
                            if (SessionStatus<=0) {
                                Logging.Log($"[APClient] Cancelled.");
                                return false;
                            }
                            Thread.Sleep(100);
                        }
                    }
                    APSessionGSData.dlock = true;
                    receivedItemsUnique = new HashSet<APItemData>(new APItemDataComparer(false));
                    //FIXME: receivedItemsUniqueB is for debugging purposes only. Remove when done 
                    receivedItemsUniqueB = new HashSet<APItemData>(new APItemDataComparer(true));
                    for (int i=0; i<ReceivedItems.Count;i++) {
                        APItemData item = ReceivedItems[i];
                        if (!receivedItemCounts.ContainsKey(item.Id))
                            receivedItemCounts[item.Id] = 1;
                        else 
                            receivedItemCounts[item.Id]++;
                        if (item.State==0) {
                            QueueItem(item, i);
                        }
                        else if (itemApplyIndex < item.State) {
                            itemApplyIndex = item.State;
                        }
                        if (item.Location>=0) receivedItemsUnique.Add(item);
                        if (item.Location>=0) receivedItemsUniqueB.Add(item);
                    }
                    APSessionGSData.dlock = false;

                    Logging.Log($"[APClient] Catching up...");
                    CatchUpChecks();
                } catch (Exception e) {
                    Logging.LogError($"[APClient] Exception: {e.Message}");
                    Logging.LogError(e.ToString(), LoggingFlags.Debug);
                    CloseArchipelagoSession(resetOnFail);
                    SessionStatus = -8;
                    return false;
                }

                SessionStatus = 9;

                LogQueueItemCounts();

                Enabled = true;
                SessionStatus = 1;

                session.SetClientState(ArchipelagoClientState.ClientConnected);

                Logging.Log($"[APClient] Done!");
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

                Logging.LogError(errorMessage);

                if (resetOnFail) Reset();
                SessionStatus = -1;
                return false;
            }
        }
        public static void ReconnectArchipelagoSession() {
            ThreadPool.QueueUserWorkItem(_ => {
                int chances = RECONNECT_MAX_RETRIES;
                while (chances<0||chances>0) {
                    Logging.Log($"[APClient] Waiting {RECONNECT_RETRY_WAIT}...");
                    Thread.Sleep(RECONNECT_RETRY_WAIT);
                    Logging.Log("[APClient] Reconnecting...");
                    bool result = ConnectArchipelagoSession(false);
                    if (result) return;
                    if (chances>0) chances--;
                }
                Logging.LogError("[APClient] Failed to reconnect!");
            });
        }
        public static bool CloseArchipelagoSession(bool reset = true) {
            bool res = false;
            Enabled = false;
            if (session!=null) {
                if (session.Socket.Connected) {
                    Logging.Log($"[APClient] Disconnecting APSession...");
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
            else Logging.LogWarning("[APClient] Cannot Reset an active session. Close the session first! ");
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
            SlotData = null;
            currentReceivedItemIndex = 0;
            doneChecksUnique = null;
            receivedItemsUnique = null;
            receivedItemsUniqueB = null;
            receivedItemCounts = new();
            scoutMapStatus = 0;
            locMap.Clear();
            itemMap.Clear();
            deathLinkService = null;
            receivedItemsQueueLock = false;
            receivedItemsQueue = new();
            itemApplyQueue = new();
            itemApplyLevelQueue = new();
            APSettings.Init();
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
            if (!LocationExists(loc)) {
                Logging.LogWarning($"[APClient] Location {loc} is missing. Skipping.");
                return false;
            }
            string locName = LocationExists(loc) ? locMap[loc].LocationName : $"#{loc}";
            Logging.Log($"[APClient] Adding check \"{locName}\"...");
            //Logging.Log(doneChecksUnique.Count);
            //Logging.Log(DoneChecks.Count);
            if (!doneChecksUnique.Contains(loc)) {
                doneChecksUnique.Add(loc);
                DoneChecks.Add(loc);
                //APData.SaveCurrent();
                if (sendChecks) SendChecks();
            } else {
                Logging.Log($"[APClient] \"{locName}\" is already checked.");
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
        public static bool LocationExists(long loc) => locMap.ContainsKey(loc);
        public static ScoutedItemInfo GetCheck(long loc) {
            if (LocationExists(loc))
                return locMap[loc];
            else
                throw new KeyNotFoundException($"[APClient] GetCheck: Invalid location id {loc}.");
        }
        public static APItemInfo GetItemInfo(long item) {
            if (itemMap.ContainsKey(item))
                return itemMap[item];
            else {
                Logging.LogWarning($"[APClient] GetItemInfo: No information on item id {item}. Item must not exist in this world!");
                return null;
            }
        }
        public static string GetItemName(long item) => GetItemInfo(item)?.Name ?? $"APItem {item}";
        public static APItemData GetItemFromLocation(long loc) {
            if (!Enabled) { 
                Logging.LogWarning("[APClient] Client must be enabled for GetLocalItem to function.");
                return null;
            }
            ScoutedItemInfo item = GetCheck(loc);
            return new APItemData(item.ItemId, loc, APSessionPlayerName);
        }
        public static void SendChecks() {
            if (DoneChecks.Count<1) return;
            long[] locs = DoneChecks.ToArray();
            if (session.Socket.Connected) {
                Logging.Log($"[APClient] Sending Checks...");
                if (!sending) {
                    sending = true;
                    ThreadPool.QueueUserWorkItem(_ => SendChecksThread(locs));
                }
                else
                    Logging.LogWarning("[APClient] Already sending checks.");
                UpdateGoal();
                Logging.Log($"[APClient] Done");
            }
            else {
                Logging.Log($"[APClient] Disconnected. Cannot send check. Will retry after connecting.");
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
                Logging.LogWarning($"[APClient] Failed to send checks! {e.Message}");
            }
            LoggingFlags loggingFlags = LoggingFlags.Network | (state?LoggingFlags.Info:LoggingFlags.Warning);
            if (Logging.IsLoggingFlagsEnabled(loggingFlags)) {
                string locsstr = "[";
                for (int i=0;i<locs.Length;i++) {
                    if (i>0) locsstr += ",";
                    locsstr += locMap.ContainsKey(locs[i])?locMap[locs[i]].LocationName:locs[i];
                    if (i==locs.Length-1) locsstr += "]";
                }
                if (state) Logging.Log($"[APClient] Location(s) {locsstr} send success", loggingFlags);
                else Logging.LogWarning($"[APClient] Location(s) {locsstr} send failure", loggingFlags);
            }
            sending = false;
            return state;
        }

        public static void GoalComplete(Goals goal) {
            APSessionGSData.AddGoals(goal);
            UpdateGoal();
        }
        public static bool IsAPGoalComplete() {
            Goals goals = APSettings.Mode switch {
                GameMode.CollectContracts => Goals.Contracts,
                GameMode.DlcBeatSaltbaker => Goals.Saltbaker,
                GameMode.DlcBeatBoth => Goals.DevilAndSaltbaker,
                GameMode.DlcCollectIngradients => Goals.Ingredients,
                GameMode.DlcCollectBoth => Goals.ContractsAndIngredients,
                GameMode.BuyOutShop => Goals.ShopBuyout,
                _ => Goals.Devil,
            };
            return APSessionGSData.IsGoalsCompleted(goals);
        }

        private static void OnMessageReceived(LogMessage message) {
            Logging.Log($"[Archipelago] {message}");
        }
        private static void OnError(Exception e, string message) {
            Logging.LogError($"[APClient] {message}: {e}");
        }
        private static void OnSocketClosed(string reason) {
            Logging.Log("[APClient] Disconnected.");
            if (reason.Length>0)
                Logging.Log($"[APClient] Disconnect Reason: {reason}", LoggingFlags.Network);
            if (Enabled) {
                ReconnectArchipelagoSession();
            }
        }
        private static void OnItemReceived(ReceivedItemsHelper helper) {
            Logging.Log("[APClient] OnItemReceived");
            Logging.Log($"[APClient] CIIndex: {currentReceivedItemIndex}; SIIndex: {ReceivedItemsIndex}; RIIndex: {helper.Index} ItemCount: {helper.AllItemsReceived.Count}");
            try {
                bool recover = helper.AllItemsReceived.Count > ReceivedItemsIndex;
                if (recover) Logging.Log("[APClient] In Item Recovery");
                ItemInfo item = helper.PeekItem();
                if (!itemMap.ContainsKey(item.ItemId)) {
                    itemMap.Add(item.ItemId, new APItemInfo(item.ItemId, item.ItemName, item.Flags));
                }
                long itemId = item.ItemId;
                string itemName = GetItemName(itemId);
                if (currentReceivedItemIndex>=ReceivedItemsIndex || recover) {
                    //Logging.Log($"[APClient] Receiving {itemName} from {item.Player}...");
                    Logging.Log($"[APClient] Receiving {itemName} from {item.Player} ({item.LocationDisplayName})...");
                    APItemData nitem = new APItemData(item);
                    if (!receivedItemsQueueLock) {
                        receivedItemsQueueLock = true;
                        receivedItemsQueue.Enqueue(nitem);
                        currentReceivedItemIndex++;
                        receivedItemsQueueLock = false;
                    } else {
                        Logging.Log("[APClient] Item Queue is locked. Will try again next time.");
                    }
                } else {
                    Logging.Log($"Skipping {itemName}");
                    currentReceivedItemIndex++;
                }
                helper.DequeueItem();
            } catch (Exception e) {
                Logging.LogError($"[APClient] Error receiving item: {e.Message}");
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
            ReceiveItemBasic(item);
            Logging.Log($"[APClient] Received {GetItemName(item.Id)} from {item.Player} ({item.Location})");
            QueueItem(item);
            
            /*if (!receivedItemsUnique.Contains(item)) {
                ReceivedItems.Add(item);
                if (item.Location>=0) receivedItemsUnique.Add(item);
                Logging.Log($"[APClient] Received {GetItemName(item.Id)} from {item.Player} ({item.Location})");
                QueueItem(item);
            }
            else {
                Logging.Log($"[APClient] Item {GetItemName(item.Id)} from {item.Player} at Loc:{item.Location} (Hash: {item.GetHashCode(false)}) already exists. Skipping.");
            }*/
        }
        internal static void ReceiveItemImmediate(APItemData item) {
            ReceiveItemBasic(item);
            Logging.Log($"[APClient] Received {GetItemName(item.Id)} from {item.Player} ({item.Location}). Applying immediately!");
            bool success = APItemMngr.ApplyItem(item);
            if (!success) {
                Logging.LogWarning($"[APClient] Failed to apply {GetItemName(item.Id)} immediately!");
                return;
            }
            item.State = ++itemApplyIndex;
        }
        private static void ReceiveItemBasic(APItemData item) {
            if (item.Location>=0) {
                if (!receivedItemsUnique.Contains(item)) receivedItemsUnique.Add(item);
                else Logging.LogWarning($"[APClient] Item {GetItemName(item.Id)} from {item.Player} at Loc:{item.Location} (Hash: {item.GetHashCode(false)}) already exists.");
                //FIXME: For debugging purposes. Remove later
                if (!receivedItemsUniqueB.Contains(item)) receivedItemsUniqueB.Add(item);
                else Logging.LogWarning($"[APClient] {{B}} Item {GetItemName(item.Id)} from {item.Player} at Loc:{item.Location} (Hash: {item.GetHashCode(false)}) already exists.");
            }
            ReceivedItems.Add(item);
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
            Logging.Log("[APClient] Queue Push");
            LogQueueItemCounts();
        }
        internal static void LogQueueItemCounts() {
            Logging.Log($"[APClient] Current ItemQueue Counts: {itemApplyQueue.Count}, {itemApplyLevelQueue.Count}"); //, LoggingFlags.Debug
        }
        public static APItemData GetReceivedItem(int index) {
            if (index >= 0 && index < ReceivedItems.Count) { 
                return ReceivedItems[index];
            } else {
                throw new IndexOutOfRangeException($"[APClient] Index Out of Range! i:{index} C:{ReceivedItems.Count}");
            }
        }
        public static int GetReceivedItemCount(long itemId) {
            if (receivedItemCounts.ContainsKey(itemId)) return receivedItemCounts[itemId];
            return 0;
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
                Logging.Log("[APClient] Queue Pop");
                itemQueue.Dequeue();
                Logging.Log($"[APClient] Current ItemQueue Counts: {itemApplyQueue.Count}, {itemApplyLevelQueue.Count}");
                return item;
            } else {
                throw new IndexOutOfRangeException($"[APClient] Index Out of Range! i:{index} C:{ReceivedItems.Count}");
            }
        }

        private static void OnDeathLinkReceived(DeathLink deathLink) {
            Logging.Log($"[APClient] DeathLink: {deathLink.Cause}");
            Logging.Log($"[APClient] Death received from {deathLink.Source}");
            if (APManager.Current!=null) {
                Logging.Log($"[APClient] Commencing...");
                APManager.Current.TriggerDeath(deathLink.Cause);
                Logging.Log($"[APClient] Enjoy your death!");
            }
            else {
                Logging.Log($"[APClient] Death avoided because level conditions were not met.");
            }
        }
        public static bool IsDeathLinkActive() => deathLinkService != null;
        public static void SendDeathLink(string cause=null, DeathLinkCauseType causeType=DeathLinkCauseType.Normal) {
            if (!IsDeathLinkActive()) return;
            Logging.Log("[APClient] Sharing your death...");
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
            Logging.Log($"[APClient] Your message: \"{causeMessage}\"");
            DeathLink death = new DeathLink(APSessionPlayerName, causeMessage);
            ThreadPool.QueueUserWorkItem(_ => SendDeathLinkThread(death));
        }

        private static bool SendDeathLinkThread(DeathLink death) {
            bool state = false;
            try {
                deathLinkService.SendDeathLink(death);
                Logging.Log("[APClient] Shared. They are enjoying your death probably...");
                state = true;
            } catch (ArchipelagoSocketClosedException e) {
                Logging.LogWarning($"[APClient] Failed to share your death! {e.Message}");
            }
            return state;
        }

        private static void OnPacketReceived(ArchipelagoPacketBase packet) {
            Logging.Log($"Packet got: {packet.PacketType}");
            switch (packet.PacketType) {
                /*case ArchipelagoPacketType.DataPackage: {
                    DataPackagePacket datapackagepkt = (DataPackagePacket)packet;
                    dataPackage = datapackagepkt.DataPackage;
                    break;
                }*/
                case ArchipelagoPacketType.Connected: {
                    RunScout();
                    break;
                }
                default: break;
            }
        }

        private static void RunScout() {
            if (scoutMapStatus!=1) {
                Logging.Log($"[APClient] Getting location data...");
                session.Locations.ScoutLocationsAsync((Dictionary<long, ScoutedItemInfo> si) => {
                    Logging.Log($" [APClient] Processing {si.Count} locations...");
                    locMap.Clear();
                    bool err = false;
                    try {
                        foreach (ScoutedItemInfo item in si.Values) {
                            if (SessionStatus<0) {
                                Logging.LogError($" [APClient] Aborted due to session error.");
                                locMap.Clear();
                                return;
                            }
                            long loc = item.LocationId;
                            string locName = item.LocationName;

                            bool loc_cond = APLocation.IdExists(loc);
                            if (!loc_cond) {
                                err = true;
                                Logging.LogError($" [APClient] Setup: Unknown Location: {locName??"MISSINGNAME"}:{loc}");
                            }
                                
                            Logging.Log($"Adding: {loc} {item.ItemId}", LoggingFlags.Debug);
                            locMap.Add(loc, item);

                            //if (item.Flags==ItemFlags.Advancement) Logging.Log($"{item.LocationName}: {item.ItemName} for {item.Player}");
                        }
                        if (err) Logging.LogError(" [APClient] Setup: Missing Locations! Make sure that your settings and apworld version are compatible with this client!");
                    } catch (Exception e) {
                        err = true;
                        Logging.LogError($" [APClient] Exception: {e.Message}");
                    }
                    if (locMap.Count<1) {
                        scoutMapStatus = -1;
                        Logging.LogError(" [APClient] scoutMap is empty!");
                        return;
                    }
                    if (err) {
                        scoutMapStatus = -2;
                        Logging.LogError(" [APClient] Errors occured during processing.");
                        return;
                    }
                    Logging.Log($" [APClient] Processed location data.");
                    if ((debug&4)>0) {
                        Logging.Log(" -- Location data dump: --");
                        foreach (KeyValuePair<long, ScoutedItemInfo> entry in locMap) {
                            Logging.Log($"  {entry.Key}: {entry.Value.ItemId}: {entry.Value.LocationId}: {entry.Value.ItemName}: {entry.Value.LocationName}");
                        }
                        Logging.Log(" -- End Location data dump --");
                    }
                    scoutMapStatus = 1;
                }, session.Locations.AllLocations.ToArray());
            } else scoutMapStatus = 1;
        }

        private static APData GetAPSessionData() {
            if (session!=null&&APSessionGSDataSlot>=0) {
                return APData.SData[APSessionGSDataSlot];
            }
            else {
                Logging.LogError("[APClient] Cannot get APSessionData");
                return null;
            }
        }
    }
}
