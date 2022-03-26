using System.Collections.Generic;
using System.Globalization;
using System;
using System.Diagnostics;

namespace Framework
{
    public static class Logger
    {
        public static event LogMessageHandler OnLogMessage;
        public delegate void LogMessageHandler(string message);

        public static Dictionary<string, string> DebugUI = new Dictionary<string, string>();

        public static void SetDebugUI(string name, string value)
        {
            DebugUI[name] = value;
        }

        public static void LogDebug(object service, string message)
        {
            Console.WriteLine
            (
                String.Format(
                    "[{3}][{0}][{1}] {2}",
                    Process.GetCurrentProcess().StartTime,
                    service.GetType().Name,
                    message,
                    System.Threading.Thread.CurrentThread.ManagedThreadId
                )
            );

            Logger.OnLogMessage?.Invoke(message);
        }
    }
}
