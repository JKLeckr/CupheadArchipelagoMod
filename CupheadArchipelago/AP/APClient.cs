/// Copyright 2025-2026 JKLeckr
/// SPDX-License-Identifier: GPL-3.0-or-later

using System;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Archipelago.MultiClient.Net;
using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Packets;
using Archipelago.MultiClient.Net.Models;
using Archipelago.MultiClient.Net.Helpers;
using Archipelago.MultiClient.Net.MessageLog.Messages;
using Archipelago.MultiClient.Net.Exceptions;
using Archipelago.MultiClient.Net.BounceFeatures.DeathLink;
using CupheadArchipelago.Mapping;
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
        public static int CompatBits { get; private set; } = 0;
        internal static APSlotData SlotData { get; private set; } = null;
        private static bool offline = false;
        private static Dictionary<long, ScoutedItemInfo> locMap = new();
        private static Dictionary<long, APItemInfo> itemMap = new();
        public static PlayerInfo APSessionPlayerInfo { get; private set; } = null;
        private static List<APItemData> receivedItems = [];
        private static List<APItemData> ReceivedItems { get => receivedItems; }
        private static long ReceivedItemsCount { get => ReceivedItems.Count; }
        private static List<long> DoneChecks { get => APSessionGSData.doneChecks; }
        private static HashSet<long> doneChecksUnique;
        private static HashSet<APItemData> receivedItemsUnique;
        private static Dictionary<long, int> receivedItemCounts = [];
        private static bool receivedItemsQueueLockA = false;
        private static bool receivedItemsQueueLockB = false;
        private static Queue<APItemData> receivedItemsQueue = new();
        private static Queue<int> itemApplyQueue = new();
        private static Queue<int> itemApplyLevelQueue = new();
        private static Queue<int> itemApplySpecialLevelQueue = new();
        private static int receivedItemIndex = 0;
        private static int itemApplyIndex = 0;
        private static int scoutMapStatus = 0;
        private static bool sending = false;
        private static bool reconnecting = false;
        private static int shownMessage = 0;
        private static DeathLinkService deathLinkService = null;
        private static readonly byte debug = 0;
        private static readonly Version AP_CLIENT_VERSION = new(0,6,7,0);
        protected const int STATUS_READY = 1;
        protected const string GAME_NAME = "Cuphead";
        private const int RECONNECT_MAX_RETRIES = 3;
        private const int RECONNECT_RETRY_WAIT = 5000;

        public delegate ArchipelagoSession ArchipelagoSessionCreate(string hostname, int port);

        private static bool ArchipelagoSessionPre() {
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
            return true;
        }

        private static void SetupArchipelagoSession(int index) {
            APSessionGSDataSlot = index;
            session.MessageLog.OnMessageReceived += OnMessageReceived;
            session.Socket.ErrorReceived += OnError;
            session.Socket.SocketClosed += OnSocketClosed;
            session.Items.ItemReceived += OnItemReceived;
            session.Socket.PacketReceived += OnPacketReceived;
            session.Socket.SocketOpened += () => {Logging.Log("Socket Opened");};
        }

        public static bool CreateAndStartArchipelagoSession(int index) =>
            CreateAndStartArchipelagoSession(index, ArchipelagoSessionFactory.CreateSession);

        public static bool CreateAndStartArchipelagoSession(int index, ArchipelagoSessionCreate sessionCreate) {
            if (!ArchipelagoSessionPre()) return false;

            bool res = false;

            APData data = APData.SData[index];
            session = sessionCreate(data.address, data.port);
            APSessionPlayerName = data.player;
            SetupArchipelagoSession(index);

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
                result = session.TryConnectAndLogin("Cuphead", data.player, ItemsHandlingFlags.AllItems, AP_CLIENT_VERSION, null, null, passwd); //FIXME: Use better Item Handling Later
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
                    string worldVersion = APSlotData.GetAPWorldVersionString(loginData.SlotData);
                    Logging.Log($"[APClient] APWorld version {worldVersion}");
                    if (slotDataVersion > APSlotData.AP_SLOTDATA_VERSION || slotDataVersion < APSlotData.AP_SLOTDATA_MIN_VERSION) {
                        Logging.LogError($"[APClient] Incompatible SlotData version: Client:{APSlotData.AP_SLOTDATA_VERSION}, Server:{slotDataVersion}! Incompatible client!");
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

                SessionStatus = 4;

                try {
                    Logging.Log($"[APClient] Getting AP Data...");
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
                    if (APSessionGSData.seed != "" && (!APSessionGSData.IsOverridden(Overrides.SeedMismatchOverride) || Enabled)) {
                        Logging.LogError("[APClient] Seed mismatch! Are you connecting to a different multiworld?");
                        CloseArchipelagoSession(resetOnFail);
                        SessionStatus = -5;
                        return false;
                    }
                    APSessionGSData.seed = seed;
                }

                //Logging.Log($"Seed: {seed}");

                SessionStatus = 6;

                Logging.Log($"[APClient] Checking client compatibility...");
                //Check feature bits here

                SessionStatus = 7;

                if (scoutMapStatus==0) Logging.Log($"[APClient] Waiting for location scout...");
                while (scoutMapStatus==0) {
                    Thread.Sleep(100);
                }
                if (scoutMapStatus<0) {
                    Logging.LogError($"[APClient] Scout failed!");
                    SessionStatus = -7;
                    return false;
                }

                SessionStatus = 8;

                try {
                    Logging.Log($"[APClient] Checking settings...");
                    Logging.Log($"DLC is {(DLCManager.DLCEnabled() ? "Enabled" : "Disabled")}");
                    if (SlotData.use_dlc && !DLCManager.DLCEnabled()) {
                        if (!APSessionGSData.IsOverridden(Overrides.DlcOverride)) {
                            Logging.LogError($"[APClient] Content Mismatch! Server requires DLC, but running on a non-DLC client!");
                            CloseArchipelagoSession(resetOnFail);
                            SessionStatus = -8;
                            return false;
                        } else {
                            Logging.LogWarning($"[APClient] Content Mismatch! Server requires DLC, but running on a non-DLC client! Override enabled, continuing...");
                        }
                    }

                    SessionStatus = 9;

                    Logging.Log($"[APClient] Applying settings...");
                    if (!Enabled) APSettings.Init();
                    APSettings.UseDLC = SlotData.use_dlc;
                    APSettings.Mode = SlotData.mode;
                    APSettings.Hard = SlotData.expert_mode;
                    APSettings.StartWeapon = SlotData.start_weapon;
                    APSettings.WeaponMode = SlotData.weapon_mode;
                    APSettings.FreemoveIsles = SlotData.freemove_isles;
                    APSettings.RandomizeAbilities = SlotData.randomize_abilities;
                    APSettings.BossPhaseChecks = SlotData.boss_phase_checks;
                    APSettings.BossSecretChecks = LocationExists(APLocation.level_boss_veggies_secret);
                    APSettings.BossGradeChecks = SlotData.boss_grade_checks;
                    APSettings.DicePalaceBossSanity = LocationExists(APLocation.level_dicepalace_boss_booze);
                    APSettings.RungunGradeChecks = SlotData.rungun_grade_checks;
                    APSettings.QuestPacifist = LocationExists(APLocation.quest_pacifist);
                    APSettings.QuestProfessional = LocationExists(APLocation.quest_silverworth);
                    APSettings.QuestJuggler = LocationExists(APLocation.quest_buster);
                    APSettings.StartMaxHealth = SlotData.start_maxhealth;
                    APSettings.StartMaxHealthP2 = SlotData.start_maxhealth_p2;
                    APSettings.RequiredContracts = SlotData.contract_requirements;
                    APSettings.DLCRequiredIngredients = SlotData.dlc_ingredient_requirements;
                    APSettings.ContractsGoal = SlotData.contract_goal_requirements;
                    APSettings.DLCIngredientsGoal = SlotData.dlc_ingredient_goal_requirements;
                    APSettings.DLCChaliceMode = SlotData.dlc_chalice;
                    APSettings.DLCBossChaliceChecks = SlotData.dlc_boss_chalice_checks;
                    APSettings.DLCRunGunChaliceChecks = SlotData.dlc_rungun_chalice_checks;
                    APSettings.DLCCurseMode = SlotData.dlc_curse_mode;
                    APSettings.ShuffleMusic = SlotData.music_shuffle;
                    APSettings.DuckLockPlatDropBug = SlotData.ducklock_platdrop;
                    APSettings.DeathLink =
                        (APSessionGSData.IsOverridden(Overrides.OverrideEnableDeathLink) ? !SlotData.deathlink : SlotData.deathlink) ?
                        DeathLinkMode.Normal : DeathLinkMode.Disabled;
                    APSettings.DeathLinkGraceCount = SlotData.deathlink_grace_count;

                    ShopMap.SetShopMap(SlotData.shop_map);

                    /*Logging.Log($"[APClient] Checking for needed workarounds...");
                    if (DLCManager.DLCEnabled() && !LocationExists(APLocation.level_dlc_boss_airplane_secret)) {
                        Logging.LogWarning($"[APClient] Airplane secret location missing, applying workaround...");
                        CompatBits |= 1;
                    }*/

                    Logging.Log($"[APClient] Setting up game...");
                    doneChecksUnique = new(DoneChecks);
                    LevelMap.Init(SlotData.level_map);
                    Weapon startWeapon = ItemMap.GetWeapon(APSettings.StartWeapon.id);
                    if (APSettings.WeaponMode == WeaponModes.Normal) {
                        uint upgradeBit = (uint)WeaponParts.All;
                        APSessionGSPlayerData.AddWeaponsBit(ItemMap.GetModularWeapons(), upgradeBit);
                        APSessionGSPlayerData.plane_ex = true;
                        APSessionGSPlayerData.dlc_cplane_ex = true;
                    }
                    else if ((APSettings.WeaponMode & WeaponModes.ExceptStart) > 0) {
                        uint supgradeBit = (uint)WeaponParts.All;
                        APSessionGSPlayerData.AddWeaponBit(startWeapon, supgradeBit);
                    }
                    else {
                        uint supgradeBit = (uint)WeaponParts.AllBasic;
                        APSessionGSPlayerData.AddWeaponBit(startWeapon, supgradeBit);
                    }
                    if (!APSettings.RandomizeAbilities)
                        APSessionGSPlayerData.SetBoolValues(true, APData.PlayerData.SetTarget.AllAbilities);
                    if (!APSettings.RandomizeAimAbilities)
                        APSessionGSPlayerData.aim_directions = AimDirections.All;
                    if (APSettings.DeathLink > 0) {
                        Logging.Log($"[APClient] Setting up DeathLink...");
                        deathLinkService?.DisableDeathLink();
                        deathLinkService = session.CreateDeathLinkService();
                        deathLinkService.EnableDeathLink();
                        deathLinkService.OnDeathLinkReceived += OnDeathLinkReceived;
                    }
                    receivedItemsUnique = new HashSet<APItemData>(new APItemDataComparer(false));

                    Logging.Log($"[APClient] Catching up...");
                    if (!Enabled) {
                        SendChecksThread(DoneChecks.ToArray(), true);
                        CatchUpChecks();
                    }
                } catch (Exception e) {
                    Logging.LogError($"[APClient] Exception: {e.Message}");
                    Logging.LogError(e.ToString(), LoggingFlags.Debug);
                    CloseArchipelagoSession(resetOnFail);
                    SessionStatus = -9;
                    return false;
                }

                SessionStatus = 10;

                LogQueueItemCounts();

                Enabled = true;
                SessionStatus = 1;

                session.SetClientState(ArchipelagoClientState.ClientPlaying);

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
            if (reconnecting) {
                Logging.Log("Already reconnecting.");
                return;
            }
            if (session.Socket.Connected) {
                Logging.Log("Connected. Not reconnecting.");
                return;
            }
            ThreadPool.QueueUserWorkItem(_ => {
                reconnecting = true;
                int chances = RECONNECT_MAX_RETRIES;
                while (chances<0||chances>0) {
                    Logging.Log($"[APClient] Waiting {RECONNECT_RETRY_WAIT}...");
                    Thread.Sleep(RECONNECT_RETRY_WAIT);
                    Logging.Log("[APClient] Reconnecting...");
                    bool result = ConnectArchipelagoSession(false);
                    if (result) {
                        reconnecting = false;
                        return;
                    }
                    if (SessionStatus < 0) {
                        Logging.LogError("[APClient] Sanity check error! Cannot reconnect!");
                        break;
                    }
                    if (chances>0) chances--;
                }
                Logging.LogError("[APClient] Failed to reconnect!");
                reconnecting = false;
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
            CompatBits = 0;
            SlotData = null;
            doneChecksUnique = null;
            receivedItemIndex = 0;
            itemApplyIndex = 0;
            receivedItemsUnique = null;
            receivedItemCounts.Clear();
            receivedItems.Clear();
            scoutMapStatus = 0;
            shownMessage = 0;
            locMap.Clear();
            itemMap.Clear();
            deathLinkService = null;
            receivedItemsQueueLockA = false;
            receivedItemsQueueLockB = false;
            receivedItemsQueue = new();
            itemApplyQueue = new();
            itemApplyLevelQueue = new();
            itemApplySpecialLevelQueue = new();
            offline = false;
            APSettings.Init();
        }

        // This is mainly for testing. Not feature complete.
        internal static void SetupOffline() {
            offline = true;
            APSessionGSDataSlot = 0;
        }
        internal static void ResetOffline() {
            if (offline) Reset();
        }

        public static bool IsLocationChecked(long loc) => doneChecksUnique.Contains(loc);
        public static bool IsAnyLocationChecked(long[] locs) {
            foreach (long loc in locs) {
                if (IsLocationChecked(loc))
                    return true;
            }
            return false;
        }
        public static bool IsAllLocationsChecked(long[] locs) {
            if (locs.Length<1) return false;
            foreach (long loc in locs) {
                if (IsLocationChecked(loc))
                    return false;
            }
            return true;
        }
        public static bool Check(long loc, bool sendChecks = true, bool logAlreadyChecked = true) {
            if (!LocationExists(loc)) {
                Logging.LogWarning($"[APClient] Location {loc} is missing. Skipping.");
                return false;
            }
            string locName = LocationExists(loc) ? locMap[loc].LocationName : $"#{loc}";
            //Logging.Log(doneChecksUnique.Count);
            //Logging.Log(DoneChecks.Count);
            if (!doneChecksUnique.Contains(loc)) {
                Logging.Log($"[APClient] Adding check \"{locName}\"...");
                doneChecksUnique.Add(loc);
                DoneChecks.Add(loc);
                if (sendChecks) SendChecks(true);
            } else {
                if (logAlreadyChecked) Logging.Log($"[APClient] \"{locName}\" is already checked.");
            }
            return true;
        }
        public static void Check(long[] locs, bool sendChecks = true) {
            foreach (long loc in locs) {
                Check(loc, false);
            }
            if (sendChecks) SendChecks(true);
        }
        public static void CatchUpChecks() {
            if (session.Socket.Connected) {
                foreach (long cloc in session.Locations.AllLocationsChecked) {
                    Check(cloc, false, Logging.IsDebugEnabled());
                }
            }
        }
        private static IEnumerator CatchUpChecks_cr() {
            while (!session.Socket.Connected || sending) yield return null;
            IEnumerable<long> allLocsChecked = session.Locations.AllLocationsChecked;
            foreach (long cloc in allLocsChecked) {
                while (sending) yield return null;
                Check(cloc, false, Logging.IsDebugEnabled());
                yield return null;
            }
            yield break;
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
        public static void SendChecks(bool sendGoal) {
            if (DoneChecks.Count<1) return;
            Logging.LogDebug("SendChecks");
            long[] locs = DoneChecks.ToArray();
            if (session.Socket.Connected) {
                Logging.Log($"[APClient] Sending Checks...");
                if (Logging.IsDebugEnabled()) {
                    string locsstr = "[";
                    for (int i=0;i<locs.Length;i++) {
                        if (i>0) locsstr += ",";
                        locsstr += locMap.ContainsKey(locs[i])?locMap[locs[i]].LocationName:locs[i];
                        if (i==locs.Length-1) locsstr += "]";
                    }
                    Logging.LogDebug($"[APClient] Sending locations: {locsstr}");
                }
                if (!sending) {
                    sending = true;
                    ThreadPool.QueueUserWorkItem(_ => SendChecksThread(locs, sendGoal));
                }
                else {
                    Logging.LogWarning("[APClient] Already sending something.");
                }
            }
            else {
                Logging.Log($"[APClient] Disconnected. Cannot send check. Will retry after connecting.");
                ReconnectArchipelagoSession();
            }
        }
        private static bool SendChecksThread(long[] locs, bool sendGoal) {
            bool state = false;
            Logging.LogDebug("SendChecksThread");
            try {
                session.Locations.CompleteLocationChecks(locs);
                state = true;
                if (sendGoal && IsAPGoalComplete()) {
                    state = SendGoalThread();
                }
                Logging.Log($"[APClient] Successfully sent checks.");
            }
            catch (ArchipelagoSocketClosedException e) {
                Logging.LogWarning($"[APClient] Failed to send checks! {e.Message}");
            }
            catch (Exception e) {
                Logging.LogError($"[APClient] Failed to send checks! Exception: {e}");
            }
            sending = false;
            return state;
        }
        public static void UpdateGoalFlags() {
            if ((APSettings.Mode & GameModes.CollectContracts) > 0) {
                Logging.Log($"Contracts: {APSessionGSPlayerData.contracts}");
                Logging.Log($"Contracts Goal: {APSettings.ContractsGoal}");
                if (APSessionGSPlayerData.contracts >= APSettings.ContractsGoal) {
                    GoalComplete(Goals.Contracts, true);
                }
            }
            if ((APSettings.Mode & GameModes.DlcCollectIngredients) > 0) {
                Logging.Log($"Ingredients: {APSessionGSPlayerData.dlc_ingredients}");
                Logging.Log($"Ingredients Goal: {APSettings.DLCIngredientsGoal}");
                if (APSessionGSPlayerData.dlc_ingredients >= APSettings.DLCIngredientsGoal) {
                    GoalComplete(Goals.Ingredients, true);
                }
            }
        }
        public static void SendGoal() {
            Logging.LogDebug("SendGoal");
            if (IsAPGoalComplete()) {
                if (session.Socket.Connected) {
                    Logging.Log($"[APClient] Sending Goal...");
                    if (!sending) {
                        sending = true;
                        ThreadPool.QueueUserWorkItem(_ => SendGoalThread());
                    }
                    else {
                        Logging.LogWarning("[APClient] Already sending something.");
                    }
                }
                else {
                    Logging.Log($"[APClient] Disconnected. Cannot send goal. Will retry after connecting.");
                    ReconnectArchipelagoSession();
                }
            }
            else {
                Logging.Log($"[APClient] Goal is not complete. Not sending.");
            }
        }
        private static bool SendGoalThread() {
            bool state = false;
            Logging.LogDebug("SendGoalThread");
            try {
                StatusUpdatePacket statusUpdate = new() { Status = ArchipelagoClientState.ClientGoal };
                session.Socket.SendPacket(statusUpdate);
                state = true;
                Logging.Log($"[APClient] Successfully sent goal.");
            } catch (ArchipelagoSocketClosedException e) {
                Logging.LogWarning($"[APClient] Failed to send goal! {e.Message}");
            } catch (Exception e) {
                Logging.LogError($"[APClient] Failed to send goal! Exception: {e}");
            }
            sending = false;
            return state;
        }

        public static void GoalComplete(Goals goal, bool sendGoal = false, bool force = false) {
            if (!APSessionGSData.AreGoalsCompleted(goal) || force) {
                Logging.Log($"[APClient] Adding Goal Flag {goal}");
                APSessionGSData.AddGoals(goal);
            }
            else {
                Logging.Log($"[APClient] Goal Flag exists: {goal}");
            }
            if (sendGoal) SendGoal();
        }
        public static bool AreGoalsCompleted(Goals goals) =>
            APSessionGSData.AreGoalsCompleted(goals);
        public static bool IsAPGoalComplete() {
            Goals goals = APSettings.Mode switch {
                GameModes.CollectContracts => Goals.Contracts,
                GameModes.DlcBeatSaltbaker => Goals.Saltbaker,
                GameModes.DlcBeatBoth => Goals.DevilAndSaltbaker,
                GameModes.DlcCollectIngredients => Goals.Ingredients,
                GameModes.DlcCollectBoth => Goals.ContractsAndIngredients,
                GameModes.BuyOutShop => Goals.ShopBuyout,
                _ => Goals.Devil,
            };
            Logging.LogDebug($"Goals to check: {goals}");
            return APSessionGSData.AreGoalsCompleted(goals);
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
            // TODO: Reduce debug message as this is proven to be stable
            Logging.Log($"[APClient] ItemReceived: RIIndex: {receivedItemIndex}; HIIndex: {helper.Index} ItemCount: {ReceivedItemsCount} HItemCount: {helper.AllItemsReceived.Count}");
            try {
                ItemInfo item = helper.PeekItem();
                if (!itemMap.ContainsKey(item.ItemId)) {
                    itemMap.Add(item.ItemId, new APItemInfo(item.ItemId, item.ItemName, item.Flags));
                }
                long itemId = item.ItemId;
                string itemName = GetItemName(itemId);
                if (helper.Index > receivedItemIndex) {
                    //Logging.Log($"[APClient] Receiving {itemName} from {item.Player}...");
                    Logging.Log($"[APClient] Receiving {itemName} from {item.Player} ({item.LocationDisplayName})...");
                    APItemData nitem = new(item);
                    receivedItemsQueueLockA = true;
                    if (receivedItemsQueueLockB) {
                        Logging.Log("[APClient] Waiting for queue lock...");
                        int timeout = 0;
                        while (receivedItemsQueueLockB) {
                            if (timeout > 500) {
                                Logging.LogError("[APClient] Queue Lock Timeout!");
                                receivedItemsQueueLockA = false;
                                return;
                            }
                            Thread.Sleep(20);
                            timeout++;
                        }
                    }
                    receivedItemsQueue.Enqueue(nitem);
                    receivedItemIndex++;
                    receivedItemsQueueLockA = false;
                }
                else {
                    Logging.Log($"Skipping {itemName}");
                }
                helper.DequeueItem();
            }
            catch (Exception e) {
                Logging.LogError($"[APClient] Error receiving item: {e.Message}");
                return;
            }
        }

        public static void ItemUpdate() {
            if (!receivedItemsQueueLockA && !APSessionGSData.dlock) {
                if (receivedItemsQueue.Count > 0) {
                    receivedItemsQueueLockB = true;
                    if (receivedItemsQueueLockA) {
                        receivedItemsQueueLockB = false;
                        if ((shownMessage & 1) == 0) {
                            Logging.Log("[APClient] Item Queue is locked. Trying when it unlocks.");
                            shownMessage |= 1;
                        }
                        return;
                    }
                    shownMessage &= ~1;
                    if (APSessionGSData.dlock) {
                        if ((shownMessage & 2) == 0) {
                            Logging.Log("[APClient] APData is locked. Trying when it unlocks.");
                            shownMessage |= 2;
                        }
                        return;
                    }
                    APSessionGSData.dlock = true;
                    APItemData item = receivedItemsQueue.Dequeue();
                    receivedItemsQueueLockB = false;
                    shownMessage &= ~2;
                    ReceiveItem(item);
                    APSessionGSData.dlock = false;
                }
            }
        }

        internal static void ReceiveItem(APItemData item) {
            ReceiveItemBasic(item);
            Logging.Log($"[APClient] Received {GetItemName(item.id)} from {item.player} ({item.location})");
            QueueItem(item);
        }
        internal static void ReceiveItemImmediate(APItemData item) {
            ReceiveItemBasic(item);
            Logging.Log($"[APClient] Received {GetItemName(item.id)} from {item.player} ({item.location}). Applying immediately!");
            bool success = APItemMngr.ApplyItem(item);
            if (!success) {
                Logging.LogWarning($"[APClient] Failed to apply {GetItemName(item.id)} immediately!");
                return;
            }
            item.State = ++itemApplyIndex;
        }
        private static void ReceiveItemBasic(APItemData item) {
            if (item.location>=0) {
                if (!receivedItemsUnique.Contains(item)) receivedItemsUnique.Add(item);
                else Logging.LogWarning($"[APClient] Item {GetItemName(item.id)} from {item.player} at Loc:{item.location} (Hash: {item.GetHashCode(false)}) already exists.");
            }
            ReceivedItems.Add(item);
            AddReceivedItemCount(item.id);
        }
        private static void QueueItem(APItemData item) => QueueItem(item, ReceivedItems.Count-1);
        private static void QueueItem(APItemData item, int itemIndex) {
            if (ItemMap.GetItemType(item.id)==APItemType.Level) {
                QueueItem(itemApplyLevelQueue, itemIndex);
            }
            else if (ItemMap.GetItemType(item.id)==APItemType.SpecialLevel) {
                QueueItem(itemApplySpecialLevelQueue, itemIndex);
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
        private static void AddReceivedItemCount(long itemId, int count = 1) {
            if (!receivedItemCounts.ContainsKey(itemId))
                receivedItemCounts[itemId] = 1;
            else
                receivedItemCounts[itemId] += count;
        }
        public static int GetReceivedItemCount(long itemId) {
            if (receivedItemCounts.ContainsKey(itemId)) return receivedItemCounts[itemId];
            return 0;
        }
        public static int GetReceivedItemGroupCount(APItemGroups.ItemGroup itemGroup) {
            int res = 0;
            foreach (APItem item in APItemGroups.GetItems(itemGroup)) {
                res += GetReceivedItemCount(item);
            }
            return res;
        }
        public static int GetReceivedCoinCount() {
            int res = 0;
            foreach (APItem item in APItemGroups.GetItems(APItemGroups.ItemGroup.Coins)) {
                int value;
                if (item == APItem.coin3) value = 3;
                else if (item == APItem.coin2) value = 2;
                else value = 1;
                res += value * GetReceivedItemCount(item);
            }
            return res;
        }
        public static int GetAppliedItemCount(long itemId) {
            return APSessionGSData.GetAppliedItemCount(itemId);
        }
        internal static void AddAppliedItem(long itemId, int count = 1) {
            APSessionGSData.AddAppliedItem(itemId, count);
        }
        public static bool ItemReceiveQueueIsEmpty() => receivedItemsQueue.Count == 0;
        public static bool ItemApplyQueueIsEmpty() => itemApplyQueue.Count == 0;
        public static bool ItemApplyLevelQueueIsEmpty() => itemApplyLevelQueue.Count == 0;
        public static bool ItemApplySpecialLevelQueueIsEmpty() => itemApplySpecialLevelQueue.Count == 0;
        public static int ItemReceiveQueueCount() => receivedItemsQueue.Count;
        public static int ItemApplyQueueCount() => itemApplyQueue.Count;
        public static int ItemApplyLevelQueueCount() => itemApplyLevelQueue.Count;
        public static int ItemApplySpecialLevelQueueCount() => itemApplySpecialLevelQueue.Count;
        public static APItemData PeekItemApplyQueue() => PeekItemQueue(itemApplyQueue);
        public static APItemData PeekItemApplyLevelQueue() => PeekItemQueue(itemApplyLevelQueue);
        public static APItemData PeekItemApplySpecialLevelQueue() => PeekItemQueue(itemApplySpecialLevelQueue);
        private static APItemData PeekItemQueue(Queue<int> itemQueue) {
            int index = itemQueue.Peek();
            if (index >= 0 && index < ReceivedItems.Count) {
                APItemData item = ReceivedItems[index];
                return item;
            } else {
                throw new IndexOutOfRangeException($"[APClient] Index Out of Range! i:{index} C:{ReceivedItems.Count}");
            }
        }
        public static APItemData PopItemApplyQueue(bool applyItem = true) => PopItemQueue(itemApplyQueue, applyItem);
        public static APItemData PopItemApplyLevelQueue(bool applyItem = true) => PopItemQueue(itemApplyLevelQueue, applyItem);
        public static APItemData PopItemApplySpecialLevelQueue(bool applyItem = true) => PopItemQueue(itemApplySpecialLevelQueue, applyItem);
        private static APItemData PopItemQueue(Queue<int> itemQueue, bool applyItem) {
            int index = itemQueue.Peek();
            if (index >= 0 && index < ReceivedItems.Count) {
                APItemData item = ReceivedItems[index];
                if (applyItem) {
                    bool success = APItemMngr.ApplyItem(item);
                    if (!success) return item;
                    item.State = ++itemApplyIndex;
                }
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
                string message = $"{deathLink.Source} plugged you!{(deathLink.Cause == "" ? "" : $" Cause: \"{deathLink.Cause}\"")}";
                APManager.Current.TriggerDeath(message);
                Logging.Log($"[APClient] Enjoy your death!");
            }
            else {
                Logging.Log($"[APClient] Death avoided because level conditions were not met.");
            }
        }
        public static bool IsDeathLinkActive() => deathLinkService != null;
        public static long GetDeathCount() {
            if (!IsDeathLinkActive())
                Logging.LogWarning("[APClient] Death Link is disabled. Death counter is inactive.");
            return APSessionGSData.deathCount;
        }
        public static void SendDeath(string cause = null, DeathLinkCauseType causeType = DeathLinkCauseType.Normal) {
            APSessionGSData.deathCount++;
            Logging.Log($"{APSessionGSData.deathCount} deaths");
            if (!IsDeathLinkActive()) return;
            int remainingGrace = APSettings.DeathLinkGraceCount - (int)(APSessionGSData.deathCount % (APSettings.DeathLinkGraceCount + 1));
            if (remainingGrace != 0) {
                Logging.Log($"[APClient] Remaining DeathLink Grace's: {remainingGrace}...");
                return;
            }
            Logging.Log("[APClient] Sharing your death...");
            string player = APSessionPlayerInfo.Alias;
            string deathTxt = "walloped";
            string chessDeathTxt = "beaten";
            string causeMessage = causeType switch {
                DeathLinkCauseType.Boss => player + " got " + deathTxt + (cause != null ? " by " + cause : "" + "!"),
                DeathLinkCauseType.Mausoleum => player + " failed to protect the Chalice" + (cause != null ? " at " + cause : "" + "!"),
                DeathLinkCauseType.Tutorial => player + " died in a tutorial level!",
                DeathLinkCauseType.ChessCastle => player + " was " + chessDeathTxt + " at The King's Leap" + (cause != null ? " by " + cause : "" + "!"),
                DeathLinkCauseType.Graveyard => player + " got taken for a ride in the graveyard!",
                _ => player + " was " + deathTxt + (cause != null ? " at " + cause : "" + "!"),
            };
            Logging.Log($"[APClient] Your message: \"{causeMessage}\"");
            DeathLink death = new(APSessionPlayerName, causeMessage);
            ThreadPool.QueueUserWorkItem(_ => SendDeathLinkThread(death));
        }

        private static bool SendDeathLinkThread(DeathLink death) {
            bool state = false;
            try {
                deathLinkService.SendDeathLink(death);
                Logging.Log("[APClient] Shared. They are enjoying your death probably...");
                state = true;
            }
            catch (ArchipelagoSocketClosedException e) {
                Logging.LogWarning($"[APClient] Failed to share your death! {e.Message}");
            }
            return state;
        }

        private static void OnPacketReceived(ArchipelagoPacketBase packet) {
            Logging.Log($"Packet got: {packet.PacketType}");
            switch (packet.PacketType) {
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

                            //Logging.Log($"Adding: {loc} {item.ItemId}", LoggingFlags.Debug);
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
            if ((session != null && APSessionGSDataSlot >= 0) || offline) {
                return APData.SData[APSessionGSDataSlot];
            }
            else {
                Logging.LogError("[APClient] Cannot get APSessionData");
                return null;
            }
        }
    }
}
