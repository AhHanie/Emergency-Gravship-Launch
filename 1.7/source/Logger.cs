using System;
using System.Diagnostics;
using Verse;

namespace GravshipEmergencyLaunch
{
    public static class Logger
    {
        private const string Prefix = "[Gravship Emergency Launch] ";

        [Conditional("DEBUG")]
        public static void Message(string message)
        {
            Log.Message(Prefix + message);
        }

        [Conditional("DEBUG")]
        public static void Warning(string message)
        {
            Log.Warning(Prefix + message);
        }

        public static void Error(string message)
        {
            Log.Error(Prefix + message);
        }

        public static void Exception(Exception exception, string context = null)
        {
            if (exception == null)
            {
                return;
            }

            string prefix = string.IsNullOrWhiteSpace(context) ? Prefix : Prefix + context + ": ";
            Log.Error(prefix + exception);
        }
    }
}
