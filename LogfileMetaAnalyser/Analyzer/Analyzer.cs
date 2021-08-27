using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Forms;

using LogfileMetaAnalyser.Helpers;
using LogfileMetaAnalyser.Datastore;
 

namespace LogfileMetaAnalyser
{
    public class Analyzer
    {
        public event EventHandler<double> OnReadProgressChanged;


        private int _AnalyzeDOP;
        public int AnalyzeDOP
        {
            get { return _AnalyzeDOP; }
            set { _AnalyzeDOP = Math.Max(1, Math.Min(value, Environment.ProcessorCount - 1)); }
        }
        private Helpers.NLog logger;

        public DatastoreStructure datastore = new DatastoreStructure(); 
        public string[] filesToAnalyze;

        
        //Constructor
        public Analyzer()
        {
            logger = new Helpers.NLog("Analyzer");
            logger.Info("Starting analyzer");

            AnalyzeDOP = 2; 
        }

        public void InitializeFiles(string[] filenameOrFolder)
        {
            if (filenameOrFolder == null || filenameOrFolder.Length == 0)
                return;

            var inputFileFolderTupels = FileHelper.GetFileAndDirectory(filenameOrFolder);
            string[] unsupportedFiles = new string[] { };

            foreach (var srcMode in new SearchOption [] { SearchOption.TopDirectoryOnly, SearchOption.AllDirectories})
            {
                var filesToGrab = inputFileFolderTupels
                                    .SelectMany(t => t.filenames
                                                        .SelectMany(f => Helpers.FileHelper.DirectoryGetFilesSave(t.directoryname, f, srcMode)))
                                    .ToArray();

                filesToAnalyze = FileHelper.OrderByContentTimestamp(filesToGrab, out unsupportedFiles);

                if (filesToAnalyze.Length == 0)
                {
                    switch (srcMode)
                    {
                        case SearchOption.TopDirectoryOnly:
                            var ret = MessageBox.Show($"No files found to analyze:\n {filenameOrFolder[0]}\n\nWould you like to include all sub folders into the file search?", "No files", MessageBoxButtons.YesNoCancel);

                            if (ret != DialogResult.Yes)
                                return;

                            break;

                        case SearchOption.AllDirectories:
                            MessageBox.Show($"No files found to analyze:\n {filenameOrFolder[0]} (incl. sub folders)");
                            return;
                    }
                }
                else
                    break;
            }

            if (unsupportedFiles.Length > 0)
            {
                string msg = string.Join(Environment.NewLine, unsupportedFiles.Take(10).ToArray());
                logger.Warning($"{unsupportedFiles.Length} unsupported log file(s) detected: {msg}");

                MessageBox.Show(msg, $"{unsupportedFiles.Length} unsupported log file(s) detected",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        //main procedure to analyze a log file
        public async Task AnalyzeStructureAsync()
        {
            //clear old datastore values
            datastore.Clear();

            logger.Info($"Starting analyzing {filesToAnalyze.Length} file(s): {string.Join(";", filesToAnalyze)}");
            if (filesToAnalyze.Length == 0)
                return;

            
            List<Detectors.ILogDetector> detectors = new List<Detectors.ILogDetector>();

            detectors.Add(new Detectors.TimeRangeDetector());
            detectors.Add(new Detectors.TimeGapDetector());
            detectors.Add(new Detectors.SyncStructureDetector());
            detectors.Add(new Detectors.ConnectorsDetector());
            detectors.Add(new Detectors.SyncStepDetailsDetector());
            detectors.Add(new Detectors.SQLStatementDetector());
            detectors.Add(new Detectors.JobServiceJobsDetector());
            detectors.Add(new Detectors.SyncJournalDetector());


            //Auslesen der Required Parent Detectors und zuweisen der resourcen
            foreach (var childDetector in detectors.Where(d => d.requiredParentDetectors.Any()))
            {
                List<Detectors.ILogDetector> listOfParentDetectors = new List<Detectors.ILogDetector>();
                foreach (var parentId in childDetector.requiredParentDetectors)
                {
                    var parentDetector = detectors.FirstOrDefault(d => d.identifier == parentId);
                    if (parentDetector != null)
                        listOfParentDetectors.Add(parentDetector);
                }
                childDetector.parentDetectors = listOfParentDetectors.ToArray();
            }


            //detector inits
            foreach (var detector in detectors)
            {
                detector.datastore = datastore;
                detector.InitializeDetector();
                detector.isEnabled = true;
            }


            //text reading
            logger.Info("Starting reading the text");
            GlobalStopWatch.StartWatch("TextReading");
            await TextReading(filesToAnalyze, detectors).ConfigureAwait(false);
            GlobalStopWatch.StopWatch("TextReading");


            //detecor finilize; call the finalize in that order, that "child" detectors's finalize is called after the parent's finalize is done
            logger.Info("Starting to perform finalizedDetector");
            GlobalStopWatch.StartWatch("finalizedDetector");
            List<string> finalizedDetectorIds = new List<string>();
            while (1 == 1)
            {
                var detectorsToFinalize = detectors.Where(d => d.requiredParentDetectors.Length == 0 ||  //the ones which are parents and not child detectors
                                                               d.requiredParentDetectors.Any(f => finalizedDetectorIds.Any(kk => kk == f)))  //all child detectors for which the parent is already finalized prior
                                                    .Where(d => !finalizedDetectorIds.Any(kk => kk == d.identifier))  //exclude already finalized detecrtors
                                                    .ToArray();
                if (detectorsToFinalize.Length == 0)
                    break;

                foreach (var detector in detectorsToFinalize)
                {
                    detector.FinalizeDetector();
                    finalizedDetectorIds.Add(detector.identifier);
                }
            }
            GlobalStopWatch.StopWatch("finalizedDetector");

#if DEBUG
            var stopwatchresults = GlobalStopWatch.GetResult();
#endif
            logger.Info("Analyzer done!");
        }

        private async Task TextReading(string[] filesToRead, List<Detectors.ILogDetector> detectors)
        {
            if (detectors == null)
                return;

            var parseStatisticPerTextfile = new List<ParseStatistic>();
             
            TextMessage msg;

            int curFileNr = 0;
            foreach (string filename in filesToRead)
            {
                logger.Info($"Starting reading file {filename}");

                curFileNr++;
                float percentDone = (100f * (curFileNr - 1) / filesToRead.Length);

                ParseStatistic parseStatistic = new ParseStatistic() { filename = filename, filesizeKb = (FileHelper.GetFileSizes(new string[] { filename }) / 1024f).Int() };

                Stopwatch sw_reading = new Stopwatch();
                Stopwatch sw_overall = new Stopwatch();
                sw_overall.Start();
                
                using (var reader = new ReadLogByBlock(filename, Constants.messageInsignificantStopTermRegexString, Constants.NumberOfContextMessages))
                {
                    //refire the progress event
                    if (OnReadProgressChanged != null)
                        reader.OnProgressChanged += new EventHandler<ReadLogByBlockEventArgs>((object o, ReadLogByBlockEventArgs args) =>
                        {
                            OnReadProgressChanged(this, (args.progressPercent / filesToRead.Length) + percentDone);
                        });
                    
                    //read the messages from this log                    
                    while (reader.HasMoreMessages)
                    {
                        sw_reading.Start();
                        GlobalStopWatch.StartWatch("TextReading.GetNextMessageAsync()");
                        msg = await reader.GetNextMessageAsync().ConfigureAwait(false);
                        sw_reading.Stop();
                        GlobalStopWatch.StopWatch("TextReading.GetNextMessageAsync()");

                        if (msg == null)
                            break;


                        //pass the message to all detectors                        
                        //skip invalid messages that would confuse the detectors
                        GlobalStopWatch.StartWatch("TextReading.ProcessMessage()");
                        if (msg.numberOfLines > 0 && msg.textLocator.fileLinePosition > 0)
                            Parallel.ForEach(detectors, new ParallelOptions() { MaxDegreeOfParallelism = AnalyzeDOP }, (detector) =>
                            {
                                detector.ProcessMessage(msg);
                            });
                        GlobalStopWatch.StopWatch("TextReading.ProcessMessage()");
                    }

                    //refire the progress event - this file is 100% done
                    OnReadProgressChanged?.Invoke(this, (100f / filesToRead.Length) + percentDone);
                }

                sw_reading.Stop();
                sw_overall.Stop();

                parseStatistic.readAndParseFileDuration = sw_reading.ElapsedMilliseconds;
                parseStatistic.overallDuration = sw_overall.ElapsedMilliseconds;

                parseStatisticPerTextfile.Add(parseStatistic);
            }

            datastore.statistics.parseStatistic.AddRange(parseStatisticPerTextfile);
        } 
        
    }
}
