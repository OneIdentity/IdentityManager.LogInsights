using System;
using System.Collections.Generic;
using System.Drawing;
using System.Data;
using System.Linq;
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

//          syntaxEditor1.Document.ReadOnly = true;
//          syntaxEditor1.Document.Multiline = true;
//          syntaxEditor1.CurrentLineHighlightingVisible = true;
//            syntaxEditor1.LineNumberMarginVisible = true;

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
                if (syntaxEditor1.Lines.Length > 5)
                {
                    syntaxEditor1.SelectionStart = syntaxEditor1.Text.Length;
                    // scroll it automatically
                    syntaxEditor1.ScrollToCaret();
                }

                //now jump to the correct pos
                //int jumppos = Math.Max(1, SelectedItem.data.fileposRelativeInView - 3);
                //syntaxEditor1.SelectedView.GoToLine(jumppos);
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

            //syntaxEditor1.LineNumberMarginVisible = !isOpenGap && theMessages[0].textLocator?.fileLinePosition > 0;

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
            syntaxEditor1.SuspendLayout();
            //syntaxEditor1.SuspendPainting();

            List<FileInformationContext> filectxLst = new List<FileInformationContext>();

            try
            {
                List<Tuple<int, int>> highlightEditorPositions = new List<Tuple<int, int>>();
                int highlightEditorPositions_tmp_start;
                if (highlightFilePositions == null)
                    highlightFilePositions = new long[] { };
                
                syntaxEditor1.Text = "";

                //if (syntaxEditor1.LineNumberMarginVisible)
                //    syntaxEditor1.Document.AutoLineNumberingBase = theMessage.contextMsgBefore.GetFirstElemOrDefault(theMessage).textLocator.fileLinePosition.Int();
                

                //pre lines
                if (theMessage.contextMsgBefore != null)
                    foreach (TextMessage tm in theMessage.contextMsgBefore)
                    {
                        if (highlightFilePositions.Contains(tm.textLocator.fileLinePosition))
                            highlightEditorPositions_tmp_start = Math.Max(0, syntaxEditor1.Lines.Length - 1);
                        else
                            highlightEditorPositions_tmp_start = -1;

                        syntaxEditor1.AppendText(tm.messageText);

                        if (highlightEditorPositions_tmp_start >= 0)
                        {
                            highlightEditorPositions.Add(new Tuple<int, int>(highlightEditorPositions_tmp_start, syntaxEditor1.Lines.Length - 1));
                            filectxLst.Add(new FileInformationContext(tm.textLocator.fileName, tm.textLocator.fileLinePosition, highlightEditorPositions_tmp_start));
                        }
                    }


                //the message itself
                if (highlightFilePositions.Contains(theMessage.textLocator.fileLinePosition))
                    highlightEditorPositions_tmp_start = Math.Max(0, syntaxEditor1.Lines.Length - 1);
                else
                    highlightEditorPositions_tmp_start = -1;

                syntaxEditor1.AppendText(theMessage.messageText);

                if (highlightEditorPositions_tmp_start >= 0)
                { 
                    highlightEditorPositions.Add(new Tuple<int, int>(highlightEditorPositions_tmp_start, syntaxEditor1.Lines.Length - 1));
                    filectxLst.Add(new FileInformationContext(theMessage.textLocator.fileName, theMessage.textLocator.fileLinePosition, highlightEditorPositions_tmp_start));
                }


                //post lines
                if (theMessage.contextMsgAfter != null)
                    foreach (TextMessage tm in theMessage.contextMsgAfter)
                        {
                            if (highlightFilePositions.Contains(tm.textLocator.fileLinePosition))
                                highlightEditorPositions_tmp_start = Math.Max(0, syntaxEditor1.Lines.Length - 1);
                            else
                                highlightEditorPositions_tmp_start = -1;

                            syntaxEditor1.AppendText(tm.messageText);

                            if (highlightEditorPositions_tmp_start >= 0)
                            { 
                                highlightEditorPositions.Add(new Tuple<int, int>(highlightEditorPositions_tmp_start, syntaxEditor1.Lines.Length - 1));
                                filectxLst.Add(new FileInformationContext(tm.textLocator.fileName, tm.textLocator.fileLinePosition, highlightEditorPositions_tmp_start));
                            }
                        }


                /*
                //highlighting
                foreach (var tp in highlightEditorPositions)
                    for (int linePos = tp.Item1; linePos < Math.Min(syntaxEditor1.Lines.Length, tp.Item2); linePos++)
                        syntaxEditor1.Lines[linePos].BackColor = Constants.contextLinesUcHighlightColor;


                //set scrollbar position (go to line)
                if (highlightEditorPositions.Count > 0)
                {
                    int jumpToEditorLine = jumpToLastMarker ? highlightEditorPositions.Last().Item1 -1 : highlightEditorPositions[0].Item1 -1;

                    var stepLinesBack = Math.Min(10, Math.Max(3, ((syntaxEditor1.ClipBounds.Height / (syntaxEditor1.SelectedView.DisplayLineHeight * 2)) / 2.1).IntDown()));
                    syntaxEditor1.SelectedView.GoToLine(Math.Max(0, jumpToEditorLine), Math.Min(jumpToEditorLine-1, stepLinesBack));
                }
                */

                //save the displayed msg for later use
                currentMsg = theMessage;
            }
            finally
            {
                //syntaxEditor1.ResumePainting();
                syntaxEditor1.ResumeLayout();
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
    }
}
