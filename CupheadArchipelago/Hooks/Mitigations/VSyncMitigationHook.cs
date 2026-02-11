/// Copyright 2025-2026 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using CupheadArchipelago.Config;
using HarmonyLib;
using UnityEngine;

namespace CupheadArchipelago.Hooks.Mitigations {
    internal class VSyncMitigationHook {
        internal static void Hook() {
            if (MConf.IsVSyncFixEnabled()) {
                Logging.Log("Applying VSync mitigations.");
                Harmony.CreateAndPatchAll(typeof(ApplySettingsOnStartup));
                Harmony.CreateAndPatchAll(typeof(SetupButtons));
                Harmony.CreateAndPatchAll(typeof(VisualHorizontalSelect));
            }
        }

        [HarmonyPatch(typeof(SettingsData), "ApplySettingsOnStartup")]
        internal static class ApplySettingsOnStartup {
            static void Postfix() {
                QualitySettings.vSyncCount = GetFixedVSyncCount(SettingsData.Data.vSyncCount);
            }
        }

        [HarmonyPatch(typeof(OptionsGUI), "SetupButtons")]
        internal static class SetupButtons {
            static void Postfix(bool ___isConsole, OptionsGUI.Button[] ___visualObjectButtons) {
                if (!___isConsole) {
                    OptionsGUI.Button button = ___visualObjectButtons[2];
                    if (button.selection == 0 && !IsScreenVsyncCompatible())
                        button.text.text += "*";
                }
            }
        }

        [HarmonyPatch(typeof(OptionsGUI), "VisualHorizontalSelect")]
        internal static class VisualHorizontalSelect {
            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
                List<CodeInstruction> codes = new(instructions);
                bool success = false;
                bool debug = false;

                MethodInfo _mi_get_Data = typeof(SettingsData).GetProperty("Data", BindingFlags.Public | BindingFlags.Static).GetGetMethod();
                MethodInfo _mi_set_vSyncCount = typeof(QualitySettings).GetProperty("vSyncCount", BindingFlags.Public | BindingFlags.Static).GetSetMethod();
                MethodInfo _mi_GetFixedVSyncCount = typeof(VSyncMitigationHook).GetMethod("GetFixedVSyncCount", BindingFlags.NonPublic | BindingFlags.Static);
                MethodInfo _mi_UpdateButtonText = typeof(VisualHorizontalSelect).GetMethod("UpdateButtonText", BindingFlags.NonPublic | BindingFlags.Static);
                FieldInfo _fi_vSyncCount = typeof(SettingsData).GetField("vSyncCount", BindingFlags.Public | BindingFlags.Instance);

                if (debug) {
                    Dbg.LogCodeInstructions(codes);
                }
                for (int i=0;i<codes.Count-3;i++) {
                    if (codes[i].opcode == OpCodes.Call && (MethodInfo)codes[i].operand == _mi_get_Data && codes[i+1].opcode == OpCodes.Ldfld && (FieldInfo)codes[i+1].operand == _fi_vSyncCount &&
                        codes[i+2].opcode == OpCodes.Call && (MethodInfo)codes[i+2].operand == _mi_set_vSyncCount && codes[i+3].opcode == OpCodes.Br) {
                            codes.Insert(i+2, new CodeInstruction(OpCodes.Call, _mi_GetFixedVSyncCount));
                            List<CodeInstruction> ncodes = [
                                new CodeInstruction(OpCodes.Ldarg_1),
                                new CodeInstruction(OpCodes.Call, _mi_UpdateButtonText)
                            ];
                            codes.InsertRange(i, ncodes);
                            success = true;
                    }
                }
                if (!success) throw new Exception($"{nameof(VisualHorizontalSelect)}: Patch Failed!");
                if (debug) {
                    Logging.Log("---");
                    Dbg.LogCodeInstructions(codes);
                }

                return codes;
            }

            private static void UpdateButtonText(OptionsGUI.Button button) {
                if (button.selection == 0 && !IsScreenVsyncCompatible())
                    button.text.text += "*";
            }
        }

        private static bool IsScreenVsyncCompatible() => Screen.currentResolution.refreshRate % 60 == 0;

        private static int GetFixedVSyncCount(int count) {
            if (count > 0 && IsScreenVsyncCompatible()) {
                return Screen.currentResolution.refreshRate / 60;
            }
            if (count > 0) Logging.LogWarning("[VSyncMitigator] Display not vsync compatible. Disabling vsync to prevent game misbehavior. If you want to override this, disable \"VSyncFix\" in the config.");
            return 0;
        }
    }
}
