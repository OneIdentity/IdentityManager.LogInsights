﻿using System;
using System.Collections.Generic;

using LogfileMetaAnalyser.Controls;
using LogfileMetaAnalyser.Helpers;


namespace LogfileMetaAnalyser.Datastore
{
    public class GeneralLogData
    {
        public DateTime logDataOverallTimeRange_Start = DateTime.MinValue;
        public DateTime logDataOverallTimeRange_Finish;
        public Helpers.Loglevels mostDetailedLogLevel = Helpers.Loglevels.Undef;
        public Dictionary<Helpers.Loglevels, long> numberOfEntriesPerLoglevel = new Dictionary<Helpers.Loglevels, long>();

        public Dictionary<string, LogfileInformation> logfileInformation = new Dictionary<string, LogfileInformation>();  //key == file name
        public List<TimeGap> timegaps = new List<TimeGap>();

        public List<DatastoreBaseDataPoint> messageErrors = new List<DatastoreBaseDataPoint>();
        public List<DatastoreBaseDataPoint> messageWarnings = new List<DatastoreBaseDataPoint>();

        public Dictionary<string, long> numberOflogSources = new Dictionary<string, long>(); //key == Log Source

        public GeneralLogData()
        { }
 
    }
        
}
