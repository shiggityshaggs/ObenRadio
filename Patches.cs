using HarmonyLib;

namespace ObenRadio
{
    internal static class Patches
    {
        [HarmonyPatch(typeof(Radio), nameof(Radio.Interact)), HarmonyPrefix]
        static bool Radio_Interact(Radio __instance)
        {
            AudioLoader.Load(__instance);
            return false;
        }
    }
}
