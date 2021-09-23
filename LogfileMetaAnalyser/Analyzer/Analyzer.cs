using LogfileMetaAnalyser.Composition;
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
using LogfileMetaAnalyser.Detectors;
using LogfileMetaAnalyser.LogReader;
using System.Globalization;
using System.Reflection;


namespace LogfileMetaAnalyser
{
    public class Analyzer
    {
        public event EventHandler<double> ReadProgressChanged;


        private int _AnalyzeDOP;
        private ILogReader m_LogReader;
        public DataStore datastore = new DataStore();
        private Helpers.NLog logger;


        public int AnalyzeDOP
        {
            get {
                return _AnalyzeDOP;
            }
            set {
                _AnalyzeDOP = Math.Max(1, Math.Min(value, Environment.ProcessorCount - 1));
            }
        }


        //Constructor
        public Analyzer()
        {
            logger = new Helpers.NLog("Analyzer");
            logger.Info("Starting analyzer");

            AnalyzeDOP = 2;
        }

        public void Initialize(ILogReader reader)
        {
            m_LogReader = reader;
        }


        //main procedure to analyze a log file
        public async Task AnalyzeStructureAsync()
        {
            //clear old datastore values
            datastore.Clear();

            if (m_LogReader == null)
                return;

            try
            {
                ReadProgressChanged?.Invoke(this, 0.5D); // trigger progress

                logger.Info($"Starting analyzing {m_LogReader.Display ?? "?"}.");

                //List<Detectors.ILogDetector> detectors = new List<Detectors.ILogDetector> {
                //    new Detectors.TimeRangeDetector(),
                //    new Detectors.TimeGapDetector(),
                //    new Detectors.SyncStructureDetector(),
                //    new Detectors.ConnectorsDetector(),
                //    new Detectors.SyncStepDetailsDetector(),
                //    new Detectors.SQLStatementDetector(),
                //    new Detectors.JobServiceJobsDetector(),
                //    new Detectors.SyncJournalDetector()
                //};

                var detectors =
                    new List<ILogDetector>(Composer.BuildRepo<ILogDetector>(Array.Empty<object>(), "*Detector.dll"));


                //Auslesen der Required Parent Detectors und zuweisen der resourcen
                foreach (var childDetector in detectors.Where(d => d.requiredParentDetectors.Length > 0))
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
                await _TextReadingAsync(m_LogReader, detectors).ConfigureAwait(false);


                //detecor finilize; call the finalize in that order, that "child" detectors's finalize is called after the parent's finalize is done
                logger.Info("Starting to perform finalizedDetector");

                List<string> finalizedDetectorIds = new List<string>();
                while (true)
                {
                    var detectorsToFinalize = detectors.Where(d => d.requiredParentDetectors.Length == 0 || //the ones which are parents and not child detectors
                                                                   d.requiredParentDetectors.Any(f => finalizedDetectorIds.Any(kk => kk == f))) //all child detectors for which the parent is already finalized prior
                        .Where(d => finalizedDetectorIds.All(kk => kk != d.identifier)) //exclude already finalized detecrtors
                        .ToArray();
                    if (detectorsToFinalize.Length == 0)
                        break;

                    foreach (var detector in detectorsToFinalize)
                    {
                        detector.FinalizeDetector();
                        finalizedDetectorIds.Add(detector.identifier);
                    }
                }

                logger.Info("Analyzer done!");
            }
            finally
            {
                ReadProgressChanged?.Invoke(this, 1D); // trigger progress
            }
        }

        private async Task _TextReadingAsync(ILogReader logReader, List<Detectors.ILogDetector> detectors)
        {
            if (detectors == null)
                return;

            logger.Info($"Starting reading {logReader.GetType().Name}");

            var reader = await LogContextReader.CreateAsync(logReader, Constants.NumberOfContextMessages, Constants.NumberOfContextMessages).ConfigureAwait(false);
            var enumerator = reader.ReadAsync()
                .Partition(1024)
                .GetAsyncEnumerator();

            var preloader = new DataPreloader<IReadOnlyList<LogEntry>>(async () =>
            {
                if (!await enumerator.MoveNextAsync().ConfigureAwait(false))
                    return null;

                return enumerator.Current;
            });

            IReadOnlyCollection<LogEntry> partition;
            while ((partition = await preloader.GetNextAsync().ConfigureAwait(false)) != null)
            {
                var textMsgs = partition.Select(p => new TextMessage(p)).ToArray();

                Parallel.ForEach(detectors, new ParallelOptions {MaxDegreeOfParallelism = AnalyzeDOP},
                    detector =>
                    {
                        foreach (var entry in textMsgs)
                            detector.ProcessMessage(entry);
                    });
            }
        }
    }

}
