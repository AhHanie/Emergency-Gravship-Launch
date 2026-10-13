using HarmonyLib;
using Verse;

namespace GravshipEmergencyLaunch
{
    public sealed class Mod : Verse.Mod
    {
        public Mod(ModContentPack content)
            : base(content)
        {
            LongEventHandler.QueueLongEvent(Init, "EmergencyGravLaunch.LoadingLabel", doAsynchronously: true, null);
        }

        private static void Init()
        {
            new Harmony("sk.emergencygravlaunch").PatchAll();
        }
    }
}
