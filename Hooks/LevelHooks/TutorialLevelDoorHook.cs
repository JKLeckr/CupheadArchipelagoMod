/// Copyright 2024 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using CupheadArchipelago.AP;
using HarmonyLib;

namespace CupheadArchipelago.Hooks.LevelHooks {
    internal class TutorialLevelDoorHook {
        internal static void Hook() {
            Harmony.CreateAndPatchAll(typeof(Activate));
        }

        [HarmonyPatch(typeof(TutorialLevelDoor), "Activate")]
        internal static class Activate {
            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
                List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
                bool debug = false;
                bool success = false;
                FieldInfo _fi_isChaliceTutorial = typeof(TutorialLevelDoor).GetField("isChaliceTutorial", BindingFlags.Instance | BindingFlags.NonPublic);
                MethodInfo _mi_base_Activate = typeof(AbstractLevelInteractiveEntity).GetMethod(
                    "Activate", BindingFlags.Instance | BindingFlags.NonPublic, null, new Type[] {}, null);
                MethodInfo _mi_APCheck = typeof(Activate).GetMethod("APCheck", BindingFlags.Static | BindingFlags.NonPublic);

                if (debug) {
                    for (int i = 0; i < codes.Count; i++) {
                        Plugin.Log($"{codes[i].opcode}: {codes[i].operand}");
                    }
                }
                for (int i = 0; i < codes.Count - 2; i++) {
                    if (codes[i].opcode == OpCodes.Ldarg_0 && codes[i+1].opcode == OpCodes.Call && (MethodInfo)codes[i+1].operand == _mi_base_Activate) {
                        codes.Insert(i+2, new CodeInstruction(OpCodes.Ldarg_0));
                        codes.Insert(i+3, new CodeInstruction(OpCodes.Ldfld, _fi_isChaliceTutorial));
                        codes.Insert(i+4, new CodeInstruction(OpCodes.Call, _mi_APCheck));
                        success = true;
                        break;
                    }
                }
                if (!success) throw new Exception($"{nameof(Activate)}: Patch Failed!");
                //if (!success) Plugin.Log("Patch failed", BepInEx.Logging.LogLevel.Warning);
                if (debug) {
                    Plugin.Log("---");
                    for (int i = 0; i < codes.Count; i++) {
                        Plugin.Log($"{codes[i].opcode}: {codes[i].operand}");
                    }
                }

                return codes;
            }

            private static void APCheck(bool isChaliceTutorial) {
                Plugin.Log("[TutorialLevelDoor] APCheck");
                if (APData.IsCurrentSlotEnabled()) {
                    if (!isChaliceTutorial) {
                        APClient.Check(APLocation.level_tutorial);
                    }
                    else {
                        APClient.Check(APLocation.level_dlc_tutorial);
                    }
                }
            }
        }
    }
}