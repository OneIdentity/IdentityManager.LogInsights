using System;
using System.Collections.Generic; 
using System.Drawing;
using System.Linq; 
using System.Windows.Forms;

using LogfileMetaAnalyser.Helpers;

namespace LogfileMetaAnalyser.Controls
{
    public partial class MultiListViewUC : UserControl
    {
        public List<ListViewUC> listOflistviewUC = new List<ListViewUC>();
        public bool isSuspended = false;

        public MultiListViewUC()
        {
            InitializeComponent();
            this.Dock = DockStyle.Fill;

            flowLayoutPanel1.Dock = DockStyle.Fill;
            flowLayoutPanel1.MinimumSize = new Size(100, 50);
            flowLayoutPanel1.AutoScroll = true;
            flowLayoutPanel1.VerticalScroll.Enabled = true;
            flowLayoutPanel1.VerticalScroll.Visible = true;

            flowLayoutPanel1.Resize += new EventHandler((object o, EventArgs a) => { ReArrangeSave(); });
        }

        public void SetupLayout(int numberOfListviews, bool toSuspend=true)
        {
            for (int i = 1; i <= numberOfListviews; i++)
            {
                ListViewUC uc = new ListViewUC();
                uc.Dock = DockStyle.None;
                uc.Anchor = AnchorStyles.Left | AnchorStyles.Right;                                 

                listOflistviewUC.Add(uc);
                flowLayoutPanel1.Controls.Add(uc);

                uc.DataChanged += new EventHandler((object o, EventArgs a) => { ReArrangeSave(); });
                uc.VisibleChanged += new EventHandler((object o, EventArgs a) => { ReArrangeSave(); });
            }

            if (toSuspend)
                Suspend();
        }

        public void SetupSubLevel(int ucNumber, int level)
        {
            if (ucNumber < 0 || ucNumber >= listOflistviewUC.Count)
                throw new ArgumentOutOfRangeException("ucNumber");

            Label label = new Label();
            //label.BorderStyle = BorderStyle.FixedSingle;
            label.Text = "\r\n└>"; // @"\->";
            label.Font = new Font("Consolas", 14f);
            label.AutoSize = true;
            label.TextAlign = ContentAlignment.TopRight;
            label.Anchor = AnchorStyles.Right | AnchorStyles.Top;

            var tableLayoutPanel = new TableLayoutPanel();
            tableLayoutPanel.ColumnCount = 2;
            tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, level*40F));
            tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel.Controls.Add(label, 0, 0);
            tableLayoutPanel.Controls.Add(listOflistviewUC[ucNumber], 1, 0);
            tableLayoutPanel.RowCount = 1;
            tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            tableLayoutPanel.Dock = DockStyle.None;
            tableLayoutPanel.Margin = new Padding(0);
            tableLayoutPanel.Padding = new Padding(0);
            tableLayoutPanel.Size = new Size(100, 100);
            tableLayoutPanel.AutoSize = true;
            tableLayoutPanel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            tableLayoutPanel.MinimumSize = new Size(50,50);

            //tableLayoutPanel.CellBorderStyle = TableLayoutPanelCellBorderStyle.Single;

            //flowLayoutPanel1.Controls.RemoveAt(ucNumber);  //will be done automatically when this control changes its parent

            flowLayoutPanel1.Controls.Add(tableLayoutPanel);
            flowLayoutPanel1.Controls.SetChildIndex(tableLayoutPanel, ucNumber);

            listOflistviewUC[ucNumber].NoDataMessage = " < No data yet to display. Try to select a data item in a box above! > ";
        }

        public ListViewUC this[int i]
        {
            get
            {
                return listOflistviewUC[i];
            }
        }

        public void Suspend()
        {
            if (isSuspended)
                return;

            this.SuspendLayout();
            foreach (var cnt in listOflistviewUC)
                cnt.Suspend();

            isSuspended = true;
        }

        public void Resume()
        {
            ReArrange();
            
            foreach (var cnt in listOflistviewUC)
                cnt.Resume();
                       
            this.ResumeLayout();

            isSuspended = false;
        }

        private void ReArrangeSave()
        {
            if (isSuspended)
                return;

            try
            {
                Suspend();
                Resume();  //includes ReArrange();
            }
            catch { }
        }

        public void ReArrange()
        {
            //this MultiListView control holds elements of type listView or tableView (which holds one row of listView)
            //so the question is: how much space does each element need?


            //flowLayoutPanel1.MinimumSize = new Size(100, 50);

            int width = flowLayoutPanel1.Width - flowLayoutPanel1.Padding.All - 32;
            int height = flowLayoutPanel1.Height - flowLayoutPanel1.Padding.All - flowLayoutPanel1.Margin.All;
            int height75 = (height * 0.75).IntDown();
            int height96 = (height * 0.96).IntDown();

            List<MultiListViewElementStorage> storageElems = new List<MultiListViewElementStorage>();


            //collecting data
            foreach (var uc in flowLayoutPanel1.Controls) //listOflistviewUC)
            {
                ListViewUC ucc;
                int w = width;

                if (uc.GetType() == typeof(ListViewUC))
                    ucc = uc as ListViewUC;
                else
                {
                    ucc = ((uc as TableLayoutPanel).Controls[1] as ListViewUC);
                    w -= Convert.ToInt32((uc as TableLayoutPanel).ColumnStyles[0].Width);
                }


                storageElems.Add(new MultiListViewElementStorage(ucc, w));
            }

            if (storageElems.Count == 0)
                return;
                  

            //rearrange
            //we have an area of <height> pixel (w/o scrolling) to provide a specific height for all elems
            if (storageElems.Count == 1)
                storageElems[0].finalHeight = storageElems[0].goodHeight.EnsureRange(storageElems[0].minHeight, height96);
            else
            {
                int space = 0;
                while (MultiListViewElementStorage.AllHeight(storageElems) < height96 && (height75 + space < height96))
                {
                    foreach (var storageElem in storageElems)
                        storageElem.finalHeight = storageElem.goodHeight.EnsureRange(storageElem.minHeight, height75 + space);

                    space += 10;
                }
            }


            //resize all elems
            storageElems.ForEach(e => e.Resize());  //finalHeight/finalWidth

            flowLayoutPanel1.Focus();
        }

        public bool HasData()
        {
            if (!listOflistviewUC.Any())
                return false;

            return listOflistviewUC.Any(uc => uc.HasData());
        }

        public int Count()
        {
            return listOflistviewUC.Count;
        }

        internal class MultiListViewElementStorage
        {
            public int goodHeight;
            public int minHeight;
            public int finalWidth;
            public ListViewUC uc;
            public int finalHeight;

            public MultiListViewElementStorage(ListViewUC uc, int width)
            {
                this.uc = uc;

                finalWidth = width;

                goodHeight = uc.GetGoodHeight();
                minHeight = uc.GetMinHeight();
            }

            public void Resize()
            {
                if (finalHeight < 1 || finalWidth < 1)
                    return;

                uc.Size = new Size(finalWidth, finalHeight);
            }

            public static int AllHeight(List<MultiListViewElementStorage> lst)
            {
                return lst.Sum(x => x.finalHeight);
            }
        }
    }
}
