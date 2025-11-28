/// Copyright 2025 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using CupheadArchipelago.AP;
using CupheadArchipelago.Mapping;
using HarmonyLib;

namespace CupheadArchipelago.Hooks.MapHooks.MapNPCHooks {
    internal class MapNPCAxemanHook {
        internal static void Hook() {
            Harmony.CreateAndPatchAll(typeof(Start));
        }

        [HarmonyPatch(typeof(MapNPCAxeman), "Start")]
        internal static class Start {
            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator il) {
                List<CodeInstruction> codes = new(instructions);
                bool success = false;
                bool debug = false;

                FieldInfo _fi_world1BossLevels = typeof(Level).GetField("world1BossLevels", BindingFlags.Public | BindingFlags.Static);
                MethodInfo _mi_get_Data = typeof(PlayerData).GetProperty("Data", BindingFlags.Public | BindingFlags.Static)?.GetGetMethod();
                MethodInfo _mi_CheckLevelsCompleted = typeof(PlayerData).GetMethod("CheckLevelsCompleted", BindingFlags.Public | BindingFlags.Instance);
                MethodInfo _mi_CheckMappedLevelsCompleted = typeof(LevelMap).GetMethod("CheckMappedLevelsCompleted", BindingFlags.Public | BindingFlags.Static);

                Label l_vanilla = il.DefineLabel();
                Label l_brf = il.DefineLabel();

                if (debug) {
                    Dbg.LogCodeInstructions(codes);
                }
                for (int i=0; i<codes.Count-3; i++) {
                    if (codes[i].opcode == OpCodes.Call && (MethodInfo)codes[i].operand == _mi_get_Data && 
                        codes[i+1].opcode == OpCodes.Ldsfld && (FieldInfo)codes[i+1].operand == _fi_world1BossLevels && 
                        codes[i+2].opcode == OpCodes.Callvirt && (MethodInfo)codes[i+2].operand == _mi_CheckLevelsCompleted && 
                        codes[i+3].opcode == OpCodes.Brfalse) {
                            codes[i].labels.Add(l_vanilla);
                            codes[i+3].labels.Add(l_brf);
                            List<CodeInstruction> ncodes = [
                                CodeInstruction.Call(() => APData.IsCurrentSlotEnabled()),
                                new CodeInstruction(OpCodes.Brfalse, l_vanilla),
                                new CodeInstruction(OpCodes.Ldsfld, _fi_world1BossLevels),
                                new CodeInstruction(OpCodes.Call, _mi_CheckMappedLevelsCompleted),
                                new CodeInstruction(OpCodes.Br, l_brf),
                            ];
                            codes.InsertRange(i, ncodes);
                            success = true;
                            break;
                    }
                }
                if (!success) throw new Exception($"{nameof(Start)}: Patch Failed!");
                if (debug) {
                    Logging.Log("---");
                    Dbg.LogCodeInstructions(codes);
                }
            
                return codes;
            }
        }
    }
}
