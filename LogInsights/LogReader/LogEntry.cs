using System;

using LogInsights.Helpers;

using System.Threading;

namespace LogInsights.LogReader
{
   public class LogEntry
    {
        private LogEntry[] _contextPreviousEntries;
        private LogEntry[] _contextNextEntries;
        private string _fullMessage;
        private string _messageDevalued;

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

        public LogEntry[] ContextPreviousEntries
        {
            get => _contextPreviousEntries ?? Array.Empty<LogEntry>();
            internal set => _contextPreviousEntries = value;
        }

        public LogEntry[] ContextNextEntries
        {
            get => _contextNextEntries ?? Array.Empty<LogEntry>();
            internal set => _contextNextEntries = value;
        }

        /// <summary>
        /// the complete log message incl. meta data (timestamp, log level, ids and log message text)
        /// </summary>
        public string FullFullMessage
        {
            get
            {
                LazyInitializer.EnsureInitialized(ref _fullMessage, () =>
                    {
                        //generate a message display
                        string id = Pid ?? "";

                        if (!string.IsNullOrEmpty(Spid))
                            id = $"{id} {Spid}";

                        string idspace = (!string.IsNullOrEmpty(id)) ? " " : "";

                        //sadly the logfiles do not hold milliseconds, but the App Insight Provider supports that
                        //we cannot differentiate between these two types, so lets cut them off in case of zero
                        string timestampFormat = "yyyy-MM-dd HH:mm:ss.ffff";
                        if (TimeStamp.Millisecond == 0)
                            timestampFormat = "yyyy-MM-dd HH:mm:ss";

                        return $"{TimeStamp.ToString(timestampFormat)} {Level} ({Logger ?? ""}{idspace}{id}): {Message ?? ""}";
                    });

                return _fullMessage;
            }
        }

        /// <summary>
        /// the log message without meta data (like timestamp) where all literals (like UIDs, names, etc.) were replaced by "<@>"
        /// </summary>
        public string MessageDevalued => _messageDevalued ??= Constants.regexLiterals.Replace(Message ?? "", "<@>");
    }
}
