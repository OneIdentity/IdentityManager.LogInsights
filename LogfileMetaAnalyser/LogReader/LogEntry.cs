using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogfileMetaAnalyser.LogReader
{
    public enum LogEntryType
    {
        Info,
        Warning,
        Error,
        Debug,
        Trace
    }

    public class LogEntry
    {
        public LogEntry(Locator locator, string id, DateTime timeStamp, LogEntryType type, int severity, string message, string logger)
        {
            Locator = locator;
            Id = id;
            TimeStamp = timeStamp;
            Severity = severity;
            Type = type;
            Message = message;
            Logger = logger;
        }

        public Locator Locator { get; }

        public DateTime TimeStamp { get; }

        public int Severity { get; }

        public string Id { get; }

        public LogEntryType Type { get; }

        public string Message { get; }

        public string Logger { get; } 
    }
}
