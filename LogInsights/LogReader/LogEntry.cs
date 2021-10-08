using System;

using LogInsights.Helpers;

using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading;

namespace LogInsights.LogReader
{
   public class LogEntry
    {
        [JsonIgnore(Condition = JsonIgnoreCondition.Always)]
        private LogEntry[] _contextPreviousEntries;
        [JsonIgnore(Condition = JsonIgnoreCondition.Always)]
        private LogEntry[] _contextNextEntries;

        private string _fullMessage;
        private string _messageDevalued;
        private Locator _locator;

        public Locator Locator
        {
            get => _locator ??= new Locator();
            init => _locator = value;
        }

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
        public int NumberOfLines { get; init; } = 1;

        [JsonIgnore(Condition = JsonIgnoreCondition.Always)]
        public LogEntry[] ContextPreviousEntries
        {
            get => _contextPreviousEntries ?? Array.Empty<LogEntry>();
            internal set => _contextPreviousEntries = value;
        }

        [JsonIgnore(Condition = JsonIgnoreCondition.Always)]
        public LogEntry[] ContextNextEntries
        {
            get => _contextNextEntries ?? Array.Empty<LogEntry>();
            internal set => _contextNextEntries = value;
        }

        /// <summary>
        /// the complete log message incl. meta data (timestamp, log level, ids and log message text)
        /// </summary>
        public string FullMessage
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

        public override string ToString()
        {
            //more or less for debugging
            //for productive usage take property messageText
            return string.Format("msg #{0} in line {1}@{2}: {3}",
                    Locator.EntryNumber,
                    Locator.Position,
                    Locator.Source,
                    Message);
        }


        /// <summary>
        /// This is to have an existing message and you need to put payload messages from other LogEntries at the end of this payload message
        /// </summary>
        /// <param name="mergeCandidates"></param>
        /// <param name="firstIndex">start position of the mergeCandidates list</param>
        /// <param name="lastIndex">last position of the mergeCandidates list</param>
        /// <returns>a new text message object with meta data of current message and message text of this message plus all merge candidates</returns>
        public LogEntry Merge(IReadOnlyList<LogEntry> mergeCandidates, int firstIndex = 0, int lastIndex = 0)
        {
            firstIndex = firstIndex.EnsureRange(0, mergeCandidates.Count - 1);
            lastIndex = lastIndex.EnsureRange(firstIndex, mergeCandidates.Count - 1);

            var sb = new StringBuilder(Message);
            int numberOfLinesPassed = 0;

            for (int i = firstIndex; i <= lastIndex; i++)
            {
                sb.Append(mergeCandidates[i].Message); //we know that each messageText object ends with <newline>
                numberOfLinesPassed += mergeCandidates[i].NumberOfLines;
            }

            return Clone(sb.ToString(), NumberOfLines + numberOfLinesPassed);
        }

        /// <summary>
        /// create a clone of current log message with a new (optional) message text 
        /// </summary>
        /// <param name="newText">optional: the new message to put into the clone (this is not parsed again, meta data is ignored)</param>
        /// <param name="numberOfLinesPassed">optional: pre-calculated number of lines (meta data for statistics) to put into the clone</param>
        /// <returns></returns>
        public LogEntry Clone(string newText = null, int numberOfLinesPassed = -1)
        {
            //so we have 2 scenarios:
            //1.) newText is null, means Clone() was called to make an exact copy of THIS
            //2.) newText is not null, means Clone("msg1\n+msg2\n") was called, basically from the Merge() Method. So some attributes needs to be recalculated, some right now, some by calling FinalizeMessage() again

            //special handling for count of lines as this attribute is not handled by the finalized method below
            int newNumberOfLines = NumberOfLines;

            //are we in scenario 2?
            if (!string.IsNullOrEmpty(newText))
            {
                newNumberOfLines = numberOfLinesPassed > 0 ? numberOfLinesPassed : newText.Count(c => c == '\n');
            }

            //put the current old message's attributes into the transfer message
            var message = newText != null
                ? $"{Message}\n{newText}"
                : Message;

            //create a new transfer message
            var tm = new LogEntry
                {
                    Locator = Locator,
                    Level = Level,
                    Logger = Logger,
                    Id = Id,
                    Pid = Pid,
                    Spid = Spid,
                    Severity = Severity,
                    Message = message,
                    AppName = AppName,
                    TimeStamp = TimeStamp,
                    NumberOfLines = newNumberOfLines,

                    ContextNextEntries = ContextNextEntries,
                    ContextPreviousEntries = ContextPreviousEntries
                };

            //return the transfer message a new cloned message
            return tm;
        }

        public bool EqualMetaData(LogEntry refMsg, int toleranceTimestampDiffMs = 85)
        {
            return
                //refMsg.loggerLevel == loggerLevel &&
                refMsg.Logger == Logger &&
                refMsg.Locator.Source == Locator.Source &&
                refMsg.Spid == Spid &&
                refMsg.Pid == Pid &&
                refMsg.TimeStamp.AlmostEqual(TimeStamp, toleranceTimestampDiffMs);
        }
    }
}
