using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace GravshipEmergencyLaunch.Patches
{
    public static class Patch_WorldComponent_GravshipController
    {
        public static bool ForceGuaranteedOutcomeChanceDisplay { get; private set; }

        public struct EmergencyLandingState
        {
            public Gravship gravship;

            public bool wasEmergencyLaunch;
        }

        [HarmonyPatch(typeof(WorldComponent_GravshipController), "LandingEnded")]
        public static class LandingEnded
        {
            public static bool Prepare()
            {
                return ModsConfig.OdysseyActive;
            }

            public static void Prefix(ref EmergencyLandingState __state, Gravship ___gravship)
            {
                __state.gravship = ___gravship;
                __state.wasEmergencyLaunch = false;

                EmergencyLaunchWorldComponent component = EmergencyLaunchUtility.Component;
                __state.wasEmergencyLaunch = component.ConsumeEmergencyLandingPending(___gravship.Engine);
                if (__state.wasEmergencyLaunch && ___gravship.Engine.launchInfo != null)
                {
                    component.NotifyEmergencyLaunchCompleted(___gravship.Engine);
                    ___gravship.Engine.launchInfo.doNegativeOutcome = false;
                }
            }

            public static void Postfix(EmergencyLandingState __state)
            {
                if (__state.wasEmergencyLaunch)
                {
                    List<LandingOutcomeDef> outcomes = EmergencyLaunchUtility.GetEmergencyLandingOutcomes(2);
                    ForceGuaranteedOutcomeChanceDisplay = true;
                    try
                    {
                        for (int i = 0; i < outcomes.Count; i++)
                        {
                            outcomes[i].Worker.ApplyOutcome(__state.gravship);
                        }
                    }
                    finally
                    {
                        ForceGuaranteedOutcomeChanceDisplay = false;
                    }
                }

            }
        }
    }
}
