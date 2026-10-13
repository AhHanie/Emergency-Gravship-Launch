using RimWorld;
using RimWorld.Planet;
using Verse;

namespace GravshipEmergencyLaunch
{
    public sealed class EmergencyLaunchWorldComponent : WorldComponent
    {
        private EmergencyLaunchRecord record;

        public EmergencyLaunchWorldComponent(World world)
            : base(world)
        {
        }

        public override void ExposeData()
        {
            Scribe_Deep.Look(ref record, "record");
        }

        public EmergencyLaunchRecord GetRecord(Building_GravEngine engine, bool createIfMissing = true)
        {
            if (record != null && record.engineId == engine.GetUniqueLoadID())
            {
                return record;
            }

            if (!createIfMissing)
            {
                return null;
            }

            record = new EmergencyLaunchRecord
            {
                engineId = engine.GetUniqueLoadID()
            };
            return record;
        }

        public void NotifyLaunchFuelConsumed(Building_GravEngine engine, float fuelCost)
        {
            EmergencyLaunchRecord record = GetRecord(engine);
            if (record != null)
            {
                record.lastLaunchFuelCost = fuelCost;
            }
        }

        public void NotifyEmergencyRitualQueued(Building_GravEngine engine)
        {
            EmergencyLaunchRecord record = GetRecord(engine);
            if (record != null)
            {
                record.pendingEmergencyRitual = true;
            }
        }

        public bool ConsumePendingEmergencyRitual(Building_GravEngine engine)
        {
            EmergencyLaunchRecord record = GetRecord(engine, false);
            if (record == null || !record.pendingEmergencyRitual)
            {
                return false;
            }

            record.pendingEmergencyRitual = false;
            return true;
        }

        public void NotifyEmergencyLaunchStarted(Building_GravEngine engine)
        {
            EmergencyLaunchRecord record = GetRecord(engine);
            if (record != null)
            {
                record.emergencyLandingPending = true;
            }
        }

        public void NotifyEmergencyLaunchCompleted(Building_GravEngine engine)
        {
            EmergencyLaunchRecord record = GetRecord(engine);
            if (record != null)
            {
                record.emergencyLaunchDisabled = true;
            }
        }

        public void NotifyNormalLaunchStarted(Building_GravEngine engine)
        {
            EmergencyLaunchRecord record = GetRecord(engine);
            if (record != null)
            {
                record.emergencyLaunchDisabled = false;
            }
        }

        public bool ConsumeEmergencyLandingPending(Building_GravEngine engine)
        {
            EmergencyLaunchRecord record = GetRecord(engine, false);
            if (record == null || !record.emergencyLandingPending)
            {
                return false;
            }

            record.emergencyLandingPending = false;
            return true;
        }

        public bool IsEmergencyLaunchDisabled(Building_GravEngine engine)
        {
            EmergencyLaunchRecord record = GetRecord(engine, false);
            return record != null && record.emergencyLaunchDisabled;
        }

        public float LastLaunchFuelCost(Building_GravEngine engine)
        {
            EmergencyLaunchRecord record = GetRecord(engine, false);
            if (record == null)
            {
                return 0f;
            }

            return record.lastLaunchFuelCost;
        }
    }
}
