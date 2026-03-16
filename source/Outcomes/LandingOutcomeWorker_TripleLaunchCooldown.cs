using RimWorld;
using RimWorld.Planet;
using Verse;

namespace GravshipEmergencyLaunch.Outcomes
{
    public class LandingOutcomeWorker_TripleLaunchCooldown : LandingOutcomeWorker
    {
        public LandingOutcomeWorker_TripleLaunchCooldown(LandingOutcomeDef def)
            : base(def)
        {
        }

        public override void ApplyOutcome(Gravship gravship)
        {
            int remainingNormalCooldown = gravship.Engine.cooldownCompleteTick - GenTicks.TicksGame;
            if (remainingNormalCooldown > 0)
            {
                gravship.Engine.cooldownCompleteTick = GenTicks.TicksGame + remainingNormalCooldown * 3;
            }

            SendStandardLetter(gravship.Engine, null, gravship.Engine);
        }
    }
}
