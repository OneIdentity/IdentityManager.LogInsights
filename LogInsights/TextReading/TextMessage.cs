using System; 
using System.Linq;
using System.Text;
using System.Collections.Generic;
using System.Text.Json.Serialization;

using LogInsights.Helpers;
using LogInsights.LogReader;
using System.Threading;


namespace LogInsights
{
    public class TextMessage
    {
        private readonly LogEntry m_Entry;

        //set from outside, no calculation
        public TextLocator Locator;


        public TextMessage[] ContextPreviousEntries=> _contextMsgBefore ??= (m_Entry.ContextPreviousEntries ?? Enumerable.Empty<LogEntry>()).Select(e => e.Tag).OfType<TextMessage>().ToArray();

        [JsonIgnore(Condition = JsonIgnoreCondition.Always)]
        private TextMessage[] _contextMsgBefore;


        public TextMessage[] ContextNextEntries
        {
            get { return _contextMsgAfter ??= (m_Entry.ContextNextEntries ?? Enumerable.Empty<LogEntry>()).Select(e => e.Tag).OfType<TextMessage>().ToArray(); }
            set { _contextMsgAfter = value; }
        }

        [JsonIgnore(Condition = JsonIgnoreCondition.Always)]
        private TextMessage[] _contextMsgAfter;



        private string _message;        
        /// <summary>
        /// the log message without the meta data (like timestamp)
        /// </summary>
        public string Message
        {
            get
            {
                return _message;
            }

            private set
            {
                _message = value;
                _payloadmessageDevalued = null;

                /*
                if (_payloadmessage != null)
                    _payloadmessage = _payloadmessage.Trim().TrimStart(": ").TrimStart("\n");   //too expensive! solved by regex in beforehand
                */
            }
        }

        private string _payloadmessageDevalued = null;
        /// <summary>
        /// the log message without meta data (like timestamp) where all literals (like UIDs, names, etc.) were replaced by "<@>"
        /// </summary>
        public string MessageDevalued
        {
            get
            {               
                //expensive task, take the cached value or execute the regex replace
                if (_payloadmessageDevalued == null)
                    _payloadmessageDevalued = Constants.regexLiterals.Replace(Message ?? "", "<@>");

                return _payloadmessageDevalued;
            }
        }

        /// <summary>
        /// the complete log message incl. meta data (timestamp, log level, ids and log message text)
        /// </summary>
        public string FullMessage
        {
            get 
            {
                LazyInitializer.EnsureInitialized(ref _MessageText, () =>
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

                return _MessageText;
            }
        }

        private string _MessageText;


        //calculated attributes
        //public LogfileType messageLogfileType { get;  set; } = LogfileType.Undef;
        public DateTime TimeStamp { get; private set; }         
        public int numberOfLines { get; private set; }
        public LogLevel Level { get; private set; }
        public string Logger { get; private set; } 
        public string Pid { get; private set; }
        public string Spid { get; private set; }
        

        

        //constructor
        public TextMessage(TextLocator textLocator, string initialText) :
            this(
                new LogEntry {
                    Locator = new Locator(
                        Convert.ToInt32(textLocator.EntryNumber),
                        Convert.ToInt32(textLocator.Position),
                        textLocator.Source),
                    TimeStamp = DateTime.MinValue,
                    Message = initialText
                }
            )
        { }

        public TextMessage(LogEntry entry)
        {
            // check the parameters
            if (entry == null) throw new ArgumentNullException(nameof(entry));

            m_Entry = entry;
            Locator = new TextLocator(entry.Locator.Source, entry.Locator.Position, entry.Locator.EntryNumber);
            Message = entry.Message;
            TimeStamp = entry.TimeStamp;
            numberOfLines = 1; // TODO ?? entry.Message.Count(t => t == '\n') ??
            Level = entry.Level;
            Logger = entry.Logger;
            Pid = entry.Pid;
            Spid = entry.Spid;
            entry.Tag = this; 
        }

        public override string ToString()
        {
            //more or less for debugging
            //for productive usage take property messageText
            return string.Format("msg #{0} in line {1}@{2}: {3}", 
                    Locator.EntryNumber, 
                    Locator.Position, 
                    Locator.Source, 
                    FullMessage);
        }


        /// <summary>
        /// this is to have an existing message and you need to put payload messages from other TextMessages at the end of this payload message
        /// </summary>
        /// <param name="mergeCandidates"></param>
        /// <param name="firstIndex">start position of the mergeCandidates list</param>
        /// <param name="lastIndex">last position of the mergeCandidates list</param>
        /// <returns>a new text message object with meta data of current message and message text of this message plus all merge candidates</returns>
        public TextMessage Merge(IReadOnlyList<TextMessage> mergeCandidates, int firstIndex = 0, int lastIndex = 0)
        {
            firstIndex = firstIndex.EnsureRange(0, mergeCandidates.Count - 1);
            lastIndex = lastIndex.EnsureRange(firstIndex, mergeCandidates.Count - 1);

            StringBuilder sb = new StringBuilder(this.Message);
            int numberOfLinesPassed = 0;

            for (int i = firstIndex; i <= lastIndex; i++)
            {
                sb.Append(mergeCandidates[i].FullMessage); //we know that each messageText object ends with <newline>
                numberOfLinesPassed += mergeCandidates[i].numberOfLines;
            }
            
            return Clone(sb.ToString(), this.numberOfLines + numberOfLinesPassed);
        }

        /// <summary>
        /// create a clone of current log message with a new (optional) message text 
        /// </summary>
        /// <param name="newText">optional: the new message to put into the clone (this is not parsed again, meta data is ignored)</param>
        /// <param name="numberOfLinesPassed">optional: pre-calculated number of lines (meta data for statistics) to put into the clone</param>
        /// <returns></returns>
        public TextMessage Clone(string newText = null, int numberOfLinesPassed = -1)
        {
            //so we have 2 scenarios:
            //1.) newText is null, means Clone() was called to make an exact copy of THIS
            //2.) newText is not null, means Clone("msg1\n+msg2\n") was called, basically from the Merge() Method. So some attributes needs to be recalculated, some right now, some by calling FinalizeMessage() again


            //create a new transfer message
            //TextMessage tm = new TextMessage(textLocator, newText ?? messageText);
            TextMessage tm = new TextMessage(m_Entry);

            //ensure all data from this text message will be available in the transfer msg too, so populate attributes which were not populated in the constructor
            tm._contextMsgAfter = ContextNextEntries;
            tm._contextMsgBefore = ContextPreviousEntries;
            //tm.messageLogfileType = messageLogfileType;

            //special handling for count of lines as this attribute is not handled by the finalized method below
            int newNumberOfLines = numberOfLines;

            //are we in scenario 2?
            if (!string.IsNullOrEmpty(newText))
            {
                if (numberOfLinesPassed > 0)
                    newNumberOfLines = numberOfLinesPassed;
                else
                    newNumberOfLines = newText.Count(c => c == '\n');
            }

            //put the current old message's attributes into the transfer message
            tm.numberOfLines = newNumberOfLines;
            if (newText != null)
                tm.Message = $"{tm.Message}\n{newText}";

            //return the transfer message a new cloned message
            return tm;
        }

        public bool EqualMetaData(TextMessage refMsg, int tolleranceTimestampDiff_ms = 85)
        {
            return
                //refMsg.loggerLevel == loggerLevel &&
                refMsg.Logger == Logger &&
                refMsg.Locator.Source == Locator.Source &&
                refMsg.Spid == Spid &&
                refMsg.Pid == Pid &&
                refMsg.TimeStamp.AlmostEqual(TimeStamp, tolleranceTimestampDiff_ms);
        }

    }
}
