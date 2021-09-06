using System; 
using System.Linq;
using System.Text;
using System.Collections.Generic;

using LogfileMetaAnalyser.Helpers;

namespace LogfileMetaAnalyser
{
    public enum LogfileType
    {
        Undef,
        NLogDefault,
        Jobservice
    }

    public enum FinalizeStates
    {
        NotFinalized, //object is writeable
        Finalized
    }

    public class TextMessage
    {
        private StringBuilder _stringbuilder; 
        public FinalizeStates finalizeState = FinalizeStates.NotFinalized; //when true, the object is read only
        private object locky = new object();


        //set from outside, no calculation
        public TextLocator textLocator;
        /*public string textLocator.fileName = "";
        public long textLocator.fileLinePosition = -1;
        public long messageNumber = -1;*/

        public TextMessage[] contextMsgBefore;
        public TextMessage[] contextMsgAfter;

        private string _payloadmessage;
        public string payloadmessage
        {
            get
            {
                if (finalizeState == FinalizeStates.NotFinalized)
                    throw new Exception("This text message property is unavailable unless you finalize the message object!");

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
        public string payloadmessageDevalued
        {
            get
            {
                if (finalizeState == FinalizeStates.NotFinalized)
                    throw new Exception("This text message property is unavailable unless you finalize the message object!");

                GlobalStopWatch.StartWatch("Get payloadmessageDevalued");
                //expensive task, take the cached value or execute the regex replace
                if (_payloadmessageDevalued == null)
                    _payloadmessageDevalued = Constants.regexLiterals.Replace(payloadmessage ?? "", "<@>");

                GlobalStopWatch.StopWatch("Get payloadmessageDevalued");
                return _payloadmessageDevalued;
            }
        }

        private string _messageText;
        public string messageText
        {
            set
            {
                if (finalizeState != FinalizeStates.NotFinalized)
                    throw new Exception("This text message property is unavailable unless you finalize the message object!");


                //the first text of a message cannot be null or empty
                if (string.IsNullOrWhiteSpace(value))
                {
                    numberOfLines = 0;
                    return;
                }

                _messageText = value;
                numberOfLines = 1;
            }

            get
            {
                return _messageText;
            }
        }


        //calculated attributes
        public LogfileType messageLogfileType { get;  set; } = LogfileType.Undef;
        public DateTime messageTimestamp { get; private set; }         
        public int numberOfLines { get; private set; }
        public string loggerLevel { get; private set; }
        public string loggerSource { get; private set; } 
        public string pid { get; private set; }
        public string spid { get; private set; }
        

        

        //constructor
        public TextMessage(TextLocator textLocator, string initialText)
        {
            this.textLocator = textLocator;
            this.messageText = initialText;
        }


        public override string ToString()
        {
            return string.Format("msg #{0} in line {1}@{2}/{3}: {4}", textLocator.messageNumber, textLocator.fileLinePosition, textLocator.fileName, textLocator.fileStreamOffset, messageText);
        }

        public TextMessage GetShrunkMessage()
        {
            //only attributes textLocator and messageText are valid

            TextMessage tm = new TextMessage(this.textLocator, this.messageText);
            tm.FillObject("", "", "", "", "", this.messageLogfileType, null, null, messageTimestamp, -1, FinalizeStates.Finalized);

            return tm;           
        }
        
        public void AppendMessageLine(string messageTextToAppend)
        {
            if (messageTextToAppend == null)
                return;
            
            if (finalizeState == FinalizeStates.Finalized)
                throw new Exception("This text message object is already finalized and marked as read only!");

            if (_stringbuilder == null)
                _stringbuilder = new StringBuilder(_messageText);

            _stringbuilder.AppendLine(messageTextToAppend);
            numberOfLines++; 
        }
                       
        internal void FillObject( 
                string loggerLevel,
                string loggerSource, 
                string pid, string spid,
                string payloadmessage,
                LogfileType messageLogfileType,
                TextMessage[] contextMsgBefore,
                TextMessage[] contextMsgAfter,
                DateTime messageTimestamp,
                int numberOfLines,
                FinalizeStates finalizeState
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
            this.finalizeState = finalizeState;

            _stringbuilder = finalizeState == FinalizeStates.NotFinalized ? 
                                new StringBuilder(this.messageText) : 
                                null;
        }

        public TextMessage Merge(List<TextMessage> mergeCandidates, int firstIndex = 0, int lastIndex = 0)
        {
            if (finalizeState == FinalizeStates.NotFinalized)
                throw new Exception("Cannot clone this message because it was not yet finalized");

            if (mergeCandidates.Any(m => m.finalizeState == FinalizeStates.NotFinalized))
                throw new Exception("Cannot clone this message because passed text messages were not yet finalized");

            firstIndex = firstIndex.EnsureRange(0, mergeCandidates.Count - 1);
            lastIndex = lastIndex.EnsureRange(firstIndex, mergeCandidates.Count - 1);

            StringBuilder sb = new StringBuilder(this.messageText);
            int numberOfLinesPassed = 0;

            for (int i = firstIndex; i <= lastIndex; i++)
            {
                sb.Append(mergeCandidates[i].messageText); //we know that each messageText object ends with <newline>
                numberOfLinesPassed += mergeCandidates[i].numberOfLines;
            }
            
            return Clone(sb.ToString(), this.numberOfLines + numberOfLinesPassed);
        }

        public TextMessage Clone(string newText = null, int numberofLinesPassed = -1)
        {
            if (finalizeState == FinalizeStates.NotFinalized)
                throw new Exception("Cannot clone this message because it was not yet finalized");

            //so we have 2 scenarios:
            //1.) newText is null, means Clone() was called to make an exact copy of THIS
            //2.) newText is not null, means Clone("msg1\n+msg2\n") was called, basically from the Merge() Method. So some attributes needs to be recalculated, some right now, some by calling FinalizeMessage() again


            GlobalStopWatch.StartWatch("Clone() (incl. Finalize())");

            //create a new transfer message
            TextMessage tm = new TextMessage(textLocator, newText ?? messageText);

            //special handling for count of lines as this attribute is not handled by the finalized method below
            int newnumberOfLines = numberOfLines;
            FinalizeStates newFinalizedState = finalizeState;

            //are we in scenario 2?
            if (newText != null)
            {
                if (numberofLinesPassed > 0)
                    newnumberOfLines = numberofLinesPassed;
                else
                    newnumberOfLines = newText.Count(c => c == '\n');

                newFinalizedState = FinalizeStates.NotFinalized;
            }

            //put the current old message's attributes into the transfer message
            tm.FillObject(
                this.loggerLevel,
                this.loggerSource, 
                this.pid,
                this.spid,
                this.payloadmessage,
                this.messageLogfileType,
                this.contextMsgBefore,  //might be OK to take the context messages of this first message
                this.contextMsgAfter,  //might be OK to take the context messages of this first message, but we might loose the last messages  
                this.messageTimestamp,
                newnumberOfLines,  //numberOfLines
                newFinalizedState
            );

            if (newFinalizedState == FinalizeStates.NotFinalized)
                tm.FinalizeMessage(this.messageLogfileType);

            GlobalStopWatch.StopWatch("Clone() (incl. Finalize())");

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

        public TextMessage FinalizeMessage(LogfileType expectedLogfileType = LogfileType.Undef)
        {
            lock (locky)
            {
                if (finalizeState == FinalizeStates.NotFinalized)
                {
                    if (messageLogfileType == LogfileType.Undef && expectedLogfileType != LogfileType.Undef)
                        messageLogfileType = expectedLogfileType;

                    FinalizeMessage();
                } 
            }
            return this;
        }
                
        private void FinalizeMessage()
        {
            GlobalStopWatch.StartWatch("FinalizeMessage");
            DateTime dt;

            if (_stringbuilder != null) //when this message consists of more than one msg, we need to take stringbuilder;
                _messageText = _stringbuilder.ToString();

            _stringbuilder = null;

            try
            {                
                if (messageLogfileType == LogfileType.Undef || messageLogfileType == LogfileType.NLogDefault)
                {
                    GlobalStopWatch.StartWatch("FinalizeMessage.NLogDefault");
                    var rm = Constants.regexMessageMetaDataNLogDefault.Match(messageText);
                    if (rm.Success)
                    {
                        GlobalStopWatch.StartWatch("FinalizeMessage.NLogDefault.ParseTimestamp");
                        if (!DateTime.TryParse(rm.Groups["Timestamp"].Value, out dt))
                            messageTimestamp = DateTime.MinValue;
                        else
                            messageTimestamp = dt;
                        GlobalStopWatch.StopWatch("FinalizeMessage.NLogDefault.ParseTimestamp");

                        loggerLevel = rm.Groups["NLevel"].Value;
                        loggerSource = rm.Groups["NSource"].Value;

                        spid = string.Format("{1}{0}",
                                                rm.Groups["SID"].Value,
                                                rm.Groups["NSourceExt"].Value != ""
                                                    ? rm.Groups["NSourceExt"].Value
                                                    : rm.Groups["NSourceExt2"].Value.Trim());

                        pid = rm.Groups["PID"].Value;                        
                        payloadmessage = rm.Groups["Payload"].Value;

                        messageLogfileType = LogfileType.NLogDefault;

                        return;
                    }
                }
                
                if (messageLogfileType == LogfileType.Undef || messageLogfileType == LogfileType.Jobservice)
                {
                    GlobalStopWatch.StartWatch("FinalizeMessage.Jobservice");
                    var rm = Constants.regexMessageMetaDataJobservice.Match(messageText);
                    if (rm.Success)
                    {
                        GlobalStopWatch.StartWatch("FinalizeMessage.Jobservice.ParseTimestamp");
                        if (!DateTime.TryParse(rm.Groups["Timestamp"].Value, out dt))
                            messageTimestamp = DateTime.MinValue;
                        else
                            messageTimestamp = dt;
                        GlobalStopWatch.StopWatch("FinalizeMessage.Jobservice.ParseTimestamp");

                        string tags = rm.Groups["tag"].Value;
                        loggerLevel = "Info";
                        loggerSource = "JobService";

                        if (tags.Contains("<d>")) loggerLevel = "Debug";
                        if (tags.Contains("<s>")) loggerLevel = "Info";
                        if (tags.Contains("<w>")) loggerLevel = "Warning";
                        if (tags.Contains("<e>")) loggerLevel = "Error";
                        if (tags.Contains("<r>")) loggerLevel = "Error";

                        spid = rm.Groups["SID"]?.Value ?? "";

                        payloadmessage = rm.Groups["Payload"].Value;

                        messageLogfileType = LogfileType.Jobservice;

                        return;
                    }
                }


                //ok, hopefully we get at least the timestamp
                GlobalStopWatch.StartWatch("FinalizeMessageStage1.at least the timestamp");
                try
                {
                    loggerLevel = "";
                    loggerSource = ""; 
                    pid = "";
                    spid = "";
                    messageLogfileType = LogfileType.Undef;


                    var rm = Constants.regexTimeStampAtLinestart.Match(messageText);

                    if (rm.Success && DateTime.TryParse(rm.Groups["Timestamp"].Value, out dt))
                    {
                        messageTimestamp = dt;
                        payloadmessage = messageText.Remove(rm.Groups["Timestamp"].Index, (rm.Groups["Timestamp"].Length));
                    }
                    else
                    {
                        messageTimestamp = DateTime.MinValue;
                        payloadmessage = messageText;
                    }
                }
                catch { }
                GlobalStopWatch.StopWatch("FinalizeMessage.at least the timestamp");
            }
            finally
            {
                finalizeState = FinalizeStates.Finalized;
                GlobalStopWatch.StopWatch("FinalizeMessage");
                GlobalStopWatch.StopWatch("FinalizeMessage.NLogDefault");
                GlobalStopWatch.StopWatch("FinalizeMessage.Jobservice");
            }
        }
    }
}
