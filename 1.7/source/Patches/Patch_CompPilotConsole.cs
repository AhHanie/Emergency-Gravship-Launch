using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using Verse;

namespace GravshipEmergencyLaunch.Patches
{
    public static class Patch_CompPilotConsole
    {
        [HarmonyPatch(typeof(CompPilotConsole), "CompGetGizmosExtra")]
        public static class CompGetGizmosExtra
        {
            public static bool Prepare()
            {
                return ModsConfig.OdysseyActive;
            }

            public static void Postfix(CompPilotConsole __instance, ref IEnumerable<Gizmo> __result)
            {
                __result = AppendEmergencyLaunchGizmo(__result, __instance);
            }

            private static IEnumerable<Gizmo> AppendEmergencyLaunchGizmo(IEnumerable<Gizmo> original, CompPilotConsole console)
            {
                foreach (Gizmo gizmo in original)
                {
                    yield return gizmo;
                }

                Command_Action emergencyLaunchGizmo = EmergencyLaunchUtility.CreateEmergencyLaunchGizmo(console);
                if (emergencyLaunchGizmo != null)
                {
                    yield return emergencyLaunchGizmo;
                }
            }
        }
    }
}
