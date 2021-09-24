using LogInsights.ExceptionHandling;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using LogInsights.LogReader;

namespace LogInsights.Controls
{
    public partial class LogFileUC : LogReaderControl
    {
        public LogFileUC()
        {
            InitializeComponent();

            tsbFiles.Image = imageList.Images[0];
            tsbFolder.Image = imageList.Images[1];
            tsbDelete.Image = imageList.Images[2];
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

                CheckValid();
            }
            catch(Exception ex)
            {
                ExceptionHandler.Instance.HandleException(ex);
            }
        }

        private void tsbDelete_Click(object sender, EventArgs e)
        {
            try
            {
                // remove all selected items
                foreach (ListViewItem lvi in lvLogFiles.SelectedItems.OfType<ListViewItem>().ToArray())
                {
                    lvLogFiles.Items.Remove(lvi);
                }

                CheckValid();
            }
            catch (Exception ex)
            {
                ExceptionHandler.Instance.HandleException(ex);
            }
        }

        private void _AddFile(string fName)
        {
            var lvItem = lvLogFiles.Items.Add(fName, 0);

            var fInfo = new FileInfo(fName);

            lvItem.SubItems.Add(fInfo.Length.ToString());

            lvItem.SubItems.Add(fInfo.CreationTime.ToString());
            lvItem.SubItems.Add(fInfo.LastWriteTime.ToString());
        }

        private void _AddPath(string fName)
        {
            var lvItem = lvLogFiles.Items.Add(fName, 1);

            var fInfo = new FileInfo(fName);
        }


        protected override bool OnCheckValid()
        {
            bool bValid = base.OnCheckValid();

            bValid &= lvLogFiles.Items.Count>0;

            return bValid;
        }

        private void lvLogFiles_SelectedIndexChanged(object sender, EventArgs e)
        {
            tsbDelete.Enabled = lvLogFiles.SelectedIndices.Count > 0;
        }

        private void tsbFolder_Click(object sender, EventArgs e)
        {
            try
            {
                DialogResult dr = dlgFolder.ShowDialog(this);

                if (dr == DialogResult.OK)
                {
                    _AddPath(dlgFolder.SelectedPath);
                }

                CheckValid();
            }
            catch (Exception ex)
            {
                ExceptionHandler.Instance.HandleException(ex);   
            }
        }
    }
}
