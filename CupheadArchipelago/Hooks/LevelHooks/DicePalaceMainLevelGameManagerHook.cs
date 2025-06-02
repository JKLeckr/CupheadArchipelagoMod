/// Copyright 2025 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using CupheadArchipelago.AP;
using CupheadArchipelago.Mapping;
using HarmonyLib;
using static DicePalaceMainLevelGameManager;

namespace CupheadArchipelago.Hooks.LevelHooks {
    internal class DicePalaceMainLevelGameManagerHook {
        internal static void Hook() {
            Harmony.CreateAndPatchAll(typeof(GameSetup));
        }

        private static readonly Dictionary<BoardSpaces, Levels> bossBoardToLevel = new() {
            {BoardSpaces.Booze, Levels.DicePalaceBooze},
            {BoardSpaces.Chips, Levels.DicePalaceChips},
            {BoardSpaces.Cigar, Levels.DicePalaceCigar},
            {BoardSpaces.Domino, Levels.DicePalaceDomino},
            {BoardSpaces.Rabbit, Levels.DicePalaceRabbit},
            {BoardSpaces.FlyingHorse, Levels.DicePalaceFlyingHorse},
            {BoardSpaces.Roulette, Levels.DicePalaceRoulette},
            {BoardSpaces.EightBall, Levels.DicePalaceEightBall},
            {BoardSpaces.FlyingMemory, Levels.DicePalaceFlyingMemory},
        };
        private static readonly Dictionary<Levels, BoardSpaces> bossLevelIdToBoard = [];

        static DicePalaceMainLevelGameManagerHook() {
            foreach (BoardSpaces key in bossBoardToLevel.Keys) {
                bossLevelIdToBoard.Add(bossBoardToLevel[key], key);
                if (!LevelMap.LevelExists(bossBoardToLevel[key])) {
                    throw new KeyNotFoundException("Boss Board Space missing in LevelMap");
                }
            }
        }

        [HarmonyPatch(typeof(DicePalaceMainLevelGameManager), "GameSetup")]
        internal static class GameSetup {
            static bool Prefix(ref BoardSpaces[] ___allBoardSpaces) {
                if (APData.IsCurrentSlotEnabled()) {
                    BoardSpaces[] nboardSpaces = (BoardSpaces[])___allBoardSpaces.Clone();

                    for (int i = 0; i < nboardSpaces.Length; i++) {
                        if (bossBoardToLevel.ContainsKey(nboardSpaces[i])) {
                            nboardSpaces[i] =
                                bossLevelIdToBoard[LevelMap.GetMappedLevel(bossBoardToLevel[nboardSpaces[i]])];
                        }
                    }
                    ___allBoardSpaces = nboardSpaces;
                }
                return true;
            }
        }
    }
}
