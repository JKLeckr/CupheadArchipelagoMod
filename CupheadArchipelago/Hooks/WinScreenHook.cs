/// Copyright 2025-2026 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using CupheadArchipelago.Config;
using HarmonyLib;

namespace CupheadArchipelago.Hooks {
    internal class WinScreenHook {
        internal static void Hook() {
            Harmony.CreateAndPatchAll(typeof(main_cr));
        }

        [HarmonyPatch(typeof(WinScreen), "main_cr", MethodType.Enumerator)]
        internal static class main_cr {
            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
                List<CodeInstruction> codes = new(instructions);
                int success = 0;
                bool debug = false;

                MethodInfo _mi_get_PreviousLevel = typeof(Level).GetProperty("PreviousLevel", BindingFlags.Public | BindingFlags.Static).GetGetMethod();
                MethodInfo _mi_APLevelTest = typeof(main_cr).GetMethod("APLevelTest", BindingFlags.NonPublic | BindingFlags.Static);

                if (debug) {
                    Dbg.LogCodeInstructions(codes);
                }
                for (int i=0; i<codes.Count-2; i++) {
                    if (codes[i].opcode == OpCodes.Call && (MethodInfo)codes[i].operand == _mi_get_PreviousLevel && codes[i+1].opcode == OpCodes.Ldc_I4 && codes[i+2].opcode == OpCodes.Bne_Un) {
                        List<Label> orig_labels = codes[i].labels;
                        codes.RemoveAt(i);
                        codes[i].labels.AddRange(orig_labels);
                        codes.Insert(i+1, new CodeInstruction(OpCodes.Call, _mi_APLevelTest));
                        codes[i+2].opcode = OpCodes.Brfalse;
                        success++;
                        i += 2;
                    }
                }
                if (success!=2) throw new Exception($"{nameof(main_cr)}: Patch Failed! {success}");
                if (debug) {
                    Logging.Log("---");
                    Dbg.LogCodeInstructions(codes);
                }

                return codes;
            }

            private static bool APLevelTest(Levels level) {
                bool origCond = Level.PreviousLevel == level;
                switch (level) {
                    case Levels.Devil:
                        if (MConf.IsSkippingCutscene(Cutscenes.EndCutscene)) {
                            Cutscene.Load(Scenes.scene_title, Scenes.scene_cutscene_credits, SceneLoader.Transition.Fade, SceneLoader.Transition.Fade, SceneLoader.Icon.None);
                            return false;
                        }
                        return origCond;
                    case Levels.Saltbaker:
                        if (MConf.IsSkippingCutscene(Cutscenes.DLCEndCutscene)) {
                            SceneLoader.LoadScene(Scenes.scene_cutscene_dlc_credits_comic, SceneLoader.Transition.Iris, SceneLoader.Transition.Iris, SceneLoader.Icon.None, null);
                            return false;
                        }
                        return origCond;
                    default:
                        return false;
                }
            }
        }
    }
}
