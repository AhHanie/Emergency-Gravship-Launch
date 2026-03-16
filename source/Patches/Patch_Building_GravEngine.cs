using System;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace GravshipEmergencyLaunch.Patches
{
    public static class Patch_Building_GravEngine
    {
        [HarmonyPatch(typeof(Building_GravEngine), "ConsumeFuel")]
        public static class ConsumeFuel
        {
            public static bool Prepare()
            {
                return ModsConfig.OdysseyActive;
            }

            public static void Prefix(Building_GravEngine __instance, ref bool __state)
            {
                __state = EmergencyLaunchUtility.IsEmergencyLaunchArmed(__instance);
            }

            public static void Postfix(Building_GravEngine __instance, PlanetTile tile, bool __state)
            {
                if (!GravshipUtility.TryGetPathFuelCost(__instance.Map.Tile, tile, out float fuelCost, out int _, 10f, __instance.FuelUseageFactor))
                {
                    if (__state)
                    {
                        EmergencyLaunchUtility.ClearArmedEmergencyLaunch();
                    }

                    return;
                }

                EmergencyLaunchWorldComponent component = EmergencyLaunchUtility.Component;
                component.NotifyLaunchFuelConsumed(__instance, fuelCost);
                if (__state)
                {
                    component.NotifyEmergencyLaunchStarted(__instance);
                }
                else
                {
                    component.NotifyNormalLaunchStarted(__instance);
                }

                if (__state)
                {
                    EmergencyLaunchUtility.ClearArmedEmergencyLaunch();
                }
            }
        }
    }
}
