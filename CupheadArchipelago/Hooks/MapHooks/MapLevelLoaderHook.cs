/// Copyright 2025 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using CupheadArchipelago.AP;
using CupheadArchipelago.Unity;
using HarmonyLib;

namespace CupheadArchipelago.Hooks.MapHooks {
    internal class MapLevelLoaderHook {
        internal static void Hook() {
            Harmony.CreateAndPatchAll(typeof(Awake));
            //Harmony.CreateAndPatchAll(typeof(Activate));
        }

        /*
        [Error  : Unity Log] Exception: Asset not cached: TitleCards_W2
        Stack trace:
        SpriteAtlas AssetLoader<UnityEngine.U2D.SpriteAtlas>.GetCachedAsset(string assetName)
        void LocalizationHelper.ApplyTranslation(TranslationElement translationElement)
        void LocalizationHelper.ApplyTranslation(TranslationElement translationElement, LocalizationSubtext[] subTranslations)
        void MapDifficultySelectStartUI.In(MapPlayerController playerController)
        void MapLevelLoader.Activate(MapPlayerController player)
        void AbstractMapInteractiveEntity.Update()

        [Error  : Unity Log] Exception: Asset not cached: TitleCards_W3
        Stack trace:
        SpriteAtlas AssetLoader<UnityEngine.U2D.SpriteAtlas>.GetCachedAsset(string assetName)
        void LocalizationHelper.ApplyTranslation(TranslationElement translationElement)
        void LocalizationHelper.ApplyTranslation(TranslationElement translationElement, LocalizationSubtext[] subTranslations)
        void MapDifficultySelectStartUI.In(MapPlayerController playerController)
        void MapLevelLoader.Activate(MapPlayerController player)
        void AbstractMapInteractiveEntity.Update()

        [Error  : Unity Log] Exception: Asset not cached: TitleCards_WDLC
        Stack trace:
        SpriteAtlas AssetLoader<UnityEngine.U2D.SpriteAtlas>.GetCachedAsset(string assetName)
        void LocalizationHelper.ApplyTranslation(TranslationElement translationElement)
        void LocalizationHelper.ApplyTranslation(TranslationElement translationElement, LocalizationSubtext[] subTranslations)
        void MapDifficultySelectStartUI.In(MapPlayerController playerController)
        void MapLevelLoader.Activate(MapPlayerController player)
        void AbstractMapInteractiveEntity.Update()
        */

        [HarmonyPatch(typeof(MapLevelLoader), "Awake")]
        internal static class Awake {
            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator il) {
                List<CodeInstruction> codes = new(instructions);
                bool debug = false;

                FieldInfo _fi_level = typeof(MapLevelLoader).GetField("level", BindingFlags.NonPublic | BindingFlags.Instance);
                MethodInfo _mi_MapLevel = typeof(Awake).GetMethod("MapLevel", BindingFlags.NonPublic | BindingFlags.Static);

                Label vanilla = il.DefineLabel();

                if (debug) {
                    Dbg.LogCodeInstructions(codes);
                }
                codes[0].labels.Add(vanilla);
                List<CodeInstruction> ncodes = [
                    CodeInstruction.Call(() => APData.IsCurrentSlotEnabled()),
                    new(OpCodes.Brfalse, vanilla),
                    new(OpCodes.Ldarg_0),
                    new(OpCodes.Ldarg_0),
                    new(OpCodes.Ldfld, _fi_level),
                    new(OpCodes.Call, _mi_MapLevel),
                    new(OpCodes.Stfld, _fi_level),
                ];
                codes.InsertRange(0, ncodes);
                if (debug) {
                    Logging.Log("---");
                    Dbg.LogCodeInstructions(codes);
                }

                return codes;
            }
            static void Postfix(MapLevelLoader __instance) {
                if (__instance is MapLevelLoaderChaliceTutorial) {
                    if (APData.IsCurrentSlotEnabled() && APSettings.DLCChaliceMode == DlcChaliceModes.Disabled) {
                        __instance.gameObject.AddComponent<Disabler>().Init(__instance);
                    }
                }
            }

            private static Levels MapLevel(Levels level) =>
                LevelMap.GetMappedLevel(level);
        }

        [HarmonyPatch(typeof(MapLevelLoader), "Activate")]
        internal static class Activate { }
    }
}
