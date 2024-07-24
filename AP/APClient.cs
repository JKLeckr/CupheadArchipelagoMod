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
using System.Net;
using CupheadArchipelago.Auxiliary;

namespace CupheadArchipelago.AP {
    public class APClient {
        public static ArchipelagoSession APSession {get; private set;}
        public static bool Enabled {get; private set;} = false;
        public static bool Connected {get => SessionStatus>=STATUS_CONNECTED;}
        public static APData APSessionGSData {get => GetAPSessionData();}
        public static string APSessionSlotName {get; private set;} = "";
        public static int APSessionDataSlotNum {get; private set;} = -1;
        public static ConnectedPacket ConnectionInfo {get; private set;}
        public static Dictionary<string, object> SlotData {get => APSessionGSData.slotData;}
        public static Queue<NetworkItem> ItemReceiveQueue {get; private set;}
        public static Queue<NetworkItem> ItemReceiveLevelQueue {get; private set;}
        public static bool IsTryingSessionConnect {get => SessionStatus > 0;}
        public static int SessionStatus { get; private set; } = 0;
        private static List<long> DoneChecks { get => APSessionGSData.doneChecks; }
        private static HashSet<long> doneChecksUnique;
        private static readonly Version AP_VERSION = new Version(0,4,4,0);
        private const int STATUS_CONNECTED = 5;
        private const int RECONNECT_MAX_RETRIES = 3;
        private const int RECONNECT_RETRY_WAIT = 5000;
        
        static APClient() {
            ResetQueues();
        }

        public static bool CreateAndStartArchipelagoSession(int index) {
            if (IsTryingSessionConnect) {
                Plugin.Log($"[APClient] Already Trying to Connect. Aborting.", LogLevel.Error);
                return false;
            }
            SessionStatus = 1;
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

        private static bool ConnectArchipelagoSession() {
            if (SessionStatus>1) {
                Plugin.LogWarning($"[APClient] Already Trying to Connect. Aborting.");
                return false;
            }
            SessionStatus = 2;
            bool res;
            APData data = APData.SData[APSessionDataSlotNum];
            Plugin.Log($"[APClient] Connecting to {data.address} as {data.slot}...");
            LoginResult result;
            try {
                string passwd = data.password.Length>0?data.password:null;
                result = APSession.TryConnectAndLogin("Cuphead", data.slot, ItemsHandlingFlags.IncludeStartingInventory, AP_VERSION, null, null, passwd);
            } catch (Exception e) {
                result = new LoginFailure(e.GetBaseException().Message);
            }

            if (result.Successful)
            {
                Plugin.Log($"[APClient] Connected to {data.address} as {data.slot}");

                LoginSuccessful loginData = (LoginSuccessful)result;
                APSessionGSData.slotData = loginData.SlotData;

                SessionStatus = 3;
                Plugin.Log($"[APClient] Checking seed...");
                string seed = APSession.RoomState.Seed;
                if (APData.CurrentSData.seed != seed) {
                    if (APData.CurrentSData.seed != "") {
                        Plugin.LogError("[APClient] Seed mismatch! Are you connecting to a different multiworld?");
                        CloseArchipelagoSession();
                        SessionStatus = -2;
                        return false;
                    }
                    APData.CurrentSData.seed = seed;
                }

                bool use_dlc = Aux.IntAsBool((int)APSessionGSData.slotData["use_dlc"]);
                Plugin.Log($"[APClient] Checking settings...");
                //TODO: Update this later
                if (DLCManager.DLCEnabled() != use_dlc) { //!DLCManager.DLCEnabled()&&APClient.APSessionSData.UseDLC
                    Plugin.LogError("[APClient] Content Mismatch! Cannot use a non-DLC client on a DLC Archipelago slot!");
                    CloseArchipelagoSession();
                    SessionStatus = -3;
                    return false;
                }

                SessionStatus = 4;

                Plugin.Log($"[APClient] Applying settings...");
                APSettings.UseDLC = use_dlc;
                APSettings.Hard = Aux.IntAsBool((int)APSessionGSData.slotData["expert_mode"]);
                APSettings.FreemoveIsles = Aux.IntAsBool((int)APSessionGSData.slotData["freemove_isles"]);
                APSettings.BossGradeChecks = (GradeChecks)(int)APSessionGSData.slotData["boss_grade_checks"];
                APSettings.RungunGradeChecks = (GradeChecks)(int)APSessionGSData.slotData["rungun_grade_checks"];
                APSettings.DeathLink = Aux.IntAsBool((int)APSessionGSData.slotData["deathlink"]);

                Plugin.Log($"[APClient] Setting up game...");
                doneChecksUnique = new HashSet<long>(APData.SData[APSessionDataSlotNum].doneChecks);

                //TODO: Add randomize client-side stuff

                SessionStatus = 5;

                Enabled = true;
                res = true;
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

                Reset();
                SessionStatus = -1;
                res = false;
            }
            SessionStatus = 0;
            return res;
        }
        public static void ReconnectArchipelagoSession() {
            /* TODO: Have a return result from the success of the connection instead of passively failing */
            Thread reconnectThread = new Thread(() => {
                int chances = RECONNECT_MAX_RETRIES;
                while (chances<0||chances>0) {
                    Plugin.Log($"[APClient] Waiting {RECONNECT_RETRY_WAIT}...");
                    Thread.Sleep(RECONNECT_RETRY_WAIT);
                    Plugin.Log("[APClient] Reconnecting...");
                    bool result = ConnectArchipelagoSession();
                    if (result) return;
                    chances--;
                }
                Plugin.LogError("[APClient] Failed to reconnect!");
            });
            reconnectThread.Start();
        }
        public static bool CloseArchipelagoSession() {
            bool res = false;
            Enabled = false;
            if (APSession!=null) {
                if (APSession.Socket.Connected) {
                    Plugin.Log($"[APClient] Disconnecting APSession...");
                    APSession.Socket.Disconnect();
                    res = true;
                }
            }
            Reset();
            return res;
        }
        private static void Reset() {
            APSession = null;
            SessionStatus = 0;
            APSessionSlotName = "";
            APSessionDataSlotNum = -1;
            ConnectionInfo = null;
            doneChecksUnique = null;
            ResetQueues();
        }

        public static void Check(long loc, bool sendChecks = true) {
            Plugin.Log(string.Format("[APClient] Adding check \"{0}\"...", APLocation.IdToName(loc)));
            Plugin.Log("0");
            Plugin.Log(doneChecksUnique);
            Plugin.Log("1");
            Plugin.Log(DoneChecks);
            Plugin.Log("2");
            if (!doneChecksUnique.Contains(loc)) {
                doneChecksUnique.Add(loc);
                DoneChecks.Add(loc);
                if (sendChecks) SendChecks();
            }
        }
        public static void SendChecks() {
            if (DoneChecks.Count<1) return;
            long[] locs = DoneChecks.ToArray();
            if (APSession.Socket.Connected) {
                APSession.Locations.CompleteLocationChecksAsync((bool state) => OnLocationSendComplete(state, locs), locs);
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

        public static bool IsLocationChecked(long loc) => APSession.Locations.AllLocationsChecked.Contains(loc);

        private static void OnMessageReceived(LogMessage message) {
            Plugin.Log($"[Archipelago] {message}");
        }
        private static void OnError(Exception e, string message) {
            Plugin.Log("[APClient] "+message, LogLevel.Error);
        }
        private static void OnSocketClosed(string reason) {
            Plugin.Log("[APClient] Disconnected.");
            Plugin.Log($"[APClient] Disconnect Reason: {reason}", LoggingFlags.Network);
            if (Enabled) {
                ReconnectArchipelagoSession();
            }
        }
        private static void OnItemReceived(ReceivedItemsHelper helper) {
            NetworkItem item = helper.PeekItem();
            if ((item.Flags&ItemFlags.Trap)>0) {
                ReceiveLevelItem(item);
            }
            else if (item.Item==APItem.extrahealth.Id||item.Item==APItem.superrecharge.Id) {
                ReceiveLevelItem(item);
            }
            else {
                ReceiveItem(item);
            }
            helper.DequeueItem();
        }
        private static void ReceiveItem(NetworkItem item) {
            ItemReceiveQueue.Enqueue(item);
        }
        private static void ReceiveLevelItem(NetworkItem item) {
            ItemReceiveLevelQueue.Enqueue(item);
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
        
        private static void ResetQueues() {
            ItemReceiveQueue = new Queue<NetworkItem>();
            ItemReceiveLevelQueue = new Queue<NetworkItem>();
        }
    }
}
