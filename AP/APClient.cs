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
using BepInEx.Logging;
using UnityEngine;

namespace CupheadArchipelago.AP {
    public class APClient {
        public static ArchipelagoSession APSession {get; private set;}
        public static bool Enabled {get; private set;} = false;
        public static APData APSessionSData {get => GetAPSessionData();}
        public static string APSessionSlotName {get; private set;} = "";
        public static int APSessionDataSlotNum {get; private set;} = -1;
        public static RoomInfoPacket RoomInfo {get; private set;}
        public static ConnectedPacket ConnectionInfo {get; private set;}
        public static Dictionary<string, object> SlotData {get => (ConnectionInfo!=null)?ConnectionInfo.SlotData:null;}
        public static DataPackage SessionDataPackage {get; private set;}
        public static int HintPoints {get; private set;} = 0;
        public static Queue<NetworkItem> ItemReceiveQueue {get; private set;}
        public static Queue<NetworkItem> ItemReceiveLevelQueue {get; private set;}
        private static bool queueLock = false;
        private static bool levelQueueLock = false;
        private static bool IsTryingSessionConnect {get; set;} = false;
        private static string DataPackageChecksum {get => (RoomInfo!=null)?RoomInfo.DataPackageChecksums["Cuphead"]:"-1";}
        private static List<long> locationSendQueue;
        private static readonly Version AP_VERSION = new Version(0,4,2,0);
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
            IsTryingSessionConnect = true;
            if (APSession!=null) {
                if (APSession.Socket.Connected) {
                    Plugin.Log($"[APClient] Already Connected. Disconnecting...", LogLevel.Warning);
                    APSession.Socket.Disconnect();
                }
            }
            bool res = false;

            APData data = APData.SData[index];
            APSession = ArchipelagoSessionFactory.CreateSession(data.address);
            APSessionSlotName = data.slot;
            APSessionDataSlotNum = index;
            SessionDataPackage = APSessionSData.data;
            APSession.Socket.SocketClosed += (string reason) => {
                Plugin.Log("[APClient] Disconnected.");
                Plugin.Log($"[APClient] Disconnect Reason: {reason}", LoggingFlags.Network);
                if (Enabled) {
                    ReconnectArchipelagoSession();
                }
            };
            APSession.Socket.PacketReceived += OnPacket;
            locationSendQueue = new List<long>();

            res = ConnectArchipelagoSession(true);

            return res;
        }
        private static bool ConnectArchipelagoSession(bool invoked=false) {
            if (!invoked&&IsTryingSessionConnect) {
                Plugin.Log($"[APClient] Already Trying to Connect. Aborting.", LogLevel.Warning);
                return false;
            }
            IsTryingSessionConnect = true;
            bool res = false;
            APData data = APData.SData[APSessionDataSlotNum];
            Plugin.Log($"[APClient] Connecting to {data.address} as {data.slot}...");
            LoginResult result;
            try {
                result = APSession.TryConnectAndLogin("Cuphead", data.slot, ItemsHandlingFlags.IncludeStartingInventory, AP_VERSION, null, null, data.password);
            } catch (Exception e) {
                result = new LoginFailure(e.GetBaseException().Message);
            }

            if (!result.Successful)
            {
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

                Plugin.Log(errorMessage, LogLevel.Error);

                APSessionSlotName = "";
                APSessionDataSlotNum = -1;
                IsTryingSessionConnect = false;
                res = false;
            }
            else {
                Plugin.Log($"[APClient] Connected to {data.address} as {data.slot}");
                if (locationSendQueue.Count>0) {
                    Plugin.Log($"[APClient] Sending previously unsent checks...", LoggingFlags.Network);
                    SendChecks(locationSendQueue.ToArray());
                }
                Enabled = true;
                res = true;
            }
            IsTryingSessionConnect = false;
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
                Plugin.Log("[APClient] Failed to reconnect!", LogLevel.Error);
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
            APSessionSlotName = "";
            APSessionDataSlotNum = -1;
            HintPoints = 0;
            RoomInfo = null;
            ConnectionInfo = null;
            SessionDataPackage = null;
            locationSendQueue = null;
            ResetQueues();
        }

        public static bool SetupArchipelagoGame() {
            /* Send location checks of completed levels */
            List<long> locs = new List<long>();
            PlayerData pd = PlayerData.Data;
            // Level Locations
            foreach (Levels level in LevelLocationMap.GetKeys()) {
                if (pd.CheckLevelsHaveMinDifficulty(new Levels[]{level}, APSessionSData.Hard?Level.Mode.Hard:Level.Mode.Normal))  {
                    locs.Add((long)LevelLocationMap.GetLocationId(level,0));
                    if (APSessionSData.BossGradeChecks>0&&Array.IndexOf(Level.platformingLevels, level)<0) {
                        if (pd.CheckLevelsHaveMinGrade(new Levels[]{level}, LevelScoringData.Grade.A+APSessionSData.BossGradeChecks)) {
                            locs.Add((long)LevelLocationMap.GetLocationId(level,1));
                        }
                    }
                    else if (APSessionSData.RungunGradeChecks>0) {
                        int rungunGradeChecks = APData.CurrentSData.RungunGradeChecks;
                        if (pd.CheckLevelsHaveMinGrade(new Levels[]{level}, LevelScoringData.Grade.APlus+(rungunGradeChecks>=2?2:0))) {
                            locs.Add((long)LevelLocationMap.GetLocationId(level,(rungunGradeChecks>=2?2:1)));
                        }
                    }
                }
            }
            SendChecks(locs.ToArray());
            /* Receive location checks (e.g. mark levels as complete) */
            long[] checkedLocations = ConnectionInfo.LocationsChecked;
            /* TODO: RECEIVE LOCATION CHECKS */
            return true;
        }

        public static void SendCheck(long loc) => SendChecks(new long[]{loc});
        public static void SendChecks(long[] locs) {
            if (locs.Length<1) return;
            if (APSession.Socket.Connected) {
                APSession.Locations.CompleteLocationChecksAsync((bool state) => OnLocationSendComplete(state, locs), locs);
            }
            else {
                Plugin.Log($"[APClient] Disconnected. Cannot send check. Will retry after connecting.");
                locationSendQueue.AddRange(locs);
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

        private static void OnPacket(ArchipelagoPacketBase packet) {
            Plugin.Log($"[APClient] Packet type {packet.PacketType} received", LoggingFlags.Network);
            switch (packet.PacketType) {
                case ArchipelagoPacketType.RoomInfo: {
                    RoomInfo = ((RoomInfoPacket)packet);
                    Plugin.Log(String.Join(", ", new List<string>(RoomInfo.DataPackageChecksums.Keys).ToArray()));
                    break;
                }
                case ArchipelagoPacketType.DataPackage: {
                    SessionDataPackage = ((DataPackagePacket)packet).DataPackage;
                    if (SessionDataPackage.Games["Cuphead"].Checksum!=APSessionSData.DataSum) {
                        Plugin.Log($"[APClient] Updating cached DataPackage...", LoggingFlags.Network);
                        APSessionSData.data = SessionDataPackage;
                    }
                    break;
                }
                case ArchipelagoPacketType.Connected: {
                    ConnectionInfo = ((ConnectedPacket)packet);
                    APSessionSData.slotData = SlotData;
                    break;
                }
                case ArchipelagoPacketType.ReceivedItems: {
                    ReceivedItemsPacket pk = ((ReceivedItemsPacket)packet);
                    NetworkItem item = pk.Items[pk.Index];
                    if ((item.Flags&ItemFlags.Trap)>0) {
                        ReceiveLevelItem(item);
                    }
                    else if (item.Item==APItem.extrahealth.Id||item.Item==APItem.superrecharge.Id) {
                        ReceiveLevelItem(item);
                    }
                    else {
                        ReceiveItem(item);
                    }
                    break;
                }
                case ArchipelagoPacketType.PrintJSON: {
                    //Console Log later
                    break;
                }
                case ArchipelagoPacketType.RoomUpdate: {
                    RoomUpdatePacket update = (RoomUpdatePacket)packet;
                    if (update.Tags!=null) RoomInfo.Tags = update.Tags;
                    if (update.Password.HasValue) RoomInfo.Password = (bool)update.Password;
                    if (update.Permissions!=null) RoomInfo.Permissions = update.Permissions;
                    if (update.HintCostPercentage.HasValue) RoomInfo.HintCostPercentage = (int)update.HintCostPercentage;
                    if (update.LocationCheckPoints.HasValue) RoomInfo.LocationCheckPoints = (int)update.LocationCheckPoints;
                    if (update.Players!=null) RoomInfo.Players = update.Players;
                    if (update.HintPoints.HasValue) HintPoints = (int)update.HintPoints;
                    if (update.CheckedLocations!=null) {
                        long[] checkLocations = new long[ConnectionInfo.LocationsChecked.LongLength + update.CheckedLocations.LongLength];
                        ConnectionInfo.LocationsChecked.CopyTo(checkLocations, 0);
                        update.CheckedLocations.CopyTo(checkLocations, ConnectionInfo.LocationsChecked.LongLength);
                        //TODO: Update
                    }
                    break;
                }
            }
        }
        private static void ReceiveItem(NetworkItem item, bool noqueue=false) {
            queueLock = true;
            ItemReceiveQueue.Enqueue(item);
            queueLock = false;
        }
        private static void ReceiveLevelItem(NetworkItem item) {
            levelQueueLock = true;
            ItemReceiveLevelQueue.Enqueue(item);
            levelQueueLock = false;
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
