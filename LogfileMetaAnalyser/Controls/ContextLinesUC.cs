using System;
using System.Collections.Generic;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text; 
using System.Windows.Forms;

using LogfileMetaAnalyser.Helpers;


namespace LogfileMetaAnalyser.Controls
{
    public partial class ContextLinesUC : UserControl
    {
        private TextMessage currentMsg = null; 

        private Exporter logfileFilterExporter;


        public ContextLinesUC(Exporter logfileFilterExporter)
        {
            InitializeComponent();

            this.logfileFilterExporter = logfileFilterExporter;

//          rtbLog.Document.ReadOnly = true;
//          rtbLog.Document.Multiline = true;
//          rtbLog.CurrentLineHighlightingVisible = true;
//            rtbLog.LineNumberMarginVisible = true;

            buttonExport.Enabled = false;
            buttonShowInEditor.Enabled = false; 

            this.Dock = DockStyle.Fill;
                
            comboBox_OpenInEditor.BackColor = Constants.contextLinesUcHighlightColor;
            comboBox_OpenInEditor.SelectedIndexChanged += new EventHandler((object o, EventArgs args) => {

                button_MessagesJumpForward.Visible = comboBox_OpenInEditor.Visible && comboBox_OpenInEditor.Items.Count > 1;
                button_MessagesJumpBack.Visible = comboBox_OpenInEditor.Visible && comboBox_OpenInEditor.Items.Count > 1;
                                
                button_MessagesJumpForward.Enabled = comboBox_OpenInEditor.SelectedIndex < comboBox_OpenInEditor.Items.Count - 1;
                button_MessagesJumpBack.Enabled = comboBox_OpenInEditor.SelectedIndex > 0;

                if (comboBox_OpenInEditor.SelectedItem == null || comboBox_OpenInEditor.Items.Count == 0)
                    return;

                var SelectedItem = ((DropdownItem<FileInformationContext>)comboBox_OpenInEditor.SelectedItem);

                if (SelectedItem == null || SelectedItem.data == null)
                    return;

                //jump to the end first 
                if (rtbLog.Lines.Length > 5)
                {
                    rtbLog.SelectionStart = rtbLog.Text.Length;
                    // scroll it automatically
                    rtbLog.ScrollToCaret();
                }

                //now jump to the correct pos
                //int jumppos = Math.Max(1, SelectedItem.data.fileposRelativeInView - 3);
                //rtbLog.SelectedView.GoToLine(jumppos);
            });

            SetupCaption("");
        }


        private void buttonShowInEditor_Click(object sender, EventArgs e)
        {
            string fn;
            long fp;

            if (comboBox_OpenInEditor.Items.Count == 0)
            {
                fn = currentMsg.textLocator.fileName;
                fp = currentMsg.textLocator.fileLinePosition;
            }
            else
            {
                fn = ((DropdownItem<FileInformationContext>)comboBox_OpenInEditor.SelectedItem).data.filename;
                fp = ((DropdownItem<FileInformationContext>)comboBox_OpenInEditor.SelectedItem).data.filepos;
            }

            if (!string.IsNullOrEmpty(fn))
                Helpers.ProcessExec.OpenFileInEditor(fn, fp);
        }


        private async void buttonExport_Click(object sender, EventArgs e)
        {
            await logfileFilterExporter.FilterAndExportFromMessage(currentMsg);
        }


        public void SetupCaption(string s)
        {
            tableLayoutPanel1.RowStyles[0].Height = string.IsNullOrEmpty(s) ? 0 : 23;
            label1.Enabled = !string.IsNullOrEmpty(s);
            label1.Visible = !string.IsNullOrEmpty(s);
            label1.Text = s;
        }

         
        public void SetData(params TextMessage[] theMessages)
        {
            if (theMessages == null || theMessages.Length == 0)
                return; 

            theMessages = theMessages.Where(m => m != null).OrderBy(m => m.textLocator.fileLinePosition).ToArray();  //clear null messages and sort the rest by line position

            //check if theMessageA.contextMsgAfter to theMessageB.contextMsgBefore will close the gap
            string fn = theMessages[0].textLocator.fileName;
            long ppos = theMessages[0].contextMsgAfter.GetLastElemOrDefault(theMessages[0]).textLocator.fileLinePosition;
            bool isOpenGap = false;
            long msgIdfirstbreak = -1;
            bool isSingleMsg = theMessages.Length == 1;

            foreach (var msg in theMessages)
            {
                //if (msg.contextMsgBefore == null) msg.contextMsgBefore = new TextMessage[] { };
                //if (msg.contextMsgAfter == null) msg.contextMsgAfter = new TextMessage[] { };
                
                if (!isOpenGap && !isSingleMsg)
                {
                    if (fn != msg.textLocator.fileName)
                        isOpenGap = true;

                    //if end of last message's context (after) message line (+1) is lower than the starting context (before) message line of current message, we have an open gap
                    if ((ppos + 1) < msg.contextMsgBefore.GetFirstElemOrDefault(msg).textLocator.fileLinePosition)
                    {
                        isOpenGap = true;
                        msgIdfirstbreak = msg.textLocator.messageNumber;
                    }
                    else
                        ppos = msg.contextMsgAfter.GetLastElemOrDefault(msg).textLocator.fileLinePosition;
                }
            }

            //we take messageA and build up a new contextMsgAfter list including the lines of messageB and so on
            if (!isSingleMsg)
            {
                Dictionary<long, TextMessage> dict_newContextLinesAfter = new Dictionary<long, TextMessage>();
                dict_newContextLinesAfter.AddRange(
                                            theMessages[0].contextMsgAfter.Select(e => e.textLocator.fileLinePosition).ToArray(),
                                            theMessages[0].contextMsgAfter.ToArray());
                
                for (int i = 1; i < theMessages.Length; i++)
                {
                    if (isOpenGap && theMessages[i].textLocator.messageNumber >= msgIdfirstbreak)
                        //append all context lines of msgB after a gap indicator 
                        dict_newContextLinesAfter.Add(theMessages[i].textLocator.fileLinePosition - 1, new TextMessage(new TextLocator(), " < ... > " + Environment.NewLine));

                    //add the context (before) messages
                    dict_newContextLinesAfter.AddRange(
                                theMessages[i].contextMsgBefore.Select(e => e.textLocator.fileLinePosition).ToArray(),
                                theMessages[i].contextMsgBefore.ToArray());

                    //add the message itself
                    dict_newContextLinesAfter.AddOrUpdate(theMessages[i].textLocator.fileLinePosition, theMessages[i]);

                    //add the context (after) messages
                    dict_newContextLinesAfter.AddRange(
                                theMessages[i].contextMsgAfter.Select(e => e.textLocator.fileLinePosition).ToArray(),
                                theMessages[i].contextMsgAfter.ToArray());
                }

                theMessages[0].contextMsgAfter = 
                    dict_newContextLinesAfter
                    .Where(msg => !theMessages[0]
                                    .contextMsgBefore
                                    .Any(msgb => msgb.textLocator.fileLinePosition == msg.Key) 
                                  && 
                                  msg.Key != theMessages[0].textLocator.fileLinePosition)
                    .OrderBy(e => e.Key)
                    .Select(e => e.Value)
                    .ToArray();
            }

            //rtbLog.LineNumberMarginVisible = !isOpenGap && theMessages[0].textLocator?.fileLinePosition > 0;

            bool fullyCompleteMessage = theMessages[0].textLocator?.fileLinePosition > 0;

            List<FileInformationContext> jumpLines = _SetData(theMessages[0],
                                                              fullyCompleteMessage,
                                                              fullyCompleteMessage ? theMessages.Select(m => m.textLocator.fileLinePosition) : new long[] { },
                                                              fullyCompleteMessage);

            currentMsg = theMessages[0];

            buttonExport.Enabled = true;
            buttonShowInEditor.Enabled = fullyCompleteMessage; 

            FillOpenInEditorDropdown(jumpLines, true);
        }

        
        private void FillOpenInEditorDropdown(List<FileInformationContext> filePos = null, bool prefillLast = false)
        {
            comboBox_OpenInEditor.Items.Clear();

            if (filePos == null || filePos.Count() <= 1)
            {
                tableLayoutPanel2.ColumnStyles[3].Width = 0;
                comboBox_OpenInEditor.Visible = false;
            }
            else
            {
                int i = 0;
                tableLayoutPanel2.ColumnStyles[3].Width = 150;
                comboBox_OpenInEditor.Visible = true;
                
                foreach (FileInformationContext aFilePos in filePos)
                    comboBox_OpenInEditor.Items.Add(new DropdownItem<FileInformationContext>($"marker #{++i} (line {aFilePos.filepos})", aFilePos));

                if (prefillLast)
                    comboBox_OpenInEditor.SelectedIndex = comboBox_OpenInEditor.Items.Count - 1;
                else
                    comboBox_OpenInEditor.SelectedIndex = 1;
            }
            
            button_MessagesJumpForward.Visible = comboBox_OpenInEditor.Visible && comboBox_OpenInEditor.Items.Count > 1;
            button_MessagesJumpBack.Visible = comboBox_OpenInEditor.Visible && comboBox_OpenInEditor.Items.Count > 1;

            button_MessagesJumpForward.Enabled = comboBox_OpenInEditor.SelectedIndex < comboBox_OpenInEditor.Items.Count - 1;
            button_MessagesJumpBack.Enabled = comboBox_OpenInEditor.SelectedIndex > 0;
        }

        private List<FileInformationContext> _SetData(TextMessage theMessage, bool setCaption, IEnumerable<long> highlightFilePositions, bool jumpToLastMarker = false)
        {
           
            //rtbLog.SuspendPainting();

            List<FileInformationContext> filectxLst = new List<FileInformationContext>();

            try
            {
                BeginUpdate();

                rtbLog.SuspendLayout();

                List<Tuple<int, int>> highlightEditorPositions = new List<Tuple<int, int>>();
                int highlightEditorPositions_tmp_start;
                if (highlightFilePositions == null)
                    highlightFilePositions = new long[] { };
                
                rtbLog.Text = "";

                //if (rtbLog.LineNumberMarginVisible)
                //    rtbLog.Document.AutoLineNumberingBase = theMessage.contextMsgBefore.GetFirstElemOrDefault(theMessage).textLocator.fileLinePosition.Int();
                

                //pre lines
                if (theMessage.contextMsgBefore != null)
                    foreach (TextMessage tm in theMessage.contextMsgBefore)
                    {
                        if (highlightFilePositions.Contains(tm.textLocator.fileLinePosition))
                            highlightEditorPositions_tmp_start = Math.Max(0, rtbLog.Lines.Length - 1);
                        else
                            highlightEditorPositions_tmp_start = -1;

                        rtbLog.AppendText(tm.messageText+Environment.NewLine);

                        if (highlightEditorPositions_tmp_start >= 0)
                        {
                            highlightEditorPositions.Add(new Tuple<int, int>(highlightEditorPositions_tmp_start, rtbLog.Lines.Length - 1));
                            filectxLst.Add(new FileInformationContext(tm.textLocator.fileName, tm.textLocator.fileLinePosition, highlightEditorPositions_tmp_start));
                        }
                    }


                //the message itself
                if (highlightFilePositions.Contains(theMessage.textLocator.fileLinePosition))
                    highlightEditorPositions_tmp_start = Math.Max(0, rtbLog.Lines.Length - 1);
                else
                    highlightEditorPositions_tmp_start = -1;

                rtbLog.AppendText(theMessage.messageText + Environment.NewLine);

                if (highlightEditorPositions_tmp_start >= 0)
                { 
                    highlightEditorPositions.Add(new Tuple<int, int>(highlightEditorPositions_tmp_start, rtbLog.Lines.Length - 1));
                    filectxLst.Add(new FileInformationContext(theMessage.textLocator.fileName, theMessage.textLocator.fileLinePosition, highlightEditorPositions_tmp_start));
                }


                //post lines
                if (theMessage.contextMsgAfter != null)
                    foreach (TextMessage tm in theMessage.contextMsgAfter)
                        {
                            if (highlightFilePositions.Contains(tm.textLocator.fileLinePosition))
                                highlightEditorPositions_tmp_start = Math.Max(0, rtbLog.Lines.Length - 1);
                            else
                                highlightEditorPositions_tmp_start = -1;

                            rtbLog.AppendText(tm.messageText + Environment.NewLine);

                            if (highlightEditorPositions_tmp_start >= 0)
                            { 
                                highlightEditorPositions.Add(new Tuple<int, int>(highlightEditorPositions_tmp_start, rtbLog.Lines.Length - 1));
                                filectxLst.Add(new FileInformationContext(tm.textLocator.fileName, tm.textLocator.fileLinePosition, highlightEditorPositions_tmp_start));
                            }
                        }


                
                //highlighting
                foreach (var tp in highlightEditorPositions)
                    for (int linePos = tp.Item1; linePos < Math.Min(rtbLog.Lines.Length, tp.Item2); linePos++)
                        SetLineColor( linePos, Constants.contextLinesUcHighlightColor);

                
                //set scrollbar position (go to line)
                if (highlightEditorPositions.Count > 0)
                {
                    int jumpToEditorLine = jumpToLastMarker ? highlightEditorPositions.Last().Item1 - 1 : highlightEditorPositions[0].Item1 - 1;

                    ScrollToLine(jumpToEditorLine);
                }
                

                //save the displayed msg for later use
                currentMsg = theMessage;
            }
            finally
            {
                //rtbLog.ResumePainting();
                rtbLog.ResumeLayout();

                EndUpdate();

                rtbLog.Invalidate();
            }

            if (setCaption)
            {
                long l1 = theMessage.textLocator.fileLinePosition;
                long lBefore = theMessage.contextMsgBefore.Length == 0 ? -1 : theMessage.contextMsgBefore[0].textLocator.fileLinePosition;
                long lAfter  = theMessage.contextMsgAfter.Length == 0 ? -1 : theMessage.contextMsgAfter.Last().textLocator.fileLinePosition;

                StringBuilder sb = new StringBuilder();

                sb.Append("[");

                if (lBefore > 0)
                    sb.Append($"{lBefore} ... ");

                sb.Append(!highlightFilePositions.Any() ? theMessage.textLocator.fileLinePosition.ToString() : string.Join(";", highlightFilePositions));

                if (lAfter > 0)
                    sb.Append($" ... {lAfter}");

                sb.Append("]");
                SetupCaption(string.Format("file {0}; line(s) {1}", 
                                            System.IO.Path.GetFileName(theMessage.textLocator.fileName), 
                                            sb.ToString()));
            }


            return filectxLst;
        }

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool GetScrollInfo(IntPtr hwnd, int fnBar, ref SCROLLINFO lpsi);

        [DllImport("user32.dll")]
        static extern int SetScrollInfo(IntPtr hwnd, int fnBar, [In] ref SCROLLINFO lpsi, bool fRedraw);

        [DllImport("User32.dll", CharSet = CharSet.Auto, EntryPoint = "SendMessage")]
        static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        struct SCROLLINFO
        {
            public uint cbSize;
            public uint fMask;
            public int nMin;
            public int nMax;
            public uint nPage;
            public int nPos;
            public int nTrackPos;
        }

        enum ScrollBarDirection
        {
            SB_HORZ = 0,
            SB_VERT = 1,
            SB_CTL = 2,
            SB_BOTH = 3
        }

        enum ScrollInfoMask
        {
            SIF_RANGE = 0x1,
            SIF_PAGE = 0x2,
            SIF_POS = 0x4,
            SIF_DISABLENOSCROLL = 0x8,
            SIF_TRACKPOS = 0x10,
            SIF_ALL = SIF_RANGE + SIF_PAGE + SIF_POS + SIF_TRACKPOS
        }

        const int WM_VSCROLL = 277;
        const int SB_LINEUP = 0;
        const int SB_LINEDOWN = 1;
        const int SB_THUMBPOSITION = 4;
        const int SB_THUMBTRACK = 5;
        const int SB_TOP = 6;
        const int SB_BOTTOM = 7;
        const int SB_ENDSCROLL = 8;

        private const int WM_SETREDRAW = 0x0b;


        private void ScrollToLine(int iLine)
        {
            if (iLine < 0)
                return;

            int iChar = rtbLog.GetFirstCharIndexFromLine(iLine);

            Point cPos = rtbLog.GetPositionFromCharIndex(iChar);

            IntPtr handle = rtbLog.Handle;

                // Get current scroller position

                SCROLLINFO si = new SCROLLINFO();
                si.cbSize = (uint)Marshal.SizeOf(si);
                si.fMask = (uint)ScrollInfoMask.SIF_ALL;
                GetScrollInfo(handle, (int)ScrollBarDirection.SB_VERT, ref si);

                // Increase position by pixles
                si.nPos += cPos.Y;

                // Reposition scroller
                SetScrollInfo(handle, (int)ScrollBarDirection.SB_VERT, ref si, true);

                // Send a WM_VSCROLL scroll message using SB_THUMBTRACK as wParam
                // SB_THUMBTRACK: low-order word of wParam, si.nPos high-order word of  wParam

                IntPtr ptrWparam = new IntPtr(SB_THUMBTRACK + 0x10000 * si.nPos);
                IntPtr ptrLparam = new IntPtr(0);
                SendMessage(handle, WM_VSCROLL, ptrWparam, ptrLparam);

                //SendMessage(rtbLog.Handle, WM_VSCROLL, (IntPtr)SB_PAGEBOTTOM, IntPtr.Zero);

            rtbLog.Select( iChar, 0);
        }

        public void BeginUpdate()
        {
            SendMessage(rtbLog.Handle, WM_SETREDRAW, (IntPtr)0, IntPtr.Zero);
        }

        public void EndUpdate()
        {
            SendMessage(rtbLog.Handle, WM_SETREDRAW, (IntPtr)1, IntPtr.Zero);
        }

        
        private void button_MessagesUp_Click(object sender, EventArgs e)
        {
            if (comboBox_OpenInEditor.CanSelect && comboBox_OpenInEditor.SelectedIndex > 0) 
                comboBox_OpenInEditor.SelectedIndex--;
        }

        private void button_MessagesDn_Click(object sender, EventArgs e)
        {
            if (comboBox_OpenInEditor.CanSelect && comboBox_OpenInEditor.SelectedIndex+1 < comboBox_OpenInEditor.Items.Count)
                comboBox_OpenInEditor.SelectedIndex++;
        }

        public void SetLineColor(int iLine, Color c)
        {
            int firstCharIndex = rtbLog.GetFirstCharIndexFromLine(iLine);

            string s = rtbLog.Lines[iLine];

            rtbLog.Select(firstCharIndex, s.Length);
            rtbLog.SelectionBackColor = c;
        }
    }
}
