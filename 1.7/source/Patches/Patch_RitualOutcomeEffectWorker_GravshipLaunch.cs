using HarmonyLib;
using RimWorld;
using Verse;

namespace GravshipEmergencyLaunch.Patches
{
    public static class Patch_RitualOutcomeEffectWorker_GravshipLaunch
    {
        [HarmonyPatch(typeof(RitualOutcomeEffectWorker_GravshipLaunch), nameof(RitualOutcomeEffectWorker_GravshipLaunch.Apply))]
        public static class Apply
        {
            public static bool Prepare()
            {
                return ModsConfig.OdysseyActive;
            }

            [HarmonyPostfix]
            [HarmonyInfix("GravshipLaunchInterrupted")]
            public static string ConsumePendingEmergencyRitual(string __result, [HarmonyOuter] LordJob_Ritual jobRitual)
            {
                Building_GravEngine engine = jobRitual.selectedTarget.Thing?.TryGetComp<CompPilotConsole>()?.engine;
                if (engine != null)
                {
                    EmergencyLaunchUtility.Component.ConsumePendingEmergencyRitual(engine);
                }

                return __result;
            }

            [HarmonyPostfix]
            [HarmonyInfix(typeof(Building_GravEngine), nameof(Building_GravEngine.launchInfo), InnerTargetKind.FieldWrite)]
            public static void ArmEmergencyLaunch(Building_GravEngine __instance)
            {
                EmergencyLaunchUtility.ArmEmergencyLaunch(__instance);
            }
        }
    }
}
