using LogInsights.ExceptionHandling;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text; 
using System.Windows.Forms;

using LogInsights.Helpers;
using LogInsights.LogReader;


namespace LogInsights.Controls
{
    public partial class ContextLinesUC : UserControl
    {
        private LogEntry currentMsg = null; 

        private Exporter logfileFilterExporter;


        public ContextLinesUC(Exporter logfileFilterExporter)
        {
            InitializeComponent();

            this.logfileFilterExporter = logfileFilterExporter;

//          rtbLog.Document.ReadOnly = true;
//          rtbLog.Document.Multiline = true;
//          rtbLog.CurrentLineHighlightingVisible = true;
            rtbLog.ShowLineNumbers = true;

            buttonExport.Enabled = false;
            buttonShowInEditor.Enabled = false; 

            this.Dock = DockStyle.Fill;
                
            comboBox_OpenInEditor.BackColor = Constants.contextLinesUcHighlightColor;
            comboBox_OpenInEditor.SelectedIndexChanged += new EventHandler((object o, EventArgs args) => {

                try
                {
                    button_MessagesJumpForward.Visible = comboBox_OpenInEditor.Visible && comboBox_OpenInEditor.Items.Count > 1;
                    button_MessagesJumpBack.Visible = comboBox_OpenInEditor.Visible && comboBox_OpenInEditor.Items.Count > 1;
                                
                    button_MessagesJumpForward.Enabled = comboBox_OpenInEditor.SelectedIndex < comboBox_OpenInEditor.Items.Count - 1;
                    button_MessagesJumpBack.Enabled = comboBox_OpenInEditor.SelectedIndex > 0;

                    if (comboBox_OpenInEditor.SelectedItem == null || comboBox_OpenInEditor.Items.Count == 0)
                        return;

                    var SelectedItem = ((DropdownItem<FileInformationContext>)comboBox_OpenInEditor.SelectedItem);

                    if (SelectedItem == null || SelectedItem.data == null)
                        return;

                    /*
                    //jump to the end first 
                    if (rtbLog.Lines.Length > 5)
                    {
                        rtbLog.SelectionStart = rtbLog.Text.Length;
                        // scroll it automatically
                        rtbLog.ScrollToLine();
                    }
                    */

                    //now jump to the correct pos
                    int jumppos = Math.Max(1, SelectedItem.data.fileposRelativeInView - 3);
                    rtbLog.ScrollToLine(jumppos);
                }
                catch (Exception e)
                {
                    ExceptionHandler.Instance.HandleException(e);
                }
            });

            SetupCaption("");
        }


        private void buttonShowInEditor_Click(object sender, EventArgs e)
        {
            try
            {
                string fn;
                long fp;

                if (comboBox_OpenInEditor.Items.Count == 0)
                {
                    fn = currentMsg.Locator.Source;
                    fp = currentMsg.Locator.Position;
                }
                else
                {
                    fn = ((DropdownItem<FileInformationContext>)comboBox_OpenInEditor.SelectedItem).data.filename;
                    fp = ((DropdownItem<FileInformationContext>)comboBox_OpenInEditor.SelectedItem).data.filepos;
                }

                if (!string.IsNullOrEmpty(fn))
                    ProcessExec.OpenFileInEditor(fn, fp);
            }
            catch (Exception exception)
            {
                ExceptionHandler.Instance.HandleException(exception);
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code", "CAC001:ConfigureAwaitChecker", Justification = "<Pending>")]
        private async void buttonExport_Click(object sender, EventArgs e)
        {
            try
            {
                await logfileFilterExporter.FilterAndExportFromMessage(currentMsg);
            }
            catch (Exception exception)
            {
                ExceptionHandler.Instance.HandleException(exception);
            }
        }


        public void SetupCaption(string s)
        {
            tableLayoutPanel1.RowStyles[0].Height = string.IsNullOrEmpty(s) ? 0 : 23;
            label1.Enabled = !string.IsNullOrEmpty(s);
            label1.Visible = !string.IsNullOrEmpty(s);
            label1.Text = s;
        }

         
        public void SetData(params LogEntry[] theMessages)
        {
            if (theMessages == null || theMessages.Length == 0)
                return; 

            theMessages = theMessages.Where(m => m != null).OrderBy(m => m.Locator.Position).ToArray();  //clear null messages and sort the rest by line position

            //check if theMessageA.contextMsgAfter to theMessageB.contextMsgBefore will close the gap
            LogEntry theMessage = theMessages[0];
            string fn = theMessage.Locator.Source;
            long ppos = theMessage.ContextNextEntries.GetLastElemOrDefault(theMessage).Locator.Position;
            bool isOpenGap = false;
            long msgIdfirstbreak = -1;
            bool isSingleMsg = theMessages.Length == 1;

            foreach (var msg in theMessages)
            {
                //if (msg.contextMsgBefore == null) msg.contextMsgBefore = new LogEntry[] { };
                //if (msg.contextMsgAfter == null) msg.contextMsgAfter = new LogEntry[] { };
                
                if (!isOpenGap && !isSingleMsg)
                {
                    if (fn != msg.Locator.Source)
                        isOpenGap = true;

                    //if end of last message's context (after) message line (+1) is lower than the starting context (before) message line of current message, we have an open gap
                    if ((ppos + 1) < msg.ContextPreviousEntries.GetFirstElemOrDefault(msg).Locator.Position)
                    {
                        isOpenGap = true;
                        msgIdfirstbreak = msg.Locator.EntryNumber;
                    }
                    else
                        ppos = msg.ContextNextEntries.GetLastElemOrDefault(msg).Locator.Position;
                }
            }

            //we take messageA and build up a new contextMsgAfter list including the lines of messageB and so on
            if (!isSingleMsg)
            {
                var dict_newContextLinesAfter = new Dictionary<int, LogEntry>();
                dict_newContextLinesAfter.AddRange(
                                            theMessage.ContextNextEntries.Select(e => e.Locator.Position).ToArray(),
                                            theMessage.ContextNextEntries.ToArray());
                
                for (int i = 1; i < theMessages.Length; i++)
                {
                    if (isOpenGap && theMessages[i].Locator.EntryNumber >= msgIdfirstbreak)
                        //append all context lines of msgB after a gap indicator 
                        dict_newContextLinesAfter.Add(
                            theMessages[i].Locator.Position - 1, 
                            new LogEntry
                            {
                                Locator = new Locator(),
                                Message = " < ... > " + Environment.NewLine
                            });

                    //add the context (before) messages
                    dict_newContextLinesAfter.AddRange(
                                theMessages[i].ContextPreviousEntries.Select(e => e.Locator.Position).ToArray(),
                                theMessages[i].ContextPreviousEntries.ToArray());

                    //add the message itself
                    dict_newContextLinesAfter.AddOrUpdate(theMessages[i].Locator.Position, theMessages[i]);

                    //add the context (after) messages
                    dict_newContextLinesAfter.AddRange(
                                theMessages[i].ContextNextEntries.Select(e => e.Locator.Position).ToArray(),
                                theMessages[i].ContextNextEntries.ToArray());
                }

                theMessage.ContextNextEntries = 
                    dict_newContextLinesAfter
                    .Where(msg => !theMessage
                                    .ContextPreviousEntries
                                    .Any(msgb => msgb.Locator.Position == msg.Key) 
                                  && 
                                  msg.Key != theMessage.Locator.Position)
                    .OrderBy(e => e.Key)
                    .Select(e => e.Value)
                    .ToArray();
            }

            rtbLog.ShowLineNumbers = !isOpenGap && theMessage.Locator?.Position > 0;

            bool fullyCompleteMessage = theMessage.Locator?.Position > 0;

            List<FileInformationContext> jumpLines = _SetData(theMessage,
                                                              fullyCompleteMessage,
                                                              fullyCompleteMessage ? theMessages.Select(m => m.Locator.Position) : Array.Empty<int>(),
                                                              fullyCompleteMessage);

            currentMsg = theMessage;

            buttonExport.Enabled = true;
            buttonShowInEditor.Enabled = fullyCompleteMessage; 

            FillOpenInEditorDropdown(jumpLines, true);
        }

        
        private void FillOpenInEditorDropdown(List<FileInformationContext> filePos = null, bool prefillLast = false)
        {
            comboBox_OpenInEditor.Items.Clear();

            if (filePos == null || filePos.Count <= 1)
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

        private List<FileInformationContext> _SetData(LogEntry theMessage, bool setCaption, IEnumerable<int> highlightFilePositions, bool jumpToLastMarker = false)
        {
           
            //rtbLog.SuspendPainting();

            List<FileInformationContext> filectxLst = new List<FileInformationContext>();

            try
            {
                rtbLog.BeginUpdate();

                rtbLog.SuspendLayout();

                List<Tuple<int, int>> highlightEditorPositions = new List<Tuple<int, int>>();
                int highlightEditorPositions_tmp_start;
                if (highlightFilePositions == null)
                    highlightFilePositions = Array.Empty<int>();
                else
                    highlightFilePositions = highlightFilePositions.ToArray();

                rtbLog.Text = "";

                if (rtbLog.ShowLineNumbers)
                {
                    var tmFirst = theMessage.ContextPreviousEntries.GetFirstElemOrDefault(theMessage);

                    rtbLog.LineNumberOffset = (int) tmFirst.Locator.Position-1;
                }


                //pre lines
                if (theMessage.ContextPreviousEntries != null)
                    foreach (LogEntry tm in theMessage.ContextPreviousEntries)
                    {
                        if (highlightFilePositions.Contains(tm.Locator.Position))
                            highlightEditorPositions_tmp_start = Math.Max(0, rtbLog.Lines.Length - 1);
                        else
                            highlightEditorPositions_tmp_start = -1;

                        rtbLog.AppendText(tm.FullMessage+Environment.NewLine);

                        if (highlightEditorPositions_tmp_start >= 0)
                        {
                            highlightEditorPositions.Add(new Tuple<int, int>(highlightEditorPositions_tmp_start, rtbLog.Lines.Length - 1));
                            filectxLst.Add(new FileInformationContext(tm.Locator.Source, tm.Locator.Position, highlightEditorPositions_tmp_start));
                        }
                    }


                //the message itself
                if (highlightFilePositions.Contains(theMessage.Locator.Position))
                    highlightEditorPositions_tmp_start = Math.Max(0, rtbLog.Lines.Length - 1);
                else
                    highlightEditorPositions_tmp_start = -1;

                rtbLog.AppendText(theMessage.FullMessage + Environment.NewLine);

                if (highlightEditorPositions_tmp_start >= 0)
                { 
                    highlightEditorPositions.Add(new Tuple<int, int>(highlightEditorPositions_tmp_start, rtbLog.Lines.Length - 1));
                    filectxLst.Add(new FileInformationContext(theMessage.Locator.Source, theMessage.Locator.Position, highlightEditorPositions_tmp_start));
                }


                //post lines
                if (theMessage.ContextNextEntries != null)
                    foreach (LogEntry tm in theMessage.ContextNextEntries)
                        {
                            if (highlightFilePositions.Contains(tm.Locator.Position))
                                highlightEditorPositions_tmp_start = Math.Max(0, rtbLog.Lines.Length - 1);
                            else
                                highlightEditorPositions_tmp_start = -1;

                            rtbLog.AppendText(tm.FullMessage + Environment.NewLine);

                            if (highlightEditorPositions_tmp_start >= 0)
                            { 
                                highlightEditorPositions.Add(new Tuple<int, int>(highlightEditorPositions_tmp_start, rtbLog.Lines.Length - 1));
                                filectxLst.Add(new FileInformationContext(tm.Locator.Source, tm.Locator.Position, highlightEditorPositions_tmp_start));
                            }
                        }


                
                //highlighting
                foreach (var tp in highlightEditorPositions)
                {
                    int iForm = tp.Item1;
                    int iTo = Math.Min(rtbLog.Lines.Length, tp.Item2);

                    SetLineColor( new Range(iForm, iTo), Constants.contextLinesUcHighlightColor);
                }


                //set scrollbar position (go to line)
                if (highlightEditorPositions.Count > 0)
                {
                    int jumpToEditorLine = jumpToLastMarker ? highlightEditorPositions.Last().Item1 - 1 : highlightEditorPositions[0].Item1 - 1;

                    rtbLog.ScrollToLine(jumpToEditorLine);
                }
                else
                    rtbLog.ScrollToLine(0);
                

                //save the displayed msg for later use
                currentMsg = theMessage;
            }
            finally
            {
                //rtbLog.ResumePainting();
                rtbLog.ResumeLayout();

                rtbLog.EndUpdate();

                rtbLog.Invalidate();
            }

            if (setCaption)
            {
                long l1 = theMessage.Locator.Position;
                long lBefore = theMessage.ContextPreviousEntries.Length == 0 ? -1 : theMessage.ContextPreviousEntries[0].Locator.Position;
                long lAfter  = theMessage.ContextNextEntries.Length == 0 ? -1 : theMessage.ContextNextEntries.Last().Locator.Position;

                StringBuilder sb = new StringBuilder();

                sb.Append("[");

                if (lBefore > 0)
                    sb.Append($"{lBefore} ... ");

                sb.Append(!highlightFilePositions.Any() ? theMessage.Locator.Position.ToString() : string.Join(";", highlightFilePositions));

                if (lAfter > 0)
                    sb.Append($" ... {lAfter}");

                sb.Append("]");
                SetupCaption(string.Format("file {0}; line(s) {1}", 
                                            System.IO.Path.GetFileName(theMessage.Locator.Source), 
                                            sb.ToString()));
            }


            return filectxLst;
        }

        
        private void button_MessagesUp_Click(object sender, EventArgs e)
        {
            try
            {
                if (comboBox_OpenInEditor.CanSelect && comboBox_OpenInEditor.SelectedIndex > 0) 
                    comboBox_OpenInEditor.SelectedIndex--;
            }
            catch (Exception exception)
            {
                ExceptionHandler.Instance.HandleException(exception);
            }
        }

        private void button_MessagesDn_Click(object sender, EventArgs e)
        {
            try
            {
                if (comboBox_OpenInEditor.CanSelect && comboBox_OpenInEditor.SelectedIndex+1 < comboBox_OpenInEditor.Items.Count)
                    comboBox_OpenInEditor.SelectedIndex++;
            }
            catch (Exception exception)
            {
                ExceptionHandler.Instance.HandleException(exception);
            }
        }

        public void SetLineColor(int iLine, Color c)
        {
            int firstCharIndex = rtbLog.GetFirstCharIndexFromLine(iLine);

            string s = rtbLog.Lines[iLine];

            rtbLog.Select(firstCharIndex, s.Length);
            rtbLog.SelectionBackColor = c;
        }

        public void SetLineColor(Range rLines, Color c)
        {
            int firstRowPos = rtbLog.GetFirstCharIndexFromLine(rLines.Start.Value);
            int lastRowPos = rtbLog.GetFirstCharIndexFromLine(rLines.End.Value);

            rtbLog.Select(firstRowPos, lastRowPos -firstRowPos);
            rtbLog.SelectionBackColor = c;
        }
    }
}
