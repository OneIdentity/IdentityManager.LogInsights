using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using LogfileMetaAnalyser.Helpers;
using LogfileMetaAnalyser.LogReader;

namespace LogfileMetaAnalyser.Controls
{
    public partial class LogFileUC : LogReaderControl
    {
        private string[] _filesToAnalyze;

        public LogFileUC()
        {
            InitializeComponent();
        }

        public override ILogReader ConnectToReader()
        {
            return new NLogReader(ConnectionString);
        }

        protected override string GetConnectionString()
        {
            DbConnectionStringBuilder csb = new DbConnectionStringBuilder();

            csb.Add("FileNames", _GetFiles());

            return csb.ConnectionString;
        }

        private string _GetFiles()
        {
            List<string> lFiles = new List<string>();

            foreach (ListViewItem lvItem in lvLogFiles.Items)
            {
                lFiles.Add(lvItem.Text);
            }

            return string.Join("|", lFiles);
        }

        private void tsbAdd_Click(object sender, EventArgs e)
        {
            try
            {
                DialogResult dr = dlgOpen.ShowDialog(this);

                if (dr == DialogResult.OK)
                {
                    foreach (var fName in dlgOpen.FileNames)
                    {
                        _AddFile(fName);
                    }
                }
            }
            catch (Exception ex)
            {
                
            }
        }

        private void _AddFile(string fName)
        {
            var lvItem = lvLogFiles.Items.Add(fName, -1);

            var fInfo = new FileInfo(fName);

            lvItem.SubItems.Add(fInfo.Length.ToString());
        }


        public void InitializeFiles(string[] filenameOrFolder)
        {
            if (filenameOrFolder == null || filenameOrFolder.Length == 0)
                return;

            var inputFileFolderTupels = FileHelper.GetFileAndDirectory(filenameOrFolder);
            string[] unsupportedFiles = new string[] { };

            foreach (var srcMode in new SearchOption[] { SearchOption.TopDirectoryOnly, SearchOption.AllDirectories })
            {
                var filesToGrab = inputFileFolderTupels
                                    .SelectMany(t => t.filenames
                                                        .SelectMany(f => Helpers.FileHelper.DirectoryGetFilesSave(t.directoryname, f, srcMode)))
                                    .ToArray();

                _filesToAnalyze = FileHelper.OrderByContentTimestamp(filesToGrab, out unsupportedFiles);

                if (_filesToAnalyze.Length == 0)
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
                
                //logger.Warning($"{unsupportedFiles.Length} unsupported log file(s) detected: {msg}");

                MessageBox.Show(msg, $"{unsupportedFiles.Length} unsupported log file(s) detected",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
    }
}
