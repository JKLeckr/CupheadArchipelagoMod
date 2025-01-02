/// Copyright 2025 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using CupheadArchipelago.AP;
using HarmonyLib;

namespace CupheadArchipelago.Hooks.MapHooks.MapNPCHooks {
    internal class MapNPCJugglerHook {
        internal static void Hook() {
            Harmony.CreateAndPatchAll(typeof(Start));
            Harmony.CreateAndPatchAll(typeof(OnDialoguerMessageEvent));
        }

        private static readonly long locationId = APLocation.quest_4parries;

        [HarmonyPatch(typeof(MapNPCJuggler), "Start")]
        internal static class Start {
            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator il) {
                List<CodeInstruction> codes = new(instructions);
                bool debug = false;
                int success = 0;

                if (debug) {
                    for (int i = 0; i < codes.Count; i++) {
                        Logging.Log($"{codes[i].opcode}: {codes[i].operand}");
                    }
                }
                for (int i=0;i<codes.Count-3;i++) {
                    if (codes[i].opcode == OpCodes.Ldloc_0 && codes[i+1].opcode == OpCodes.Ldc_I4_3 && codes[i+2].opcode == OpCodes.Ble) {
                        Label iftrue = il.DefineLabel();
                        codes[i+3].labels.Add(iftrue);
                        Label end = (Label)codes[i+2].operand;
                        codes[i+2] = new CodeInstruction(OpCodes.Bgt, iftrue);
                        List<CodeInstruction> ncodes = [
                            CodeInstruction.Call(() => APCheck()),
                            new CodeInstruction(OpCodes.Brfalse, end),
                        ];
                        codes.InsertRange(i+3,ncodes);
                        i+=ncodes.Count;
                        success++;
                    }
                }
                if (debug) {
                    for (int i = 0; i < codes.Count; i++) {
                        Logging.Log("---");
                        Logging.Log($"{codes[i].opcode}: {codes[i].operand}");
                    }
                }
                if (success!=1) throw new Exception($"{nameof(Start)}: Patch Failed! {success}");

                return codes;
            }
            private static bool APCheck() {
                if (APData.IsCurrentSlotEnabled()) {
                    return !APClient.IsLocationChecked(locationId);
                }
                else return true;
            }
        }

        [HarmonyPatch(typeof(MapNPCJuggler), "OnDialoguerMessageEvent")]
        internal static class OnDialoguerMessageEvent {
            static bool Prefix(string message, bool ___SkipDialogueEvent) {
                if (APData.IsCurrentSlotEnabled() && APSettings.QuestJuggler) {
                    if (___SkipDialogueEvent) return false;
                    if (message == "JugglerCoin") {
                        if (!APClient.IsLocationChecked(locationId))
                            APClient.Check(locationId);
                        PlayerData.SaveCurrentFile();
                        //MapEventNotification.Current.ShowEvent(MapEventNotification.Type.Coin);
                    }
                    return false;
                }
                else return true;
            }
        }

        private static void LogDialoguerGlobalFloat(int floatId) => 
            Logging.Log($"{nameof(MapNPCJuggler)}: {Dialoguer.GetGlobalFloat(floatId)}");
    }
}
