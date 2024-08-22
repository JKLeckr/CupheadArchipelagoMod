/// Copyright 2024 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using CupheadArchipelago.AP;
using HarmonyLib;

namespace CupheadArchipelago.Hooks.MapHooks.MapNPCHooks {
    internal class MapNPCCanteenHook {
        internal static void Hook() {
            Harmony.CreateAndPatchAll(typeof(Start));
            Harmony.CreateAndPatchAll(typeof(OnDialoguerMessageEvent));
        }

        private static readonly long locationId = APLocation.npc_canteen;

        [HarmonyPatch(typeof(MapNPCCanteen), "Start")]
        internal static class Start {
            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator il) {
                List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
                bool debug = false;
                int labelInsert = 0;
                FieldInfo _fi_dialoguerVariableID = typeof(MapNPCCanteen).GetField("dialoguerVariableID", BindingFlags.NonPublic | BindingFlags.Instance);
                MethodInfo _mi_Dialoguer_SetGlobalFloat = typeof(Dialoguer).GetMethod("SetGlobalFloat", BindingFlags.Public | BindingFlags.Static);
                MethodInfo _mi_AddDialoguerEvents = typeof(MapNPCCanteen).GetMethod("AddDialoguerEvents", BindingFlags.Public | BindingFlags.Instance);

                Label tgt_label = il.DefineLabel();
                Label end_label = il.DefineLabel();

                if (debug) {
                    for (int i = 0; i < codes.Count; i++) {
                        Plugin.Log($"{codes[i].opcode}: {codes[i].operand}");
                    }
                }
                List<CodeInstruction> ncodes = [
                    CodeInstruction.Call(() => IsAPChecked()),
                    new CodeInstruction(OpCodes.Brtrue, tgt_label),
                    CodeInstruction.Call(() => APData.IsCurrentSlotEnabled()),
                    new CodeInstruction(OpCodes.Brtrue, end_label)
                ];
                codes.InsertRange(0, ncodes);
                int start = ncodes.Count;
                for (int i=start;i<codes.Count-3;i++) {
                    if ((labelInsert&1)==0 && codes[i].opcode == OpCodes.Ldarg_0 && codes[i+1].opcode == OpCodes.Ldfld && 
                        (FieldInfo)codes[i+1].operand == _fi_dialoguerVariableID && codes[i+2].opcode == OpCodes.Ldc_R4 && 
                        (float)codes[i+2].operand == 1f && codes[i+3].opcode == OpCodes.Call && 
                        (MethodInfo)codes[i+3].operand == _mi_Dialoguer_SetGlobalFloat) {
                            codes[i].labels.Add(tgt_label);
                            labelInsert |= 1;
                            start = i+1;
                            break;
                    }
                }
                for (int i=start;i<codes.Count-1;i++) {
                    if ((labelInsert&2)==0 && codes[i].opcode == OpCodes.Ldarg_0 && codes[i+1].opcode == OpCodes.Call &&
                        (MethodInfo)codes[i+1].operand == _mi_AddDialoguerEvents) {
                            codes[i].labels.Add(end_label);
                            labelInsert |= 2;
                            break;
                    }
                }
                if (labelInsert!=3) throw new Exception($"{nameof(Start)}: Patch Failed! {labelInsert}");
                if (debug) {
                    Plugin.Log("---");
                    for (int i = 0; i < codes.Count; i++) {
                        Plugin.Log($"{codes[i].opcode}: {codes[i].operand}");
                    }
                }

                return codes;
            }

            private static bool IsAPChecked() => APData.IsCurrentSlotEnabled() && APClient.IsLocationChecked(locationId);
        }

        [HarmonyPatch(typeof(MapNPCCanteen), "OnDialoguerMessageEvent")]
        internal static class OnDialoguerMessageEvent {
            static bool Prefix(string message, bool ___SkipDialogueEvent, int ___dialoguerVariableID) {
                if (APData.IsCurrentSlotEnabled()) {
                    if (___SkipDialogueEvent) return false;
                    if (message == "CanteenWeaponTwo") {
                        Dialoguer.SetGlobalFloat(___dialoguerVariableID, 1f);
                        LogDialoguerGlobalFloat(___dialoguerVariableID);
                        if (!APClient.IsLocationChecked(locationId))
                            APClient.Check(locationId);
                        PlayerData.SaveCurrentFile();
                        //MapEventNotification.Current.ShowEvent(MapEventNotification.Type.Canteen);
                    }
                    return false;
                }
                else return true;
            }
        }

        private static void LogDialoguerGlobalFloat(int floatId) => 
            Plugin.Log($"{nameof(MapNPCCanteen)}: {Dialoguer.GetGlobalFloat(floatId)}");
    }
}
