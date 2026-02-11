/// Copyright 2025-2026 JKLeckr
/// SPDX-License-Identifier: Apache-2.0

using System.Collections.Generic;
using CupheadArchipelago.AP;

namespace CupheadArchipelago.Mapping {
    public class CoinIdMap {
        private static readonly Dictionary<string,APLocation> idToLoc = new() {
            {"scene_level_tutorial::Level_Coin :: a53bbd1a-734e-4e60-ada8-d11c62eabcec", APLocation.level_tutorial_coin},

            {"675028e9-b9d6-4d31-8536-8ff8e98e2ddf", APLocation.coin_isle1_secret},
            {"scene_level_platforming_1_1F::Level_Coin :: 5fd52d1b-a7f2-43a6-80e2-cb170cbc7d4d", APLocation.level_rungun_forest_coin1},
            {"scene_level_platforming_1_1F::Level_Coin :: 63c021bf-52f0-41de-bedf-c77117d244cc", APLocation.level_rungun_forest_coin2},
            {"scene_level_platforming_1_1F::Level_Coin :: 245037a6-1fa2-4167-a631-0723abff8138", APLocation.level_rungun_forest_coin3},
            {"scene_level_platforming_1_1F::Level_Coin :: eaefb009-c117-4b9a-96c1-7abc5558d213", APLocation.level_rungun_forest_coin4},
            {"scene_level_platforming_1_1F::Level_Coin :: 5526f7bc-a902-4c13-9e7a-1632a5abe378", APLocation.level_rungun_forest_coin5},

            {"scene_level_platforming_1_2F::Level_Coin :: 323989de-349e-4740-a764-dbc12217a27c", APLocation.level_rungun_tree_coin1},
            {"scene_level_platforming_1_2F::Level_Coin :: 55a46261-b14c-4065-9ada-18524eaed9f3", APLocation.level_rungun_tree_coin2},
            {"scene_level_platforming_1_2F::Level_Coin :: da0983f6-62d4-4ace-81f2-cad7181d5fe9", APLocation.level_rungun_tree_coin3},
            {"scene_level_platforming_1_2F::Level_Coin :: 7088ec51-4792-49c0-ab2c-c45ec9deb9f0", APLocation.level_rungun_tree_coin4},
            {"scene_level_platforming_1_2F::Level_Coin :: e02954c1-ff76-4ba4-849f-90aae53a7787", APLocation.level_rungun_tree_coin5},

            {"1782e7b4-2edf-45c0-b312-3083397307bf", APLocation.coin_isle2_secret},
            {"scene_level_platforming_2_1F::Level_Coin :: 24ef654a-a65b-4a1c-b5e5-c3c64e250646", APLocation.level_rungun_circus_coin1},
            {"scene_level_platforming_2_1F::Level_Coin :: b8d96f03-d264-4a61-9ab9-07de34f660aa", APLocation.level_rungun_circus_coin2},
            {"scene_level_platforming_2_1F::Level_Coin :: 383d9b3b-c280-4825-a6b3-1a21fe42d0ac", APLocation.level_rungun_circus_coin3},
            {"scene_level_platforming_2_1F::Level_Coin :: f1b99bcd-0fa8-4aac-9a54-f310e173ddf9", APLocation.level_rungun_circus_coin4},
            {"scene_level_platforming_2_1F::Level_Coin :: c763ef21-2ee7-491c-a143-b906856fed6c", APLocation.level_rungun_circus_coin5},

            {"scene_level_platforming_2_2F::Level_Coin :: 9025a0e9-fff1-4f14-93d1-1930eef27405", APLocation.level_rungun_funhouse_coin1},
            {"scene_level_platforming_2_2F::Level_Coin :: 284ea6f9-5db4-4f80-b0e5-1d9513a8acb7", APLocation.level_rungun_funhouse_coin2},
            {"scene_level_platforming_2_2F::Level_Coin :: 43a8fc82-b8b8-4a92-b56f-c3e718b46b2c", APLocation.level_rungun_funhouse_coin3},
            {"scene_level_platforming_2_2F::Level_Coin :: bf86d025-4524-4ce8-ba07-540ef3f61ed8", APLocation.level_rungun_funhouse_coin4},
            {"scene_level_platforming_2_2F::Level_Coin :: a7c0e2b9-9560-4ed7-a3a4-428365222cb9", APLocation.level_rungun_funhouse_coin5},

            {"e312336e-010f-4ea4-975b-922aca63629e", APLocation.coin_isle3_secret},
            {"scene_level_platforming_3_1F::Level_Coin :: 26ba2e1d-4b0a-4964-ba4d-f58655ef47db", APLocation.level_rungun_harbour_coin1},
            {"scene_level_platforming_3_1F::Level_Coin :: 0f13fbe6-1041-445f-97ed-1bbe2cb0339e", APLocation.level_rungun_harbour_coin2},
            {"scene_level_platforming_3_1F::Level_Coin :: 0086a9b3-87b8-4406-b97b-b94a1fd60bb0", APLocation.level_rungun_harbour_coin3},
            {"scene_level_platforming_3_1F::Level_Coin :: 0a6fbbe4-5c13-4b17-9b58-91e7bbdacde4", APLocation.level_rungun_harbour_coin4},
            {"scene_level_platforming_3_1F::Level_Coin :: beb664ad-5577-4055-9164-b1b2f77430f3", APLocation.level_rungun_harbour_coin5},

            {"scene_level_platforming_3_2F::Level_Coin :: 5da68904-6505-4841-9684-71d2931c1bd6", APLocation.level_rungun_mountain_coin1},
            {"scene_level_platforming_3_2F::Level_Coin :: 999c9b0d-d554-471d-ad96-ee6d57ccfd19", APLocation.level_rungun_mountain_coin2},
            {"scene_level_platforming_3_2F::Level_Coin :: cf0a7cae-d8d9-4be0-9502-8b8544606e04", APLocation.level_rungun_mountain_coin3},
            {"scene_level_platforming_3_2F::Level_Coin :: e671db16-cf6e-421c-937c-2b6f5c7ad0e7", APLocation.level_rungun_mountain_coin4},
            {"scene_level_platforming_3_2F::Level_Coin :: 084a7b75-e752-452f-8710-687db1e165fe", APLocation.level_rungun_mountain_coin5},

            {"43dfad5b-65dc-42f1-9ab3-25e0174f4ee8", APLocation.coin_isleh_secret},

            {"619e92f1-e0fd-4f6e-9c2d-5ce5dbaf393f", APLocation.dlc_coin_isle4_secret},
            {"scene_level_chalice_tutorial::Level_Coin :: 578c0218-df9e-4cdd-932a-a1277b5b7129", APLocation.level_dlc_tutorial_coin},
        };

        public static bool CoinIDExists(string coinId) => idToLoc.ContainsKey(coinId);
        public static APLocation GetAPLocation(string coinId) => idToLoc[coinId];
    }
}
