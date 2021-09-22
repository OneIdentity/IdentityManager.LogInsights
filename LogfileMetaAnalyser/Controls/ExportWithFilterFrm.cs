using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using LogfileMetaAnalyser.Datastore;
using LogfileMetaAnalyser.ExceptionHandling;
using LogfileMetaAnalyser.Helpers;

namespace LogfileMetaAnalyser.Controls
{
    // ReSharper disable LocalizableElement
    public partial class ExportWithFilterFrm : Form
    {
        public ExportSettings exportSettings;

        private readonly DatastoreStructure myDataStore;
        private readonly ExportProfiles exportProfiles;
        private const string BeforeOtherFilters = "before other filters";
        private const string AfterOtherFilters = "after other filters";

        public ExportWithFilterFrm(DatastoreStructure datastore, ExportSettings exportSettings)
        {
            myDataStore = datastore;
            this.exportSettings = exportSettings;

            exportProfiles = new ExportProfiles(datastore);

            InitializeComponent();

            treeActivities.AfterCheck += TreeActivities_AfterCheck;

            comboBox_Profiles.SelectedIndexChanged += (_, _) =>
            {
                try
                {
                    LoadSettingsByProfilename();
                }
                catch (Exception e)
                {
                    ExceptionHandler.Instance.HandleException(e);
                }
            };


            if (!myDataStore.HasData())
                return;

            FillProfileDropdown();
            FillInputFilePanel();
            FillOutputFilePanel();
            FillActivityPanel();
            FillLogTypePanel();
            FillRegexFilters();
        }

        private void TreeActivities_AfterCheck(object sender, TreeViewEventArgs e)
        {
            try
            {
                if (e.Action != TreeViewAction.Unknown)
                    SwitchNodeState(e.Node, e.Node.Checked ? 1 : 0);
            }
            catch (Exception exception)
            {
                ExceptionHandler.Instance.HandleException(exception);
            }
        }

        private void LoadSettingsByProfilename()
        {
            if (!myDataStore.HasData())
                return;

            string profilename = comboBox_Profiles.SelectedItem as string;

            if (string.IsNullOrEmpty(profilename))
                return;

            if (!exportProfiles.profiles_predef.TryGetValue(profilename, out exportSettings))
                if (!exportProfiles.profiles_custom.TryGetValue(profilename, out exportSettings))
                    exportSettings = new ExportSettings(myDataStore);

            //exportSettings.PutDatastoreRef(myDataStore);

            FillInputFilePanel();
            FillOutputFilePanel();
            FillActivityPanel();
            FillLogTypePanel();
            FillRegexFilters();
        }

        private void SwitchNodeState(TreeNode node, int setState = -1)
        {

            switch (setState)
            {
                case -1: //switch
                    node.Checked = !node.Checked;
                    break;

                case 0: //disable
                    node.Checked = false;
                    break;

                case 1: //enable
                    node.Checked = true;
                    break;

                case 2: //enable this from child 
                    node.Checked = true;
                    break;
            }

            sbyte newstate = node.Checked ? (sbyte)1 : (sbyte)0;

            //in case the current node was enabled also enable the parent nodes too
            if (node.Parent != null) //this node has a parent
                if (newstate == 1 //either the current node was switched, so handle its parent
                    || setState == 2) //we already came from a child and are now on its parent, so handle the parent of the parent
                    SwitchNodeState(node.Parent, 2);

            //set current state to all child nodes as well
            if (setState < 2)
                foreach (var subnode in node.Nodes.OfType<TreeNode>())
                    SwitchNodeState(subnode, newstate);

            //node.Invalidate(true);
        }


        private void FillProfileDropdown()
        {
            var currentItem = comboBox_Profiles.SelectedItem;

            comboBox_Profiles.Items.Clear();
            comboBox_Profiles.Items.Add("");
            comboBox_Profiles.Items.AddRange(exportProfiles.profiles_predef.Keys.Cast<object>().ToArray());
            comboBox_Profiles.Items.AddRange(exportProfiles.profiles_custom.Keys.Cast<object>().ToArray());

            try
            {
                comboBox_Profiles.SelectedItem = currentItem;
            }
            catch
            {
                // ignored
            } 
        }

        /// <summary>
        /// initialize gui and fill gui by data from passed exportsetting
        /// </summary>
        private void FillInputFilePanel()
        {
            //input file name list
            gridInputfiles.Rows.Clear();
            foreach (var (_, value) in myDataStore.generalLogData.logfileInformation)
            {
                var rowIdx = gridInputfiles.Rows.Add();
                
                gridInputfiles.Rows[rowIdx].Height = 20;
                gridInputfiles.Rows[rowIdx].Cells[0].Value = true;
                gridInputfiles.Rows[rowIdx].Cells[1].Value = value.filename; 
                gridInputfiles.Rows[rowIdx].Tag = value;
            }
             
        }

        /// <summary>
        /// initialize gui and fill gui by data from passed exportsetting
        /// </summary>
        private void FillOutputFilePanel()
        {
            textBox_postfix.Text = exportSettings.inputOutputOptions.filenamePostfix ?? "";
            textBox_destFolder.Text = exportSettings.inputOutputOptions.outputFolder ?? "";
            checkBox_mergeFiles.Checked = exportSettings.inputOutputOptions.mergeFiles;
        }

        /// <summary>
        /// initialize gui and fill gui by data from passed exportsetting
        /// </summary>
        private void FillActivityPanel()
        {
            Stack<Tuple<TreeNode, bool>> joblst = new Stack<Tuple<TreeNode, bool>>();
            try
            {
                treeActivities.BeginUpdate();
                treeActivities.Nodes.Clear();
                
                //part 1: Projections
                var category = "Projection activities";
                var on = exportSettings.filterByActivity.isfilterEnabled_ProjectionActivity;
                var node = treeActivities.Nodes.Add(category);
                node.Checked = on;
                joblst.Push(new Tuple<TreeNode, bool>(node, on));

                category = "Projections";
                on = exportSettings.filterByActivity.isfilterEnabled_ProjectionActivity_Projections;
                node = treeActivities.Nodes[0].Nodes
                    .Add($"{category} ({myDataStore.projectionActivity.projections.Count})");
                node.Checked = on;
                joblst.Push(new Tuple<TreeNode, bool>(node, on));

                category = "AdHoc jobs";
                on = exportSettings.filterByActivity.isfilterEnabled_ProjectionActivity_Projections_AdHoc;
                var aNode = treeActivities.Nodes[0].Nodes[0].Nodes.Add(
                    $"{category} ({myDataStore.projectionActivity.NumberOfAdHocProjections})");
                aNode.Checked = on;
                joblst.Push(new Tuple<TreeNode, bool>(aNode, on));

                category = "Jobservice jobs";
                on = exportSettings.filterByActivity.isfilterEnabled_ProjectionActivity_Projections_Sync;
                var bNode = treeActivities.Nodes[0].Nodes[0].Nodes.Add(
                    $"{category} ({myDataStore.projectionActivity.NumberOfSyncProjections})");
                bNode.Checked = on;
                joblst.Push(new Tuple<TreeNode, bool>(bNode, on));

                if (myDataStore.projectionActivity.projections.Count > 0)
                {
                    int i = 0;
                    foreach (var proj in myDataStore.projectionActivity.projections.Where(t =>
                        t.projectionType == ProjectionType.AdHocProvision))
                    {
                        var xNode = aNode.Nodes.Add($"#{++i} {proj.GetLabel()}");
                        xNode.Tag = proj.uuid;
                        joblst.Push(new Tuple<TreeNode, bool>(xNode,
                            exportSettings.filterByActivity.filterProjectionActivity_Projections_AdHocLst.Contains(
                                proj.uuid)));
                    }

                    i = 0;
                    foreach (var proj in myDataStore.projectionActivity.projections.Where(t =>
                        t.projectionType != ProjectionType.AdHocProvision))
                    {
                        var xNode = bNode.Nodes.Add($"#{++i} {proj.GetLabel()}");
                        xNode.Tag = proj.uuid;
                        joblst.Push(new Tuple<TreeNode, bool>(xNode,
                            exportSettings.filterByActivity.filterProjectionActivity_Projections_SyncLst.Contains(
                                proj.uuid)));
                    }
                }

                //change gui
                while (joblst.Count > 0)
                {
                    var job = joblst.Pop();
                    SwitchNodeState(job.Item1, job.Item2 ? 1 : 0);
                }


                //part 2: Jobservice
                on = exportSettings.filterByActivity.isfilterEnabled_JobServiceActivity;
                node = treeActivities.Nodes.Add(category);
                node.Checked = on;
                joblst.Push(new Tuple<TreeNode, bool>(node, on));

                category = "by Component";
                on = exportSettings.filterByActivity.isfilterEnabled_JobServiceActivity_ByComponent;
                var treeNodeByComponent = treeActivities.Nodes[1].Nodes
                    .Add($"{category} ({myDataStore.jobserviceActivities.distinctTaskfull.Count})");
                treeNodeByComponent.Checked = on;
                joblst.Push(new Tuple<TreeNode, bool>(treeNodeByComponent, on));

                foreach (var taskname in myDataStore.jobserviceActivities.jobserviceJobs.Select(job => job.taskfull).OrderBy(x => x).Distinct())
                {
                    on = exportSettings.filterByActivity.filterJobServiceActivity_ByComponentLst.Contains(taskname);
                    var xNode = treeNodeByComponent.Nodes.Add(taskname);
                    xNode.Checked = on;
                }

                category = "by Queue";
                on = exportSettings.filterByActivity.isfilterEnabled_JobServiceActivity_ByQueue;
                var treeNodeByQueuename = treeActivities.Nodes[1].Nodes
                    .Add($"{category} ({myDataStore.jobserviceActivities.distinctQueuename.Count})");
                treeNodeByQueuename.Checked = on;
                joblst.Push(new Tuple<TreeNode, bool>(treeNodeByQueuename, on));
                 
                foreach (var queuename in myDataStore.jobserviceActivities.jobserviceJobs.Select(job => job.queuename).OrderBy(x => x).Distinct())
                {
                    on = exportSettings.filterByActivity.filterJobServiceActivity_ByQueueLst.Contains(queuename);
                    var xNode = treeNodeByQueuename.Nodes.Add(queuename);
                    xNode.Checked = on;
                } 

            
                //change gui
                while (joblst.Count > 0)
                {
                    var job = joblst.Pop();
                    SwitchNodeState(job.Item1, job.Item2 ? 1 : 0);
                }
            }
            finally
            {
                treeActivities.EndUpdate();
                treeActivities.ExpandAll();
            }
        }

        /// <summary>
        /// initialize gui and fill gui by data from passed exportsetting
        /// </summary>
        private void FillLogTypePanel()
        {
            tableLayoutPanel_FilterByLogProperties.SuspendLayout();
                        
            try
            {
                tableLayoutPanel_FilterByLogProperties.RowStyles.Clear();
                tableLayoutPanel_FilterByLogProperties.Controls.Clear();
                tableLayoutPanel_FilterByLogProperties.RowCount = 1;

                //Min timestamp - not part of profile
                tableLayoutPanel_FilterByLogProperties.RowStyles.Add(new RowStyle(SizeType.AutoSize, 40F));

                Label l1 = new Label()
                {
                    Text = "Start time:",
                    Anchor = AnchorStyles.Left | AnchorStyles.Right
                };
                l1.Font = new Font(l1.Font, FontStyle.Bold | FontStyle.Underline);

                DateTimePicker dtp1 = new DateTimePicker()
                {
                    Name = "DateTimePicker_startDate",
                    Value = exportSettings.filterByLogtype.startDate,
                    MinDate = myDataStore.generalLogData.logDataOverallTimeRange_Start,
                    MaxDate = myDataStore.generalLogData.logDataOverallTimeRange_Finish,
                    //Format = DateTimePickerFormat.Long,
                    Format = DateTimePickerFormat.Custom,
                    CustomFormat = "yyy-MM-dd HH:mm:ss",
                    Anchor = AnchorStyles.Left | AnchorStyles.Right
                };

                tableLayoutPanel_FilterByLogProperties.Controls.Add(l1, 0, 0);
                tableLayoutPanel_FilterByLogProperties.Controls.Add(dtp1, 1, 0);
                tableLayoutPanel_FilterByLogProperties.RowCount++;


                //Max timestamp - not part of profile
                tableLayoutPanel_FilterByLogProperties.RowStyles.Add(new RowStyle(SizeType.AutoSize, 40F));
                Label l2 = new Label()
                {
                    Text = "End time:",
                    Anchor = AnchorStyles.Left | AnchorStyles.Right
                };
                l2.Font = new Font(l2.Font, FontStyle.Bold | FontStyle.Underline);

                DateTimePicker dtp2 = new DateTimePicker()
                {
                    Name = "DateTimePicker_endDate",
                    Value = exportSettings.filterByLogtype.endDate,
                    MinDate = myDataStore.generalLogData.logDataOverallTimeRange_Start,
                    MaxDate = myDataStore.generalLogData.logDataOverallTimeRange_Finish,
                    //Format = DateTimePickerFormat.Long,
                    Format = DateTimePickerFormat.Custom,
                    CustomFormat = "yyy-MM-dd HH:mm:ss",
                    Anchor = AnchorStyles.Left | AnchorStyles.Right
                };

                tableLayoutPanel_FilterByLogProperties.Controls.Add(l2, 0, 1);
                tableLayoutPanel_FilterByLogProperties.Controls.Add(dtp2, 1, 1);
                tableLayoutPanel_FilterByLogProperties.RowCount++;


                //Log level - part of profile
                Label l3 = new Label()
                {
                    Text = "Log levels",
                    Anchor = AnchorStyles.Left | AnchorStyles.Right
                };
                l3.Font = new Font(l3.Font, FontStyle.Bold | FontStyle.Underline);

                CheckBox cb;

                int colY = 2;
                tableLayoutPanel_FilterByLogProperties.Controls.Add(l3, 0, colY);

                Button bl3 = new Button()
                {
                    Text = "toggle log level switches",
                    TextAlign = ContentAlignment.MiddleLeft,
                    Anchor = AnchorStyles.Left | AnchorStyles.Right
                };
                tableLayoutPanel_FilterByLogProperties.Controls.Add(bl3, 1, colY++);
                tableLayoutPanel_FilterByLogProperties.RowStyles.Add(new RowStyle(SizeType.AutoSize, 40F));
                tableLayoutPanel_FilterByLogProperties.RowCount++;


                List<CheckBox> cbLst_LogLevelBoxes = new List<CheckBox>();

                foreach (var iLevel in Enumerable.Range(LogLevelTools.FewestDetailedLevel, LogLevelTools.MostDetailedLevel))
                {
                    var Llevel = LogLevelTools.ConvertFromNumberToEnum(Convert.ToByte(iLevel));
                    string sLevel = LogLevelTools.ConvertFromEnumToString(Llevel);

                    cb = new CheckBox()
                    {
                        AutoSize = true,
                        TextAlign = ContentAlignment.MiddleLeft,
                        Name = $"cb_LogLevel {sLevel}",
                        Text = $"Log level: {sLevel} ({myDataStore.generalLogData.numberOfEntriesPerLoglevel.GetOrReturnDefault(Llevel)})",
                        Checked = exportSettings.filterByLogtype.logLevelFilters.GetBoolOrAdd(Llevel, true)
                    };

                    tableLayoutPanel_FilterByLogProperties.Controls.Add(cb, 1, colY++);
                    tableLayoutPanel_FilterByLogProperties.RowStyles.Add(new RowStyle(SizeType.AutoSize, 40F));
                    tableLayoutPanel_FilterByLogProperties.RowCount++;

                    cbLst_LogLevelBoxes.Add(cb);
                }

                bl3.Click += (_, _) =>
                {
                    try
                    {
                        foreach (var ckb in cbLst_LogLevelBoxes)
                            ckb.Checked = !ckb.Checked;
                    }
                    catch (Exception e)
                    {
                        ExceptionHandler.Instance.HandleException(e);
                    }
                };



                //log source
                Label l4 = new Label()
                {
                    Text = "Log sources (modules, components)",
                    Anchor = AnchorStyles.Left | AnchorStyles.Right
                };
                l4.Font = new Font(l4.Font, FontStyle.Bold | FontStyle.Underline);

                tableLayoutPanel_FilterByLogProperties.Controls.Add(l4, 0, colY);

                Button bl4 = new Button()
                {
                    Text = "toggle log sources switches",
                    TextAlign = ContentAlignment.MiddleLeft,
                    Anchor = AnchorStyles.Left | AnchorStyles.Right
                };
                tableLayoutPanel_FilterByLogProperties.Controls.Add(bl4, 1, colY++);
                tableLayoutPanel_FilterByLogProperties.RowStyles.Add(new RowStyle(SizeType.AutoSize, 40F));
                tableLayoutPanel_FilterByLogProperties.RowCount++;

                List<CheckBox> cbLst_LogSourceBoxes = new List<CheckBox>();

                Dictionary<string, long> data = Constants.logSourcesOfInterest.Select(t => new KeyValuePair<string, long>(t, 0)).ToDictionary(t => t.Key, v => v.Value, StringComparer.InvariantCultureIgnoreCase);
                foreach (var elem in myDataStore.generalLogData.numberOflogSources)
                    data.AddOrUpdate(elem.Key, elem.Value);

                foreach (var kp in data.OrderBy(t => t.Key))
                {
                    cb = new CheckBox()
                    {
                        AutoSize = true,
                        Name = $"cb_LogSource {kp.Key}",
                        TextAlign = ContentAlignment.MiddleLeft,
                        Checked = exportSettings.filterByLogtype.logSourceFilters.GetBoolOrAdd(kp.Key, true),
                        Text = $"Log message source: {kp.Key} ({kp.Value})"
                    };

                    tableLayoutPanel_FilterByLogProperties.Controls.Add(cb, 1, colY++);
                    tableLayoutPanel_FilterByLogProperties.RowStyles.Add(new RowStyle(SizeType.AutoSize, 40F));
                    tableLayoutPanel_FilterByLogProperties.RowCount++;

                    cbLst_LogSourceBoxes.Add(cb);
                }

                bl4.Click += (_, _) =>
                {
                    try
                    {
                        foreach (var ckb in cbLst_LogSourceBoxes)
                            ckb.Checked = !ckb.Checked;
                    }
                    catch (Exception e)
                    {
                        ExceptionHandler.Instance.HandleException(e);
                    }
                };
            }
            finally
            {
                tableLayoutPanel_FilterByLogProperties.ResumeLayout();
            }
        }

        /// <summary>
        /// initialize gui and fill gui by data from passed exportsetting
        /// </summary>
        private void FillRegexFilters()
        {
            gridRegexFilter.Rows.Clear();

            foreach (var filter in exportSettings.filterByRegex.rxFilters)
            {
                var rowIdx = gridRegexFilter.Rows.Add();

                gridRegexFilter.Rows[rowIdx].Height = 20;
                gridRegexFilter.Rows[rowIdx].Cells[0].Value = filter.enabled;
                gridRegexFilter.Rows[rowIdx].Cells[1].Value = filter.regexText;
                gridRegexFilter.Rows[rowIdx].Cells[2].Value = filter.ignoreCase;
                gridRegexFilter.Rows[rowIdx].Cells[3].Value = filter.isMatch;
                gridRegexFilter.Rows[rowIdx].Cells[4].Value = filter.isAppliedAtStart ? BeforeOtherFilters : AfterOtherFilters;

                gridRegexFilter.Rows[rowIdx].Tag = filter;
            }

        }

        /// <summary>
        /// get data from gui and finally put it into exportsetting object; also calls the exportsetting.prepare method to consolidate everything to be ready for usage
        /// </summary>
        private void GetExportSettingFromGui()
        {
            //input files
            exportSettings.inputOutputOptions.includeFiles.Clear();

            foreach (var row in gridInputfiles.Rows
                .OfType<DataGridViewRow>()
                .Where(r => (bool)r.Cells[0].Value))
            {
                var lfi = (LogfileInformation)row.Tag;
                exportSettings.inputOutputOptions.includeFiles.Add(lfi!.filename);
            }
                        
            //output file options
            exportSettings.inputOutputOptions.filenamePostfix = textBox_postfix.Text;
            exportSettings.inputOutputOptions.outputFolder = textBox_destFolder.Text;
            exportSettings.inputOutputOptions.mergeFiles = checkBox_mergeFiles.Checked;


            //Filter By Activity
            //projections activities
            exportSettings.filterByActivity.isfilterEnabled_ProjectionActivity = treeActivities.Nodes[0].Checked;
            exportSettings.filterByActivity.isfilterEnabled_ProjectionActivity_Projections = treeActivities.Nodes[0].Nodes[0].Checked;
            exportSettings.filterByActivity.isfilterEnabled_ProjectionActivity_Projections_AdHoc = treeActivities.Nodes[0].Nodes[0].Nodes[0].Checked;
            exportSettings.filterByActivity.isfilterEnabled_ProjectionActivity_Projections_Sync = treeActivities.Nodes[0].Nodes[0].Nodes[1].Checked;

            exportSettings.filterByActivity.filterProjectionActivity_Projections_AdHocLst.Clear();
            foreach (var subnode in treeActivities.Nodes[0].Nodes[0].Nodes[0].Nodes.OfType<TreeNode>().Where(t => t.Checked))
                exportSettings.filterByActivity.filterProjectionActivity_Projections_AdHocLst.Add(subnode.Tag.ToString());

            exportSettings.filterByActivity.filterProjectionActivity_Projections_SyncLst.Clear();
            foreach (var subnode in treeActivities.Nodes[0].Nodes[0].Nodes[1].Nodes.OfType<TreeNode>().Where(t => t.Checked))
                exportSettings.filterByActivity.filterProjectionActivity_Projections_SyncLst.Add(subnode.Tag.ToString());

            //jobservice activities
            exportSettings.filterByActivity.isfilterEnabled_JobServiceActivity = treeActivities.Nodes[1].Checked;
            exportSettings.filterByActivity.isfilterEnabled_JobServiceActivity_ByComponent = treeActivities.Nodes[1].Nodes[0].Checked;
            foreach (var subnode in treeActivities.Nodes[1].Nodes[0].Nodes.OfType<TreeNode>().Where(t => t.Checked))
                exportSettings.filterByActivity.filterJobServiceActivity_ByComponentLst.Add(subnode.Text);

            exportSettings.filterByActivity.isfilterEnabled_JobServiceActivity_ByQueue = treeActivities.Nodes[1].Nodes[1].Checked;
            foreach (var subnode in treeActivities.Nodes[1].Nodes[1].Nodes.OfType<TreeNode>().Where(t => t.Checked))
                exportSettings.filterByActivity.filterJobServiceActivity_ByQueueLst.Add(subnode.Text);

            //Filter by Log type
            //Dates
            exportSettings.filterByLogtype.startDate = ((DateTimePicker)tableLayoutPanel_FilterByLogProperties.Controls["DateTimePicker_startDate"]).Value;
            exportSettings.filterByLogtype.endDate = ((DateTimePicker)tableLayoutPanel_FilterByLogProperties.Controls["DateTimePicker_endDate"]).Value;


            //Log level
            foreach (var iLevel in Enumerable.Range(LogLevelTools.FewestDetailedLevel, LogLevelTools.MostDetailedLevel))
            {
                var Llevel = LogLevelTools.ConvertFromNumberToEnum(Convert.ToByte(iLevel));
                string sLevel = LogLevelTools.ConvertFromEnumToString(Llevel);

                exportSettings.filterByLogtype.logLevelFilters[Llevel] = ((CheckBox)tableLayoutPanel_FilterByLogProperties.Controls[$"cb_LogLevel {sLevel}"]).Checked;
            }


            //log source
            foreach (Control ctl in tableLayoutPanel_FilterByLogProperties.Controls)
            {
                if (!ctl.Name.StartsWith("cb_LogSource "))
                    continue;

                string source = ctl.Name.Substring("cb_LogSource ".Length);

                exportSettings.filterByLogtype.logSourceFilters.AddOrUpdate(source, ((CheckBox)ctl).Checked);
            }


            //Filter by Regex
            for (int i = 0; i < exportSettings.filterByRegex.rxFilters.Count; i++)
            {
                var row = gridRegexFilter.Rows[i];

                exportSettings.filterByRegex.rxFilters[i].enabled = (bool)row.Cells[0].Value;
                exportSettings.filterByRegex.rxFilters[i].regexText = (string)row.Cells[1].Value;
                exportSettings.filterByRegex.rxFilters[i].ignoreCase = (bool)row.Cells[2].Value;
                exportSettings.filterByRegex.rxFilters[i].isMatch = (bool)row.Cells[3].Value;
                exportSettings.filterByRegex.rxFilters[i].isAppliedAtStart = (string)row.Cells[4].Value == BeforeOtherFilters;
            }

            exportSettings.PrepareForFilterAndExport();
        }


        private void buttonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void button_Export_Click(object sender, EventArgs e)
        {
            try
            {
                GetExportSettingFromGui();

                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception exception)
            {
                ExceptionHandler.Instance.HandleException(exception);
            }
        }

        private void button_Newprofile_Click(object sender, EventArgs e)
        {
            try
            {
                string profilename = comboBox_Profiles.SelectedItem as string;
                string newprofilename = profilename;

                bool keepAsking = true;

                GetExportSettingFromGui();

                using (QuestionFrm frm = new QuestionFrm())
                    while (keepAsking)
                    {   
                        frm.SetupLabel("Please name your new profile:");
                        frm.SetupData(profilename);
                        var result = frm.ShowDialog();

                        if (result == DialogResult.Cancel)
                            break;
                        else
                        {
                            newprofilename = frm.QuestionDataText;

                            if (exportProfiles.profiles_predef.ContainsKey(newprofilename))
                            {
                                MessageBox.Show("This profile name is already taken by a predefined profile which cannot be overwritten. Choose another profile name!", "predefined profile", MessageBoxButtons.OK);
                            }
                            else if (exportProfiles.profiles_custom.ContainsKey(newprofilename))
                            {
                                var qRes = MessageBox.Show("Profile already exists with this name. Overwrite it?", "Overwrite profile?", MessageBoxButtons.YesNoCancel);

                                if (qRes == DialogResult.Cancel)
                                {
                                    keepAsking = false;
                                    newprofilename = "";
                                }
                                else
                                    keepAsking = qRes == DialogResult.No; //keep asking for profile name if profile name is NOt to overwrite
                            }
                            else
                                keepAsking = false;
                        }
                    }

                if (!string.IsNullOrEmpty(newprofilename))
                {
                    exportProfiles.AddOrUpdateProfile(newprofilename, exportSettings);

                    FillProfileDropdown();
                    comboBox_Profiles.SelectedItem = newprofilename;
                }
            }
            catch (Exception exception)
            {
                ExceptionHandler.Instance.HandleException(exception);
            }
        }

        private void button_delprofile_Click(object sender, EventArgs e)
        {
            try
            {
                string profilename = comboBox_Profiles.SelectedItem as string;
                if (string.IsNullOrEmpty(profilename))
                    MessageBox.Show("No profile selected!");
                else
                {
                    string msg = exportProfiles.DeleteProfile(profilename);

                    if (msg == "")
                    {
                        comboBox_Profiles.Items.Remove(profilename);
                        comboBox_Profiles.SelectedIndex = 0;
                    }
                    else
                        MessageBox.Show(msg);
                }
            }
            catch (Exception exception)
            {
                ExceptionHandler.Instance.HandleException(exception);
            }
                
        }

      
        
    }
}

