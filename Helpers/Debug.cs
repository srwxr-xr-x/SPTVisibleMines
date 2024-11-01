using BepInEx.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisibleMines.Helpers
{
    public static class Debug
    {
        public static ManualLogSource Logger { get; set; }

        public static void SetLogger(ManualLogSource _logger)
        {
            Logger = _logger;
        }

        public static void LogInfo(string message)
        {
            Logger?.LogInfo(message);
        }

        public static void LogWarning(string message)
        {
            Logger?.LogWarning(message);
        }

        public static void LogError(string message)
        {
            Logger?.LogError(message);
        }
    }
}
