using System;
using System.Collections.Generic;

using LogInsights.Helpers;


namespace LogInsights.Datastore
{
    public class GeneralLogData : IDataStoreContent
    {
        public DateTime LogDataOverallTimeRangeStart { get; set; } = DateTime.MinValue;
        public DateTime LogDataOverallTimeRangeFinish { get; set; }

        public LogLevel mostDetailedLogLevel = LogLevel.Undef;
        public Dictionary<LogLevel, long> NumberOfEntriesPerLoglevel { get; } = new();

        public Dictionary<string, LogfileInformation> LogfileInformation { get; } = new();  //key == file name
        public List<TimeGap> TimeGaps { get; } = new();

        public List<DatastoreBaseDataPoint> MessageErrors { get; } = new();
        public List<DatastoreBaseDataPoint> MessageWarnings { get; } = new();

        public Dictionary<string, long> NumberOflogSources { get; } = new(); //key == Log Source

        public bool HasData => !LogDataOverallTimeRangeStart.IsNull() &&
                               !LogDataOverallTimeRangeFinish.IsNull() &&
                               NumberOflogSources.Count > 0;
    }
        
}
