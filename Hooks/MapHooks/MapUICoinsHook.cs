/// Copyright 2024 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using CupheadArchipelago.AP;
using HarmonyLib;

namespace CupheadArchipelago.Hooks.MapHooks {
    internal class MapUICoinsHook {
        internal static void Hook() {
            //Harmony.CreateAndPatchAll(typeof(Start));
            Harmony.CreateAndPatchAll(typeof(Update));
        }

        [HarmonyPatch(typeof(MapUICoins), "Start")]
        internal static class Start {}

        [HarmonyPatch(typeof(MapUICoins), "Update")]
        internal static class Update {
            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
                List<CodeInstruction> codes = new(instructions);
                bool debug = false;
                int success = 0;

                FieldInfo _fi_Multiplayer = typeof(PlayerManager).GetField("Multiplayer", BindingFlags.Public | BindingFlags.Static);
                FieldInfo _fi_playerId = typeof(MapUICoins).GetField("playerId", BindingFlags.NonPublic | BindingFlags.Instance);
                MethodInfo _mi_get_Data = typeof(PlayerData).GetProperty("Data", BindingFlags.Public | BindingFlags.Static).GetGetMethod();
                MethodInfo _mi_GetCurrency = typeof(PlayerData).GetMethod("GetCurrency", BindingFlags.Public | BindingFlags.Instance);
                MethodInfo _mi_APMult = typeof(Update).GetMethod("APMult", BindingFlags.NonPublic | BindingFlags.Static);
                MethodInfo _mi_APGetCurrency = typeof(Update).GetMethod("APGetCurrency", BindingFlags.NonPublic | BindingFlags.Static);

                if (debug) {
                    foreach (CodeInstruction code in codes) {
                        Logging.Log($"{code.opcode}: {code.operand}");
                    }
                }
                for (int i=0;i<codes.Count-4;i++) {
                    if ((success&1)==0 && codes[i].opcode == OpCodes.Ldsfld && (FieldInfo)codes[i].operand == _fi_Multiplayer && codes[i+1].opcode == OpCodes.Brfalse) {
                        codes.Insert(i+1, new CodeInstruction(OpCodes.Call, _mi_APMult));
                        i++;
                        success |= 1;
                    }
                    if ((success&2)==0 && codes[i].opcode == OpCodes.Call && (MethodInfo)codes[i].operand == _mi_get_Data && codes[i+1].opcode == OpCodes.Ldarg_0 &&
                        codes[i+2].opcode == OpCodes.Ldfld && codes[i+3].opcode == OpCodes.Callvirt && (MethodInfo)codes[i+3].operand == _mi_GetCurrency &&
                        codes[i+4].opcode == OpCodes.Stloc_0) {
                            codes[i+3] = new CodeInstruction(OpCodes.Call, _mi_APGetCurrency);
                            codes[i+1].labels = codes[i].labels;
                            codes.RemoveAt(i);
                            success |= 2;
                    }
                    if (success==3) break;
                }
                if (success!=3) throw new Exception($"{nameof(Update)}: Patch Failed! {success}");
                if (debug) {
                    Logging.Log("---");
                    foreach (CodeInstruction code in codes) {
                        Logging.Log($"{code.opcode}: {code.operand}");
                    }
                }

                return codes;
            }

            private static bool APMult(bool multiplayer) => multiplayer || APData.IsCurrentSlotEnabled();
            private static int APGetCurrency(PlayerId playerId) {
                if (APData.IsCurrentSlotEnabled() && playerId != PlayerId.PlayerOne) {
                    if (playerId == PlayerId.PlayerTwo)
                        return APData.CurrentSData.playerData.contracts;
                    /*if (playerId == PlayerId.Any)
                        return APData.CurrentSData.playerData.dlc_ingredients;*/
                }
                return PlayerData.Data.GetCurrency(playerId);
            }
        }
    }
}
