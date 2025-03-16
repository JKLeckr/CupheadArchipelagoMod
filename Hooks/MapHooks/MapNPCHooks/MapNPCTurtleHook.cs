/// Copyright 2025 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using CupheadArchipelago.AP;
using HarmonyLib;

namespace CupheadArchipelago.Hooks.MapHooks.MapNPCHooks {
    internal class MapNPCTurtleHook {
        internal static void Hook() {
            //Harmony.CreateAndPatchAll(typeof(Start));
            Harmony.CreateAndPatchAll(typeof(OnDialoguerMessageEvent));
        }

        private static readonly long locationId = APLocation.quest_pacifist;

        [HarmonyPatch(typeof(MapNPCTurtle), "Start")]
        internal static class Start {
            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator il) {
                List<CodeInstruction> codes = new(instructions);
                bool debug = false;
                bool success = false;
                FieldInfo _fi_platformingLevels = typeof(Level).GetField("platformingLevels", BindingFlags.Public | BindingFlags.Static);
                MethodInfo _mi_get_Data = typeof(PlayerData).GetProperty("Data", BindingFlags.Public | BindingFlags.Static)?.GetGetMethod();
                MethodInfo _mi_CheckLevelsHaveMinGrade = typeof(PlayerData).GetMethod("CheckLevelsHaveMinGrade", BindingFlags.Public | BindingFlags.Instance);

                if (debug) {
                    for (int i = 0; i < codes.Count; i++) {
                        Logging.Log($"{codes[i].opcode}: {codes[i].operand}");
                    }
                }
                for (int i=0;i<codes.Count-5;i++) {
                    if (!success && codes[i].opcode == OpCodes.Call && (MethodInfo)codes[i].operand == _mi_get_Data && 
                        codes[i+1].opcode == OpCodes.Ldsfld && (FieldInfo)codes[i+1].operand == _fi_platformingLevels && 
                        codes[i+2].opcode == OpCodes.Ldc_I4_S && (sbyte)codes[i+2].operand == (int)LevelScoringData.Grade.P && 
                        codes[i+3].opcode == OpCodes.Callvirt && (MethodInfo)codes[i+3].operand == _mi_CheckLevelsHaveMinGrade && 
                        codes[i+4].opcode == OpCodes.Brfalse) {
                            Label iftrue = il.DefineLabel();
                            codes[i+5].labels.Add(iftrue);
                            Label nextif = (Label)codes[i+4].operand;
                            codes[i+4] = new CodeInstruction(OpCodes.Brtrue, iftrue);
                            List<CodeInstruction> ncodes = [
                                CodeInstruction.Call(() => APCheck()),
                                new CodeInstruction(OpCodes.Brfalse, nextif),
                            ];
                            codes.InsertRange(i+5, ncodes);
                            i+=ncodes.Count;
                            success = true;
                    }
                }
                if (debug) {
                    for (int i = 0; i < codes.Count; i++) {
                        Logging.Log($"{codes[i].opcode}: {codes[i].operand}");
                    }
                }
                if (!success) throw new Exception($"{nameof(Start)}: Patch Failed!");

                return codes;
            }
            private static bool APCheck() {
                if (APData.IsCurrentSlotEnabled() && APSettings.QuestPacifist) {
                    return !APClient.IsLocationChecked(locationId);
                }
                else return true;
            }
        }

        [HarmonyPatch(typeof(MapNPCTurtle), "OnDialoguerMessageEvent")]
        internal static class OnDialoguerMessageEvent {
            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator il) {
                return MapNPCHookBase.
                        MapNPCQuestHookBase.
                        MapNPCQuestHookTranspiler(instructions, locationId);
            }
            static void Postfix(string message, bool ___SkipDialogueEvent, int ___dialoguerVariableID) {
                if (___SkipDialogueEvent) return;
                if (message == "Pacifist") {
                    LogDialoguerGlobalFloat(___dialoguerVariableID);
                }
            }
        }
        private static void LogDialoguerGlobalFloat(int floatId) => 
            Logging.Log($"{nameof(MapNPCTurtle)}: {Dialoguer.GetGlobalFloat(floatId)}");
    }
}
