/// Copyright 2025-2026 JKLeckr
/// SPDX-License-Identifier: GPL-3.0-or-later

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using CupheadArchipelago.AP;
using HarmonyLib;

namespace CupheadArchipelago.Hooks.MapHooks {
    internal class MapCastleZonesHook {
        internal static void Hook() {
            Harmony.CreateAndPatchAll(typeof(onMapCastleZoneCollision));
        }

        [HarmonyPatch(typeof(MapCastleZones), "onMapCastleZoneCollision")]
        internal static class onMapCastleZoneCollision {
            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator il) {
                List<CodeInstruction> codes = new(instructions);
                bool success = false;
                bool debug = false;

                FieldInfo _fi_kingOfGamesLevels = typeof(Level).GetField("kingOfGamesLevels", BindingFlags.Public | BindingFlags.Static);
                MethodInfo _mi_CountLevelsCompleted = typeof(PlayerData).GetMethod(
                    "CountLevelsCompleted",
                    BindingFlags.Public | BindingFlags.Instance,
                    null,
                    [typeof(Levels[])],
                    null
                );

                Label l_docktrue = il.DefineLabel();

                if (debug) {
                    Dbg.LogCodeInstructions(codes);
                }
                for (int i = 0; i < codes.Count - 7; i++) {
                    if (codes[i].opcode == OpCodes.Ldloc_0 && codes[i + 1].opcode == OpCodes.Ldsfld && (FieldInfo)codes[i + 1].operand == _fi_kingOfGamesLevels &&
                        codes[i + 2].opcode == OpCodes.Callvirt && (MethodInfo)codes[i + 2].operand == _mi_CountLevelsCompleted &&
                        codes[i + 3].opcode == OpCodes.Ldsfld && (FieldInfo)codes[i + 3].operand == _fi_kingOfGamesLevels && codes[i + 4].opcode == OpCodes.Ldlen &&
                        codes[i + 5].opcode == OpCodes.Conv_I4 && codes[i + 6].opcode == OpCodes.Bne_Un
                    ) {
                        codes[i + 7].labels.Add(l_docktrue);

                        List<CodeInstruction> ncodes = [
                            CodeInstruction.Call(() => APCondition()),
                            new(OpCodes.Brtrue, l_docktrue)
                        ];

                        codes.InsertRange(i, ncodes);

                        success = true;
                        break;
                    }
                }
                if (!success) throw new Exception($"{nameof(onMapCastleZoneCollision)}: Patch Failed!");
                if (debug) {
                    Logging.Log("---");
                    Dbg.LogCodeInstructions(codes);
                }

                return codes;
            }

            private static bool APCondition() => APData.IsCurrentSlotEnabled();
        }
    }
}
