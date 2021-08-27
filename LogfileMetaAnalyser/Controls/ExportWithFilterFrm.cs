using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

using LogfileMetaAnalyser.Datastore;
using LogfileMetaAnalyser.Helpers;

using VI.Controls;

namespace LogfileMetaAnalyser.Controls
{
    public partial class ExportWithFilterFrm : Form
    {
        public ExportSettings exportSettings;

        private Datastore.DatastoreStructure myDataStore;
        private ExportProfiles exportProfiles;
        

        public ExportWithFilterFrm(DatastoreStructure datastore, ExportSettings exportSettings)
        {
            this.myDataStore = datastore;
            this.exportSettings = exportSettings;

            exportProfiles = new ExportProfiles(datastore);

            InitializeComponent();

            treelist_Inputfiles.ImageList = VI.ImageLibrary.ImagelistHandler.StockImageListSmall;
            treelist_regexFilters.ImageList = VI.ImageLibrary.ImagelistHandler.StockImageListSmall;
            treelist_activity.ImageList = VI.ImageLibrary.ImagelistHandler.StockImageListSmall;

            treelist_Inputfiles.NodeIconDoubleClick += new TreeListEventHandler((object o, TreeListEventArgs args) =>
                {
                    try
                    {
                        SwitchNodeState(args.Node);
                    }
                    catch { }
                });

            treelist_activity.NodeIconDoubleClick += new TreeListEventHandler((object o, TreeListEventArgs args) =>
            {
                try
                {
                    SwitchNodeState(args.Node);
                }
                catch { }
            });

            treelist_regexFilters.NodeIconDoubleClick += new TreeListEventHandler((object o, TreeListEventArgs args) =>
            {
                try
                {
                    SwitchNodeState(args.Node);
                }
                catch { }
            });

            
            comboBox_Profiles.SelectedIndexChanged += new EventHandler((object o, EventArgs args) =>
               {
                   LoadSettingsByProfilename();
               });


            if (!myDataStore.HasData())
                return;

            FillProfileDropdown();
            FillInputFilePanel();
            FillOutputFilePanel();
            FillActivityPanel();
            FillLogTypePanel();
            FillRegexFilters();
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
                

        private void SwitchNodeState(TreeListNode node, sbyte setState = -1)
        {
            switch (setState)
            {
                case -1:  //switch
                    node.ImageIndex = isNodeEnabled(node) ? (int)VI.ImageLibrary.StockImage.AssignedNone : (int)VI.ImageLibrary.StockImage.AssignedDirect;
                    break;

                case 0:  //disable
                    node.ImageIndex = (int)VI.ImageLibrary.StockImage.AssignedNone;
                    break;

                case 1:  //enable
                    node.ImageIndex = (int)VI.ImageLibrary.StockImage.AssignedDirect;
                    break;

                case 2: //enable this from child 
                    node.ImageIndex = (int)VI.ImageLibrary.StockImage.AssignedDirect;
                    break;
            }

            sbyte newstate = node.ImageIndex == (int)VI.ImageLibrary.StockImage.AssignedDirect ? (sbyte)1 : (sbyte)0;

            //in case the current node was enabled also enable the parent nodes too
            if (node.ParentNode != null)  //this node has a parent
                if ((newstate == 1 && setState == -1)  //either the current node was switched, so handle its parent
                    || (setState == 2))  //we already came from a child and are now on its parent, so handle the parent of the parent
                    SwitchNodeState(node.ParentNode, 2);

            //set current state to all child nodes as well
            if (setState < 2)
                foreach (var subnode in node.Nodes)
                    SwitchNodeState(subnode, newstate);
            
            node.Invalidate(true);
        }

        private bool isNodeEnabled(TreeListNode node)
        {
            return (node.ImageIndex == (int)VI.ImageLibrary.StockImage.AssignedDirect);
        }

        private void FillProfileDropdown()
        {
            var currentItem = comboBox_Profiles.SelectedItem;

            comboBox_Profiles.Items.Clear();
            comboBox_Profiles.Items.Add("");
            comboBox_Profiles.Items.AddRange(exportProfiles.profiles_predef.Keys.ToArray());
            comboBox_Profiles.Items.AddRange(exportProfiles.profiles_custom.Keys.ToArray());

            try
            {
                comboBox_Profiles.SelectedItem = currentItem;
            }
            catch { };
        }

        /// <summary>
        /// initialize gui and fill gui by data from passed exportsetting
        /// </summary>
        private void FillInputFilePanel()
        {
            TreeListNode node;

            //input file name list
            using (new VI.FormBase.UpdateHelper(treelist_Inputfiles))
            {
                treelist_Inputfiles.Nodes.Clear();

                foreach (var f in myDataStore.generalLogData.logfileInformation)
                {
                    node = treelist_Inputfiles.Nodes.Add("", (int)VI.ImageLibrary.StockImage.AssignedDirect);
                    node.SubItems.Add(new TreeListItemTextBox(f.Value.filenameBestNotation) { Data = f.Value.filename });
                    node.SubItems.Add(new TreeListItemTextBox(f.Value.logfileType.ToString()));

                    if (exportSettings.inputOutputOptions.includeFiles.Contains(f.Value.filename))
                        SwitchNodeState(node, 1);
                }
            }

            //file type
            if (!exportSettings.inputOutputOptions.includeFileType_NLog && !exportSettings.inputOutputOptions.includeFileType_JSLog)
            {
                exportSettings.inputOutputOptions.includeFileType_NLog = true;
                exportSettings.inputOutputOptions.includeFileType_JSLog = true;
            }

            checkBox_inputOpt_nlog.Checked = exportSettings.inputOutputOptions.includeFileType_NLog;
            checkBox_inputOpt_jobservice.Checked = exportSettings.inputOutputOptions.includeFileType_JSLog;
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
            string category;
            bool on;
            int imgOn = (int)VI.ImageLibrary.StockImage.AssignedDirect;
            int imgOff = (int)VI.ImageLibrary.StockImage.AssignedNone;
            TreeListNode node;
            Stack<Tuple<TreeListNode, bool>> joblst = new Stack<Tuple<TreeListNode, bool>>();

            using (new VI.FormBase.UpdateHelper(treelist_activity))
            {
                treelist_activity.Nodes.Clear();
                treelist_activity.Columns[0].Width = 1024;

                //part 1: Projections
                category = "Projection activities";
                on = exportSettings.filterByActivity.isfilterEnabled_ProjectionActivity;
                node = treelist_activity.Nodes.Add(category, on ? imgOn : imgOff);
                joblst.Push(new Tuple<TreeListNode, bool>(node, on));

                category = "Projections";
                on = exportSettings.filterByActivity.isfilterEnabled_ProjectionActivity_Projections;
                node = treelist_activity.Nodes[0].Nodes.Add($"{category} ({myDataStore.projectionActivity.projections.Count})", on ? imgOn : imgOff);
                joblst.Push(new Tuple<TreeListNode, bool>(node, on));

                category = "AdHoc jobs";
                on = exportSettings.filterByActivity.isfilterEnabled_ProjectionActivity_Projections_AdHoc;
                var aNode = treelist_activity.Nodes[0].Nodes[0].Nodes.Add($"{category} ({myDataStore.projectionActivity.NumberOfAdHocProjections})", on ? imgOn : imgOff);
                joblst.Push(new Tuple<TreeListNode, bool>(aNode, on));

                category = "Jobservice jobs";
                on = exportSettings.filterByActivity.isfilterEnabled_ProjectionActivity_Projections_Sync;
                var bNode = treelist_activity.Nodes[0].Nodes[0].Nodes.Add($"{category} ({myDataStore.projectionActivity.NumberOfSyncProjections})", on ? imgOn : imgOff);
                joblst.Push(new Tuple<TreeListNode, bool>(bNode, on));

                if (myDataStore.projectionActivity.projections.Count > 0)
                {
                    int i = 0;
                    foreach (var proj in myDataStore.projectionActivity.projections.Where(t => t.projectionType == ProjectionType.AdHocProvision))
                    {
                        var xNode = aNode.Nodes.Add($"#{++i} {proj.GetLabel()}", imgOff);
                        xNode.Data = proj.uuid;
                        joblst.Push(new Tuple<TreeListNode, bool>(xNode, exportSettings.filterByActivity.filterProjectionActivity_Projections_AdHocLst.Contains(proj.uuid)));
                    }

                    i = 0;
                    foreach (var proj in myDataStore.projectionActivity.projections.Where(t => t.projectionType != ProjectionType.AdHocProvision))
                    {
                        var xNode = bNode.Nodes.Add($"#{++i} {proj.GetLabel()}", imgOff);
                        xNode.Data = proj.uuid;
                        joblst.Push(new Tuple<TreeListNode, bool>(xNode, exportSettings.filterByActivity.filterProjectionActivity_Projections_SyncLst.Contains(proj.uuid)));
                    }
                }

                //change gui
                while (joblst.Count > 0)
                {
                    var job = joblst.Pop();
                    SwitchNodeState(job.Item1, job.Item2 ? (sbyte)1 : (sbyte)0);
                }


                //part 2: Jobservice
                on = exportSettings.filterByActivity.isfilterEnabled_JobServiceActivity;
                node = treelist_activity.Nodes.Add(category, on ? imgOn : imgOff);
                joblst.Push(new Tuple<TreeListNode, bool>(node, on));

                category = "by Component";
                on = exportSettings.filterByActivity.isfilterEnabled_JobServiceActivity_ByComponent;
                var cNode = treelist_activity.Nodes[1].Nodes.Add($"{category} ({myDataStore.jobserviceActivities.distinctTaskfull.Count}", on ? imgOn : imgOff);
                joblst.Push(new Tuple<TreeListNode, bool>(cNode, on));

                category = "by Queue";
                on = exportSettings.filterByActivity.isfilterEnabled_JobServiceActivity_ByQueue;
                var dNode = treelist_activity.Nodes[1].Nodes.Add($"{category} ({myDataStore.jobserviceActivities.distinctQueuename.Count})", on ? imgOn : imgOff);
                joblst.Push(new Tuple<TreeListNode, bool>(dNode, on));

                if (myDataStore.jobserviceActivities.jobserviceJobs.Count > 0)
                {
                    foreach (var task in myDataStore.jobserviceActivities.jobserviceJobs)
                    {   
                        on = exportSettings.filterByActivity.filterJobServiceActivity_ByComponentLst.Contains(task.taskfull);
                        cNode.Nodes.Add(task.taskfull, on ? imgOn : imgOff);
                        
                        on = exportSettings.filterByActivity.filterJobServiceActivity_ByQueueLst.Contains(task.queuename);
                        dNode.Nodes.Add(task.queuename, on ? imgOn : imgOff);
                    }
                }

                //change gui
                while (joblst.Count > 0)
                {
                    var job = joblst.Pop();
                    SwitchNodeState(job.Item1, job.Item2 ? (sbyte)1 : (sbyte)0);
                }
            } //update helper

            treelist_activity.Nodes.Expand(true);
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
                    Text = "include messages later than Start Time:",
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
                    Text = "include messages earlier than End Time:",
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

                System.Windows.Forms.CheckBox cb;

                int colY = 2;
                tableLayoutPanel_FilterByLogProperties.Controls.Add(l3, 0, colY);

                System.Windows.Forms.Button bl3 = new System.Windows.Forms.Button()
                {
                    Text = "toggle log level switches",
                    TextAlign = ContentAlignment.MiddleLeft,
                    Anchor = AnchorStyles.Left | AnchorStyles.Right
                };
                tableLayoutPanel_FilterByLogProperties.Controls.Add(bl3, 1, colY++);
                tableLayoutPanel_FilterByLogProperties.RowStyles.Add(new RowStyle(SizeType.AutoSize, 40F));
                tableLayoutPanel_FilterByLogProperties.RowCount++;


                List<System.Windows.Forms.CheckBox> cbLst_LogLevelBoxes = new List<System.Windows.Forms.CheckBox>();

                foreach (byte iLevel in Enumerable.Range(Loglevel.FewestDetailedLevel, Loglevel.MostDetailedLevel))
                {
                    Loglevels Llevel = Loglevel.ConvertFromNumberToEnum(iLevel);
                    string sLevel = Loglevel.ConvertFromEnumToString(Llevel);

                    cb = new System.Windows.Forms.CheckBox()
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

                bl3.Click += new EventHandler((object o, EventArgs args) =>
                {
                    foreach (var ckb in cbLst_LogLevelBoxes)
                        ckb.Checked = !ckb.Checked;
                });



                //log source
                Label l4 = new Label()
                {
                    Text = "Log sources (modules, components)",
                    Anchor = AnchorStyles.Left | AnchorStyles.Right
                };
                l4.Font = new Font(l4.Font, FontStyle.Bold | FontStyle.Underline);

                tableLayoutPanel_FilterByLogProperties.Controls.Add(l4, 0, colY);

                System.Windows.Forms.Button bl4 = new System.Windows.Forms.Button()
                {
                    Text = "toggle log sources switches",
                    TextAlign = ContentAlignment.MiddleLeft,
                    Anchor = AnchorStyles.Left | AnchorStyles.Right
                };
                tableLayoutPanel_FilterByLogProperties.Controls.Add(bl4, 1, colY++);
                tableLayoutPanel_FilterByLogProperties.RowStyles.Add(new RowStyle(SizeType.AutoSize, 40F));
                tableLayoutPanel_FilterByLogProperties.RowCount++;

                List<System.Windows.Forms.CheckBox> cbLst_LogSourceBoxes = new List<System.Windows.Forms.CheckBox>();

                Dictionary<string, long> data = Constants.logSourcesOfInterest.Select(t => new KeyValuePair<string, long>(t, 0)).ToDictionary(t => t.Key, v => v.Value, StringComparer.InvariantCultureIgnoreCase);
                foreach (var elem in myDataStore.generalLogData.numberOflogSources)
                    data.AddOrUpdate(elem.Key, elem.Value);

                foreach (var kp in data.OrderBy(t => t.Key))
                {
                    cb = new System.Windows.Forms.CheckBox()
                    {
                        AutoSize = true,
                        Name = $"cb_LogSource {kp.Key.ToString()}",
                        TextAlign = ContentAlignment.MiddleLeft,
                        Checked = exportSettings.filterByLogtype.logSourceFilters.GetBoolOrAdd(kp.Key, true),
                        Text = string.Format("Log message source: {0} ({1})", kp.Key, kp.Value)
                    };

                    tableLayoutPanel_FilterByLogProperties.Controls.Add(cb, 1, colY++);
                    tableLayoutPanel_FilterByLogProperties.RowStyles.Add(new RowStyle(SizeType.AutoSize, 40F));
                    tableLayoutPanel_FilterByLogProperties.RowCount++;

                    cbLst_LogSourceBoxes.Add(cb);
                }

                bl4.Click += new EventHandler((object o, EventArgs args) =>
                {
                    foreach (var ckb in cbLst_LogSourceBoxes)
                        ckb.Checked = !ckb.Checked;
                });
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
            using (new VI.FormBase.UpdateHelper(treelist_regexFilters))
            {
                treelist_regexFilters.Nodes.Clear();
                TreeListNode node;                

                foreach (var filter in exportSettings.filterByRegex.rxFilters)
                {
                    node = treelist_regexFilters.Nodes.Add("", filter.enabled ? (int)VI.ImageLibrary.StockImage.AssignedDirect : (int)VI.ImageLibrary.StockImage.AssignedNone);
                    node.SubItems.Add(new TreeListItemTextBox(filter.regexText));
                    node.SubItems.Add(new TreeListItemCheckBox(filter.ignoreCase));
                    node.SubItems.Add(new TreeListItemCheckBox(filter.isMatch));                    
                    node.SubItems.Add(new TreeListItemComboBox(new string[] { "before other filters", "after other filters" }, filter.isAppliedAtStart ? 0 : 1));
                }
            }
        }

        /// <summary>
        /// get data from gui and finally put it into exportsetting object; also calls the exportsetting.prepare method to consolidate everything to be ready for usage
        /// </summary>
        private void GetExportSettingFromGui()
        {
            //input files
            exportSettings.inputOutputOptions.includeFiles.Clear();
            foreach (var node in treelist_Inputfiles.Nodes.Where(t => isNodeEnabled(t)))
                exportSettings.inputOutputOptions.includeFiles.Add(node.SubItems[0].Data.ToString());

            exportSettings.inputOutputOptions.includeFileType_JSLog = checkBox_inputOpt_jobservice.Checked;
            exportSettings.inputOutputOptions.includeFileType_NLog = checkBox_inputOpt_nlog.Checked;
            

            //output file options
            exportSettings.inputOutputOptions.filenamePostfix = textBox_postfix.Text;
            exportSettings.inputOutputOptions.outputFolder = textBox_destFolder.Text;
            exportSettings.inputOutputOptions.mergeFiles = checkBox_mergeFiles.Checked;


            //Filter By Activity
            //projections activities
            exportSettings.filterByActivity.isfilterEnabled_ProjectionActivity = isNodeEnabled(treelist_activity.Nodes[0]);
            exportSettings.filterByActivity.isfilterEnabled_ProjectionActivity_Projections = isNodeEnabled(treelist_activity.Nodes[0].Nodes[0]);
            exportSettings.filterByActivity.isfilterEnabled_ProjectionActivity_Projections_AdHoc = isNodeEnabled(treelist_activity.Nodes[0].Nodes[0].Nodes[0]);
            exportSettings.filterByActivity.isfilterEnabled_ProjectionActivity_Projections_Sync = isNodeEnabled(treelist_activity.Nodes[0].Nodes[0].Nodes[1]);

            exportSettings.filterByActivity.filterProjectionActivity_Projections_AdHocLst.Clear();
            foreach (var subnode in treelist_activity.Nodes[0].Nodes[0].Nodes[0].Nodes.Where(t => isNodeEnabled(t)))
                exportSettings.filterByActivity.filterProjectionActivity_Projections_AdHocLst.Add(subnode.Data.ToString());

            exportSettings.filterByActivity.filterProjectionActivity_Projections_SyncLst.Clear();
            foreach (var subnode in treelist_activity.Nodes[0].Nodes[0].Nodes[1].Nodes.Where(t => isNodeEnabled(t)))
                exportSettings.filterByActivity.filterProjectionActivity_Projections_SyncLst.Add(subnode.Data.ToString());

            //jobservice activities
            exportSettings.filterByActivity.isfilterEnabled_JobServiceActivity = isNodeEnabled(treelist_activity.Nodes[1]);
            exportSettings.filterByActivity.isfilterEnabled_JobServiceActivity_ByComponent = isNodeEnabled(treelist_activity.Nodes[1].Nodes[0]);
            foreach (var subnode in treelist_activity.Nodes[1].Nodes[0].Nodes.Where(t => isNodeEnabled(t)))
                exportSettings.filterByActivity.filterJobServiceActivity_ByComponentLst.Add(subnode.Caption);

            exportSettings.filterByActivity.isfilterEnabled_JobServiceActivity_ByQueue = isNodeEnabled(treelist_activity.Nodes[1].Nodes[1]);
            foreach (var subnode in treelist_activity.Nodes[1].Nodes[1].Nodes.Where(t => isNodeEnabled(t)))
                exportSettings.filterByActivity.filterJobServiceActivity_ByQueueLst.Add(subnode.Caption);


            //Filter by Log type
            //Dates
            exportSettings.filterByLogtype.startDate = ((DateTimePicker)tableLayoutPanel_FilterByLogProperties.Controls["DateTimePicker_startDate"]).Value;
            exportSettings.filterByLogtype.endDate = ((DateTimePicker)tableLayoutPanel_FilterByLogProperties.Controls["DateTimePicker_endDate"]).Value;


            //Log level
            foreach (byte iLevel in Enumerable.Range(Loglevel.FewestDetailedLevel, Loglevel.MostDetailedLevel))
            {
                Loglevels Llevel = Loglevel.ConvertFromNumberToEnum(iLevel);
                string sLevel = Loglevel.ConvertFromEnumToString(Llevel);

                exportSettings.filterByLogtype.logLevelFilters[Llevel] = ((System.Windows.Forms.CheckBox)tableLayoutPanel_FilterByLogProperties.Controls[$"cb_LogLevel {sLevel}"]).Checked;
            }


            //log source
            foreach (Control ctl in tableLayoutPanel_FilterByLogProperties.Controls)
            {
                if (!ctl.Name.StartsWith("cb_LogSource "))
                    continue;

                string source = ctl.Name.Substring("cb_LogSource ".Length);

                exportSettings.filterByLogtype.logSourceFilters.AddOrUpdate(source, ((System.Windows.Forms.CheckBox)ctl).Checked);
            }


            //Filter by Regex
            for (int i = 0; i < exportSettings.filterByRegex.rxFilters.Count; i++)
            {
                exportSettings.filterByRegex.rxFilters[i].enabled = isNodeEnabled(treelist_regexFilters.Nodes[i]);
                exportSettings.filterByRegex.rxFilters[i].regexText = ((TreeListItemTextBox)treelist_regexFilters.Nodes[i].SubItems[0]).Caption;
                exportSettings.filterByRegex.rxFilters[i].ignoreCase = ((TreeListItemCheckBox)treelist_regexFilters.Nodes[i].SubItems[1]).Checked;
                exportSettings.filterByRegex.rxFilters[i].isMatch = ((TreeListItemCheckBox)treelist_regexFilters.Nodes[i].SubItems[2]).Checked;
                exportSettings.filterByRegex.rxFilters[i].isAppliedAtStart= ((TreeListItemComboBox)treelist_regexFilters.Nodes[i].SubItems[3]).SelectedIndex == 0;
            }

            exportSettings.PrepareForFilterAndExport();
        }


        private void buttonCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void button_Export_Click(object sender, EventArgs e)
        {
            GetExportSettingFromGui();

            DialogResult = DialogResult.OK;
            Close();
        }

        private void button_Newprofile_Click(object sender, EventArgs e)
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

        private void button_delprofile_Click(object sender, EventArgs e)
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

      
        
    }
}

