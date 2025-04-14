/// Copyright 2025 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

using System;
using System.Collections.Generic;
using CupheadArchipelago.AP;
using CupheadArchipelago.Util;
using HarmonyLib;
using static DicePalaceMainLevelGameManager;

namespace CupheadArchipelago.Hooks.LevelHooks {
    internal class DicePalaceMainLevelGameManagerHook {
        internal static void Hook() {
            Harmony.CreateAndPatchAll(typeof(GameSetup));
        }

        [HarmonyPatch(typeof(DicePalaceMainLevelGameManager), "GameSetup")]
        internal static class GameSetup {
            static bool Prefix(ref BoardSpaces[] ___allBoardSpaces, ref DicePalaceMainLevelBoardSpace[] ___boardSpacesObj) {
                if (APData.IsCurrentSlotEnabled()) {
                    string seed = APClient.APSessionGSData.seed;
                    Random rand = new(seed.GetHashCode());
                    BoardSpaces[] nboardSpaces = (BoardSpaces[])___allBoardSpaces.Clone();
                    //DicePalaceMainLevelBoardSpace[] nboardSpacesObj = (DicePalaceMainLevelBoardSpace[])___boardSpacesObj.Clone();
                    List<int> boardSpaceIndexes = [];
                    List<int> boardSpaceShuffledIndexes = [];

                    for (int i=0;i<___allBoardSpaces.Length;i++) {
                        if (___allBoardSpaces[i] != BoardSpaces.FreeSpace && ___allBoardSpaces[i] != BoardSpaces.StartOver) {
                            boardSpaceIndexes.Add(i);
                            boardSpaceShuffledIndexes.Add(i);
                        }
                    }
                    Aux.Shuffle(boardSpaceShuffledIndexes, rand);
                    for (int i=0;i<boardSpaceIndexes.Count;i++) {
                        int bsindex = boardSpaceIndexes[i];
                        int bssindex = boardSpaceShuffledIndexes[i];
                        nboardSpaces[bsindex] = ___allBoardSpaces[bssindex];
                        //nboardSpacesObj[bsindex+1] = ___boardSpacesObj[bssindex+1];
                    }
                    ___allBoardSpaces = nboardSpaces;
                    //___boardSpacesObj = nboardSpacesObj;
                }
                return true;
            }
        }
    }
}
