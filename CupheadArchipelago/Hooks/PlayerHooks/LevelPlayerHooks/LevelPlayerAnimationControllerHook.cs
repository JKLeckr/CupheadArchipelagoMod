/// Copyright 2025-2026 JKLeckr
/// SPDX-License-Identifier: GPL-3.0-or-later

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using CupheadArchipelago.AP;
using HarmonyLib;

namespace CupheadArchipelago.Hooks.PlayerHooks.LevelPlayerHooks {
    internal class LevelPlayerAnimationControllerHook {
        internal static void Hook() {
            Harmony.CreateAndPatchAll(typeof(Update));
            Harmony.CreateAndPatchAll(typeof(OnChaliceDashSparkle));
        }

        [HarmonyPatch(typeof(LevelPlayerAnimationController), "Update")]
        internal static class Update {
            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
                List<CodeInstruction> codes = new(instructions);
                bool success = false;
                bool debug = false;

                FieldInfo _fi_isChalice = typeof(PlayerStatsManager).GetField("isChalice", BindingFlags.Public | BindingFlags.Instance);
                FieldInfo _fi_chaliceDashEffect = typeof(LevelPlayerAnimationController).GetField("chaliceDashEffect", BindingFlags.NonPublic | BindingFlags.Instance);
                MethodInfo _mi_get_player = typeof(AbstractLevelPlayerComponent).GetProperty("player", BindingFlags.Public | BindingFlags.Instance).GetGetMethod();
                MethodInfo _mi_APCondition = typeof(Update).GetMethod("APCondition", BindingFlags.NonPublic | BindingFlags.Static);

                if (debug) {
                    Dbg.LogCodeInstructions(codes);
                }
                for (int i=0; i<codes.Count-7;i++) {
                    if (codes[i].opcode == OpCodes.Ldarg_0 && codes[i+1].opcode == OpCodes.Call && (MethodInfo)codes[i+1].operand == _mi_get_player &&
                        codes[i+2].opcode == OpCodes.Callvirt && codes[i+3].opcode == OpCodes.Ldfld && (FieldInfo)codes[i+3].operand == _fi_isChalice &&
                        codes[i+4].opcode == OpCodes.Brfalse && codes[i+5].opcode == OpCodes.Ldarg_0 && codes[i+6].opcode == OpCodes.Ldarg_0 &&
                        codes[i+7].opcode == OpCodes.Ldfld && (FieldInfo)codes[i+7].operand == _fi_chaliceDashEffect) {
                            codes.Insert(i+4, new CodeInstruction(OpCodes.Call, _mi_APCondition));
                            success = true;
                            break;
                    }
                }
                if (!success) throw new Exception($"{nameof(Update)}: Patch Failed!");
                if (debug) {
                    Logging.Log("---");
                    Dbg.LogCodeInstructions(codes);
                }

                return codes;
            }
            private static bool APCondition(bool orig) {
                return orig && (!APData.IsCurrentSlotEnabled() || APClient.APSessionGSPlayerData.dlc_cparry);
            }
        }

        [HarmonyPatch(typeof(LevelPlayerAnimationController), "OnChaliceDashSparkle")]
        internal static class OnChaliceDashSparkle {
            static bool Prefix() {
                return !APData.IsCurrentSlotEnabled() || APClient.APSessionGSPlayerData.dlc_cparry;
            }
        }
    }
}
