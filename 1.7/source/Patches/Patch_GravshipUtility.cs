using HarmonyLib;
using RimWorld;
using Verse;

namespace GravshipEmergencyLaunch.Patches
{
    public static class Patch_GravshipUtility
    {
        [HarmonyPatch(typeof(GravshipUtility), nameof(GravshipUtility.NegativeLandingOutcomeFromQuality))]
        public static class NegativeLandingOutcomeFromQuality
        {
            public static bool Prepare()
            {
                return ModsConfig.OdysseyActive;
            }

            public static bool Prefix(ref float __result)
            {
                if (!Patch_WorldComponent_GravshipController.ForceGuaranteedOutcomeChanceDisplay)
                {
                    return true;
                }

                __result = 1f;
                return false;
            }
        }
    }
}
