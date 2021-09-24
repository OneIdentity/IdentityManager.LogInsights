using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using LogInsights.Controls;
using LogInsights.Helpers;
using LogInsights.Datastore;
using LogInsights.ExceptionHandling;
using LogInsights.LogReader;

namespace LogInsights
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

            _logfileFilterExporter.LogReader = _activeReader; 

            RefreshStatusLabel(1);

            StartAnalysis();
        }

        private bool _CheckForClose()
        {
            return _activeReader == null || MessageBox.Show(this, "Do you want to start a new analysis?", "Discard current report?", MessageBoxButtons.YesNo) == DialogResult.Yes;

        }

        private Analyzer _analyzerCore;
        private DataStoreViewer _datastoreViewer;
        private Exporter _logfileFilterExporter;


        public MainForm()
        {             
            InitializeComponent();

            var cWelcome = new LogInsights.Controls.WelcomeUC();

            cWelcome.StartAnalysis += loadLogsToolStripMenuItem_Click;

            splitContainerRightIn.Panel1.Controls.Add(cWelcome);


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

        private void CWelcome_StartAnalysis(object sender, EventArgs e)
        {
            
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
            string basetip = "";

            if (_activeReader != null)
                basetip = $"Analyzing logs with ({_activeReader.Display})...";

            switch (phase)
            {
                case 1:
                    if (_analyzerCore == null)
                    {
                        toolStripStatusLabel.Text = "ready";
                        toolStripProgressBar1.Visible = false;
                    }
                    else
                        toolStripStatusLabel.Text = basetip;

                    break;

                case 2:
                    toolStripStatusLabel.Text = basetip + " please wait...";
                    break;

                case 3:
                    toolStripStatusLabel.Text = basetip + " done!";
                    toolStripProgressBar1.Visible = false;
                    break;
            }
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
            try
            {

                Close();
            }
            catch (Exception ex)
            {
                ExceptionHandler.Instance.HandleException(ex);
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code", "CAC002:ConfigureAwaitChecker", Justification = "<Pending>")]
        private async void StartAnalysis()
        {
            if (_activeReader == null)
                return;

            RefreshStatusLabel(2);
            await _analyzerCore.AnalyzeStructureAsync().ConfigureAwait(true);

            RefreshStatusLabel(3);
            _datastoreViewer.ExportAsNavigationTreeView(treeViewLeft);
        }

        private void debugDatastoreToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                using (var fm = new TextBoxFrm())
                {
                    fm.SetupLabel("Data Store JSON export");
                    fm.SetupData(_datastoreViewer.ExportAsJson());

                    fm.ShowDialog(this);
                }
            }
            catch (Exception exception)
            {
                ExceptionHandler.Instance.HandleException(exception);
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code", "CAC002:ConfigureAwaitChecker", Justification = "<Pending>")]
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
