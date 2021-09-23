using System.Collections.Generic;
using System.Linq;

namespace LogfileMetaAnalyser.Datastore
{
    public class StatisticsStore : IDataStoreContent
    {
        public List<DetectorStatistic> DetectorStatistics { get; } = new();
        public List<ParseStatistic> ParseStatistic { get; } = new();

        public int FilesParsed => ParseStatistic.Count(f => f.readAndParseFileDuration >= 0);

        public bool HasData => DetectorStatistics.Count > 0 ||
                               ParseStatistic.Count > 0;
    }    
}
