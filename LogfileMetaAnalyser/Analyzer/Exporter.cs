using System;
using System.Collections.Generic;
using System.Linq; 
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
 
using LogfileMetaAnalyser.Helpers;
using LogfileMetaAnalyser.Datastore;
using LogfileMetaAnalyser.LogReader;


namespace LogfileMetaAnalyser
{
    public class Exporter
    {
        private DataStore datastore;
        private ExportSettings exportSettings;

        public event EventHandler<double> OnExportProgressChanged;


        public Exporter(DataStore datastore)
        {
            this.datastore = datastore;
            exportSettings = new ExportSettings(datastore);
        }

        public async Task FilterAndExport()
        {
            await FilterAndExportBase().ConfigureAwait(false);
        }

        public async Task FilterAndExportFromMessage(TextMessage inputMsg)
        {
            //change exportSettings before GUI display??
            if (inputMsg != null && !string.IsNullOrEmpty(inputMsg.textLocator.fileName))
            {
                //try to preselect FileName and Folder
                exportSettings.inputOutputOptions.includeFiles.AddIfNotPresent(inputMsg.textLocator.fileName);
                exportSettings.inputOutputOptions.outputFolder = FileHelper.EnsureFolderisWritableOrReturnDefault(Path.GetDirectoryName(inputMsg.textLocator.fileName));
                             
                //try to preselect ProjectionActivity
                ProjectionType ptype;
                string uuid = datastore.GetOrAdd<ProjectionActivity>().GetUuidByLoggerId(inputMsg.spid, out ptype);
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
            if (!datastore.HasData())
            {
                MessageBox.Show("No data was yet collected. Please let us analyze at least one logfile first!", Constants.AppDisplay);
                return;
            }

            //show the export setting form
            var frm = new Controls.ExportWithFilterFrm(datastore, exportSettings);

            if (frm.ShowDialog() != DialogResult.OK || frm.exportSettings == null)
                return;

            exportSettings = frm.exportSettings;


            //prepare the export
            var files = datastore.GetOrAdd<GeneralLogData>().LogfileInformation
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


            //let's go through each file
            TextMessage msg;
            int curFileNr = 0;
            float percentDone = 0;

            foreach (string inputfilename in files)
            {
                curFileNr++;
                percentDone = (100f * (curFileNr - 1) / files.Length);

                string exportfilename = FileHelper.GetNewFilename(inputfilename, exportSettings.inputOutputOptions.filenamePostfix, exportSettings.inputOutputOptions.outputFolder);

                using (var reader = new NLogReader(new[]{inputfilename}, Encoding.UTF8))
                using (var writer = new StreamWriter(exportfilename, false, Encoding.UTF8))
                {
                    //refire the progress event
                    //if (OnExportProgressChanged != null)
                    //    reader.OnProgressChanged += new EventHandler<ReadLogByBlockEventArgs>((object o, ReadLogByBlockEventArgs args) =>
                    //    {
                    //        OnExportProgressChanged(this, (args.progressPercent / files.Length) + percentDone);
                    //    });

                    //reading
                    await foreach (var entry in reader.ReadAsync().ConfigureAwait(false))
                    {
                        msg = new TextMessage(entry);

                        //checking and writing
                        if (exportSettings.IsMessageMatch(msg))                            
                            await writer.WriteAsync(msg.messageText).ConfigureAwait(false); //do not use WriteLineAsync as the newline is still at the end of each message
                    }
                }

                //refire the progress event - this file is 100% done
                OnExportProgressChanged?.Invoke(this, (100f / files.Length) + percentDone);
            } //each file
        }

    }
}
