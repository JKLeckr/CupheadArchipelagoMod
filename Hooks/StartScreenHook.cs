using CupheadArchipelago.AP;
using HarmonyLib;

namespace CupheadArchipelago.Hooks {
    public class StartScreenHook {
        public static void Hook() {
            Harmony.CreateAndPatchAll(typeof(Awake));
        }

        [HarmonyPatch(typeof(StartScreen), "Awake")]
        internal static class Awake {
            static void Postfix() {
                APClient.CloseArchipelagoSession();
            }
        }
    }
}