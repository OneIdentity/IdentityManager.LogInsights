using System; 
using System.Linq;
using System.Text;
using System.Collections.Generic;
using System.Text.Json.Serialization;

using LogfileMetaAnalyser.Helpers;
using LogfileMetaAnalyser.LogReader;



namespace LogfileMetaAnalyser
{
    public enum LogfileType
    {
        Undef,
        NLogDefault,
        Jobservice
    }


    public class TextMessage
    {
        private readonly LogEntry m_Entry;
        private object locky = new object();


        //set from outside, no calculation
        public TextLocator textLocator;


        [JsonIgnore(Condition = JsonIgnoreCondition.Always)]
        public TextMessage[] contextMsgBefore;


        [JsonIgnore(Condition = JsonIgnoreCondition.Always)]
        public TextMessage[] contextMsgAfter;

                

        private string _payloadmessage;        
        /// <summary>
        /// the log message without the meta data (like timestamp)
        /// </summary>
        public string payloadmessage
        {
            get
            {
                return _payloadmessage;
            }

            private set
            {
                _payloadmessage = value;
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
        public string payloadmessageDevalued
        {
            get
            {               
                //expensive task, take the cached value or execute the regex replace
                if (_payloadmessageDevalued == null)
                    _payloadmessageDevalued = Constants.regexLiterals.Replace(payloadmessage ?? "", "<@>");

                return _payloadmessageDevalued;
            }
        }

        /// <summary>
        /// the complete log message incl. meta data (timestamp, log level, ids and log message text)
        /// </summary>
        public string messageText
        {
            get
            {
                //generate a message display
                string id = pid ?? "";

                if (!string.IsNullOrEmpty(spid))
                    id = $"{id} {spid}";
                                                             
                return $"{messageTimestamp:yyyy-MM-dd HH:mm:ss.ffff} {loggerLevel} ({id}): {payloadmessage}";                
            }
        }


        //calculated attributes
        public LogfileType messageLogfileType { get;  set; } = LogfileType.Undef;
        public DateTime messageTimestamp { get; private set; }         
        public int numberOfLines { get; private set; }
        public LogLevel loggerLevel { get; private set; }
        public string loggerSource { get; private set; } 
        public string pid { get; private set; }
        public string spid { get; private set; }
        

        

        //constructor
        public TextMessage(TextLocator textLocator, string initialText): 
            this(
                new LogEntry(
                    new Locator(
                        Convert.ToInt32(textLocator.fileStreamOffset),
                        Convert.ToInt32(textLocator.fileLinePosition),
                        textLocator.fileName), 
                    "", 
                    DateTime.MinValue, 
                    LogLevel.Info, 
                    0, 
                    initialText, 
                    "", "", "", "")
                )
        { }

        public TextMessage(LogEntry entry)
        {
            // check the parameters
            if (entry == null) throw new ArgumentNullException(nameof(entry));

            m_Entry = entry;
            textLocator = new TextLocator(entry.Locator.Source, -1, entry.Locator.Position, entry.Locator.EntryNumber);
            payloadmessage = entry.Message;
            messageTimestamp = entry.TimeStamp;
            numberOfLines = 1; // TODO ?? entry.Message.Count(t => t == '\n') ??
            loggerLevel = entry.Level;
            loggerSource = entry.Logger;
            pid = entry.Pid;
            spid = entry.Spid; 
        }

        public override string ToString()
        {
            //more or less for debugging
            //for productive usage take property messageText
            return string.Format("msg #{0} in line {1}@{2}/{3}: {4}", 
                    textLocator.messageNumber, 
                    textLocator.fileLinePosition, 
                    textLocator.fileName, 
                    textLocator.fileStreamOffset, 
                    messageText);
        }

        //public TextMessage GetShrunkMessage()
        //{
        //    //only attributes textLocator and messageText are valid

        //    TextMessage tm = new TextMessage(this.textLocator, this.messageText);
        //    tm.FillObject("", "", "", "", "", this.messageLogfileType, null, null, messageTimestamp, -1, FinalizeStates.Finalized);

        //    return tm;           
        //}

        //public void AppendMessageLine(string messageTextToAppend)
        //{
        //    if (messageTextToAppend == null)
        //        return;

        //    if (finalizeState == FinalizeStates.Finalized)
        //        throw new Exception("This text message object is already finalized and marked as read only!");

        //    if (_stringbuilder == null)
        //        _stringbuilder = new StringBuilder(_messageText);

        //    _stringbuilder.AppendLine(messageTextToAppend);
        //    numberOfLines++; 
        //}

        /*internal void FillObject(
                LogLevel loggerLevel,
                string loggerSource, 
                string pid, string spid,
                string payloadmessage,
                LogfileType messageLogfileType,
                TextMessage[] contextMsgBefore,
                TextMessage[] contextMsgAfter,
                DateTime messageTimestamp,
                int numberOfLines 
            )
        {
            this.loggerLevel = loggerLevel;
            this.loggerSource = loggerSource;                        
            this.pid = pid;
            this.spid = spid;
            this.payloadmessage = payloadmessage;
            this.messageLogfileType = messageLogfileType;
            this.contextMsgBefore = contextMsgBefore;
            this.contextMsgAfter = contextMsgAfter;
            this.messageTimestamp = messageTimestamp;
            this.numberOfLines = numberOfLines;
        }*/

        /// <summary>
        /// this is to have an existing message and you need to put payload messages from other TextMessages at the end of this payload message
        /// </summary>
        /// <param name="mergeCandidates"></param>
        /// <param name="firstIndex">start position of the mergeCandidates list</param>
        /// <param name="lastIndex">last position of the mergeCandidates list</param>
        /// <returns>a new text message object with meta data of current message and message text of this message plus all merge candidates</returns>
        public TextMessage Merge(List<TextMessage> mergeCandidates, int firstIndex = 0, int lastIndex = 0)
        {
            firstIndex = firstIndex.EnsureRange(0, mergeCandidates.Count - 1);
            lastIndex = lastIndex.EnsureRange(firstIndex, mergeCandidates.Count - 1);

            StringBuilder sb = new StringBuilder(this.payloadmessage);
            int numberOfLinesPassed = 0;

            for (int i = firstIndex; i <= lastIndex; i++)
            {
                sb.Append(mergeCandidates[i].messageText); //we know that each messageText object ends with <newline>
                numberOfLinesPassed += mergeCandidates[i].numberOfLines;
            }
            
            return Clone(sb.ToString(), this.numberOfLines + numberOfLinesPassed);
        }

        /// <summary>
        /// create a clone of current log message with a new (optional) message text 
        /// </summary>
        /// <param name="newText">optional: the new message to put into the clone (this is not parsed again, meta data is ignored)</param>
        /// <param name="numberofLinesPassed">optional: pre-calculated number of lines (meta data for statistics) to put into the clone</param>
        /// <returns></returns>
        public TextMessage Clone(string newText = null, int numberofLinesPassed = -1)
        {
            //so we have 2 scenarios:
            //1.) newText is null, means Clone() was called to make an exact copy of THIS
            //2.) newText is not null, means Clone("msg1\n+msg2\n") was called, basically from the Merge() Method. So some attributes needs to be recalculated, some right now, some by calling FinalizeMessage() again


            //create a new transfer message
            //TextMessage tm = new TextMessage(textLocator, newText ?? messageText);
            TextMessage tm = new TextMessage(m_Entry);

            //ensure all data from this text message will be available in the transfer msg too, so populate attributes which were not populated in the constructor
            tm.contextMsgAfter = contextMsgAfter;
            tm.contextMsgBefore = contextMsgBefore;
            tm.messageLogfileType = messageLogfileType;

            //special handling for count of lines as this attribute is not handled by the finalized method below
            int newnumberOfLines = numberOfLines;

            //are we in scenario 2?
            if (!string.IsNullOrEmpty(newText))
            {
                if (numberofLinesPassed > 0)
                    newnumberOfLines = numberofLinesPassed;
                else
                    newnumberOfLines = newText.Count(c => c == '\n');
            }

            //put the current old message's attributes into the transfer message
            tm.numberOfLines = newnumberOfLines;
            if (newText != null)
                tm.payloadmessage = $"{tm.payloadmessage}\n{newText}";

            /*tm.FillObject(
                this.loggerLevel,
                this.loggerSource, 
                this.pid,
                this.spid,
                this.payloadmessage,
                this.messageLogfileType,
                this.contextMsgBefore,  //might be OK to take the context messages of this first message
                this.contextMsgAfter,  //might be OK to take the context messages of this first message, but we might loose the last messages  
                this.messageTimestamp,
                newnumberOfLines  //numberOfLines
            );
            */

            //tm.FinalizeMessage(this.messageLogfileType);

            
            //return the transfer message a new cloned message
            return tm;
        }

        public bool EqualMetaData(TextMessage refMsg, int tolleranceTimestampDiff_ms = 85)
        {
            return (
                //refMsg.loggerLevel == loggerLevel &&
                refMsg.loggerSource == loggerSource && 
                refMsg.textLocator.fileName == textLocator.fileName &&
                refMsg.spid == spid &&
                refMsg.pid == pid &&
                refMsg.messageTimestamp.AlmostEqual(messageTimestamp, tolleranceTimestampDiff_ms)
                );
        }

        /*
        public TextMessage FinalizeMessage(LogfileType expectedLogfileType = LogfileType.Undef)
        {
            lock (locky)
            {                 
                if (messageLogfileType == LogfileType.Undef && expectedLogfileType != LogfileType.Undef)
                    messageLogfileType = expectedLogfileType;

                FinalizeMessage();                
            }
            return this;
        }*/
                
        private void FinalizeMessage()
        {
            return;

            //GlobalStopWatch.StartWatch("FinalizeMessage");
            //DateTime dt;

            //if (_stringbuilder != null) //when this message consists of more than one msg, we need to take stringbuilder;
            //    _messageText = _stringbuilder.ToString();

            //_stringbuilder = null;

            //try
            //{                
            //    if (messageLogfileType == LogfileType.Undef || messageLogfileType == LogfileType.NLogDefault)
            //    {
            //        GlobalStopWatch.StartWatch("FinalizeMessage.NLogDefault");
            //        var rm = Constants.regexMessageMetaDataNLogDefault.Match(messageText);
            //        if (rm.Success)
            //        {
            //            GlobalStopWatch.StartWatch("FinalizeMessage.NLogDefault.ParseTimestamp");
            //            if (!DateTime.TryParse(rm.Groups["Timestamp"].Value, out dt))
            //                messageTimestamp = DateTime.MinValue;
            //            else
            //                messageTimestamp = dt;
            //            GlobalStopWatch.StopWatch("FinalizeMessage.NLogDefault.ParseTimestamp");

            //            loggerLevel = rm.Groups["NLevel"].Value;
            //            loggerSource = rm.Groups["NSource"].Value;

            //            spid = string.Format("{1}{0}",
            //                                    rm.Groups["SID"].Value,
            //                                    rm.Groups["NSourceExt"].Value != ""
            //                                        ? rm.Groups["NSourceExt"].Value
            //                                        : rm.Groups["NSourceExt2"].Value.Trim());

            //            pid = rm.Groups["PID"].Value;                        
            //            payloadmessage = rm.Groups["Payload"].Value;

            //            messageLogfileType = LogfileType.NLogDefault;

            //            return;
            //        }
            //    }
                
            //    if (messageLogfileType == LogfileType.Undef || messageLogfileType == LogfileType.Jobservice)
            //    {
            //        GlobalStopWatch.StartWatch("FinalizeMessage.Jobservice");
            //        var rm = Constants.regexMessageMetaDataJobservice.Match(messageText);
            //        if (rm.Success)
            //        {
            //            GlobalStopWatch.StartWatch("FinalizeMessage.Jobservice.ParseTimestamp");
            //            if (!DateTime.TryParse(rm.Groups["Timestamp"].Value, out dt))
            //                messageTimestamp = DateTime.MinValue;
            //            else
            //                messageTimestamp = dt;
            //            GlobalStopWatch.StopWatch("FinalizeMessage.Jobservice.ParseTimestamp");

            //            string tags = rm.Groups["tag"].Value;
            //            loggerLevel = "Info";
            //            loggerSource = "JobService";

            //            if (tags.Contains("<d>")) loggerLevel = "Debug";
            //            if (tags.Contains("<s>")) loggerLevel = "Info";
            //            if (tags.Contains("<w>")) loggerLevel = "Warning";
            //            if (tags.Contains("<e>")) loggerLevel = "Error";
            //            if (tags.Contains("<r>")) loggerLevel = "Error";

            //            spid = rm.Groups["SID"]?.Value ?? "";

            //            payloadmessage = rm.Groups["Payload"].Value;

            //            messageLogfileType = LogfileType.Jobservice;

            //            return;
            //        }
            //    }


            //    //ok, hopefully we get at least the timestamp
            //    GlobalStopWatch.StartWatch("FinalizeMessageStage1.at least the timestamp");
            //    try
            //    {
            //        loggerLevel = "";
            //        loggerSource = ""; 
            //        pid = "";
            //        spid = "";
            //        messageLogfileType = LogfileType.Undef;


            //        var rm = Constants.regexTimeStampAtLinestart.Match(messageText);

            //        if (rm.Success && DateTime.TryParse(rm.Groups["Timestamp"].Value, out dt))
            //        {
            //            messageTimestamp = dt;
            //            payloadmessage = messageText.Remove(rm.Groups["Timestamp"].Index, (rm.Groups["Timestamp"].Length));
            //        }
            //        else
            //        {
            //            messageTimestamp = DateTime.MinValue;
            //            payloadmessage = messageText;
            //        }
            //    }
            //    catch { }
            //    GlobalStopWatch.StopWatch("FinalizeMessage.at least the timestamp");
            //}
            //finally
            //{
            //    finalizeState = FinalizeStates.Finalized;
            //    GlobalStopWatch.StopWatch("FinalizeMessage");
            //    GlobalStopWatch.StopWatch("FinalizeMessage.NLogDefault");
            //    GlobalStopWatch.StopWatch("FinalizeMessage.Jobservice");
            //}
        }
    }
}
