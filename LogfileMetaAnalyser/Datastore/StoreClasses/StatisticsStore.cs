using System;
using System.Collections.Generic;
using System.Linq;

using LogfileMetaAnalyser.Helpers;


namespace LogfileMetaAnalyser.Datastore
{
    public class StatisticsStore
    {
        public List<DetectorStatistic> detectorStatistics = new List<DetectorStatistic>();
        public List<ParseStatistic> parseStatistic = new List<ParseStatistic>();

        public int filesParsed
        {
            get { return parseStatistic.Count(f => f.readAndParseFileDuration >= 0); }
        }


        public StatisticsStore() { }

    }    
}
