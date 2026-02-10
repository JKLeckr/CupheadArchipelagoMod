/// Copyright 2025-2026 JKLeckr
/// SPDX-License-Identifier: GPL-3.0-or-later

using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using CupheadArchipelago.AP;
using UnityEngine;
using HarmonyLib;

namespace CupheadArchipelago.Hooks.CutsceneHooks {
    internal class CreditsScreenHook {
        internal static void Hook() {
            Harmony.CreateAndPatchAll(typeof(Start));
            Harmony.CreateAndPatchAll(typeof(credits_cr));
            Harmony.CreateAndPatchAll(typeof(skip_cr));
        }

        [HarmonyPatch(typeof(CreditsScreen), "Start")]
        internal static class Start {
            static bool Prefix(CreditsScreen __instance) {
                __instance.StartCoroutine(SendChecks_cr());
                return true;
            }

            private static IEnumerator SendChecks_cr() {
                while (SceneLoader.CurrentlyLoading) {
                    yield return null;
                }
                APClient.SendChecks(true);
            }
        }

        [HarmonyPatch(typeof(CreditsScreen), "credits_cr", MethodType.Enumerator)]
        internal static class credits_cr {
            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator il) {
                List<CodeInstruction> codes = new(instructions);
                int success = 0;
                bool debug = false;

                if (debug) {
                    Dbg.LogCodeInstructions(codes);
                }
                success |= APCommonPatch(ref codes, il);
                if (success!=1) throw new Exception($"{nameof(credits_cr)}: Patch Failed! {success}");
                if (debug) {
                    Logging.Log("---");
                    Dbg.LogCodeInstructions(codes);
                }

                return codes;
            }
        }

        [HarmonyPatch(typeof(CreditsScreen), "skip_cr", MethodType.Enumerator)]
        internal static class skip_cr {
            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator il) {
                List<CodeInstruction> codes = new(instructions);
                int success = 0;
                bool debug = false;

                if (debug) {
                    Dbg.LogCodeInstructions(codes);
                }
                success |= APCommonPatch(ref codes, il);
                if (success!=1) throw new Exception($"{nameof(skip_cr)}: Patch Failed! {success}");
                if (debug) {
                    Logging.Log("---");
                    Dbg.LogCodeInstructions(codes);
                }

                return codes;
            }
        }

        private static int APCommonPatch(ref List<CodeInstruction> codes, ILGenerator il) {
            int success = 0;

            MethodInfo _mi_ResetPlayers = typeof(PlayerManager).GetMethod("ResetPlayers", BindingFlags.Public | BindingFlags.Static);
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

            Label endl = il.DefineLabel();

            for (int i=0; i<codes.Count-7; i++) {
                if (codes[i].opcode == OpCodes.Call && (MethodInfo)codes[i].operand == _mi_ResetPlayers && codes[i+6].opcode == OpCodes.Call && (MethodInfo)codes[i+6].operand == _mi_LoadScene) {
                    codes[i+7].labels.Add(endl);

                    CodeInstruction[] ncodes = [
                        CodeInstruction.Call(() => APLoadScene()),
                        new CodeInstruction(OpCodes.Brfalse_S, endl)
                    ];
                    codes.InsertRange(i, ncodes);

                    success++;
                    i += 7;
                }
            }

            return success;
        }
        private static bool APLoadScene() {
            if (APData.IsCurrentSlotEnabled()) {
                SceneLoader.LoadLastMap();
                return false;
            }
            return true;
        }
    }
}
