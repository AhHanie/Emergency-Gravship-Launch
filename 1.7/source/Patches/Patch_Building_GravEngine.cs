using HarmonyLib;
using RimWorld;
using Verse;

namespace GravshipEmergencyLaunch.Patches
{
    public static class Patch_Building_GravEngine
    {
        [HarmonyPatch(typeof(Building_GravEngine), nameof(Building_GravEngine.ConsumeFuel))]
        public static class ConsumeFuel
        {
            public static bool Prepare()
            {
                return ModsConfig.OdysseyActive;
            }

            [HarmonyInfix(typeof(GravshipUtility), nameof(GravshipUtility.TryGetPathFuelCost))]
            public static bool Postfix(bool __result, float cost, [HarmonyOuter] Building_GravEngine __instance)
            {
                bool emergencyLaunch = EmergencyLaunchUtility.IsEmergencyLaunchArmed(__instance);
                if (__result)
                {
                    EmergencyLaunchWorldComponent component = EmergencyLaunchUtility.Component;
                    component.NotifyLaunchFuelConsumed(__instance, cost);
                    if (emergencyLaunch)
                    {
                        component.NotifyEmergencyLaunchStarted(__instance);
                    }
                    else
                    {
                        component.NotifyNormalLaunchStarted(__instance);
                    }
                }

                if (emergencyLaunch)
                {
                    EmergencyLaunchUtility.ClearArmedEmergencyLaunch();
                }

                return __result;
            }
        }
    }
}
