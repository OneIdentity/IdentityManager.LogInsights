using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LogfileMetaAnalyser.Datastore;
using LogfileMetaAnalyser.Helpers;

namespace LogfileMetaAnalyser.Detectors
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

            DateTime finStartpoint = DateTime.Now;

            foreach (var gap in timegaps.Where(g => isRelevantGap(g.logfileNameStart, g.GetGapInSecods())))
                if (!_datastore.GeneralLogData.TimeGaps.Any(t => t.dtTimestampStart.AlmostEqual(gap.dtTimestampStart)))
                {
                    _datastore.GeneralLogData.TimeGaps.Add(gap);
                    logger.Debug($"pushing to ds: generalLogData timegap {gap}");
                }


            //stats
            detectorStats.detectorName = string.Format("{0} <{1}>", this.GetType().Name, this.identifier);
            detectorStats.numberOfDetections = _datastore.GeneralLogData.TimeGaps.Count;
            detectorStats.finalizeDuration = (DateTime.Now - finStartpoint).TotalMilliseconds;
            _datastore.Statistics.DetectorStatistics.Add(detectorStats);
            logger.Debug(detectorStats.ToString());


            //dispose
            timegaps = null;
        }

        private bool isRelevantGap(string filename, long gapSec)
        {
            int gap_threshold = gap_threshold_WhenLogIsOnLevel_Min;

            switch (_datastore.GeneralLogData.LogfileInformation[filename].mostDetailedLogLevel)
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

        
        public void ProcessMessage(TextMessage msg)
        {
            if (!_isEnabled)
                return;

			DateTime procMsgStartpoint = DateTime.Now;

            var rt = ProcessMessageBase(ref msg);
            if (rt != null)
                foreach (var xmsg in rt)
                    ProcessMessage(xmsg);
            if (msg == null)
                return;

			detectorStats.numberOfLinesParsed += msg.numberOfLines;    

            var currentTime = msg.messageTimestamp;
            if (currentTime == DateTime.MinValue)  //skip invalid timestamps
            {
                logger.Trace($"invalid timestamp: {msg.messageText}; skip processing :(");
                return; // :(
            }

            //init? or new logfile? In this case time gaps are expected
            if (CurrentLogfilename != msg.textLocator.fileName)
            {
                CurrentLogfilename = msg.textLocator.fileName;
                CurrentLogfileTime = currentTime;

                return;
            }

            //min time gap?
            if ((msg.messageTimestamp - CurrentLogfileTime).TotalSeconds > gap_threshold_WhenLogIsOnLevel_Min)
                timegaps.Add(new TimeGap()
                {                    
                    dtTimestampStart = CurrentLogfileTime,
                    dtTimestampEnd = msg.messageTimestamp,                    
                    message = msg
                });


            CurrentLogfileTime = currentTime;
			
			detectorStats.parseDuration += (DateTime.Now - procMsgStartpoint).TotalMilliseconds;			
        }

         

    }


}