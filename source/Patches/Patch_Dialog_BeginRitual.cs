using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using Verse;
using System;

namespace GravshipEmergencyLaunch.Patches
{
    public static class Patch_Dialog_BeginRitual
    {
        private static readonly System.Reflection.MethodInfo MemberwiseCloneMethod = AccessTools.Method(typeof(object), "MemberwiseClone");

        [HarmonyPatch(typeof(Dialog_BeginRitual))]
        [HarmonyPatch(MethodType.Constructor)]
        [HarmonyPatch(new Type[]
        {
            typeof(string),
            typeof(Precept_Ritual),
            typeof(TargetInfo),
            typeof(Map),
            typeof(Dialog_BeginRitual.ActionCallback),
            typeof(Pawn),
            typeof(RitualObligation),
            typeof(Dialog_BeginRitual.PawnFilter),
            typeof(string),
            typeof(List<Pawn>),
            typeof(Dictionary<string, Pawn>),
            typeof(RitualOutcomeEffectDef),
            typeof(List<string>),
            typeof(Pawn)
        })]
        public static class Constructor
        {
            public static bool Prepare()
            {
                return ModsConfig.OdysseyActive;
            }

            public static void Postfix(Dialog_BeginRitual __instance)
            {
                if (!EmergencyLaunchUtility.IsCreatingEmergencyLaunchWindow())
                {
                    return;
                }

                __instance.ritualLabel = "EmergencyGravLaunch.RitualTitle".Translate();

                RitualOutcomeEffectDef outcome = __instance.outcome;
                if (outcome != null)
                {
                    __instance.outcome = CloneForEmergencyLaunch(outcome);
                }
            }
        }

        private static RitualOutcomeEffectDef CloneForEmergencyLaunch(RitualOutcomeEffectDef source)
        {
            if (!(MemberwiseCloneMethod?.Invoke(source, null) is RitualOutcomeEffectDef clone))
            {
                UserLogger.Warning("Failed to clone gravship ritual outcome def for emergency dialog.");
                return source;
            }

            if (source.extraOutcomeDescriptions != null)
            {
                clone.extraOutcomeDescriptions = new List<RitualOutcomeEffectDef.ExtraOutcomeChanceDescription>(source.extraOutcomeDescriptions.Count);
                for (int i = 0; i < source.extraOutcomeDescriptions.Count; i++)
                {
                    RitualOutcomeEffectDef.ExtraOutcomeChanceDescription original = source.extraOutcomeDescriptions[i];
                    RitualOutcomeEffectDef.ExtraOutcomeChanceDescription copied = new RitualOutcomeEffectDef.ExtraOutcomeChanceDescription
                    {
                        description = original.description,
                        qualityToValue = original.qualityToValue
                    };

                    if (i == 0 && copied.qualityToValue != null)
                    {
                        copied.qualityToValue = quality => original.qualityToValue(quality) * 3f;
                    }
                    else if (i == 1)
                    {
                        copied.description = "EmergencyGravLaunch.RitualGuaranteedIncidents".Translate();
                        copied.qualityToValue = quality => 1f;
                    }

                    clone.extraOutcomeDescriptions.Add(copied);
                }
            }

            return clone;
        }
    }
}
