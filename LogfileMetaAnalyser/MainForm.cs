using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using LogfileMetaAnalyser.Controls;
using LogfileMetaAnalyser.Helpers;
using LogfileMetaAnalyser.Datastore;
using LogfileMetaAnalyser.ExceptionHandling;
using LogfileMetaAnalyser.LogReader;

namespace LogfileMetaAnalyser
{
    public partial class MainForm : Form
    {
        private ILogReader _activeReader;

        private void _Load(ILogReader reader)
        {
            if (reader == _activeReader)
                return;

            _activeReader?.Dispose();
            _activeReader = reader;

            _analyzerCore.Initialize(_activeReader);
            // todo toolStripStatusLabel.Text = analyzerCore.filesToAnalyze.Length + " files to analyze";
            RefreshStatusLabel(1);

            StartAnalysis();
        }

        private bool _CheckForClose()
        {
            return _activeReader == null || MessageBox.Show("Do you want to start a new analysis?", "Discard current report?", MessageBoxButtons.YesNo) == DialogResult.Yes;

        }

        private Analyzer _analyzerCore;
        private DataStoreViewer _datastoreViewer;
        private Exporter _logfileFilterExporter;


        public MainForm()
        {             
            InitializeComponent();

            splitContainerRightIn.Panel1.Controls.Add(new LogfileMetaAnalyser.Controls.WelcomeUC());


            //init controls
            treeViewLeft.AllowDrop = true;
            treeViewLeft.DragEnter += DragEnterMethod;
            treeViewLeft.DragDrop += DragDropMethod;

            RefreshStatusLabel(1);


            //initialize analyzer
            _analyzerCore = new Analyzer();
            _logfileFilterExporter = new Exporter(_analyzerCore.datastore);
            _datastoreViewer = new DataStoreViewer(_analyzerCore.datastore, _logfileFilterExporter, this, splitContainerRightIn.Panel1.Controls, splitContainerRightIn.Panel2.Controls);


            _analyzerCore.ReadProgressChanged += new EventHandler<double>((object o, double d) =>
            {
                try
                {
                    GuiHelper.SetGuiSave(statusStrip1, () =>
                    {
                        toolStripProgressBar1.Visible = d < 1D;
                    });
                }
                catch (Exception e)
                {
                    ExceptionHandler.Instance.HandleException(e); 

                }
            });

            _logfileFilterExporter.OnExportProgressChanged += new EventHandler<double>((object o, double d) =>
            {
                try
                {
                    GuiHelper.SetGuiSave(statusStrip1, () =>
                    {
                        toolStripProgressBar1.Visible = d < 1D;
                        //toolStripProgressBar1.Value = d.Int();
                    });
                }
                catch (Exception e)
                {
                    ExceptionHandler.Instance.HandleException(e);
                }
            });

            treeViewLeft.AfterSelect += new TreeViewEventHandler((object o, TreeViewEventArgs args) => {
                try
                {
                    GuiHelper.SetGuiSave(treeViewLeft, async () =>
                    {
                        string key = args.Node.Name;
                        await _datastoreViewer.ExportAsViewContent(key).ConfigureAwait(false);
                    });
                }
                catch (Exception e)
                {
                    ExceptionHandler.Instance.HandleException(e);
                }
            });

            HandleCmdLineParams();
        }

        private void HandleCmdLineParams()
        {
            List<string> _filesOrDirectoryToLoad = new List<string>();

            foreach (string param in Environment.GetCommandLineArgs().ToArray().Skip(1))
            {
                if (!param.StartsWith("/") && !param.StartsWith("-"))
                    _filesOrDirectoryToLoad.Add(param);
            }

            if (_filesOrDirectoryToLoad.Count > 0 && _CheckForClose())
                _Load(new NLogReader(_filesOrDirectoryToLoad.ToArray()));
        }

        public void RefreshStatusLabel(byte phase)
        {
            // TODO
            //string basetip = "";

            //if (_analyzerCore != null && _analyzerCore.filesToAnalyze != null && _analyzerCore.filesToAnalyze.Any())
            //{
            //    if (_analyzerCore.filesToAnalyze.Length == 1)
            //        basetip = "1 file to analyze: " + _analyzerCore.filesToAnalyze[0];
            //    else
            //        basetip = _analyzerCore.filesToAnalyze.Length + " files to analyze";
            //}

            //switch (phase)
            //{
            //    case 1:
            //        if (_analyzerCore == null || _analyzerCore.filesToAnalyze.Length == 0)
            //        {
            //            toolStripStatusLabel.Text = "ready";
            //            toolStripProgressBar1.Visible = false;
            //        }
            //        else
            //            toolStripStatusLabel.Text = basetip;

            //        break;

            //    case 2:
            //        toolStripStatusLabel.Text = basetip + "; analyzing, please wait ...";
            //        break;

            //    case 3:
            //        toolStripStatusLabel.Text = basetip + "; done!";
            //        toolStripProgressBar1.Visible = false;
            //        break;
            //}
        }

        private void DragEnterMethod(object sender, DragEventArgs e)
        {
            try
            {
                e.Effect = DragDropEffects.None;
                if (!e.Data.GetDataPresent(DataFormats.FileDrop))
                    return;

                var files = e.Data.GetData(DataFormats.FileDrop) as string[];
                if (files == null || files.Length < 1)
                    return;

                e.Effect = DragDropEffects.Copy;
            }
            catch (Exception exception)
            {
                ExceptionHandler.Instance.HandleException(exception);
            }
        }

        private void DragDropMethod(object sender, DragEventArgs e)
        {
            try
            {
                var files = e.Data.GetData(DataFormats.FileDrop) as string[];
                if (files == null || files.Length < 1)
                    return;

                if (_CheckForClose())
                    _Load(new NLogReader(files));
            }
            catch (Exception exception)
            {
                ExceptionHandler.Instance.HandleException(exception);
            }
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        //private void loadDirectoryToolStripMenuItem_Click(object sender, EventArgs e)
        //{
        //    folderBrowserDialog1.ShowNewFolderButton = false;
        //    folderBrowserDialog1.RootFolder = Environment.SpecialFolder.Desktop;
        //    if (_activeReader == null) // first open
        //        folderBrowserDialog1.SelectedPath = "C:\\";

        //    if (filesOrDirectoryToLoad.Length == 0 || MessageBox.Show("Do you want to start a new analysis?", "Discard current report?", MessageBoxButtons.YesNo) == DialogResult.Yes)
        //        if (folderBrowserDialog1.ShowDialog() == DialogResult.OK && folderBrowserDialog1.SelectedPath != "")
        //            filesOrDirectoryToLoad = new string[] { folderBrowserDialog1.SelectedPath};
        //}

        //private void loadFilesToolStripMenuItem_Click(object sender, EventArgs e)
        //{
        //    if (filesOrDirectoryToLoad.Length == 0 || MessageBox.Show("Do you want to start a new analysis?", "Discard current report?", MessageBoxButtons.YesNo) == DialogResult.Yes)
        //        if (openFileDialog1.ShowDialog() == DialogResult.OK)
        //            filesOrDirectoryToLoad = openFileDialog1.FileNames;
        //}

        private async void StartAnalysis()
        {
            if (_activeReader == null)
                return;

            RefreshStatusLabel(2);
            await _analyzerCore.AnalyzeStructureAsync().ConfigureAwait(true);

            RefreshStatusLabel(3);
            bool rt = _datastoreViewer.ExportAsNavigationTreeView(ref treeViewLeft);
        }

        private void debugDatastoreToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                TextBoxFrm fm = new TextBoxFrm();
                fm.SetupLabel($"Data Store JSON export");
                fm.SetupData(_datastoreViewer.ExportAsJson());            

                fm.ShowDialog();
            }
            catch (Exception exception)
            {
                ExceptionHandler.Instance.HandleException(exception);
            }
        }

        private async void filterLogfilesToScopeTheImportantStuffToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                await _logfileFilterExporter.FilterAndExport().ConfigureAwait(true);
                RefreshStatusLabel(3);
            }
            catch (Exception exception)
            {
                ExceptionHandler.Instance.HandleException(exception);
            }
        }

        private void loadLogsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (!_CheckForClose())
                    return;

                using (var dlgProvider = new LogReaderForm())
                {
                    DialogResult dr = dlgProvider.ShowDialog(this);

                    if (dr == DialogResult.OK)
                    {
                        dlgProvider.StoreCredentials();
                        // start Load
                        _Load(dlgProvider.ConnectToReader());
                    }
                }
            }
            catch (Exception exception)
            {
                ExceptionHandler.Instance.HandleException(exception);
            }
        }
    }
}
