using HarmonyLib;

namespace CupheadArchipelago.Hooks {
    public class CupheadHook {
        public static void Hook() {}

        [HarmonyPatch(typeof(Cuphead), "Update")]
        internal static class Update {
            static void Postfix() {}
        }
    }
}