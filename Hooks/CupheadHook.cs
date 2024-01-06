using HarmonyLib;

namespace CupheadArchipelago.Hooks {
    public class CupheadHook {
        public static void Hook() {}

        // TODO: Add a control panel attached to this.

        [HarmonyPatch(typeof(Cuphead), "Update")]
        internal static class Update {
            static void Postfix() {}
        }
    }
}