/// Copyright 2024 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using BepInEx.Logging;
using System.Reflection;

namespace CupheadArchipelago.Hooks {
    internal class SaveKeyUpdaterHook {
        public static string SaveKeyBaseName {get; private set;} = "cuphead_player_data_v1_ap_slot_";
        public static string[] SaveKeyNames {get; private set;}
        private static bool _nameLock = false;
        private static FieldInfo _fi_SAVE_FILE_KEYS;

        static SaveKeyUpdaterHook() {
            _fi_SAVE_FILE_KEYS = typeof(PlayerData).GetField("SAVE_FILE_KEYS", BindingFlags.NonPublic | BindingFlags.Static);
        }

        internal static void SetSaveKeyBaseName(string name) {
            if (!_nameLock) SaveKeyBaseName = name; else Logging.Log("Cannot Set SaveKeyBaseName after Hook", LogLevel.Warning);
        }

        internal static void Hook() {
            _nameLock = true;
            SaveKeyNames = new string[3] {SaveKeyBaseName+0, SaveKeyBaseName+1, SaveKeyBaseName+2};
            Harmony.CreateAndPatchAll(typeof(OnCloudStorageInitialized));
            Harmony.CreateAndPatchAll(typeof(OnLoaded));
            Harmony.CreateAndPatchAll(typeof(Save));
            Harmony.CreateAndPatchAll(typeof(SaveAll));
        }

        [HarmonyPatch(typeof(PlayerData), "OnCloudStorageInitialized")]
        internal static class OnCloudStorageInitialized {
            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
                List<CodeInstruction> codes = DigestKeysInstructions(instructions);
                return codes;
            }
        }

        [HarmonyPatch(typeof(PlayerData), "OnLoaded")]
        internal static class OnLoaded {
            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
                List<CodeInstruction> codes = DigestKeysInstructions(instructions);
                return codes;
            }
        }

        [HarmonyPatch(typeof(PlayerData), "Save")]
        internal static class Save {
            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
                List<CodeInstruction> codes = DigestKeysInstructions(instructions);
                return codes;
            }
        }

        [HarmonyPatch(typeof(PlayerData), "SaveAll")]
        internal static class SaveAll {
            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
                List<CodeInstruction> codes = DigestKeysInstructions(instructions);
                return codes;
            }
        }

        private static List<CodeInstruction> DigestKeysInstructions(IEnumerable<CodeInstruction> instructions) {
            List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
            List<CodeInstruction> ncodes = new List<CodeInstruction>();

            ncodes.Add(new CodeInstruction(OpCodes.Ldc_I4_3));
            ncodes.Add(new CodeInstruction(OpCodes.Newarr, typeof(string)));

            ncodes.Add(new CodeInstruction(OpCodes.Dup));
            ncodes.Add(new CodeInstruction(OpCodes.Ldc_I4_0));
            ncodes.Add(new CodeInstruction(OpCodes.Ldstr, SaveKeyNames[0]));
            ncodes.Add(new CodeInstruction(OpCodes.Stelem_Ref));

            ncodes.Add(new CodeInstruction(OpCodes.Dup));
            ncodes.Add(new CodeInstruction(OpCodes.Ldc_I4_1));
            ncodes.Add(new CodeInstruction(OpCodes.Ldstr, SaveKeyNames[1]));
            ncodes.Add(new CodeInstruction(OpCodes.Stelem_Ref));

            ncodes.Add(new CodeInstruction(OpCodes.Dup));
            ncodes.Add(new CodeInstruction(OpCodes.Ldc_I4_2));
            ncodes.Add(new CodeInstruction(OpCodes.Ldstr, SaveKeyNames[2]));
            ncodes.Add(new CodeInstruction(OpCodes.Stelem_Ref));

            for (int i=0;i<codes.Count-1;i++) {
                if (codes[i].opcode == OpCodes.Ldsfld && (FieldInfo)codes[i].operand == _fi_SAVE_FILE_KEYS && codes[i+1].opcode != OpCodes.Ldlen) {
                    codes.RemoveAt(i);
                    codes.InsertRange(i, ncodes);
                }
            }

            return codes;
        }
    }
}
