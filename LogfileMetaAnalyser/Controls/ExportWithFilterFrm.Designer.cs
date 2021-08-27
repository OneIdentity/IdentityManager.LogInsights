namespace LogfileMetaAnalyser.Controls
{
    partial class ExportWithFilterFrm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            VI.Controls.TreeListColumn treeListColumn1 = new VI.Controls.TreeListColumn();
            VI.Controls.TreeListColumn treeListColumn2 = new VI.Controls.TreeListColumn();
            VI.Controls.TreeListColumn treeListColumn3 = new VI.Controls.TreeListColumn();
            VI.Controls.TreeListColumn treeListColumn4 = new VI.Controls.TreeListColumn();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ExportWithFilterFrm));
            VI.Controls.TreeListColumn treeListColumn5 = new VI.Controls.TreeListColumn();
            VI.Controls.TreeListColumn treeListColumn6 = new VI.Controls.TreeListColumn();
            VI.Controls.TreeListColumn treeListColumn7 = new VI.Controls.TreeListColumn();
            VI.Controls.TreeListColumn treeListColumn8 = new VI.Controls.TreeListColumn();
            VI.Controls.TreeListColumn treeListColumn9 = new VI.Controls.TreeListColumn();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.button_delprofile = new System.Windows.Forms.Button();
            this.button_saveProfile = new System.Windows.Forms.Button();
            this.comboBox_Profiles = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.tableLayoutPanel5 = new System.Windows.Forms.TableLayoutPanel();
            this.treelist_Inputfiles = new VI.Controls.TreeListControl();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.panel2 = new System.Windows.Forms.Panel();
            this.checkBox_inputOpt_nlog = new System.Windows.Forms.CheckBox();
            this.checkBox_inputOpt_jobservice = new System.Windows.Forms.CheckBox();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.panel1 = new System.Windows.Forms.Panel();
            this.tableLayoutPanel3 = new System.Windows.Forms.TableLayoutPanel();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.textBox_postfix = new System.Windows.Forms.TextBox();
            this.checkBox_mergeFiles = new System.Windows.Forms.CheckBox();
            this.textBox_destFolder = new System.Windows.Forms.TextBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.treelist_activity = new VI.Controls.TreeListControl();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel_FilterByLogProperties = new System.Windows.Forms.TableLayoutPanel();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.button_Export = new System.Windows.Forms.Button();
            this.button_Close = new System.Windows.Forms.Button();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel4 = new System.Windows.Forms.TableLayoutPanel();
            this.treelist_regexFilters = new VI.Controls.TreeListControl();
            this.tableLayoutPanel1.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tableLayoutPanel5.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.panel2.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.panel1.SuspendLayout();
            this.tableLayoutPanel3.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.flowLayoutPanel1.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.tableLayoutPanel4.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel2, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.tabControl1, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.groupBox1, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.groupBox2, 0, 3);
            this.tableLayoutPanel1.Controls.Add(this.flowLayoutPanel1, 0, 5);
            this.tableLayoutPanel1.Controls.Add(this.groupBox3, 0, 4);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 6;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 32F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 32F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(808, 720);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.ColumnCount = 4;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 65F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 45F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 45F));
            this.tableLayoutPanel2.Controls.Add(this.button_delprofile, 3, 0);
            this.tableLayoutPanel2.Controls.Add(this.button_saveProfile, 2, 0);
            this.tableLayoutPanel2.Controls.Add(this.comboBox_Profiles, 1, 0);
            this.tableLayoutPanel2.Controls.Add(this.label1, 0, 0);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel2.Margin = new System.Windows.Forms.Padding(0);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 1;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(808, 32);
            this.tableLayoutPanel2.TabIndex = 1;
            // 
            // button_delprofile
            // 
            this.button_delprofile.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.button_delprofile.Location = new System.Drawing.Point(766, 4);
            this.button_delprofile.Name = "button_delprofile";
            this.button_delprofile.Size = new System.Drawing.Size(39, 23);
            this.button_delprofile.TabIndex = 4;
            this.button_delprofile.Text = "del";
            this.button_delprofile.UseVisualStyleBackColor = true;
            this.button_delprofile.Click += new System.EventHandler(this.button_delprofile_Click);
            // 
            // button_saveProfile
            // 
            this.button_saveProfile.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.button_saveProfile.Location = new System.Drawing.Point(721, 4);
            this.button_saveProfile.Name = "button_saveProfile";
            this.button_saveProfile.Size = new System.Drawing.Size(39, 23);
            this.button_saveProfile.TabIndex = 3;
            this.button_saveProfile.Text = "save";
            this.button_saveProfile.UseVisualStyleBackColor = true;
            this.button_saveProfile.Click += new System.EventHandler(this.button_Newprofile_Click);
            // 
            // comboBox_Profiles
            // 
            this.comboBox_Profiles.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBox_Profiles.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox_Profiles.FormattingEnabled = true;
            this.comboBox_Profiles.Location = new System.Drawing.Point(68, 5);
            this.comboBox_Profiles.Name = "comboBox_Profiles";
            this.comboBox_Profiles.Size = new System.Drawing.Size(647, 21);
            this.comboBox_Profiles.TabIndex = 2;
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(59, 13);
            this.label1.TabIndex = 3;
            this.label1.Text = "Profile:";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tabControl1.Location = new System.Drawing.Point(3, 35);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(802, 158);
            this.tabControl1.TabIndex = 2;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.tableLayoutPanel5);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Margin = new System.Windows.Forms.Padding(0);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(794, 132);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "input files";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // tableLayoutPanel5
            // 
            this.tableLayoutPanel5.ColumnCount = 2;
            this.tableLayoutPanel5.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 67F));
            this.tableLayoutPanel5.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33F));
            this.tableLayoutPanel5.Controls.Add(this.treelist_Inputfiles, 0, 0);
            this.tableLayoutPanel5.Controls.Add(this.groupBox4, 1, 0);
            this.tableLayoutPanel5.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel5.Location = new System.Drawing.Point(3, 3);
            this.tableLayoutPanel5.Name = "tableLayoutPanel5";
            this.tableLayoutPanel5.RowCount = 1;
            this.tableLayoutPanel5.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel5.Size = new System.Drawing.Size(788, 126);
            this.tableLayoutPanel5.TabIndex = 1;
            // 
            // treelist_Inputfiles
            // 
            this.treelist_Inputfiles.AlternateNodeBackground = true;
            this.treelist_Inputfiles.Appearance = new VI.Controls.TreeListControlAppearance(true);
            this.treelist_Inputfiles.BackColor = System.Drawing.SystemColors.Window;
            treeListColumn1.BackColor = System.Drawing.SystemColors.Window;
            treeListColumn1.Width = 24D;
            treeListColumn2.BackColor = System.Drawing.SystemColors.Window;
            treeListColumn2.Caption = "File name";
            treeListColumn2.Width = 400D;
            treeListColumn3.BackColor = System.Drawing.SystemColors.Window;
            treeListColumn3.Caption = "File type";
            treeListColumn3.Width = 120D;
            this.treelist_Inputfiles.Columns.Add(treeListColumn1);
            this.treelist_Inputfiles.Columns.Add(treeListColumn2);
            this.treelist_Inputfiles.Columns.Add(treeListColumn3);
            this.treelist_Inputfiles.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treelist_Inputfiles.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.treelist_Inputfiles.Location = new System.Drawing.Point(0, 0);
            this.treelist_Inputfiles.Margin = new System.Windows.Forms.Padding(0);
            this.treelist_Inputfiles.Name = "treelist_Inputfiles";
            this.treelist_Inputfiles.ScrollPadding = new System.Windows.Forms.Padding(0);
            this.treelist_Inputfiles.ShowRootLines = false;
            this.treelist_Inputfiles.Size = new System.Drawing.Size(527, 126);
            this.treelist_Inputfiles.SubTextColor = System.Drawing.Color.Blue;
            this.treelist_Inputfiles.TabIndex = 0;
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.panel2);
            this.groupBox4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox4.Location = new System.Drawing.Point(530, 3);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(255, 120);
            this.groupBox4.TabIndex = 1;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Input options";
            // 
            // panel2
            // 
            this.panel2.AutoScroll = true;
            this.panel2.Controls.Add(this.checkBox_inputOpt_nlog);
            this.panel2.Controls.Add(this.checkBox_inputOpt_jobservice);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.panel2.Location = new System.Drawing.Point(3, 16);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(249, 101);
            this.panel2.TabIndex = 0;
            // 
            // checkBox_inputOpt_nlog
            // 
            this.checkBox_inputOpt_nlog.AutoSize = true;
            this.checkBox_inputOpt_nlog.Location = new System.Drawing.Point(6, 3);
            this.checkBox_inputOpt_nlog.Name = "checkBox_inputOpt_nlog";
            this.checkBox_inputOpt_nlog.Size = new System.Drawing.Size(92, 17);
            this.checkBox_inputOpt_nlog.TabIndex = 4;
            this.checkBox_inputOpt_nlog.Text = "NLog log type";
            this.checkBox_inputOpt_nlog.UseVisualStyleBackColor = true;
            // 
            // checkBox_inputOpt_jobservice
            // 
            this.checkBox_inputOpt_jobservice.AutoSize = true;
            this.checkBox_inputOpt_jobservice.Location = new System.Drawing.Point(6, 26);
            this.checkBox_inputOpt_jobservice.Name = "checkBox_inputOpt_jobservice";
            this.checkBox_inputOpt_jobservice.Size = new System.Drawing.Size(119, 17);
            this.checkBox_inputOpt_jobservice.TabIndex = 3;
            this.checkBox_inputOpt_jobservice.Text = "JobService log type";
            this.checkBox_inputOpt_jobservice.UseVisualStyleBackColor = true;
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.panel1);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(794, 132);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "output files";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // panel1
            // 
            this.panel1.AutoScroll = true;
            this.panel1.Controls.Add(this.tableLayoutPanel3);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(3, 3);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(788, 126);
            this.panel1.TabIndex = 0;
            // 
            // tableLayoutPanel3
            // 
            this.tableLayoutPanel3.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tableLayoutPanel3.ColumnCount = 2;
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 120F));
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel3.Controls.Add(this.label3, 0, 2);
            this.tableLayoutPanel3.Controls.Add(this.label2, 0, 0);
            this.tableLayoutPanel3.Controls.Add(this.textBox_postfix, 1, 0);
            this.tableLayoutPanel3.Controls.Add(this.checkBox_mergeFiles, 1, 1);
            this.tableLayoutPanel3.Controls.Add(this.textBox_destFolder, 1, 2);
            this.tableLayoutPanel3.Dock = System.Windows.Forms.DockStyle.Top;
            this.tableLayoutPanel3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tableLayoutPanel3.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel3.Name = "tableLayoutPanel3";
            this.tableLayoutPanel3.RowCount = 3;
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 32F));
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 32F));
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel3.Size = new System.Drawing.Size(788, 104);
            this.tableLayoutPanel3.TabIndex = 0;
            // 
            // label3
            // 
            this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(3, 77);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(114, 13);
            this.label3.TabIndex = 3;
            this.label3.Text = "file destination folder";
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(3, 9);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(114, 13);
            this.label2.TabIndex = 0;
            this.label2.Text = "file name post fix";
            // 
            // textBox_postfix
            // 
            this.textBox_postfix.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_postfix.Location = new System.Drawing.Point(123, 6);
            this.textBox_postfix.Name = "textBox_postfix";
            this.textBox_postfix.Size = new System.Drawing.Size(662, 20);
            this.textBox_postfix.TabIndex = 1;
            this.textBox_postfix.Text = "_filtered";
            // 
            // checkBox_mergeFiles
            // 
            this.checkBox_mergeFiles.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.checkBox_mergeFiles.AutoSize = true;
            this.checkBox_mergeFiles.Location = new System.Drawing.Point(123, 39);
            this.checkBox_mergeFiles.Name = "checkBox_mergeFiles";
            this.checkBox_mergeFiles.Size = new System.Drawing.Size(662, 17);
            this.checkBox_mergeFiles.TabIndex = 2;
            this.checkBox_mergeFiles.Text = "produce only one file (merge all messages)";
            this.checkBox_mergeFiles.UseVisualStyleBackColor = true;
            // 
            // textBox_destFolder
            // 
            this.textBox_destFolder.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_destFolder.Location = new System.Drawing.Point(123, 74);
            this.textBox_destFolder.Name = "textBox_destFolder";
            this.textBox_destFolder.Size = new System.Drawing.Size(662, 20);
            this.textBox_destFolder.TabIndex = 1;
            this.textBox_destFolder.Text = ".";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.treelist_activity);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox1.Location = new System.Drawing.Point(3, 199);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(802, 158);
            this.groupBox1.TabIndex = 3;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "filer by activity (job):";
            // 
            // treelist_activity
            // 
            this.treelist_activity.AlternateNodeBackground = true;
            this.treelist_activity.Appearance = new VI.Controls.TreeListControlAppearance(true);
            this.treelist_activity.BackColor = System.Drawing.SystemColors.Window;
            treeListColumn4.BackColor = System.Drawing.SystemColors.Window;
            treeListColumn4.Caption = "data";
            treeListColumn4.Width = 400D;
            this.treelist_activity.Columns.Add(treeListColumn4);
            this.treelist_activity.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treelist_activity.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.treelist_activity.Location = new System.Drawing.Point(3, 16);
            this.treelist_activity.Margin = new System.Windows.Forms.Padding(0);
            this.treelist_activity.Name = "treelist_activity";
            this.treelist_activity.ScrollPadding = new System.Windows.Forms.Padding(0);
            this.treelist_activity.ShowHeader = false;
            this.treelist_activity.Size = new System.Drawing.Size(796, 139);
            this.treelist_activity.SubTextColor = System.Drawing.Color.Blue;
            this.treelist_activity.TabIndex = 2;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.tableLayoutPanel_FilterByLogProperties);
            this.groupBox2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox2.Location = new System.Drawing.Point(3, 363);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(802, 158);
            this.groupBox2.TabIndex = 4;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "filter by log type, log source and log level:";
            // 
            // tableLayoutPanel_FilterByLogProperties
            // 
            this.tableLayoutPanel_FilterByLogProperties.AutoScroll = true;
            this.tableLayoutPanel_FilterByLogProperties.ColumnCount = 2;
            this.tableLayoutPanel_FilterByLogProperties.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel_FilterByLogProperties.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel_FilterByLogProperties.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel_FilterByLogProperties.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tableLayoutPanel_FilterByLogProperties.Location = new System.Drawing.Point(3, 16);
            this.tableLayoutPanel_FilterByLogProperties.Name = "tableLayoutPanel_FilterByLogProperties";
            this.tableLayoutPanel_FilterByLogProperties.Size = new System.Drawing.Size(796, 139);
            this.tableLayoutPanel_FilterByLogProperties.TabIndex = 0;
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.Controls.Add(this.button_Export);
            this.flowLayoutPanel1.Controls.Add(this.button_Close);
            this.flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutPanel1.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            this.flowLayoutPanel1.Location = new System.Drawing.Point(0, 688);
            this.flowLayoutPanel1.Margin = new System.Windows.Forms.Padding(0);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(808, 32);
            this.flowLayoutPanel1.TabIndex = 0;
            // 
            // button_Export
            // 
            this.button_Export.Image = ((System.Drawing.Image)(resources.GetObject("button_Export.Image")));
            this.button_Export.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.button_Export.Location = new System.Drawing.Point(673, 3);
            this.button_Export.Name = "button_Export";
            this.button_Export.Size = new System.Drawing.Size(132, 23);
            this.button_Export.TabIndex = 1;
            this.button_Export.Text = "Filter && Export";
            this.button_Export.UseVisualStyleBackColor = true;
            this.button_Export.Click += new System.EventHandler(this.button_Export_Click);
            // 
            // button_Close
            // 
            this.button_Close.Image = ((System.Drawing.Image)(resources.GetObject("button_Close.Image")));
            this.button_Close.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.button_Close.Location = new System.Drawing.Point(592, 3);
            this.button_Close.Name = "button_Close";
            this.button_Close.Size = new System.Drawing.Size(75, 23);
            this.button_Close.TabIndex = 2;
            this.button_Close.Text = "Close";
            this.button_Close.UseVisualStyleBackColor = true;
            this.button_Close.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.tableLayoutPanel4);
            this.groupBox3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox3.Location = new System.Drawing.Point(3, 527);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(802, 158);
            this.groupBox3.TabIndex = 5;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "filter by regex:";
            // 
            // tableLayoutPanel4
            // 
            this.tableLayoutPanel4.ColumnCount = 1;
            this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel4.Controls.Add(this.treelist_regexFilters, 0, 0);
            this.tableLayoutPanel4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel4.Location = new System.Drawing.Point(3, 16);
            this.tableLayoutPanel4.Name = "tableLayoutPanel4";
            this.tableLayoutPanel4.RowCount = 1;
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel4.Size = new System.Drawing.Size(796, 139);
            this.tableLayoutPanel4.TabIndex = 1;
            // 
            // treelist_regexFilters
            // 
            this.treelist_regexFilters.AlternateNodeBackground = true;
            this.treelist_regexFilters.Appearance = new VI.Controls.TreeListControlAppearance(true);
            this.treelist_regexFilters.BackColor = System.Drawing.SystemColors.Window;
            treeListColumn5.BackColor = System.Drawing.SystemColors.Window;
            treeListColumn5.Width = 24D;
            treeListColumn6.BackColor = System.Drawing.SystemColors.Window;
            treeListColumn6.Caption = "regex filter";
            treeListColumn6.Width = 270D;
            treeListColumn7.BackColor = System.Drawing.SystemColors.Window;
            treeListColumn7.Caption = "ignore case";
            treeListColumn7.CaptionAlign = System.Windows.Forms.HorizontalAlignment.Center;
            treeListColumn7.ContentAlign = System.Windows.Forms.HorizontalAlignment.Center;
            treeListColumn7.Width = 80D;
            treeListColumn8.BackColor = System.Drawing.SystemColors.Window;
            treeListColumn8.Caption = "must match (include)";
            treeListColumn8.CaptionAlign = System.Windows.Forms.HorizontalAlignment.Center;
            treeListColumn8.ContentAlign = System.Windows.Forms.HorizontalAlignment.Center;
            treeListColumn8.Width = 122D;
            treeListColumn9.BackColor = System.Drawing.SystemColors.Window;
            treeListColumn9.Caption = "apply when";
            treeListColumn9.Width = 115D;
            this.treelist_regexFilters.Columns.Add(treeListColumn5);
            this.treelist_regexFilters.Columns.Add(treeListColumn6);
            this.treelist_regexFilters.Columns.Add(treeListColumn7);
            this.treelist_regexFilters.Columns.Add(treeListColumn8);
            this.treelist_regexFilters.Columns.Add(treeListColumn9);
            this.treelist_regexFilters.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treelist_regexFilters.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.treelist_regexFilters.Location = new System.Drawing.Point(0, 0);
            this.treelist_regexFilters.Margin = new System.Windows.Forms.Padding(0);
            this.treelist_regexFilters.Name = "treelist_regexFilters";
            this.treelist_regexFilters.ScrollPadding = new System.Windows.Forms.Padding(0);
            this.treelist_regexFilters.ShowRootLines = false;
            this.treelist_regexFilters.Size = new System.Drawing.Size(796, 139);
            this.treelist_regexFilters.SubTextColor = System.Drawing.Color.Blue;
            this.treelist_regexFilters.TabIndex = 1;
            // 
            // ExportWithFilterFrm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(808, 720);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(400, 366);
            this.Name = "ExportWithFilterFrm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Filter and Export";
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel2.PerformLayout();
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tableLayoutPanel5.ResumeLayout(false);
            this.groupBox4.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.tabPage2.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.tableLayoutPanel3.ResumeLayout(false);
            this.tableLayoutPanel3.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.flowLayoutPanel1.ResumeLayout(false);
            this.groupBox3.ResumeLayout(false);
            this.tableLayoutPanel4.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.Button button_Export;
        private System.Windows.Forms.Button button_Close;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.Button button_saveProfile;
        private System.Windows.Forms.Button button_delprofile;
        private System.Windows.Forms.ComboBox comboBox_Profiles;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBox_postfix;
        private System.Windows.Forms.CheckBox checkBox_mergeFiles;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel4;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel_FilterByLogProperties;
        private VI.Controls.TreeListControl treelist_Inputfiles;
        private VI.Controls.TreeListControl treelist_regexFilters;
        private VI.Controls.TreeListControl treelist_activity;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel5;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.CheckBox checkBox_inputOpt_nlog;
        private System.Windows.Forms.CheckBox checkBox_inputOpt_jobservice;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox textBox_destFolder;
    }
}