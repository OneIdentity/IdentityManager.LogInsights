using System;
using System.Collections.Generic;
using System.Linq;

namespace LogInsights.Datastore
{
    public class LogfileInformation
    {
        public string filename = "";
        public string filenameBestNotation = "";
        public long filesize; //in Byte

        public DateTime logfileTimerange_Start = DateTime.MinValue;
        public DateTime logfileTimerange_Finish;
                
        public Helpers.LogLevel mostDetailedLogLevel = Helpers.LogLevel.Undef;

        public Dictionary<Helpers.LogLevel, long> numberOfEntriesPerLoglevel = new Dictionary<Helpers.LogLevel, long>();

        public TextMessage firstMessage;

        //log file text stats
        public float avgCharsPerLine = -1;  //line.Length()
        public float avgCharsPerBlockmsg = -1;  //message.Length()
        public float avgLinesPerBlockmsg = -1;  
        public long cntLines = 0;  //number of raw rows
        public long cntBlockMsgs = 0;  //number of message objects
        public long charsRead; //we cannot simple mark FileSize for that as the low level optimizer will not push all messages to the connectors


        public LogfileInformation()
        { }

        public string GetLabel()
        {
            return ($"{filenameBestNotation} with log level {mostDetailedLogLevel} and range '{logfileTimerange_Start}' to '{logfileTimerange_Finish}'");
        }
    }
}
