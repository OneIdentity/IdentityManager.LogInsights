using LogfileMetaAnalyser.ExceptionHandling;
using System;
using System.Linq;
using System.Windows.Forms;

using LogfileMetaAnalyser.Helpers;

namespace LogfileMetaAnalyser.Controls
{
    public partial class ListViewUC : UserControl
    {
        public event ListViewItemSelectionChangedEventHandler ItemClicked;
        public event EventHandler DataChanged;
        private bool isSuspended;
        public int CountDataLines;

        private string _NoDataMessage = " < No data to display > ";
        public string NoDataMessage
        {
            get => _NoDataMessage;
            set { _NoDataMessage = value; SwitchNoDataMessage(true, true); }
        }

        private bool isNoDataMessageDisplayed = false;

        public ListViewUC()
        {
            InitializeComponent();

            SetupCaption(null);
            Dock = DockStyle.Fill;
            CountDataLines = 0;
            Visible = false;

            listView1.Enabled = false;

            listView1.ItemSelectionChanged += new ListViewItemSelectionChangedEventHandler((object o, ListViewItemSelectionChangedEventArgs args) =>
               {
                   try
                   {
                       if (((ListView)o).SelectedIndices.Count == 0)
                           return;

                       ItemClicked?.Invoke(o, args);
                   }
                   catch (Exception e)
                   {
                       ExceptionHandler.Instance.HandleException(e);
                   }
               }
            );
        }

        public void SetupCaption(string s)
        {
            Suspend();

            if (!string.IsNullOrEmpty(s))
                Visible = true;

            tableLayoutPanel1.RowStyles[0].Height = string.IsNullOrEmpty(s) ? 0 : 23;
            //label1.Enabled = !string.IsNullOrEmpty(s);
            label1.Visible = !string.IsNullOrEmpty(s);
            label1.Text = s;
        }

        public void SetupGroups(string[] groups)
        {
            listView1.Groups.AddRange(groups.Select(g => new ListViewGroup(g, g)).ToArray());
            listView1.ShowGroups = listView1.Groups.Count > 0;
        }

        public void SetupHeaders(string[] headers)
        {
            Suspend();

            Visible = true;

            listView1.Columns.AddRange(headers.Select(h => new System.Windows.Forms.ColumnHeader() {Text = h }).ToArray());

            SwitchNoDataMessage(true);
        }

        private void SwitchNoDataMessage(bool show, bool messageChanged = false)
        {
            if (listView1 == null || listView1.Columns == null  || listView1.Columns.Count == 0)
                return;

            if (show || messageChanged)
            {
                if (listView1.Items?.Count != 0)
                    listView1.Items.Clear();

                listView1.Items.Add(new ListViewItem(NoDataMessage));
                isNoDataMessageDisplayed = true;

                return;
            }

            if (!show && isNoDataMessageDisplayed)
            {
                listView1.Items.Clear();
                isNoDataMessageDisplayed = false;
            }
        }

        public void AddItemRow(string keyname, string [] columnValues, string group = "", System.Drawing.Color backColor = new System.Drawing.Color())
        {
            Suspend();

            SwitchNoDataMessage(false);

            ListViewItem listViewItem = new ListViewItem(columnValues, -1);

            listViewItem.Name = keyname;
            if (!backColor.IsEmpty)
                listViewItem.BackColor = backColor;

            if (!string.IsNullOrEmpty(group))
                listViewItem.Group = listView1.Groups[group];

            listView1.Items.Add(listViewItem);
            CountDataLines++;

            int cnt = GetRowCount() - 1;

            if (cnt % 2 == 0)
                listView1.Items[cnt].BackColor = listView1.Items[cnt].BackColor.Darken(15);


            listView1.Enabled = GetRowCount() > 0;

            DataChanged?.Invoke(this, null);
        }

        public void Suspend()
        {
            if (isSuspended)
                return;

            isSuspended = true;

            SuspendLayout();
            listView1.SuspendLayout();
            listView1.BeginUpdate();
        }

        public void Resume()
        {
            if (!isSuspended)
                return;

            isSuspended = false;

            listView1.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
            listView1.Enabled = GetRowCount() > 0;

            listView1.EndUpdate();
            listView1.ResumeLayout();
            ResumeLayout();
        }

        public void Clear(bool all = false)
        {
            if (all)
                listView1.Clear();
            else
            {
                listView1.Items.Clear();
                listView1.Groups.Clear();
            }

            SwitchNoDataMessage(true);

            CountDataLines = 0;
            listView1.Enabled = false;

            DataChanged?.Invoke(this, null);
        }

        public int GetRowCount()
        {
            return CountDataLines;
        }

        public bool HasData()
        {
            return CountDataLines > 0;
        }

        /// <summary>
        /// returns that height which is necessary to display all the content without scrolling
        /// </summary>
        /// <returns></returns>
        public int GetGoodHeight()
        {
            float h = GetRowCount();

            var minH = GetMinHeight();

            if (h == 0)
                return minH;
            else
                return Math.Max(minH, GetHeightForRowcount(h + 0.5f));
        }

        /// <summary>
        /// min height is the height incl. header and column header (if at least one row) and 3 rows (if more than 3 rows present)
        /// </summary>
        /// <returns></returns>
        public int GetMinHeight()
        {
            return GetHeightForRowcount(3);
        }

        private int GetHeightForRowcount(float cnt)
        {
            if (!Visible)
                return 0;

            float singleRowHeight = GetRowHeight() * 1.125f;
            float res = /*label/caption of that listviewUC*/ tableLayoutPanel1.RowStyles[0].Height  +  /*column header height*/ 25;
            res += Math.Max(3f, cnt) * singleRowHeight; //tableLayoutPanel1.RowStyles[1].Height;

            res += tableLayoutPanel1.Padding.Top + tableLayoutPanel1.Padding.Bottom + listView1.Margin.Top + listView1.Margin.Bottom;

            if (listView1.Scrollable)
                res += singleRowHeight;

            return Convert.ToInt32(res);
        }

        private int GetRowHeight()
        {
            return (HasData() ? listView1.Items[0].Bounds.Height : 12);
        }
    }
}

