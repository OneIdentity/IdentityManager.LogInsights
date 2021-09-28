using System;
using LogInsights.Helpers;

namespace LogInsights.Datastore
{
    /// <summary>
    /// represents an event, which spreads data occurrences over time, it has therefor a duration
    /// use this class for events where the start and the end matters, so the duration is greater than zero
    /// </summary>
    public class DatastoreBaseDataRange : DatastoreBaseData
    {
        public DateTime dtTimestampStart = DateTime.MinValue;
        public DateTime dtTimestampEnd = DateTime.MinValue;

        public TextMessage messageEnd;

        public long logfilePositionStart
        {
            get { return logfilePosition; }
        }

        public long logfilePositionEnd
        {
            get { return messageEnd == null ? -1 : messageEnd.Locator.fileLinePosition; }
        }
        

        public string logfileNameStart
        {
            get { return logfileName; }           
        }
 
        public string logfileNameEnd
        {
            get { return messageEnd == null ? "" : messageEnd.Locator.fileName; }
        }

        public uint durationMin
        {
            get
            {
                if (!dtTimestampStart.IsNull() && !dtTimestampEnd.IsNull())
                    return Convert.ToUInt32(Math.Abs(Math.Ceiling((dtTimestampEnd - dtTimestampStart).TotalMinutes)));

                return 0;
            }
        }
         
        public uint durationSec
        {          
            get
            {         
                if (!dtTimestampStart.IsNull() && !dtTimestampEnd.IsNull())
                    return Convert.ToUInt32(Math.Abs(Math.Ceiling((dtTimestampEnd- dtTimestampStart).TotalSeconds)));

                return 0;
            }
        }


        public DatastoreBaseDataRange() : base()
        { }

        public override string ToString()
        {
            string dura = DurationString();
            string st = dtTimestampStart.ToString("yyyy-MM-dd hh:mm:ss");
            string en = dtTimestampEnd.IsNull() ? "?" : (dtTimestampEnd.ToString("yyyy-MM-dd hh:mm:ss") + (isDataComplete ? "" : "?"));
            string fn1 = logfileNameStart;
            string fn2 = logfileNameEnd == logfileNameStart ? "" : logfileNameEnd;

            //case 1: pos + file end point data given -> diff: from date:line@file to date:line@file 
            if (logfilePositionEnd > 0 && fn2 != "")
                return string.Format("{6}: from {0}:{1}@{2} to {3}:{4}@{5}",
                    st, logfilePositionStart, fn1,
                    en, logfilePositionEnd, fn2,
                    dura);

            //case 2: only end position given -> from date:line to date:line @file (diff)
            if (logfilePositionEnd > 0 && fn2 == "")
                return string.Format("{5}: from {0}:{1} to {2}:{3} @{4}",
                    st, logfilePositionStart,
                    en, logfilePositionEnd, fn1,
                    dura);

            //case 3: end pos + end file are missing -> date to date @file (diff)
            if (logfilePositionEnd <= 0 && fn2 == "")
                return string.Format("{3}: from {0} to {1} @{2}", st, en, logfileNameStart, dura);


            return string.Format("from {0} to {1}", st, en);
        }

        public string DurationString(bool ignoreIncomplete = false)
        {
            if (ignoreIncomplete)
                return (dtTimestampEnd - dtTimestampStart).EnsurePositive().ToHumanString();

            if (dtTimestampEnd.IsNull() || dtTimestampStart.IsNull())
                return ("N/A");

            return (dtTimestampEnd - dtTimestampStart).EnsurePositive().ToHumanString() + (isDataComplete || ignoreIncomplete ? "" : "?");
        }

        public string GetEventLocatorStart()
        {
            return base.GetEventLocator();
        }
        public string GetEventLocatorEnd()
        {
            if (messageEnd == null)
                return "";


            string logfilenameShort = System.IO.Path.GetFileName(messageEnd.Locator.fileName);

            return ($"{logfilenameShort}@+{messageEnd.Locator.fileLinePosition.ToString()}");
        }
        public override string GetEventLocator()
        {
            return $"{GetEventLocatorStart()} -> {GetEventLocatorEnd()}";
        }
    }
}
