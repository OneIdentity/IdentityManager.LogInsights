using System;

using LogfileMetaAnalyser.Helpers;

namespace LogfileMetaAnalyser.LogReader
{
   public class LogEntry
    {
        public LogEntry(Locator locator, string id, DateTime timeStamp, LogLevel type, int severity, string message, string logger, string appName, string pid, string spid)
        {
            Locator = locator;
            Id = id;
            TimeStamp = timeStamp;
            Severity = severity;
            Level = type;
            Message = message;
            Logger = logger;
			AppName = appName;
            Pid = pid;
            Spid = spid;
        }

        public Locator Locator { get; }

        public DateTime TimeStamp { get; }

        public int Severity { get; }

        public string Id { get; }

        public LogLevel Level { get; }

        public string Message { get; }

        public string Logger { get; }

		public string AppName { get; }

        public string Pid { get; }

        public string Spid { get; }
    }
}
