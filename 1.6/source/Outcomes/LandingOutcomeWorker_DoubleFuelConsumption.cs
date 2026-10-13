using RimWorld;
using RimWorld.Planet;
using Verse;

namespace GravshipEmergencyLaunch.Outcomes
{
    public class LandingOutcomeWorker_DoubleFuelConsumption : LandingOutcomeWorker
    {
        public LandingOutcomeWorker_DoubleFuelConsumption(LandingOutcomeDef def)
            : base(def)
        {
        }

        public override void ApplyOutcome(Gravship gravship)
        {
            EmergencyLaunchWorldComponent component = EmergencyLaunchUtility.Component;
            float extraFuel = component.LastLaunchFuelCost(gravship.Engine);
            EmergencyLaunchUtility.ConsumeExtraFuel(gravship.Engine, extraFuel);
            SendStandardLetter(gravship.Engine, "EmergencyGravLaunch.OutcomeDoubleFuelExtra".Translate(extraFuel.ToString("F0")).Resolve(), gravship.Engine);
        }
    }
}
