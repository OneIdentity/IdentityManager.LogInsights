using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using LogfileMetaAnalyser.Datastore;
using LogfileMetaAnalyser.Helpers;
using LogfileMetaAnalyser.LogReader;


namespace LogfileMetaAnalyser.Detectors
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
                

        public override string caption
        {
            get {return "Log file time ranges and log level information"; }
        }

        public override string category
        {
            get {return "General Log Data";}
        }

        public override string identifier
        {
            get { return "#TimeRangeDetector"; }
        }


        

        public void InitializeDetector()
        { 
            _currentHighestLogLevelFound = LogLevel.Undef;
            startDateSet = new Dictionary<string, bool>();
			detectorStats.Clear();
        }

        public void FinalizeDetector()
        {
            logger.Debug("entering FinalizeDetector()");

            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();

            //on the very last moment: calc highest loglevel per file; fire the event
            string[] allFiles = _datastore.generalLogData.logfileInformation.Keys.ToArray();
            foreach (var d in _datastore.generalLogData.logfileInformation)
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
            detectorStats.numberOfDetections += _datastore.generalLogData.logfileInformation.Count;
            detectorStats.numberOfDetections += _datastore.generalLogData.numberOfEntriesPerLoglevel.Count;
            detectorStats.numberOfDetections += (_datastore.generalLogData.mostDetailedLogLevel != LogLevel.Undef) ? 1 : 0;
            detectorStats.numberOfDetections += (_datastore.generalLogData.logDataOverallTimeRange_Start.IsNull()) ? 0 : 1;
            detectorStats.numberOfDetections += (_datastore.generalLogData.logDataOverallTimeRange_Finish.IsNull()) ? 0 : 1;

            detectorStats.detectorName = string.Format("{0} <{1}>", this.GetType().Name, this.identifier);
            detectorStats.finalizeDuration = sw.ElapsedMilliseconds;
            _datastore.statistics.detectorStatistics.Add(detectorStats);
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

            var rt = ProcessMessageBase(ref msg);
            if (rt != null)
                foreach (var xmsg in rt)
                    ProcessMessage(xmsg);
            if (msg == null)
                return;

			detectorStats.numberOfLinesParsed += msg.numberOfLines;

            //count msgs - msg source
            _datastore.generalLogData.numberOflogSources.AddOrIncrease(msg.loggerSource);

            
            //handle time ranges generally and per log file
            if (startDateSet.Count == 0)
                _datastore.generalLogData.logDataOverallTimeRange_Start = msg.messageTimestamp;

            if (startDateSet.EnableKey(msg.textLocator.fileName)) //first occurance of any line of this log file
            {
                _datastore.generalLogData.logfileInformation.GetOrAdd(msg.textLocator.fileName).filename = msg.textLocator.fileName;
                _datastore.generalLogData.logfileInformation[msg.textLocator.fileName].firstMessage = msg;
                _datastore.generalLogData.logfileInformation[msg.textLocator.fileName].logfileTimerange_Start = msg.messageTimestamp;
                _datastore.generalLogData.logfileInformation[msg.textLocator.fileName].logfileType = msg.messageLogfileType;
                _datastore.generalLogData.logfileInformation[msg.textLocator.fileName].filesize = Helpers.FileHelper.GetFileSizes(new String[] { msg.textLocator.fileName });
            }

            //stats per log file
            _datastore.generalLogData.logfileInformation[msg.textLocator.fileName].cntBlockMsgs++;
            _datastore.generalLogData.logfileInformation[msg.textLocator.fileName].cntLines += msg.numberOfLines;
            _datastore.generalLogData.logfileInformation[msg.textLocator.fileName].charsRead += msg.messageText.Length;


			//count msgs - msg log level
			_datastore.generalLogData.numberOfEntriesPerLoglevel.AddOrIncrease(msg.loggerLevel);
			_datastore.generalLogData.logfileInformation[msg.textLocator.fileName].numberOfEntriesPerLoglevel
				.AddOrIncrease(msg.loggerLevel);

			if (msg.loggerLevel != _currentHighestLogLevelFound )
			{
				bool levelChanged = SetHighestLogLevel(msg.loggerLevel);

				if ( levelChanged )
					_datastore.generalLogData.mostDetailedLogLevel = msg.loggerLevel;
			}


			//errors and warnings
            if (msg.loggerLevel == LogLevel.Warn)
            {
                _datastore.generalLogData.messageWarnings.Add(new DatastoreBaseDataPoint()
                {
                    dtTimestamp = msg.messageTimestamp,
                    isDataComplete = true,
                    message = msg,
                    metaData = msg.loggerLevel.ToString() 
                });
            }

            if (msg.loggerLevel == LogLevel.Error || msg.loggerLevel == LogLevel.Critical)
            {
                _datastore.generalLogData.messageErrors.Add(new DatastoreBaseDataPoint
					{
                    dtTimestamp = msg.messageTimestamp,
                    isDataComplete = true,
                    message = msg,
                    metaData = msg.loggerLevel.ToString()
                });
            }

            // as we do not know if this is the last log line of this file, we need to store every timestamp as the last one
            sw.Stop();
            _datastore.generalLogData.logDataOverallTimeRange_Finish = msg.messageTimestamp;
            _datastore.generalLogData.logfileInformation.GetOrAdd(msg.textLocator.fileName).logfileTimerange_Finish = msg.messageTimestamp;

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
