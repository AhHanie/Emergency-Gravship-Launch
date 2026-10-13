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

        [HarmonyPatch(typeof(WorldComponent_GravshipController), "LandingEnded")]
        public static class LandingEnded
        {
            private static Gravship emergencyGravship;

            public static bool Prepare()
            {
                return ModsConfig.OdysseyActive;
            }

            [HarmonyPostfix]
            [HarmonyInfix(typeof(LaunchInfo), nameof(LaunchInfo.doNegativeOutcome), InnerTargetKind.FieldRead)]
            public static bool SuppressVanillaNegativeOutcome(bool __result, [HarmonyOuter] Gravship ___gravship)
            {
                EmergencyLaunchWorldComponent component = EmergencyLaunchUtility.Component;
                if (!component.ConsumeEmergencyLandingPending(___gravship.Engine))
                {
                    emergencyGravship = null;
                    return __result;
                }

                component.NotifyEmergencyLaunchCompleted(___gravship.Engine);
                emergencyGravship = ___gravship;
                return false;
            }

            [HarmonyPostfix]
            [HarmonyInfix(typeof(Scenario), nameof(Scenario.PostGravshipLanded))]
            public static void ApplyEmergencyOutcomes()
            {
                if (emergencyGravship == null)
                {
                    return;
                }

                Gravship gravship = emergencyGravship;
                emergencyGravship = null;

                List<LandingOutcomeDef> outcomes = EmergencyLaunchUtility.GetEmergencyLandingOutcomes(2);
                ForceGuaranteedOutcomeChanceDisplay = true;
                try
                {
                    for (int i = 0; i < outcomes.Count; i++)
                    {
                        outcomes[i].Worker.ApplyOutcome(gravship);
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
