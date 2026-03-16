using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace GravshipEmergencyLaunch.Outcomes
{
    public class LandingOutcomeWorker_StructuralHullDamage : LandingOutcomeWorker
    {
        private const float MinDamageFraction = 0.3f;

        private const float MaxDamageFraction = 0.5f;

        public LandingOutcomeWorker_StructuralHullDamage(LandingOutcomeDef def)
            : base(def)
        {
        }

        public override void ApplyOutcome(Gravship gravship)
        {
            Map map = gravship.Engine.Map;
            HashSet<IntVec3> substructure = gravship.Engine.ValidSubstructureNoRegen;
            List<Building> candidates = substructure
                .Select(delegate(IntVec3 cell)
                {
                    return cell.GetEdifice(map);
                })
                .Where(delegate(Building thing)
                {
                    return thing != null && thing.Spawned && thing.def == ThingDefOf.GravshipHull;
                }).Distinct().ToList();

            if (candidates.Count == 0)
            {
                return;
            }

            int count = Mathf.Max(1, Mathf.CeilToInt(candidates.Count * 0.5f));
            List<Building> damaged = candidates.InRandomOrder().Take(count).ToList();
            for (int i = 0; i < damaged.Count; i++)
            {
                Building thing = damaged[i];
                float damageFraction = Rand.Range(MinDamageFraction, MaxDamageFraction);
                int damageAmount = Mathf.Max(1, Mathf.RoundToInt(thing.MaxHitPoints * damageFraction));
                thing.TakeDamage(new DamageInfo(DamageDefOf.Crush, damageAmount));
            }

            TaggedString extraText = "EmergencyGravLaunch.OutcomeStructuralHullDamageExtra".Translate() + ":\n" +
                damaged.Select(delegate(ThingWithComps thing)
                {
                    return thing.LabelCap;
                }).ToLineList(" - ");
            SendStandardLetter(gravship.Engine, extraText, new LookTargets(damaged));
        }
    }
}
