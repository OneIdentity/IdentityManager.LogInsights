using System;

using LogInsights.Helpers;

namespace LogInsights.LogReader
{
   public class LogEntry
    {
        public LogEntry[] ContextPreviousEntries { get; internal set; }
        public LogEntry[] ContextNextEntries { get; internal set; }

        public Locator Locator { get; init; }
        public DateTime TimeStamp { get; init; }
        public int Severity { get; init; }
        public string Id { get; init; }
        public LogLevel Level { get; init; } = LogLevel.Info;
        public string Message { get; init; }
        public string Logger { get; init; }
        public string AppName { get; init; }
        public string Pid { get; init; }
        public string Spid { get; init; }
        public object Tag { get; set; }
        public override string ToString() => Message;
    }
}
