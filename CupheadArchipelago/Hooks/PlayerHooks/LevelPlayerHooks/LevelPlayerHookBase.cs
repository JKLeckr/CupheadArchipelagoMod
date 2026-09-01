/// Copyright 2025-2026 JKLeckr
/// SPDX-License-Identifier: GPL-3.0-or-later

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using CupheadArchipelago.AP;
using HarmonyLib;

namespace CupheadArchipelago.Hooks.PlayerHooks.LevelPlayerHooks {
    internal class LevelPlayerHookBase {
        internal static class LevelPlayerParryChessHookBase {
            internal static IEnumerable<CodeInstruction> LevelPlayerParryChessHookTranspiler(IEnumerable<CodeInstruction> instructions, bool debug = false) {
                List<CodeInstruction> codes = new(instructions);
                int success = 0;

                FieldInfo _fi_charm = typeof(PlayerData.PlayerLoadouts.PlayerLoadout).GetField("charm", BindingFlags.Public | BindingFlags.Instance);
                MethodInfo _mi_get_player = typeof(AbstractLevelPlayerComponent).GetProperty("player", BindingFlags.Public | BindingFlags.Instance).GetGetMethod();
                MethodInfo _mi_get_Loadout = typeof(PlayerStatsManager).GetProperty("Loadout", BindingFlags.Public | BindingFlags.Instance).GetGetMethod();
                MethodInfo _mi_get_IsChessBoss = typeof(Level).GetProperty("IsChessBoss", BindingFlags.Public | BindingFlags.Static).GetGetMethod();
                MethodInfo _mi_APCondition = typeof(LevelPlayerParryChessHookBase).GetMethod("APCondition", BindingFlags.NonPublic | BindingFlags.Static);

                if (debug) {
                    Dbg.LogCodeInstructions(codes);
                }
                for (int i = 0; i < codes.Count - 8; i++) {
                    if (codes[i].opcode == OpCodes.Ldarg_0 && codes[i + 1].opcode == OpCodes.Call && (MethodInfo)codes[i + 1].operand == _mi_get_player && codes[i + 2].opcode == OpCodes.Callvirt &&
                        codes[i + 3].opcode == OpCodes.Callvirt && (MethodInfo)codes[i + 3].operand == _mi_get_Loadout && codes[i + 4].opcode == OpCodes.Ldfld &&
                        (FieldInfo)codes[i + 4].operand == _fi_charm && codes[i + 5].opcode == OpCodes.Ldc_I4 && (int)codes[i + 5].operand == (int)Charm.charm_parry_plus &&
                        codes[i + 6].opcode == OpCodes.Bne_Un && codes[i + 7].opcode == OpCodes.Call && (MethodInfo)codes[i + 7].operand == _mi_get_IsChessBoss && codes[i + 8].opcode == OpCodes.Brtrue)
                    {
                        codes.Insert(i + 8, new(OpCodes.Call, _mi_APCondition));
                        i++;
                        success++;
                    }
                }
                if (success != 1) throw new Exception($"{nameof(LevelPlayerParryChessHookBase)}: Patch Failed! {success}");
                if (debug) {
                    Logging.Log("---");
                    Dbg.LogCodeInstructions(codes);
                }

                return codes;
            }

            private static bool APCondition(bool orig) {
                /// When true, (orig is IsChessCastle), P Sugar parrying is disabled.
                //Logging.Log($"{orig} || ({APData.IsCurrentSlotEnabled()} && {APSettings.DlcChessPSugar})");
                return orig && !(APData.IsCurrentSlotEnabled() && APSettings.DlcChessPSugar);
            }
        }
    }
}
