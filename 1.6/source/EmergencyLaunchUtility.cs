using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace GravshipEmergencyLaunch
{
    public static class EmergencyLaunchUtility
    {
        private static readonly CachedTexture EmergencyLaunchIcon = new CachedTexture("EGL/EmergencyLaunchGravship");

        private static string armedEmergencyLaunchEngineId;
        private static bool creatingEmergencyLaunchWindow;

        public static EmergencyLaunchWorldComponent Component
        {
            get
            {
                return Find.World.GetComponent<EmergencyLaunchWorldComponent>();
            }
        }

        public static bool ShouldShowEmergencyLaunchGizmo(CompPilotConsole console)
        {
            return console.engine != null && GenTicks.TicksGame < console.engine.cooldownCompleteTick;
        }

        public static AcceptanceReport CanEmergencyLaunchForRitual(CompPilotConsole console)
        {
            return CanEmergencyLaunchCommon(console);
        }

        public static Command_Action CreateEmergencyLaunchGizmo(CompPilotConsole console)
        {
            if (!ShouldShowEmergencyLaunchGizmo(console))
            {
                return null;
            }

            Command_Action command = new Command_Action
            {
                defaultLabel = "EmergencyGravLaunch.CommandLabel".Translate(),
                defaultDesc = GetEmergencyLaunchDescription(),
                icon = EmergencyLaunchIcon.Texture,
                action = delegate
                {
                    AcceptanceReport report = CanEmergencyLaunchForRitual(console);
                    if (!report.Accepted)
                    {
                        Messages.Message(report.Reason, MessageTypeDefOf.RejectInput, false);
                        return;
                    }

                    Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation(
                        "EmergencyGravLaunch.ConfirmText".Translate(console.engine.RenamableLabel.Named("GRAVSHIP")),
                        delegate
                        {
                            ShowEmergencyLaunchRitualWindow(console);
                        },
                        destructive: true));
                }
            };
            command.groupKey = 79861124;

            AcceptanceReport actionReport = CanEmergencyLaunchForRitual(console);
            if (!actionReport.Accepted)
            {
                command.Disable(actionReport.Reason);
            }

            return command;
        }

        public static void ShowEmergencyLaunchRitualWindow(CompPilotConsole console)
        {
            AcceptanceReport report = CanEmergencyLaunchForRitual(console);
            if (!report.Accepted)
            {
                Messages.Message(report.Reason, MessageTypeDefOf.RejectInput, false);
                return;
            }

            Precept_Ritual ritual = GetGravshipLaunchRitual();

            BeginEmergencyLaunchWindowCreation();
            Window window = ritual.GetRitualBeginWindow(console.parent, null, delegate
            {
                EmergencyLaunchWorldComponent component = Component;
                component.NotifyEmergencyRitualQueued(console.engine);
            });
            EndEmergencyLaunchWindowCreation();
            Find.WindowStack.Add(window);
        }

        public static void ArmEmergencyLaunch(Building_GravEngine engine)
        {
            if (!Component.ConsumePendingEmergencyRitual(engine))
            {
                armedEmergencyLaunchEngineId = null;
                return;
            }

            armedEmergencyLaunchEngineId = engine.GetUniqueLoadID();
        }

        public static bool IsEmergencyLaunchArmed(Building_GravEngine engine)
        {
            if (armedEmergencyLaunchEngineId == null)
            {
                return false;
            }

            return engine.GetUniqueLoadID() == armedEmergencyLaunchEngineId;
        }

        public static void ClearArmedEmergencyLaunch()
        {
            armedEmergencyLaunchEngineId = null;
        }

        public static void BeginEmergencyLaunchWindowCreation()
        {
            creatingEmergencyLaunchWindow = true;
        }

        public static void EndEmergencyLaunchWindowCreation()
        {
            creatingEmergencyLaunchWindow = false;
        }

        public static bool IsCreatingEmergencyLaunchWindow()
        {
            return creatingEmergencyLaunchWindow;
        }

        public static List<LandingOutcomeDef> GetEmergencyLandingOutcomes(int count)
        {
            List<LandingOutcomeDef> available = DefDatabase<LandingOutcomeDef>.AllDefsListForReading
                .Where(delegate(LandingOutcomeDef def)
                {
                    return IsEmergencyLandingOutcome(def);
                }).ToList();

            List<LandingOutcomeDef> selected = new List<LandingOutcomeDef>();
            if (count <= 0)
            {
                return selected;
            }

            selected.Add(LandingOutcomeDefOf.EGL_OverstressedGravEngine);
            available.Remove(LandingOutcomeDefOf.EGL_OverstressedGravEngine);

            while (selected.Count < count && available.Count > 0)
            {
                LandingOutcomeDef next = available.RandomElementByWeight(delegate(LandingOutcomeDef def)
                {
                    return GetEmergencyLandingOutcomeWeight(def);
                });
                selected.Add(next);
                available.Remove(next);
            }

            return selected;
        }

        public static void ConsumeExtraFuel(Building_GravEngine engine, float amount)
        {
            List<CompRefuelable> refuelables = engine.GravshipComponents
                .Where(delegate(CompGravshipFacility comp)
                {
                    return comp.CanBeActive && comp.Props.providesFuel;
                })
                .Select(delegate(CompGravshipFacility comp)
                {
                    return comp.parent.GetComp<CompRefuelable>();
                })
                .Where(delegate(CompRefuelable comp)
                {
                    return comp != null && comp.Fuel > 0f;
                }).ToList();

            float availableFuel = refuelables.Sum(delegate(CompRefuelable comp)
            {
                return comp.Fuel;
            });
            if (availableFuel <= 0f)
            {
                return;
            }

            float clampedAmount = Mathf.Min(amount, availableFuel);
            float ratio = clampedAmount / availableFuel;
            for (int i = 0; i < refuelables.Count; i++)
            {
                refuelables[i].ConsumeFuel(refuelables[i].Fuel * ratio);
            }
        }

        private static AcceptanceReport CanEmergencyLaunchCommon(CompPilotConsole console)
        {
            if (console.parent == null)
            {
                return new AcceptanceReport("CannotLaunchNoEngine".Translate().CapitalizeFirst());
            }

            Building_GravEngine engine = console.engine;
            if (engine == null)
            {
                return new AcceptanceReport("CannotLaunchNoEngine".Translate().CapitalizeFirst());
            }

            if (Find.GravshipController.IsGravshipTravelling || WorldComponent_GravshipController.CutsceneInProgress)
            {
                return new AcceptanceReport("EmergencyGravLaunch.InProgress".Translate().CapitalizeFirst());
            }

            Map map = engine.Map;
            if (map != null && map.generatorDef != null && map.generatorDef.pocketMapProperties != null && !map.generatorDef.pocketMapProperties.canLaunchGravship)
            {
                return new AcceptanceReport(map.generatorDef.label);
            }

            CompBreakdownable breakdownable = console.parent.GetComp<CompBreakdownable>();
            if (breakdownable != null && breakdownable.BrokenDown)
            {
                return new AcceptanceReport("BrokenDown".Translate().CapitalizeFirst());
            }

            if (!engine.ValidSubstructureAt(console.parent.InteractionCell))
            {
                return new AcceptanceReport("PilotConsoleInaccessible".Translate().CapitalizeFirst());
            }

            if (engine.ValidSubstructure.Count == 0)
            {
                return new AcceptanceReport("CannotLaunchNoSubstructure".Translate().CapitalizeFirst());
            }

            if (!console.CanBeActive)
            {
                return new AcceptanceReport("CannotLaunchDisconnected".Translate().CapitalizeFirst());
            }

            if (engine.TotalFuel < 10f)
            {
                return new AcceptanceReport("CannotLaunchNotEnoughFuel".Translate().CapitalizeFirst());
            }

            if (engine.MaxLaunchDistance <= 0)
            {
                return new AcceptanceReport("CannotLaunchNoThrusters".Translate().CapitalizeFirst());
            }

            if (GenTicks.TicksGame >= engine.cooldownCompleteTick)
            {
                return new AcceptanceReport("EmergencyGravLaunch.NotOnCooldown".Translate().CapitalizeFirst());
            }

            if (Component.IsEmergencyLaunchDisabled(engine))
            {
                return new AcceptanceReport("EmergencyGravLaunch.DisabledUntilNormalLaunch".Translate().CapitalizeFirst());
            }

            return AcceptanceReport.WasAccepted;
        }

        private static string GetEmergencyLaunchDescription()
        {
            string baseText = "EmergencyGravLaunch.CommandDesc".Translate();
            string warning = "EmergencyGravLaunch.CommandWarning".Translate().Colorize(ColoredText.WarningColor);
            return baseText + "\n\n" + warning;
        }

        private static bool IsEmergencyLandingOutcome(LandingOutcomeDef def)
        {
            return def.GetModExtension<EmergencyLandingOutcomeExtension>() != null;
        }

        private static float GetEmergencyLandingOutcomeWeight(LandingOutcomeDef def)
        {
            EmergencyLandingOutcomeExtension extension = def.GetModExtension<EmergencyLandingOutcomeExtension>();
            return extension.emergencyWeight;
        }

        private static Precept_Ritual GetGravshipLaunchRitual()
        {
            return Faction.OfPlayer.ideos.AllIdeos
                .SelectMany(delegate(Ideo ideo)
                {
                    return ideo.PreceptsListForReading;
                })
                .OfType<Precept_Ritual>()
                .FirstOrDefault(delegate(Precept_Ritual ritual)
                {
                    return ritual.def == PreceptDefOf.GravshipLaunch;
                });
        }
    }
}
