/// Copyright 2025-2026 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using CupheadArchipelago.AP;
using CupheadArchipelago.Mapping;
using HarmonyLib;

namespace CupheadArchipelago.Hooks.MapHooks {
    internal class AbstractMapLevelDependentEntityHook {
        internal static void Hook() {
            Harmony.CreateAndPatchAll(typeof(Awake));
        }

        [HarmonyPatch(typeof(AbstractMapLevelDependentEntity), "Awake")]
        internal static class Awake {
            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator il) {
                List<CodeInstruction> codes = new(instructions);
                bool debug = false;

                FieldInfo _fi__levels = typeof(AbstractMapLevelDependentEntity).GetField("_levels", BindingFlags.NonPublic | BindingFlags.Instance);
                MethodInfo _mi_MapLevels = typeof(Awake).GetMethod("MapLevels", BindingFlags.NonPublic | BindingFlags.Static);

                Label vanilla = il.DefineLabel();

                if (debug) {
                    Dbg.LogCodeInstructions(codes);
                }
                codes[0].labels.Add(vanilla);
                CodeInstruction[] ncodes = [
                    CodeInstruction.Call(() => APData.IsCurrentSlotEnabled()),
                    new(OpCodes.Brfalse, vanilla),
                    new(OpCodes.Ldarg_0),
                    new(OpCodes.Ldarg_0),
                    new(OpCodes.Ldfld, _fi__levels),
                    new(OpCodes.Call, _mi_MapLevels),
                    new(OpCodes.Stfld, _fi__levels),
                ];
                codes.InsertRange(0, ncodes);
                if (debug) {
                    Dbg.LogCodeInstructions(codes);
                }

                return codes;
            }

            private static Levels[] MapLevels(Levels[] levels) {
                Levels[] res = new Levels[levels.Length];
                for (int i = 0; i < res.Length; i++) {
                    res[i] = LevelMap.GetMappedLevel(levels[i]);
                }
                return res;
            }
        }
    }
}
