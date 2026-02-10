/// Copyright 2025-2026 JKLeckr
/// SPDX-License-Identifier: GPL-3.0-or-later

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using CupheadArchipelago.AP;
using HarmonyLib;

namespace CupheadArchipelago.Hooks.MapHooks.MapNPCHooks {
    internal class MapNPCNewsieCatHook {
        internal static void Hook() {
            Harmony.CreateAndPatchAll(typeof(Start));
            Harmony.CreateAndPatchAll(typeof(OnDialoguerMessageEvent));
        }

        private const int DIALOGUER_ID = 39;
        private static readonly long locationId = APLocation.dlc_npc_newscat;

        [HarmonyPatch(typeof(MapNPCNewsieCat), "Start")]
        internal static class Start {
            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator il) {
                List<CodeInstruction> codes = new(instructions);
                bool debug = false;
                int success = 0;

                FieldInfo _fi_coinManager = typeof(PlayerData).GetField("coinManager", BindingFlags.Public | BindingFlags.Instance);
                FieldInfo _fi_coinID1 = typeof(MapNPCNewsieCat).GetField("coinID1", BindingFlags.NonPublic | BindingFlags.Instance);
                MethodInfo _mi_get_Data = typeof(PlayerData).GetProperty("Data", BindingFlags.Public | BindingFlags.Static)?.GetGetMethod();
                MethodInfo _mi_GetCoinCollected =
                    typeof(PlayerData.PlayerCoinManager).GetMethod("GetCoinCollected", BindingFlags.Public | BindingFlags.Instance, null, [typeof(string)], null);
                MethodInfo _mi_ProcessLevels = typeof(Start).GetMethod("ProcessLevels", BindingFlags.NonPublic | BindingFlags.Static);
                MethodInfo _mi_APCondition = typeof(Start).GetMethod("APCondition", BindingFlags.NonPublic | BindingFlags.Static);

                if (debug) {
                    Dbg.LogCodeInstructions(codes);
                }
                for (int i=0;i<codes.Count-5;i++) {
                    if ((success&1)==0 && codes[i].opcode == OpCodes.Call && (MethodInfo)codes[i].operand == _mi_get_Data && codes[i+1].opcode == OpCodes.Ldfld &&
                        (FieldInfo)codes[i+1].operand == _fi_coinManager && codes[i+2].opcode == OpCodes.Ldarg_0 && codes[i+3].opcode == OpCodes.Ldfld &&
                        (FieldInfo)codes[i+3].operand == _fi_coinID1 && codes[i+4].opcode == OpCodes.Callvirt && (MethodInfo)codes[i+4].operand == _mi_GetCoinCollected &&
                        codes[i+5].opcode == OpCodes.Brtrue) {
                            codes.Insert(i+5, new CodeInstruction(OpCodes.Call, _mi_APCondition));
                            i++;
                            success |= 1;
                    }
                    if ((success&6)!=6 && IsArraySetup(codes, i)) {
                        if ((success&2)==0 && codes[i+5].opcode == OpCodes.Stloc_0) {
                            codes.Insert(i+5, new CodeInstruction(OpCodes.Call, _mi_ProcessLevels));
                            i++;
                            success |= 2;
                        }
                        else if ((success&4)==0 && codes[i+5].opcode == OpCodes.Stloc_1) {
                            codes.Insert(i+5, new CodeInstruction(OpCodes.Call, _mi_ProcessLevels));
                            i++;
                            success |= 4;
                        }
                    }
                }
                if (debug) {
                    Logging.Log("---");
                    Dbg.LogCodeInstructions(codes);
                }
                if (success!=7) throw new Exception($"{nameof(Start)}: Patch Failed! {success}");

                return codes;
            }
            private static bool IsArraySetup(List<CodeInstruction> codes, int i) {
                return
                    codes[i+1].opcode == OpCodes.Newarr &&
                    (Type)codes[i+1].operand == typeof(Levels) &&
                    codes[i+2].opcode == OpCodes.Dup &&
                    codes[i+3].opcode == OpCodes.Ldtoken &&
                    codes[i+4].opcode == OpCodes.Call;
            }
            private static Levels[] ProcessLevels(Levels[] orig) {
                return orig;
            }
            private static bool APCondition(bool orig) {
                if (APData.IsCurrentSlotEnabled()) {
                    return !APClient.IsLocationChecked(locationId);
                }
                else return orig;
            }
        }

        [HarmonyPatch(typeof(MapNPCNewsieCat), "OnDialoguerMessageEvent")]
        internal static class OnDialoguerMessageEvent {
            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator il) {
                return MapNPCHookBase.
                        MapNPCCoinHookBase.
                        MapNPCCoinHookTranspiler(instructions, il, locationId);
            }
            static void Postfix(string message) {
                if (message == "NewsieCoin") {
                    LogDialoguerGlobalFloat(DIALOGUER_ID);
                }
            }
        }

        private static void LogDialoguerGlobalFloat(int floatId) =>
            Logging.Log($"{nameof(MapNPCAppletraveller)}: {Dialoguer.GetGlobalFloat(floatId)}");
    }
}
