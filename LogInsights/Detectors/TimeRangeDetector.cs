using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using LogInsights.Datastore;
using LogInsights.Helpers;
using LogInsights.LogReader;


namespace LogInsights.Detectors
{   
    /// <summary>
    /// this detector 
    ///     a) checks each passed logfile for the provided min and max timestamp
    ///     b) counts the messags per LogLevel and determines the highest log level
    /// </summary>


    public class TimeRangeDetector : DetectorBase, ILogDetector
    {        
        private LogLevel _currentHighestLogLevelFound = LogLevel.Undef;
        private Dictionary<string, bool> startDateSet = new Dictionary<string, bool>();
                

        public override string caption =>"Log file time ranges and log level information"; 

        public override string category => "General Log Data";

        public override string identifier => "#TimeRangeDetector"; 


        

        public void InitializeDetector()
        { 
            _currentHighestLogLevelFound = LogLevel.Undef;
            startDateSet = new Dictionary<string, bool>();
			detectorStats.Clear();
        }

        public void FinalizeDetector()
        {
            var generalLogData = _datastore.GetOrAdd<GeneralLogData>();
            var statisticsStore = _datastore.GetOrAdd<StatisticsStore>();

            logger.Debug("entering FinalizeDetector()");

            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();

            //on the very last moment: calc highest loglevel per file; fire the event
            string[] allFiles = generalLogData.LogfileInformation.Keys.ToArray();
            foreach (var d in generalLogData.LogfileInformation)
            {
                var levels = d.Value.numberOfEntriesPerLoglevel.Select(l => l.Key).ToArray();
                d.Value.mostDetailedLogLevel = LogLevelTools.GetHighestLevel(levels);
                d.Value.filenameBestNotation = Helpers.FileHelper.GetBestRelativeFilename(d.Value.filename, allFiles);

                //stats per log file
                d.Value.avgCharsPerBlockmsg = 1.0f * d.Value.charsRead / d.Value.cntBlockMsgs;  // filesize / count blocks
                d.Value.avgCharsPerLine = 1.0f * d.Value.charsRead / d.Value.cntLines; //filesize / count lines
                d.Value.avgLinesPerBlockmsg = 1.0f * d.Value.cntLines / d.Value.cntBlockMsgs;  //count lines / count blocks
            }

            //stats
            sw.Stop();
            detectorStats.numberOfDetections += generalLogData.LogfileInformation.Count;
            detectorStats.numberOfDetections += generalLogData.NumberOfEntriesPerLoglevel.Count;
            detectorStats.numberOfDetections += (generalLogData.mostDetailedLogLevel != LogLevel.Undef) ? 1 : 0;
            detectorStats.numberOfDetections += (generalLogData.LogDataOverallTimeRangeStart.IsNull()) ? 0 : 1;
            detectorStats.numberOfDetections += (generalLogData.LogDataOverallTimeRangeFinish.IsNull()) ? 0 : 1;

            detectorStats.detectorName = string.Format("{0} <{1}>", this.GetType().Name, this.identifier);
            detectorStats.finalizeDuration = sw.ElapsedMilliseconds;
            statisticsStore.DetectorStatistics.Add(detectorStats);
            logger.Debug(detectorStats.ToString());


            //dispose
            startDateSet = null;
        }
        
        public void ProcessMessage(TextMessage msg)
        {
            if (!_isEnabled)
                return;

            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();

            var generalLogData = _datastore.GetOrAdd<GeneralLogData>();

            var rt = ProcessMessageBase(ref msg);
            if (rt != null)
                foreach (var xmsg in rt)
                    ProcessMessage(xmsg);
            if (msg == null)
                return;

			detectorStats.numberOfLinesParsed += msg.numberOfLines;

            //count msgs - msg source
            generalLogData.NumberOflogSources.AddOrIncrease(msg.Logger);

            
            //handle time ranges generally and per log file
            if (startDateSet.Count == 0)
                generalLogData.LogDataOverallTimeRangeStart = msg.TimeStamp;

            if (startDateSet.EnableKey(msg.Locator.Source)) //first occurance of any line of this log file
            {
                generalLogData.LogfileInformation.GetOrAdd(msg.Locator.Source).filename = msg.Locator.Source;
                generalLogData.LogfileInformation[msg.Locator.Source].firstMessage = msg;
                generalLogData.LogfileInformation[msg.Locator.Source].logfileTimerange_Start = msg.TimeStamp;                
                generalLogData.LogfileInformation[msg.Locator.Source].filesize = Helpers.FileHelper.GetFileSizes(new String[] { msg.Locator.Source });
            }

            //stats per log file
            generalLogData.LogfileInformation[msg.Locator.Source].cntBlockMsgs++;
            generalLogData.LogfileInformation[msg.Locator.Source].cntLines += msg.numberOfLines;
            generalLogData.LogfileInformation[msg.Locator.Source].charsRead += msg.FullMessage.Length;


			//count msgs - msg log level
			generalLogData.NumberOfEntriesPerLoglevel.AddOrIncrease(msg.Level);
			generalLogData.LogfileInformation[msg.Locator.Source].numberOfEntriesPerLoglevel
				.AddOrIncrease(msg.Level);

			if (msg.Level != _currentHighestLogLevelFound )
			{
				bool levelChanged = SetHighestLogLevel(msg.Level);

				if ( levelChanged )
					generalLogData.mostDetailedLogLevel = msg.Level;
			}


			//errors and warnings
            if (msg.Level == LogLevel.Warn)
            {
                generalLogData.MessageWarnings.Add(new DatastoreBaseDataPoint()
                {
                    dtTimestamp = msg.TimeStamp,
                    isDataComplete = true,
                    message = msg,
                    metaData = msg.Level.ToString() 
                });
            }

            if (msg.Level == LogLevel.Error || msg.Level == LogLevel.Critical)
            {
                generalLogData.MessageErrors.Add(new DatastoreBaseDataPoint
					{
                    dtTimestamp = msg.TimeStamp,
                    isDataComplete = true,
                    message = msg,
                    metaData = msg.Level.ToString()
                });
            }

            // as we do not know if this is the last log line of this file, we need to store every timestamp as the last one
            sw.Stop();
            generalLogData.LogDataOverallTimeRangeFinish = msg.TimeStamp;
            generalLogData.LogfileInformation.GetOrAdd(msg.Locator.Source).logfileTimerange_Finish = msg.TimeStamp;

            detectorStats.parseDuration += sw.ElapsedMilliseconds;
        }

        private bool SetHighestLogLevel(LogLevel loglevel)
        {            
            if (loglevel.IsGreater(_currentHighestLogLevelFound))
            {
                _currentHighestLogLevelFound = loglevel;
                return true;
            }

            return false;
        }
          

        public TimeRangeDetector()
        { }
    }

     
}
