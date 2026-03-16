using RimWorld;
using Verse;

namespace GravshipEmergencyLaunch
{
    [DefOf]
    public static class LandingOutcomeDefOf
    {
        public static LandingOutcomeDef EGL_OverstressedGravEngine;

        static LandingOutcomeDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(LandingOutcomeDefOf));
        }
    }
}
