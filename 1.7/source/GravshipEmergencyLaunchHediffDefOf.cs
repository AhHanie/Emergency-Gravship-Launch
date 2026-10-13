using RimWorld;
using Verse;

namespace GravshipEmergencyLaunch
{
    [DefOf]
    public static class HediffDefOf
    {
        public static HediffDef EGL_SevereGravNausea;

        static HediffDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(HediffDefOf));
        }
    }
}
