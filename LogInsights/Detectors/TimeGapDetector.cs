using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LogInsights.Datastore;
using LogInsights.Helpers;
using LogInsights.LogReader;

namespace LogInsights.Detectors
{
    /// <summary>
    /// detector tries to find time gaps in log files. Based on the log level a specific amount of time between gaps is allowed
    /// </summary>
    class TimeGapDetector : DetectorBase, ILogDetector
    {
        private static int gap_threshold_WhenLogIsOnLevel_Error = 60 * 60 * 8; //8 h between 2 messages allowed
        private static int gap_threshold_WhenLogIsOnLevel_Warning = 60 * 60*3; //3 h between 2 messages allowed
        private static int gap_threshold_WhenLogIsOnLevel_Info = 60 * 120; //120 min between 2 messages allowed
        private static int gap_threshold_WhenLogIsOnLevel_Debug = 60 * 20; //20 min between 2 messages allowed
        private static int gap_threshold_WhenLogIsOnLevel_Trace = 60 * 10; //10 min between 2 messages allowed

        private static int gap_threshold_WhenLogIsOnLevel_Min = gap_threshold_WhenLogIsOnLevel_Trace;

        private string CurrentLogfilename = "";
        private DateTime CurrentLogfileTime = DateTime.MinValue;

        private List<TimeGap> timegaps = new List<TimeGap>();

        public override string caption => "Found large time gaps";


        public override string identifier =>"#TimeGapDetector";


        public override string[] requiredParentDetectors => new []{"#TimeRangeDetector"};


        public void InitializeDetector()
        {
            CurrentLogfilename = "";
            CurrentLogfileTime = DateTime.MinValue;
            timegaps = new List<TimeGap>();
			detectorStats.Clear();

            logger.Debug($"globol set gap_threshold_WhenLogIsOnLevel = {gap_threshold_WhenLogIsOnLevel_Min} min");
        }

        public void FinalizeDetector()
        {
            logger.Debug("entering FinalizeDetector()");

            long tcStart = Environment.TickCount64;

            var generalLogData = _datastore.GetOrAdd<GeneralLogData>();
            var statisticsStore = _datastore.GetOrAdd<StatisticsStore>();

            foreach (var gap in timegaps.Where(g => isRelevantGap(g.logfileNameStart, g.GetGapInSecods())))
                if (!generalLogData.TimeGaps.Any(t => t.dtTimestampStart.AlmostEqual(gap.dtTimestampStart)))
                {
                    generalLogData.TimeGaps.Add(gap);
                    logger.Debug($"pushing to ds: generalLogData timegap {gap}");
                }


            //stats
            detectorStats.detectorName = string.Format("{0} <{1}>", this.GetType().Name, this.identifier);
            detectorStats.numberOfDetections = generalLogData.TimeGaps.Count;
            detectorStats.finalizeDuration = new TimeSpan( Environment.TickCount64 -tcStart).TotalMilliseconds;
            statisticsStore.DetectorStatistics.Add(detectorStats);
            logger.Debug(detectorStats.ToString());


            //dispose
            timegaps = null;
        }

        private bool isRelevantGap(string filename, long gapSec)
        {
            int gap_threshold = gap_threshold_WhenLogIsOnLevel_Min;

            var generalLogData = _datastore.GetOrAdd<GeneralLogData>();

            switch (generalLogData.LogfileInformation[filename].mostDetailedLogLevel)
            {
                case Helpers.LogLevel.Undef: return false;
                case Helpers.LogLevel.Info:
                    gap_threshold = gap_threshold_WhenLogIsOnLevel_Info;
                    break;
                case Helpers.LogLevel.Debug:
                    gap_threshold = gap_threshold_WhenLogIsOnLevel_Debug;
                    break;
                case Helpers.LogLevel.Warn:
                    gap_threshold = gap_threshold_WhenLogIsOnLevel_Warning;
                    break;
                case Helpers.LogLevel.Error:
                    gap_threshold = gap_threshold_WhenLogIsOnLevel_Error;
                    break;
            }

            return (gapSec >= gap_threshold);
        }

        
        public void ProcessMessage(LogEntry msg)
        {
            if (!_isEnabled)
                return;

			long tcStart = Environment.TickCount64;

            var rt = ProcessMessageBase(ref msg);
            if (rt != null)
                foreach (var xmsg in rt)
                    ProcessMessage(xmsg);
            if (msg == null)
                return;

			detectorStats.numberOfLinesParsed += msg.NumberOfLines;    

            var currentTime = msg.TimeStamp;
            if (currentTime == DateTime.MinValue)  //skip invalid timestamps
            {
                logger.Trace($"invalid timestamp: {msg.FullMessage}; skip processing :(");
                return; // :(
            }

            //init? or new logfile? In this case time gaps are expected
            if (CurrentLogfilename != msg.Locator.Source)
            {
                CurrentLogfilename = msg.Locator.Source;
                CurrentLogfileTime = currentTime;

                return;
            }

            //min time gap?
            if ((msg.TimeStamp - CurrentLogfileTime).TotalSeconds > gap_threshold_WhenLogIsOnLevel_Min)
                timegaps.Add(new TimeGap()
                {                    
                    dtTimestampStart = CurrentLogfileTime,
                    dtTimestampEnd = msg.TimeStamp,                    
                    message = msg
                });


            CurrentLogfileTime = currentTime;

            long tcEnd = Environment.TickCount64;
			
			detectorStats.parseDuration += new TimeSpan(tcEnd - tcStart).TotalMilliseconds;			
        }

         

    }


}