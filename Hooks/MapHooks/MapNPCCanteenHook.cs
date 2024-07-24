/// Copyright 2024 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using CupheadArchipelago.AP;
using HarmonyLib;

namespace CupheadArchipelago.Hooks {
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
                bool labelInserted = false;
                FieldInfo _fi_dialoguerVariableID = typeof(MapNPCCanteen).GetField("dialoguerVariableID", BindingFlags.NonPublic | BindingFlags.Instance);
                MethodInfo _mi_Dialoguer_SetGlobalFloat = typeof(Dialoguer).GetMethod("SetGlobalFloat", BindingFlags.Public | BindingFlags.Static);

                Label tgt_label = il.DefineLabel();

                if (debug) {
                    for (int i = 0; i < codes.Count; i++) {
                        Plugin.Log($"{codes[i].opcode}: {codes[i].operand}");
                    }
                }
                List<CodeInstruction> ncodes = [
                    CodeInstruction.Call(() => APIsChecked()),
                    new CodeInstruction(OpCodes.Brtrue, tgt_label),
                ];
                codes.InsertRange(0, ncodes);
                for (int i=ncodes.Count;i<codes.Count-3;i++) {
                    if (codes[i].opcode == OpCodes.Ldarg_0 && codes[i+1].opcode == OpCodes.Ldfld && 
                        (FieldInfo)codes[i+1].operand == _fi_dialoguerVariableID && codes[i+2].opcode == OpCodes.Ldc_R4 && 
                        (float)codes[i+2].operand == 1f && codes[i+3].opcode == OpCodes.Call && 
                        (MethodInfo)codes[i+3].operand == _mi_Dialoguer_SetGlobalFloat) {
                            codes[i].labels.Add(tgt_label);
                            labelInserted = true;
                    }
                }
                if (!labelInserted) throw new Exception($"{nameof(Start)}: Patch Failed!");
                if (debug) {
                    Plugin.Log("---");
                    for (int i = 0; i < codes.Count; i++) {
                        Plugin.Log($"{codes[i].opcode}: {codes[i].operand}");
                    }
                }

                return codes;
            }

            private static bool APIsChecked() => APData.IsCurrentSlotEnabled() && APClient.IsLocationChecked(locationId);
        }

        [HarmonyPatch(typeof(MapNPCCanteen), "OnDialoguerMessageEvent")]
        internal static class OnDialoguerMessageEvent {
            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator il) {
                List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
                bool debug = false;
                bool success = false;
                bool labelInserted = false;
                FieldInfo _fi_dialoguerVariableID = typeof(MapNPCCanteen).GetField("dialoguerVariableID", BindingFlags.NonPublic | BindingFlags.Instance);
                MethodInfo _mi_Dialoguer_SetGlobalFloat = typeof(Dialoguer).GetMethod("SetGlobalFloat", BindingFlags.Public | BindingFlags.Static);
                MethodInfo _mi_MapEventNotification_ShowTooltipEvent = 
                    typeof(MapEventNotification).GetMethod("ShowTooltipEvent", BindingFlags.Public | BindingFlags.Instance);

                Label end_label = il.DefineLabel();

                if (debug) {
                    for (int i = 0; i < codes.Count; i++) {
                        Plugin.Log($"{codes[i].opcode}: {codes[i].operand}");
                    }
                }
                for (int i=0;i<codes.Count-3;i++) {
                    if (!success && codes[i].opcode == OpCodes.Call && codes[i+1].opcode == OpCodes.Ldc_I4_1 && codes[i+2].opcode == OpCodes.Callvirt &&
                        (MethodInfo)codes[i+2].operand == _mi_MapEventNotification_ShowTooltipEvent) {
                            //codes[i+1] = CodeInstruction.Call(() => APEventType());
                            List<CodeInstruction> ncodes = [
                                CodeInstruction.Call(() => APCheck()),
                                new CodeInstruction(OpCodes.Brtrue, end_label),
                            ];
                            codes.InsertRange(i, ncodes);
                            i+=ncodes.Count;
                            success = true;
                    }
                    if (!labelInserted && codes[i].opcode == OpCodes.Ldarg_0 && codes[i+1].opcode == OpCodes.Ldfld && 
                        (FieldInfo)codes[i+1].operand == _fi_dialoguerVariableID && codes[i+2].opcode == OpCodes.Ldc_R4 && 
                        (float)codes[i+2].operand == 1f && codes[i+3].opcode == OpCodes.Call && 
                        (MethodInfo)codes[i+3].operand == _mi_Dialoguer_SetGlobalFloat) {
                            codes[i].labels.Add(end_label);
                            labelInserted = true;
                    }
                    if (labelInserted&&success) break;
                }
                if (!labelInserted||!success) throw new Exception($"{nameof(Start)}: Patch Failed! {labelInserted}:{success}");
                if (debug) {
                    Plugin.Log("---");
                    for (int i = 0; i < codes.Count; i++) {
                        Plugin.Log($"{codes[i].opcode}: {codes[i].operand}");
                    }
                }

                return codes;
            }

            private static bool APCheck() {
                if (APData.IsCurrentSlotEnabled()) {
                    APClient.Check(locationId);
                    return true;
                }
                return false;
            }
            private static TooltipEvent APEventType() => TooltipEvent.Canteen;
        }
    }
}
