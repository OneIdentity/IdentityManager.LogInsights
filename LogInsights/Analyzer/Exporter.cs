using System;
using System.Collections.Generic;
using System.Linq; 
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
 
using LogInsights.Helpers;
using LogInsights.Datastore;
using LogInsights.LogReader;


namespace LogInsights
{
    public class Exporter
    {
        private DataStore m_datastore;
        private ExportSettings exportSettings;
        public ILogReader LogReader;

        public event EventHandler<double> OnExportProgressChanged;


        public Exporter(DataStore datastore)
        {
            m_datastore = datastore;            
            exportSettings = new ExportSettings(datastore);
        }

        public async Task FilterAndExport()
        {
            await FilterAndExportBase().ConfigureAwait(false);
        }

        public async Task FilterAndExportFromMessage(LogEntry inputMsg)
        {
            //change exportSettings before GUI display??
            if (inputMsg != null && !string.IsNullOrEmpty(inputMsg.Locator.Source))
            {
                //try to preselect FileName and Folder
                exportSettings.inputOutputOptions.includeFiles.AddIfNotPresent(inputMsg.Locator.Source);
                exportSettings.inputOutputOptions.outputFolder = FileHelper.EnsureFolderisWritableOrReturnDefault(Path.GetDirectoryName(inputMsg.Locator.Source));
                             
                //try to preselect ProjectionActivity
                ProjectionType ptype;
                string uuid = m_datastore.GetOrAdd<ProjectionActivity>().GetUuidByLoggerId(inputMsg.Spid, out ptype);
                if (!string.IsNullOrEmpty(uuid))
                {
                    exportSettings.filterByActivity.isfilterEnabled_ProjectionActivity = true;
                    exportSettings.filterByActivity.isfilterEnabled_ProjectionActivity_Projections = true;

                    if (ptype == ProjectionType.AdHocProvision)
                    {
                        exportSettings.filterByActivity.isfilterEnabled_ProjectionActivity_Projections_AdHoc = true;
                        exportSettings.filterByActivity.filterProjectionActivity_Projections_AdHocLst.AddIfNotPresent(uuid);
                    }
                    else
                    {
                        exportSettings.filterByActivity.isfilterEnabled_ProjectionActivity_Projections_Sync = true;
                        exportSettings.filterByActivity.filterProjectionActivity_Projections_SyncLst.AddIfNotPresent(uuid);
                    }
                }
            }

            //and go!
            await FilterAndExportBase().ConfigureAwait(false);
        }

        private async Task FilterAndExportBase()
        {
            //check general data vailability
            if (!m_datastore.HasData() || LogReader == null)
            {
                MessageBox.Show("No data was yet collected. Please let us analyze at least one logfile first!", Constants.AppDisplay);
                return;
            }

            //show the export setting form
            var frm = new Controls.ExportWithFilterFrm(m_datastore, exportSettings);

            if (frm.ShowDialog() != DialogResult.OK || frm.exportSettings == null)
                return;

            exportSettings = frm.exportSettings;


            //prepare the export
            var files = m_datastore.GetOrAdd<GeneralLogData>().LogfileInformation
                            .Where(f => frm.exportSettings.inputOutputOptions.includeFiles.Count == 0 ||
                                        frm.exportSettings.inputOutputOptions.includeFiles.Contains(f.Value.filename))
                            .OrderBy(f => f.Value.logfileTimerange_Start)
                            .Select(f => f.Key)
                            .ToArray();

            if (files.Length == 0)
            {
                MessageBox.Show("No file or file type to filter was selected!", Constants.AppDisplay);
                return;
            }

            if (exportSettings.inputOutputOptions.mergeFiles && files.Length > 1)
                throw new NotSupportedException("The option to merge the export rows into one single file is not yet supported. Sorry.");


            OnExportProgressChanged?.Invoke(this, 0.5D);


            string exportfilename = FileHelper.GetNewFilename(LogReader.Display,  //TODO
                                                    exportSettings.inputOutputOptions.filenamePostfix, 
                                                    exportSettings.inputOutputOptions.outputFolder);

            if (string.IsNullOrEmpty(Path.GetExtension(exportfilename)))
                exportfilename = $"{exportfilename}.txt";


            var enumerator = LogReader.ReadAsync()
                        .Partition(1024)
                        .GetAsyncEnumerator();

            var preloader = new DataPreloader<IReadOnlyList<LogEntry>>(async () =>
            {
                if (!await enumerator.MoveNextAsync().ConfigureAwait(false))
                    return null;

                return enumerator.Current;
            });


            IReadOnlyCollection<LogEntry> partition;
            bool? writeLn = null;
            using (var writer = new StreamWriter(exportfilename, false, Encoding.UTF8))
                while ((partition = await preloader.GetNextAsync().ConfigureAwait(false)) != null)
                {
                    foreach (var msg in partition)
                        if (exportSettings.IsMessageMatch(msg))
                        {
                            //do we need a line break at the end?
                            if (writeLn == null && !string.IsNullOrEmpty(msg.FullMessage))
                                writeLn = !msg.FullMessage.EndsWith(Environment.NewLine);
                             
                            if (writeLn == false)
                                await writer.WriteAsync(msg.FullMessage).ConfigureAwait(false);
                            else
                                await writer.WriteLineAsync(msg.FullMessage).ConfigureAwait(false);
                        }
                } 

            OnExportProgressChanged?.Invoke(this, 1D);
        }

    }
}
