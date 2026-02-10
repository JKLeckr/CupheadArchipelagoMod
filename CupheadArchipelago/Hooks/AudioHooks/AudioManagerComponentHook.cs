/// Copyright 2025-2026 JKLeckr
/// SPDX-License-Identifier: GPL-3.0-or-later

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;

namespace CupheadArchipelago.Hooks.AudioHooks {
    internal class AudioManagerComponentHook {
        internal static void Hook() { }

        [HarmonyPatch(typeof(AudioManagerComponent), "OnPlay")]
        internal static class OnPlay {
            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator il) {
                List<CodeInstruction> codes = new(instructions);
                bool success = false;
                bool debug = false;

                FieldInfo _fi_dict = typeof(AudioManagerComponent).GetField("dict", BindingFlags.NonPublic | BindingFlags.Instance);
                MethodInfo _mi_AudioNotFoundResponse = typeof(OnPlay).GetMethod("AudioNotFoundResponse", BindingFlags.NonPublic | BindingFlags.Static);

                Label l_ctrue = il.DefineLabel();

                if (debug) {
                    Dbg.LogCodeInstructions(codes);
                }
                for (int i = 0; i < codes.Count - 5; i++) {
                    if (codes[i].opcode == OpCodes.Ldarg_0 && codes[i + 1].opcode == OpCodes.Ldfld && (FieldInfo)codes[i + 1].operand == _fi_dict &&
                        codes[i + 2].opcode == OpCodes.Ldarg_1 && codes[i + 3].opcode == OpCodes.Callvirt && codes[i + 4].opcode == OpCodes.Brfalse
                    ) {
                        Label endlabel = (Label)codes[i + 4].operand;
                        codes[i + 5].labels.Add(l_ctrue);
                        codes[i + 4] = new(OpCodes.Brtrue, l_ctrue);

                        CodeInstruction[] ncodes = [
                            new(OpCodes.Ldarg_1),
                            new(OpCodes.Call, _mi_AudioNotFoundResponse),
                            new(OpCodes.Br, endlabel),
                        ];
                        codes.InsertRange(i + 5, ncodes);

                        success = true;
                        break;
                    }
                }
                if (!success) throw new Exception($"{nameof(OnPlay)}: Patch Failed!");
                if (debug) {
                    Logging.Log("---");
                    Dbg.LogCodeInstructions(codes);
                }

                return codes;
            }

            private static void AudioNotFoundResponse(string key) {
                Logging.LogWarning($"[AudioManagerComponent] Audio \"{key}\" not found. Not playing.");
            }
        }
    }
}
