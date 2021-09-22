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
                GuiHelper.SetGuiSave(statusStrip1, () =>
                {
                    toolStripProgressBar1.Visible = d < 1D;
               //     toolStripProgressBar1.Value = d.Int();
                });
            });

            _logfileFilterExporter.OnExportProgressChanged += new EventHandler<double>((object o, double d) =>
            {
                GuiHelper.SetGuiSave(statusStrip1, () =>
                {
                    toolStripProgressBar1.Visible = d < 1D;
                    //toolStripProgressBar1.Value = d.Int();
                });
            });

            treeViewLeft.AfterSelect += new TreeViewEventHandler((object o, TreeViewEventArgs args) => {
                GuiHelper.SetGuiSave(treeViewLeft, async () =>
                {
                    string key = args.Node.Name;
                    await _datastoreViewer.ExportAsViewContent(key).ConfigureAwait(false);
                });
            });

            HandleCmdLineParams();



            //Debug

            var c1 = Helpers.SqlHelper.SplitSqlValues("'Eins','Zwei'");
            var c2 = Helpers.SqlHelper.SplitSqlValues("'Eins',   'Zwei'");
            var c3 = Helpers.SqlHelper.SplitSqlValues("'Eins''ses','Zwei'");
            var c4 = Helpers.SqlHelper.SplitSqlValues("N'Eins','Zwei'");
            var c5 = Helpers.SqlHelper.SplitSqlValues("'Eins',null");
            var cq = Helpers.SqlHelper.SplitSqlValues("null,null");
            var c6 = Helpers.SqlHelper.SplitSqlValues("'Eins',  null  ");
            var c7 = Helpers.SqlHelper.SplitSqlValues("1, 2.77, null, 'hey'");


            string cc = "CreationTime, ProjectionConfigDisplay, ProjectionContext, ProjectionStartInfoDisplay, SystemVariableSetDisplay, UID_DPRJournal, UID_DPRProjectionConfig, UID_DPRProjectionStartInfo, UID_DPRSystemVariableSet";
            string vv = "'2018-11-21 16:33:40.570', N'Copy of Initial Synchronization', N'Full', N'Group (m_aut_fk@domino.schindler.com)', N'default variable set', 'cbd6bf17-9e89-423b-8d23-97333ee48d48', 'CCC-44390AEB9787B54496DB9ABC467DFDF7', 'CCC-A4A71169FBF5154FAA3850DFDB726FCC', 'CCC-CB4E87D36F5DC4409773BC5288C93A83'";

            var xx = Helpers.SqlHelper.SplitSqlValues(vv);
            var res = Helpers.SqlHelper.GetValuePairsFromInsertCmd(cc, vv);


            string updateList1 = "A='A,B,C', B=N'x=ü', C=99, \"Z\"='z'";
            var ures1 = Helpers.SqlHelper.GetValuePairsFromUpdateCmd(updateList1);


            Dictionary<string, int> testdict = new Dictionary<string, int>(StringComparer.InvariantCultureIgnoreCase);

            testdict.Add("A", 1);           

            int a1 = testdict["A"];
            int a2 = testdict["a"];

            string ll = Helpers.StringHelper.TranslateLds("#LDS#{0}. Updating revision information\nProcessing steps: {1}\nExecution time: {2}{3}|2|10|0,52|s");

            /*
         TimeRangeMatchCandidate Sync1 = new TimeRangeMatchCandidate("Sync1", new DateTime(2020, 01, 31, 15, 30, 00), new DateTime(2020, 01, 31, 17, 30, 00), "all Projection 1 - AD");  //umfasst conn
         TimeRangeMatchCandidate Sync2 = new TimeRangeMatchCandidate("Sync2", new DateTime(2020, 01, 31, 16, 10, 00), new DateTime(2020, 01, 31, 19, 00, 00), "Projection 2 - EBS");//umfasst conn NICHT
         TimeRangeMatchCandidate Sync3 = new TimeRangeMatchCandidate("Sync3", new DateTime(2020, 01, 30, 16, 10, 00), new DateTime(2020, 01, 31, 22, 00, 00), "Projection Pion 2 - ADS"); //umfasst conn, ist aber vom Namen zu weit weg

         List<TimeRangeMatchCandidate> projectionCanditateMatrix = (new TimeRangeMatchCandidate[] { Sync1, Sync2, Sync3 }).ToList();

         Helpers.PeriodsMatchCandidate systemConnectionTotest = new PeriodsMatchCandidate((new DateTime[] { new DateTime(2020, 01, 31, 16, 00, 00) }).ToList(),
                                                                                          (new DateTime[] { new DateTime(2020, 01, 31, 17, 00, 00) }).ToList(),
                                                                                                  "Project 1");

         var allCandidateMatches = Helpers.Closeness<int>.GetKeyOfBestMatch(systemConnectionTotest, projectionCanditateMatrix);




         System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
         sw.Start();
         int c = 0;
         using (var reader = new ReadLogByBlock(@"c:\Ralph_Local\Cases\temp\TestStdioProcessor.txt", 21))
         {
             foreach (var msg in reader.GetMessages())
             {
                 c++;
             }
         }

         sw.Stop();
         var timedura = sw.ElapsedMilliseconds / 1000f;


         int br = 0;
         */




            /*
           LogfileMetaAnalyser.Controls.MultiListViewUC muc = new LogfileMetaAnalyser.Controls.MultiListViewUC();
           muc.SetupLayout(2);
           muc.SetupSubLevel(1, 1);

           muc.listOflistviewUC[0].SetupCaption("Control 1");
           muc.listOflistviewUC[1].SetupCaption("Control 2");

           muc.listOflistviewUC[0].SetupHeaders(new string[] { "Head1", "head2" });
           muc.listOflistviewUC[1].SetupHeaders(new string[] { "meine Spaltenbez 1", "Values" });

           for (int i=0; i<= 8; i++)
               muc.listOflistviewUC[0].AddItem("test"+i.ToString(), new string[] { "X1." + i.ToString(), "Y1" + i.ToString() });

           for (int i = 0; i <= 4; i++)
               muc.listOflistviewUC[1].AddItem("test" + i.ToString(), new string[] { " eqeqwe X1." + i.ToString(), "e2e 2e2e 2e 2e2 edkndqwhd qhdquhd Y1" + i.ToString() });

           splitContainerRightIn.Panel1.Controls.Add(muc);

           return;
         */

            /*
            LogfileMetaAnalyser.Controls.ContextLinesUC uc = new LogfileMetaAnalyser.Controls.ContextLinesUC();
            List<TextMessage> before1 = new List<TextMessage>();
            List<TextMessage> after1 = new List<TextMessage>();
            List<TextMessage> before2 = new List<TextMessage>();
            List<TextMessage> after2 = new List<TextMessage>();

            int start1 = 313;
            for (int i = start1; i < start1+6; i++)
                before1.Add(new TextMessage("FN.txt", i, 5000 + i, "msg1 Text before ... " + i.ToString()+Environment.NewLine));
            for (int i = before1.Last().textLocator.fileLinePosition.Int()+2+2; i < before1.Last().textLocator.fileLinePosition.Int() + 2+2+4; i++)
                after1.Add(new TextMessage("FN.txt", i, 5000 + i, "msg1 Text after ... " + i.ToString() + Environment.NewLine));

            int start2 = after1.Last().textLocator.fileLinePosition.Int() -1;
            for (int i = start2; i < start2+3; i++)
                before2.Add(new TextMessage("FN.txt", i, 5000 + i, "msg2 Text before ... TextMessage 2 - " + i.ToString() + Environment.NewLine));
            for (int i = before2.Last().textLocator.fileLinePosition.Int() +2; i < before2.Last().textLocator.fileLinePosition.Int() + 2+3; i++)
                after2.Add(new TextMessage("FN.txt", i, 5000 + i, "msg2 Text after ... TextMessage 2 - " + i.ToString() + Environment.NewLine));

            TextMessage tmA = new TextMessage("FN.txt", before1.Last().textLocator.fileLinePosition.Int() +1, 22222, "msg1 - main Das ist der eigentliche Text<break>\r\nUnd der geht sogar noch weiter<break>\r\nobwoooohl errrrerr gaaaaannnnzzzz lang sein könnte der Test Text und sooo ;D<break>" + Environment.NewLine);
            TextMessage tmB = new TextMessage("FN.txt", before2.Last().textLocator.fileLinePosition.Int() +1, 22223, "msg 2 - main - Message 2 bla bla <bre>" + Environment.NewLine);


            tmA.contextMsgBefore = before1.ToArray();
            tmA.contextMsgAfter = after1.ToArray();
            tmB.contextMsgBefore = before2.ToArray();
            tmB.contextMsgAfter = after2.ToArray();


            uc.SetData(tmA, tmB, true);
            splitContainerRightIn.Panel1.Controls.Add(uc);

            return;
            */
            /*

            Datastore.JobserviceActivity js = new Datastore.JobserviceActivity();

            js.jobserviceJobs.Add(new Datastore.JobserviceJob());
            js.jobserviceJobs[0].jobserviceJobattempts.Add(new Datastore.JobserviceJobattempt());
            js.jobserviceJobs[0].jobserviceJobattempts.Add(new Datastore.JobserviceJobattempt());
            js.jobserviceJobs[0].jobserviceJobattempts.Add(new Datastore.JobserviceJobattempt());

            js.jobserviceJobs.Add(new Datastore.JobserviceJob());
            js.jobserviceJobs[1].jobserviceJobattempts.Add(new Datastore.JobserviceJobattempt());

            js.jobserviceJobs.Add(new Datastore.JobserviceJob());
            js.jobserviceJobs[2].jobserviceJobattempts.Add(new Datastore.JobserviceJobattempt());
            js.jobserviceJobs[2].jobserviceJobattempts.Add(new Datastore.JobserviceJobattempt());


            var data = js.jobserviceJobs.SelectMany(t => t.jobserviceJobattempts);
            //return;
            */

            //analyzerCore.ShowExportDialog();
            //return;           
            /*
            string testfile1 = @"C:\folder\meineLogs\stdio.log";
            string testfile2 = @"C:\folder\oben.log";
            string testfile3 = @"C:\folder\meineLogs\archive\untenstdio.log";

            string[] allfiles = new string[] { testfile1, testfile2, testfile3 };

            var res1 = Helpers.FileHelper.GetFileFolderparts(testfile2);

            var res_t1 = Helpers.FileHelper.GetBestRelativeFilename(testfile1, allfiles);
            var res_t2 = Helpers.FileHelper.GetBestRelativeFilename(testfile2, allfiles);
            var res_t3 = Helpers.FileHelper.GetBestRelativeFilename(testfile3, allfiles);

            return;
            */



            //filesOrDirectoryToLoad = new string[] { @"..\..\..\Testdata\20190506_StdioProcessor_80_InfoOnly_pid4776.log" };

            //filesOrDirectoryToLoad = new string[] { @"..\..\..\Testdata\20190506_StdioProcessor_80_Trace_pid4776.log" };  //parse dura: 2.6 - 4.4

            //filesOrDirectoryToLoad = new string[] { @"..\..\..\Testdata\ADSSync_Errors2_FullInitialWorkflow.StdioProcessor.log" };
            //filesOrDirectoryToLoad = new string[] { @"..\..\..\Testdata\XError.txt", "XError.log" };
            //filesOrDirectoryToLoad = new string[] { @"..\..\..\Testdata\04-02_JobService (35).log" };//{ @"..\..\..\Testdata\ADSSync_Errors.txt" }; //, @"..\..\..\Testdata\JobService.log_2018-10-16-00-00-00" };


            /*
             using (var reader = new ReadLogByBlock(filesOrDirectoryToLoad[0]))
             {
                 System.Diagnostics.Stopwatch sww = new System.Diagnostics.Stopwatch();
                 sww.Start();
                 long A1 = 0;  //		A1	793513203	long
                 long A2 = 0;  //		A2	856855145	long
                 long Cnt = 0;  //39837
                 long MaxPos = 0;  //42863
                 int lenSpid = -1;

                 foreach (var msg in reader.GetMessages())
                 {
                     A1 += msgtextLocator.messageNumber;
                     A2 += msg.textLocator.fileLinePosition;
                     Cnt = msgtextLocator.messageNumber;
                     MaxPos = msg.textLocator.fileLinePosition;
                     //lenSpid = Math.Max(lenSpid, msg.spid.Length);
                 }
                 sww.Stop();
                 var ela = sww.ElapsedMilliseconds;  //3179 - 3212  ==> 950
             }

             /*
             LogfileMetaAnalyser.Controls.ContextLinesUC uc = new LogfileMetaAnalyser.Controls.ContextLinesUC();
             List<TextMessage> before = new List<TextMessage>();
             List<TextMessage> after = new List<TextMessage>();
             for (int i = 0; i < 30; i++)
                 before.Add(new TextMessage() {messageText = "Text before ... "+i.ToString(), textLocator.fileLinePosition = 313 + i });
             for (int i = 0; i < 33; i++)
                 after.Add(new TextMessage() { messageText = "Text after ... " + i.ToString(), textLocator.fileLinePosition = 333 + i });

             TextMessage tm = new TextMessage();
             tm.messageText = "Das ist der eigentliche Text<break>\r\nUnd der geht sogar noch weiter<break>\r\nobwoooohl errrrerr gaaaaannnnzzzz lang sein könnte der Test Text und sooo ;D<break>";
             tm.textLocator.fileName = @"p:\Projects\_nn\xMyDevelopment\LogfileMetaAnalyser\TestData\ADSAccountUpdate.StdioProcessor__Contf2.log";
             tm.textLocator.fileLinePosition = 313 + before.Count;
             tm.contextMsgBefore = before.ToArray();
             //tm.contextMsgAfter = after.ToArray();

             uc.SetData(tm, true);
             splitContainerRightIn.Panel1.Controls.Add(uc);
             */
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
            e.Effect = DragDropEffects.None;
            if (!e.Data.GetDataPresent(DataFormats.FileDrop))
                return;

            var files = e.Data.GetData(DataFormats.FileDrop) as string[];
            if (files == null || files.Length < 1)
                return;

            e.Effect = DragDropEffects.Copy;             
        }

        private void DragDropMethod(object sender, DragEventArgs e)
        {
            var files = e.Data.GetData(DataFormats.FileDrop) as string[];
            if (files == null || files.Length < 1)
                return;

            if (_CheckForClose())
                _Load(new NLogReader(files));
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
            Controls.TextBoxFrm fm = new Controls.TextBoxFrm();
            fm.SetupLabel($"Data Store JSON export");
            fm.SetupData(_datastoreViewer.ExportAsJson());            

            fm.ShowDialog();
        }

        private async void filterLogfilesToScopeTheImportantStuffToolStripMenuItem_Click(object sender, EventArgs e)
        {
            await _logfileFilterExporter.FilterAndExport().ConfigureAwait(true);
            RefreshStatusLabel(3);
        }

        private void loadLogsToolStripMenuItem_Click(object sender, EventArgs e)
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
    }
}
