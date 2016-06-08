using System;

namespace ServiceCommonLib
{
    public class LogEventArgs : EventArgs
    {
        public string Message { get; }
        public LogEventArgs(string message)
        {
            this.Message = message;
        }
    }

    public delegate void LogEventHandler(object sender, LogEventArgs e);
    public static class LogHelper
    {
        public static event LogEventHandler OnLogEvent;
        public static void CreateNewLogMessage(object sender, string message)
        {
            OnLogEvent?.Invoke(sender, new LogEventArgs(message));
        }
    }
}