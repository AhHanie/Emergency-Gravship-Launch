using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using Verse;
using UnityEngine;

namespace GravshipEmergencyLaunch.Outcomes
{
    public class LandingOutcomeWorker_PsychicShockwave : LandingOutcomeWorker
    {
        private const int WorldRangeTiles = 4;

        private const float MentalStateRadius = 60f;

        private const int GoodwillImpactForNeuroquake = -10;

        private const int GoodwillImpactForBerserk = -75;

        private readonly Dictionary<Faction, Pair<bool, Pawn>> affectedFactions = new Dictionary<Faction, Pair<bool, Pawn>>();

        private readonly List<Pawn> giveMentalStateTo = new List<Pawn>();

        public LandingOutcomeWorker_PsychicShockwave(LandingOutcomeDef def)
            : base(def)
        {
        }

        public override void ApplyOutcome(Gravship gravship)
        {
            Map map = gravship.Engine.Map;
            IntVec3 center = gravship.Engine.Position;
            List<Pawn> nearbyPlayerColonists = new List<Pawn>();

            affectedFactions.Clear();
            giveMentalStateTo.Clear();

            foreach (Pawn pawn in map.mapPawns.AllPawnsSpawned)
            {
                if (!CanApplyEffects(pawn) || pawn.Fogged())
                {
                    continue;
                }

                bool outsideMentalBreakRadius = !pawn.Position.InHorDistOf(center, MentalStateRadius);
                AffectGoodwill(pawn.HomeFaction, !outsideMentalBreakRadius, pawn);
                if (outsideMentalBreakRadius)
                {
                    GiveNeuroquakeThought(pawn);
                }
                else if (pawn.IsFreeColonist && pawn.Faction == Faction.OfPlayer)
                {
                    nearbyPlayerColonists.Add(pawn);
                }
                else
                {
                    giveMentalStateTo.Add(pawn);
                }
            }

            foreach (Map otherMap in Find.Maps)
            {
                if (otherMap == map || Find.WorldGrid.TraversalDistanceBetween(otherMap.Tile, map.Tile, true, WorldRangeTiles + 1) > WorldRangeTiles)
                {
                    continue;
                }

                foreach (Pawn pawn in otherMap.mapPawns.AllPawns)
                {
                    if (CanApplyEffects(pawn))
                    {
                        GiveNeuroquakeThought(pawn);
                    }
                }
            }

            foreach (Caravan caravan in Find.WorldObjects.Caravans)
            {
                if (Find.WorldGrid.TraversalDistanceBetween(caravan.Tile, map.Tile, true, WorldRangeTiles + 1) > WorldRangeTiles)
                {
                    continue;
                }

                foreach (Pawn pawn in caravan.pawns)
                {
                    if (CanApplyEffects(pawn))
                    {
                        GiveNeuroquakeThought(pawn);
                    }
                }
            }

            if (nearbyPlayerColonists.Count > 0)
            {
                int colonistBerserkLimit = Mathf.Max(1, Mathf.CeilToInt(nearbyPlayerColonists.Count * 0.1f));
                List<Pawn> berserkColonists = nearbyPlayerColonists.InRandomOrder().Take(Mathf.Min(colonistBerserkLimit, nearbyPlayerColonists.Count)).ToList();
                giveMentalStateTo.AddRange(berserkColonists);
                foreach (Pawn pawn in nearbyPlayerColonists)
                {
                    if (!berserkColonists.Contains(pawn))
                    {
                        GiveNeuroquakeThought(pawn);
                    }
                }
            }

            foreach (Pawn pawn in giveMentalStateTo)
            {
                MentalStateDef mentalStateDef = pawn.RaceProps.IsMechanoid ? MentalStateDefOf.BerserkMechanoid : MentalStateDefOf.Berserk;
                pawn.mindState?.mentalStateHandler?.TryStartMentalState(mentalStateDef, null, false, true, false, gravship.Engine.Faction?.leader);
                RestUtility.WakeUp(pawn);
            }

            if (gravship.Engine.Faction == Faction.OfPlayer)
            {
                foreach (KeyValuePair<Faction, Pair<bool, Pawn>> affectedFaction in affectedFactions)
                {
                    int goodwillChange = affectedFaction.Value.First ? GoodwillImpactForBerserk : GoodwillImpactForNeuroquake;
                    Faction.OfPlayer.TryAffectGoodwillWith(affectedFaction.Key, goodwillChange, true, true, HistoryEventDefOf.UsedHarmfulAbility);
                }
            }

            SendStandardLetter(gravship.Engine, null, gravship.Engine);
            affectedFactions.Clear();
            giveMentalStateTo.Clear();
        }

        private void AffectGoodwill(Faction faction, bool gaveMentalBreak, Pawn pawn = null)
        {
            if (faction != null && !faction.IsPlayer && !faction.HostileTo(Faction.OfPlayer) && (pawn == null || !pawn.IsSlaveOfColony) && (!affectedFactions.TryGetValue(faction, out Pair<bool, Pawn> value) || (!value.First && gaveMentalBreak)))
            {
                affectedFactions[faction] = new Pair<bool, Pawn>(gaveMentalBreak, pawn);
            }
        }

        private static void GiveNeuroquakeThought(Pawn pawn)
        {
            if (ThoughtDefOf.NeuroquakeEcho != null)
            {
                pawn.needs?.mood?.thoughts.memories.TryGainMemory(ThoughtDefOf.NeuroquakeEcho);
            }
        }

        private static bool CanApplyEffects(Pawn pawn)
        {
            if (pawn.kindDef.isBoss)
            {
                return false;
            }

            return !pawn.Dead && !pawn.Suspended && pawn.GetStatValue(StatDefOf.PsychicSensitivity) > float.Epsilon;
        }
    }
}
