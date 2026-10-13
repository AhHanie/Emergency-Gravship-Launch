using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using Verse;

namespace GravshipEmergencyLaunch.Patches
{
    public static class Patch_RitualOutcomeEffectWorker_GravshipLaunch
    {
        [HarmonyPatch(typeof(RitualOutcomeEffectWorker_GravshipLaunch), "Apply")]
        public static class Apply
        {
            public static bool Prepare()
            {
                return ModsConfig.OdysseyActive;
            }

            private static readonly FieldInfo SelectedTargetField = AccessTools.Field(typeof(LordJob_Ritual), "selectedTarget");

            private static readonly MethodInfo TargetThingGetter = AccessTools.PropertyGetter(typeof(TargetInfo), nameof(TargetInfo.Thing));

            private static readonly MethodInfo TryGetPilotConsoleMethod = typeof(ThingCompUtility)
                .GetMethods(BindingFlags.Public | BindingFlags.Static)
                .Single(method => method.Name == nameof(ThingCompUtility.TryGetComp) && method.IsGenericMethodDefinition && method.GetParameters().Length == 1)
                .MakeGenericMethod(typeof(CompPilotConsole));

            private static readonly FieldInfo EngineField = AccessTools.Field(typeof(CompGravshipFacility), "engine");

            private static readonly MethodInfo ComponentGetter = AccessTools.PropertyGetter(typeof(EmergencyLaunchUtility), nameof(EmergencyLaunchUtility.Component));

            private static readonly MethodInfo ConsumePendingEmergencyRitualMethod = AccessTools.Method(typeof(EmergencyLaunchWorldComponent), nameof(EmergencyLaunchWorldComponent.ConsumePendingEmergencyRitual));
            private static readonly FieldInfo LaunchInfoField = AccessTools.Field(typeof(Building_GravEngine), "launchInfo");

            private static readonly MethodInfo ArmEmergencyLaunchMethod = AccessTools.Method(typeof(EmergencyLaunchUtility), nameof(EmergencyLaunchUtility.ArmEmergencyLaunch));

            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                List<CodeInstruction> codes = new List<CodeInstruction>(instructions);

                try
                {
                    bool injectedInterruptedConsume = InsertBeforeInterruptedMessage(codes);
                    bool injectedArmLaunch = InsertAfterLaunchInfo(codes);

                    if (injectedInterruptedConsume && injectedArmLaunch)
                    {
                        Logger.Message("Patched RitualOutcomeEffectWorker_GravshipLaunch.Apply successfully.");
                    }
                    else
                    {
                        Logger.Warning($"RitualOutcomeEffectWorker_GravshipLaunch.Apply transpiler was only partially applied. interrupted={injectedInterruptedConsume}, arm={injectedArmLaunch}");
                    }
                }
                catch (Exception exception)
                {
                    Logger.Exception(exception, "Failed to transpile RitualOutcomeEffectWorker_GravshipLaunch.Apply");
                }

                return codes;
            }

            private static bool InsertBeforeInterruptedMessage(List<CodeInstruction> codes)
            {
                int messageIndex = FindIndex(codes, instruction => instruction.opcode == OpCodes.Ldstr && Equals(instruction.operand, "GravshipLaunchInterrupted"));
                if (messageIndex < 0)
                {
                    return false;
                }

                codes.InsertRange(messageIndex, BuildConsumePendingEmergencyRitualInstructions());
                return true;
            }

            private static bool InsertAfterLaunchInfo(List<CodeInstruction> codes)
            {
                int launchInfoIndex = FindIndex(codes, instruction => instruction.opcode == OpCodes.Stfld && Equals(instruction.operand, LaunchInfoField));
                if (launchInfoIndex < 0)
                {
                    return false;
                }

                codes.InsertRange(launchInfoIndex + 1, BuildArmEmergencyLaunchInstructions());
                return true;
            }

            private static List<CodeInstruction> BuildConsumePendingEmergencyRitualInstructions()
            {
                return new List<CodeInstruction>
                {
                    new CodeInstruction(OpCodes.Call, ComponentGetter),
                    new CodeInstruction(OpCodes.Ldarg_3),
                    new CodeInstruction(OpCodes.Ldflda, SelectedTargetField),
                    new CodeInstruction(OpCodes.Call, TargetThingGetter),
                    new CodeInstruction(OpCodes.Call, TryGetPilotConsoleMethod),
                    new CodeInstruction(OpCodes.Ldfld, EngineField),
                    new CodeInstruction(OpCodes.Callvirt, ConsumePendingEmergencyRitualMethod),
                    new CodeInstruction(OpCodes.Pop)
                };
            }

            private static List<CodeInstruction> BuildArmEmergencyLaunchInstructions()
            {
                return new List<CodeInstruction>
                {
                    new CodeInstruction(OpCodes.Ldarg_3),
                    new CodeInstruction(OpCodes.Ldflda, SelectedTargetField),
                    new CodeInstruction(OpCodes.Call, TargetThingGetter),
                    new CodeInstruction(OpCodes.Call, TryGetPilotConsoleMethod),
                    new CodeInstruction(OpCodes.Ldfld, EngineField),
                    new CodeInstruction(OpCodes.Call, ArmEmergencyLaunchMethod)
                };
            }

            private static int FindIndex(List<CodeInstruction> codes, Func<CodeInstruction, bool> predicate, int startIndex = 0)
            {
                for (int i = startIndex; i < codes.Count; i++)
                {
                    if (predicate(codes[i]))
                    {
                        return i;
                    }
                }

                return -1;
            }
        }
    }
}
