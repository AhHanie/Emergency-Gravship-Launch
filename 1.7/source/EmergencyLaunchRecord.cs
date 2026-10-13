using Verse;

namespace GravshipEmergencyLaunch
{
    public sealed class EmergencyLaunchRecord : IExposable
    {
        public string engineId;

        public float lastLaunchFuelCost;

        public bool pendingEmergencyRitual;

        public bool emergencyLandingPending;

        public bool emergencyLaunchDisabled;

        public void ExposeData()
        {
            Scribe_Values.Look(ref engineId, "engineId");
            Scribe_Values.Look(ref lastLaunchFuelCost, "lastLaunchFuelCost", 0f);
            Scribe_Values.Look(ref pendingEmergencyRitual, "pendingEmergencyRitual", false);
            Scribe_Values.Look(ref emergencyLandingPending, "emergencyLandingPending", false);
            Scribe_Values.Look(ref emergencyLaunchDisabled, "emergencyLaunchDisabled", false);
        }
    }
}
