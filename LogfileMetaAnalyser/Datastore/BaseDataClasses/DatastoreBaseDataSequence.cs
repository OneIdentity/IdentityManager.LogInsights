using System;
using System.Collections.Generic;
using System.Linq;

using LogfileMetaAnalyser.Helpers;

namespace LogfileMetaAnalyser.Datastore
{
    /// <summary>
    /// represents a chain of ordered time points while the whole overall event has a duration, an start point and an end point
    /// </summary>
    class DatastoreBaseDataSequence
    {
        public List<DatastoreBaseDataPoint> subEventPoints = new List<DatastoreBaseDataPoint>();

        public long logfilePositionStart
        {
            get { return subEventPoints.Count > 0 ? subEventPoints[0].logfilePosition : -1 ; }
        }

        public long logfilePositionEnd
        {
            get { return subEventPoints.Count > 0 ? subEventPoints[^1].logfilePosition : -1; }
        }


        public string logfileNameStart
        {
            get { return subEventPoints.Count > 0 ? subEventPoints[0].logfileName : ""; }
        }

        public string logfileNameEnd
        {
            get { return subEventPoints.Count > 0 ? subEventPoints[^1].logfileName : ""; }
        }

        public DateTime dtTimestampSequenceStart
        {
            get { return subEventPoints.Count > 0 ? subEventPoints[0].dtTimestamp : DateTime.MinValue; }
        }

        public DateTime dtTimestampSequenceEnd
        {
            get { return subEventPoints.Count > 0 ? subEventPoints[^1].dtTimestamp : DateTime.MinValue; }
        }

        public uint durationMin
        {
            get { return subEventPoints.Count > 0 ? Convert.ToUInt32(Math.Abs(Math.Ceiling((dtTimestampSequenceEnd - dtTimestampSequenceStart).TotalMinutes))) : 0; } 
        }

        public uint durationSec
        {
            get { return subEventPoints.Count > 0 ? Convert.ToUInt32(Math.Abs(Math.Ceiling((dtTimestampSequenceEnd - dtTimestampSequenceStart).TotalSeconds))) : 0; }
        }

        public string DurationString()
        {
            if (subEventPoints.Count == 0)
                return ("N/A");
            else
                return (dtTimestampSequenceEnd - dtTimestampSequenceStart).EnsurePositive().ToHumanString();
        }


        public DatastoreBaseDataSequence()  
        { }

        public override string ToString()
        {
            if (subEventPoints.Count == 0)
                return "event sequence with no data";

            int cnt = subEventPoints.Count;

            if (cnt == 1)
                return $"event sequence with one event at {subEventPoints[0].ToString()}";

            return $"event sequence with {cnt} events from {dtTimestampSequenceStart.ToString("yyyy-MM-dd hh:mm:ss")} to {dtTimestampSequenceEnd.ToString("yyyy-MM-dd hh:mm:ss")}";
        }

        public string GetEventLocator()
        {
            return string.Join(";", subEventPoints.Select(e => e.GetEventLocator()));
        }
    }
}
