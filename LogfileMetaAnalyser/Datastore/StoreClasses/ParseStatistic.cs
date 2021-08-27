using System;

namespace LogfileMetaAnalyser.Datastore
{
    public class ParseStatistic
    {
        public string filename = "N/A";
        public long filesizeKb = -1;
        public double readAndParseFileDuration = -1d; //unit is ms
        public double overallDuration = -1d; //unit is ms


        public ParseStatistic() { }
    }
}
