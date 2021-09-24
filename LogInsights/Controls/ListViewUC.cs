using LogInsights.ExceptionHandling;
using System;
using System.Linq;
using System.Windows.Forms;

using LogInsights.Helpers;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace LogInsights.Controls
{
    public partial class ListViewUC : UserControl
    {
        public event ListViewItemSelectionChangedEventHandler ItemClicked;
        public event EventHandler DataChanged;
        public int CountDataLines;
        private int _iSuspended = 0; 

        private string _NoDataMessage = " < No data to display > ";
        public string NoDataMessage
        {
            get => _NoDataMessage;
            set { _NoDataMessage = value; SwitchNoDataMessage(true, true); }
        }

        private bool isNoDataMessageDisplayed = false;

        #region helper class


        class ListViewItemComparer : IComparer
        {
            private int col;
            public ListViewItemComparer()
            {
                col = 0;
            }
            public ListViewItemComparer(int column)
            {
                col = column;
            }

            public bool Ascending
            {
                get;
                set;
            }

            public int Compare(object x, object y)
            {
                int iReturn = string.Compare(((ListViewItem)x).SubItems[col].Text, ((ListViewItem)y).SubItems[col].Text, StringComparison.OrdinalIgnoreCase);

                if (!Ascending)
                    iReturn *= -1;

                return iReturn;
            }
        }

        #endregion

        public ListViewUC()
        {
            InitializeComponent();

            SetupCaption(null);
            Dock = DockStyle.Fill;
            CountDataLines = 0;
            Visible = false;

            listView.Enabled = false;

            listView.ItemSelectionChanged += new ListViewItemSelectionChangedEventHandler((object o, ListViewItemSelectionChangedEventArgs args) =>
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
            if (!string.IsNullOrEmpty(s))
                Visible = true;

            tableLayoutPanel1.RowStyles[0].Height = string.IsNullOrEmpty(s) ? 0 : 23;
            //label1.Enabled = !string.IsNullOrEmpty(s);
            label1.Visible = !string.IsNullOrEmpty(s);
            label1.Text = s;
        }

        public void SetupGroups(string[] groups)
        {
            listView.Groups.AddRange(groups.Select(g => new ListViewGroup(g, g)).ToArray());
            listView.ShowGroups = listView.Groups.Count > 0;
        }

        public void SetupHeaders(string[] headers)
        {
            Visible = true;

            listView.Columns.AddRange(headers.Select(h => new System.Windows.Forms.ColumnHeader() {Text = h }).ToArray());

            SwitchNoDataMessage(true);
        }

        private void SwitchNoDataMessage(bool show, bool messageChanged = false)
        {
            if (listView == null || listView.Columns == null  || listView.Columns.Count == 0)
                return;

            if (show || messageChanged)
            {
                if (listView.Items?.Count != 0)
                    listView.Items.Clear();

                listView.Items.Add(new ListViewItem(NoDataMessage));
                isNoDataMessageDisplayed = true;

                return;
            }

            if (!show && isNoDataMessageDisplayed)
            {
                listView.Items.Clear();
                isNoDataMessageDisplayed = false;
            }
        }

        public void AddItemRow(string keyName, string [] columnValues, string group = "", System.Drawing.Color backColor = new System.Drawing.Color())
        {
            SwitchNoDataMessage(false);

            ListViewItem listViewItem = new ListViewItem(columnValues, -1);

            listViewItem.Name = keyName;

            if (!backColor.IsEmpty)
                listViewItem.BackColor = backColor;

            if (!string.IsNullOrEmpty(group))
                listViewItem.Group = listView.Groups[group];

            listView.Items.Add(listViewItem);

            CountDataLines++;

            if (CountDataLines % 2 == 0)
                listViewItem.BackColor = listViewItem.BackColor.Darken(15);

            DataChanged?.Invoke(this, null);
        }

        public void Suspend()
        {
            if (_iSuspended == 0)
            {
                SuspendLayout();
                listView.SuspendLayout();
                listView.BeginUpdate();
            }

            _iSuspended++;
        }

        public bool IsSuspended => _iSuspended > 0;

        public void Resume()
        {
            if (_iSuspended == 0)
                throw new Exception("Control is not suspended.");

            _iSuspended--;

            if (_iSuspended == 0)
            {
                listView.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
                listView.Enabled = GetRowCount() > 0;

                listView.EndUpdate();
                listView.ResumeLayout();
                ResumeLayout();
            }
         }

        public void Clear(bool all = false)
        {
            if (all)
                listView.Clear();
            else
            {
                listView.Items.Clear();
                listView.Groups.Clear();
            }

            SwitchNoDataMessage(true);

            CountDataLines = 0;
            listView.Enabled = false;

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

            res += tableLayoutPanel1.Padding.Top + tableLayoutPanel1.Padding.Bottom + listView.Margin.Top + listView.Margin.Bottom;

            if (listView.Scrollable)
                res += singleRowHeight;

            return Convert.ToInt32(res);
        }

        private int GetRowHeight()
        {
            return (HasData() ? listView.Items[0].Bounds.Height : 12);
        }

        private void listView_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            try
            {
                ListView lv = (ListView)sender;

                var sOrder = GetSortIcon(lv, e.Column);

                ListViewItemComparer columnSorter = new ListViewItemComparer(e.Column);
                columnSorter.Ascending = sOrder == SortOrder.Descending;

                lv.ListViewItemSorter = (IComparer)columnSorter;

                SetSortIcon(lv, e.Column, columnSorter.Ascending ? SortOrder.Ascending : SortOrder.Descending);
            }
            catch (Exception ex)
            {
                ExceptionHandler.Instance.HandleException(ex);
            }
        }

        #region WinCalls

        [StructLayout(LayoutKind.Sequential)]
        public struct HDITEM
        {
            public Mask mask;
            public int cxy;
            [MarshalAs(UnmanagedType.LPTStr)] public string pszText;
            public IntPtr hbm;
            public int cchTextMax;
            public Format fmt;
            public IntPtr lParam;
            // _WIN32_IE >= 0x0300 
            public int iImage;
            public int iOrder;
            // _WIN32_IE >= 0x0500
            public uint type;
            public IntPtr pvFilter;
            // _WIN32_WINNT >= 0x0600
            public uint state;

            [Flags]
            public enum Mask
            {
                Format = 0x4,       // HDI_FORMAT
            };

            [Flags]
            public enum Format
            {
                SortDown = 0x200,   // HDF_SORTDOWN
                SortUp = 0x400,     // HDF_SORTUP
            };
        };

        public const int LVM_FIRST = 0x1000;
        public const int LVM_GETHEADER = LVM_FIRST + 31;

        public const int HDM_FIRST = 0x1200;
        public const int HDM_GETITEM = HDM_FIRST + 11;
        public const int HDM_SETITEM = HDM_FIRST + 12;

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr SendMessage(IntPtr hWnd, UInt32 msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr SendMessage(IntPtr hWnd, UInt32 msg, IntPtr wParam, ref HDITEM lParam);

        public static SortOrder GetSortIcon(ListView listViewControl, int columnIndex)
        {
            IntPtr columnHeader = SendMessage(listViewControl.Handle, LVM_GETHEADER, IntPtr.Zero, IntPtr.Zero);
            
            var columnPtr = new IntPtr(columnIndex);
            var item = new HDITEM
            {
                mask = HDITEM.Mask.Format
            };

            if (SendMessage(columnHeader, HDM_GETITEM, columnPtr, ref item) == IntPtr.Zero)
            {
                throw new Win32Exception();
            }

            if ((item.fmt & HDITEM.Format.SortUp) == HDITEM.Format.SortUp)
                return SortOrder.Ascending;

            if ((item.fmt & HDITEM.Format.SortDown) == HDITEM.Format.SortDown)
                return SortOrder.Descending;

            return SortOrder.None;
        }

        public static void SetSortIcon(ListView listViewControl, int columnIndex, SortOrder order)
        {
            IntPtr columnHeader = SendMessage(listViewControl.Handle, LVM_GETHEADER, IntPtr.Zero, IntPtr.Zero);
            for (int columnNumber = 0; columnNumber <= listViewControl.Columns.Count - 1; columnNumber++)
            {
                var columnPtr = new IntPtr(columnNumber);
                var item = new HDITEM
                {
                    mask = HDITEM.Mask.Format
                };

                if (SendMessage(columnHeader, HDM_GETITEM, columnPtr, ref item) == IntPtr.Zero)
                {
                    throw new Win32Exception();
                }

                if (order != SortOrder.None && columnNumber == columnIndex)
                {
                    switch (order)
                    {
                        case SortOrder.Ascending:
                            item.fmt &= ~HDITEM.Format.SortDown;
                            item.fmt |= HDITEM.Format.SortUp;
                            break;
                        case SortOrder.Descending:
                            item.fmt &= ~HDITEM.Format.SortUp;
                            item.fmt |= HDITEM.Format.SortDown;
                            break;
                    }
                }
                else
                {
                    item.fmt &= ~HDITEM.Format.SortDown & ~HDITEM.Format.SortUp;
                }

                if (SendMessage(columnHeader, HDM_SETITEM, columnPtr, ref item) == IntPtr.Zero)
                {
                    throw new Win32Exception();
                }
            }
        }

        #endregion
    }
}

