namespace LogfileMetaAnalyser
{
    partial class MainForm
    {
        /// <summary>
        /// Erforderliche Designervariable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Verwendete Ressourcen bereinigen.
        /// </summary>
        /// <param name="disposing">True, wenn verwaltete Ressourcen gelöscht werden sollen; andernfalls False.</param>
        protected override void Dispose(bool disposing)
        {
            _activeReader?.Dispose();
            _activeReader = null;

            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Vom Windows Form-Designer generierter Code

        /// <summary>
        /// Erforderliche Methode für die Designerunterstützung.
        /// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.loadLogsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exportToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.filterLogfilesToScopeTheImportantStuffToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.infoToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.debugDatastoreToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.toolStripProgressBar1 = new System.Windows.Forms.ToolStripProgressBar();
            this.toolStripStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.splitContainerOutVert = new System.Windows.Forms.SplitContainer();
            this.treeViewLeft = new System.Windows.Forms.TreeView();
            this.imageListForTreeview = new System.Windows.Forms.ImageList(this.components);
            this.splitContainerRightIn = new System.Windows.Forms.SplitContainer();
            this.folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.menuStrip1.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerOutVert)).BeginInit();
            this.splitContainerOutVert.Panel1.SuspendLayout();
            this.splitContainerOutVert.Panel2.SuspendLayout();
            this.splitContainerOutVert.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerRightIn)).BeginInit();
            this.splitContainerRightIn.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.exportToolStripMenuItem,
            this.helpToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Padding = new System.Windows.Forms.Padding(7, 2, 0, 2);
            this.menuStrip1.Size = new System.Drawing.Size(1588, 24);
            this.menuStrip1.TabIndex = 0;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.loadLogsToolStripMenuItem,
            this.toolStripSeparator1,
            this.exitToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // loadLogsToolStripMenuItem
            // 
            this.loadLogsToolStripMenuItem.Name = "loadLogsToolStripMenuItem";
            this.loadLogsToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.loadLogsToolStripMenuItem.Text = "Load logs...";
            this.loadLogsToolStripMenuItem.Click += new System.EventHandler(this.loadLogsToolStripMenuItem_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(177, 6);
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.exitToolStripMenuItem.Text = "Exit";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
            // 
            // exportToolStripMenuItem
            // 
            this.exportToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.filterLogfilesToScopeTheImportantStuffToolStripMenuItem});
            this.exportToolStripMenuItem.Name = "exportToolStripMenuItem";
            this.exportToolStripMenuItem.Size = new System.Drawing.Size(53, 20);
            this.exportToolStripMenuItem.Text = "Export";
            // 
            // filterLogfilesToScopeTheImportantStuffToolStripMenuItem
            // 
            this.filterLogfilesToScopeTheImportantStuffToolStripMenuItem.Name = "filterLogfilesToScopeTheImportantStuffToolStripMenuItem";
            this.filterLogfilesToScopeTheImportantStuffToolStripMenuItem.Size = new System.Drawing.Size(295, 22);
            this.filterLogfilesToScopeTheImportantStuffToolStripMenuItem.Text = "Filter Logfiles to scope the important stuff";
            this.filterLogfilesToScopeTheImportantStuffToolStripMenuItem.Click += new System.EventHandler(this.filterLogfilesToScopeTheImportantStuffToolStripMenuItem_Click);
            // 
            // helpToolStripMenuItem
            // 
            this.helpToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.infoToolStripMenuItem,
            this.debugDatastoreToolStripMenuItem});
            this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            this.helpToolStripMenuItem.Size = new System.Drawing.Size(44, 20);
            this.helpToolStripMenuItem.Text = "Help";
            // 
            // infoToolStripMenuItem
            // 
            this.infoToolStripMenuItem.Name = "infoToolStripMenuItem";
            this.infoToolStripMenuItem.Size = new System.Drawing.Size(272, 22);
            this.infoToolStripMenuItem.Text = "Info";
            // 
            // debugDatastoreToolStripMenuItem
            // 
            this.debugDatastoreToolStripMenuItem.Name = "debugDatastoreToolStripMenuItem";
            this.debugDatastoreToolStripMenuItem.Size = new System.Drawing.Size(272, 22);
            this.debugDatastoreToolStripMenuItem.Text = "Export data store (for debug purpose)";
            this.debugDatastoreToolStripMenuItem.Click += new System.EventHandler(this.debugDatastoreToolStripMenuItem_Click);
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripProgressBar1,
            this.toolStripStatusLabel});
            this.statusStrip1.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.Flow;
            this.statusStrip1.Location = new System.Drawing.Point(0, 647);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Padding = new System.Windows.Forms.Padding(1, 0, 16, 0);
            this.statusStrip1.Size = new System.Drawing.Size(1588, 20);
            this.statusStrip1.TabIndex = 1;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // toolStripProgressBar1
            // 
            this.toolStripProgressBar1.Name = "toolStripProgressBar1";
            this.toolStripProgressBar1.Size = new System.Drawing.Size(292, 18);
            this.toolStripProgressBar1.Step = 1;
            this.toolStripProgressBar1.Visible = false;
            // 
            // toolStripStatusLabel
            // 
            this.toolStripStatusLabel.Name = "toolStripStatusLabel";
            this.toolStripStatusLabel.Size = new System.Drawing.Size(10, 15);
            this.toolStripStatusLabel.Text = " ";
            // 
            // splitContainerOutVert
            // 
            this.splitContainerOutVert.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainerOutVert.Location = new System.Drawing.Point(0, 24);
            this.splitContainerOutVert.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.splitContainerOutVert.Name = "splitContainerOutVert";
            // 
            // splitContainerOutVert.Panel1
            // 
            this.splitContainerOutVert.Panel1.Controls.Add(this.treeViewLeft);
            // 
            // splitContainerOutVert.Panel2
            // 
            this.splitContainerOutVert.Panel2.Controls.Add(this.splitContainerRightIn);
            this.splitContainerOutVert.Size = new System.Drawing.Size(1588, 623);
            this.splitContainerOutVert.SplitterDistance = 323;
            this.splitContainerOutVert.SplitterWidth = 5;
            this.splitContainerOutVert.TabIndex = 2;
            // 
            // treeViewLeft
            // 
            this.treeViewLeft.AllowDrop = true;
            this.treeViewLeft.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treeViewLeft.HideSelection = false;
            this.treeViewLeft.HotTracking = true;
            this.treeViewLeft.ImageIndex = 0;
            this.treeViewLeft.ImageList = this.imageListForTreeview;
            this.treeViewLeft.Location = new System.Drawing.Point(0, 0);
            this.treeViewLeft.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.treeViewLeft.Name = "treeViewLeft";
            this.treeViewLeft.SelectedImageIndex = 0;
            this.treeViewLeft.Size = new System.Drawing.Size(323, 623);
            this.treeViewLeft.TabIndex = 0;
            // 
            // imageListForTreeview
            // 
            this.imageListForTreeview.ColorDepth = System.Windows.Forms.ColorDepth.Depth8Bit;
            this.imageListForTreeview.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageListForTreeview.ImageStream")));
            this.imageListForTreeview.TransparentColor = System.Drawing.Color.Transparent;
            this.imageListForTreeview.Images.SetKeyName(0, "timetrace");
            this.imageListForTreeview.Images.SetKeyName(1, "information");
            this.imageListForTreeview.Images.SetKeyName(2, "report");
            this.imageListForTreeview.Images.SetKeyName(3, "warning");
            this.imageListForTreeview.Images.SetKeyName(4, "stats");
            this.imageListForTreeview.Images.SetKeyName(5, "error");
            this.imageListForTreeview.Images.SetKeyName(6, "activity");
            this.imageListForTreeview.Images.SetKeyName(7, "component");
            this.imageListForTreeview.Images.SetKeyName(8, "sync");
            this.imageListForTreeview.Images.SetKeyName(9, "job");
            // 
            // splitContainerRightIn
            // 
            this.splitContainerRightIn.BackColor = System.Drawing.SystemColors.Control;
            this.splitContainerRightIn.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.splitContainerRightIn.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainerRightIn.Location = new System.Drawing.Point(0, 0);
            this.splitContainerRightIn.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.splitContainerRightIn.Name = "splitContainerRightIn";
            this.splitContainerRightIn.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainerRightIn.Panel1
            // 
            this.splitContainerRightIn.Panel1.AutoScroll = true;
            this.splitContainerRightIn.Panel1.BackColor = System.Drawing.Color.White;
            // 
            // splitContainerRightIn.Panel2
            // 
            this.splitContainerRightIn.Panel2.BackColor = System.Drawing.Color.White;
            this.splitContainerRightIn.Size = new System.Drawing.Size(1260, 623);
            this.splitContainerRightIn.SplitterDistance = 410;
            this.splitContainerRightIn.SplitterWidth = 5;
            this.splitContainerRightIn.TabIndex = 0;
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "openFileDialog1";
            this.openFileDialog1.Filter = "Logfiles|*.log;*.txt|all files|*";
            this.openFileDialog1.Multiselect = true;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1588, 667);
            this.Controls.Add(this.splitContainerOutVert);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.menuStrip1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.menuStrip1;
            this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Log file meta analyzer - One Identity Manager";
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.splitContainerOutVert.Panel1.ResumeLayout(false);
            this.splitContainerOutVert.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerOutVert)).EndInit();
            this.splitContainerOutVert.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerRightIn)).EndInit();
            this.splitContainerRightIn.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripProgressBar toolStripProgressBar1;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel;
        private System.Windows.Forms.SplitContainer splitContainerOutVert;
        private System.Windows.Forms.SplitContainer splitContainerRightIn;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.TreeView treeViewLeft;
        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog1;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.ImageList imageListForTreeview;
        private System.Windows.Forms.ToolStripMenuItem infoToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem debugDatastoreToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exportToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem filterLogfilesToScopeTheImportantStuffToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem loadLogsToolStripMenuItem;
    }
}

