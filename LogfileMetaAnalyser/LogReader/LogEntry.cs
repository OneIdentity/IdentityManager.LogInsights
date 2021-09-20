using System;

namespace LogfileMetaAnalyser.LogReader
{
    public enum LogLevel
    {
        Info,
        Warning,
        Error,
        Debug,
        Trace,
        Critical
    }

    public class LogEntry
    {
        public LogEntry(Locator locator, string id, DateTime timeStamp, LogLevel type, int severity, string message, string logger, string appName)
        {
            Locator = locator;
            Id = id;
            TimeStamp = timeStamp;
            Severity = severity;
            Type = type;
            Message = message;
            Logger = logger;
			AppName = appName;
		}

        public Locator Locator { get; }

        public DateTime TimeStamp { get; }

        public int Severity { get; }

        public string Id { get; }

        public LogLevel Type { get; }

        public string Message { get; }

        public string Logger { get; }

		public string AppName { get; }
	}
}
