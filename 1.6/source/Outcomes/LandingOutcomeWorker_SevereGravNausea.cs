using RimWorld;
using RimWorld.Planet;
using Verse;

namespace GravshipEmergencyLaunch.Outcomes
{
    public class LandingOutcomeWorker_SevereGravNausea : LandingOutcomeWorker
    {
        public LandingOutcomeWorker_SevereGravNausea(LandingOutcomeDef def)
            : base(def)
        {
        }

        public override void ApplyOutcome(Gravship gravship)
        {
            foreach (Pawn pawn in gravship.Pawns)
            {
                pawn.health?.AddHediff(HediffDefOf.EGL_SevereGravNausea);
            }

            SendStandardLetter(gravship.Engine, null, new LookTargets(gravship.Pawns));
        }
    }
}
