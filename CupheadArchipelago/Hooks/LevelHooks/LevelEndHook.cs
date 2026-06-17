/// Copyright 2025-2026 JKLeckr
/// SPDX-License-Identifier: GPL-3.0-or-later

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;

namespace CupheadArchipelago.Hooks.LevelHooks {
    internal class LevelEndHook {
        internal static void Hook() {
            //Harmony.CreateAndPatchAll(typeof(win_cr));
        }

        [HarmonyPatch(typeof(LevelEnd), "win_cr", MethodType.Enumerator)]
        internal static class win_cr {
            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
                List<CodeInstruction> codes = new(instructions);
                bool success = false;
                bool debug = false;

                ConstructorInfo _ci_GauntletContext = typeof(GauntletContext).GetConstructor([typeof(bool)]);
                MethodInfo _mi_LoadScene = typeof(SceneLoader).GetMethod(
                    "LoadScene",
                    BindingFlags.Public | BindingFlags.Static,
                    null,
                    [
                        typeof(Scenes),
                        typeof(SceneLoader.Transition),
                        typeof(SceneLoader.Transition),
                        typeof(SceneLoader.Icon),
                        typeof(SceneLoader.Context)
                    ],
                    null
                );
                MethodInfo _mi_APCheck = typeof(win_cr).GetMethod("APCheck", BindingFlags.NonPublic | BindingFlags.Static);

                if (debug) {
                    Dbg.LogCodeInstructions(codes);
                }
                for (int i = 0; i < codes.Count - 6; i++) {
                    if (codes[i].opcode == OpCodes.Ldc_I4_S && (sbyte)codes[i].operand == (sbyte)Scenes.scene_level_chess_castle &&
                        codes[i + 4].opcode == OpCodes.Ldc_I4_1 && codes[i + 5].opcode == OpCodes.Newobj && (ConstructorInfo)codes[i + 5].operand == _ci_GauntletContext &&
                        codes[i + 6].opcode == OpCodes.Call && (MethodInfo)codes[i + 6].operand == _mi_LoadScene
                    ) {
                        codes.Insert(i, new CodeInstruction(OpCodes.Call, _mi_APCheck));
                        success = true;
                        break;
                    }
                }
                if (!success) throw new Exception($"{nameof(win_cr)}: Patch Failed!");
                if (debug) {
                    Logging.Log("---");
                    Dbg.LogCodeInstructions(codes);
                }

                return codes;
            }

            private static void APCheck() {
                Logging.LogWarning("empty");
                /*if (APData.IsCurrentSlotEnabled()) {
                    long locId = LevelHookBase.IsChalice() ? locationIdRunChaliced : locationIdRun;
                    if (!APClient.IsLocationChecked(locId))
                        APClient.Check(locId);
                }*/
            }
        }
    }
}
