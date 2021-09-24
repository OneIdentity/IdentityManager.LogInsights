
namespace LogInsights.Controls
{
    partial class LogFileUC
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(LogFileUC));
            this.lvLogFiles = new System.Windows.Forms.ListView();
            this.colFileName = new System.Windows.Forms.ColumnHeader();
            this.colFileSize = new System.Windows.Forms.ColumnHeader();
            this.colFileFrom = new System.Windows.Forms.ColumnHeader();
            this.colFileTo = new System.Windows.Forms.ColumnHeader();
            this.imageList = new System.Windows.Forms.ImageList(this.components);
            this.dlgOpen = new System.Windows.Forms.OpenFileDialog();
            this.toolStrip = new System.Windows.Forms.ToolStrip();
            this.tsbFiles = new System.Windows.Forms.ToolStripButton();
            this.tsbFolder = new System.Windows.Forms.ToolStripButton();
            this.tsbDelete = new System.Windows.Forms.ToolStripButton();
            this.lblHeader = new System.Windows.Forms.Label();
            this.panelTop = new System.Windows.Forms.Panel();
            this.dlgFolder = new System.Windows.Forms.FolderBrowserDialog();
            this.toolStrip.SuspendLayout();
            this.panelTop.SuspendLayout();
            this.SuspendLayout();
            // 
            // lvLogFiles
            // 
            this.lvLogFiles.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.lvLogFiles.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.colFileName,
            this.colFileSize,
            this.colFileFrom,
            this.colFileTo});
            this.lvLogFiles.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lvLogFiles.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.lvLogFiles.HideSelection = false;
            this.lvLogFiles.Location = new System.Drawing.Point(0, 69);
            this.lvLogFiles.Margin = new System.Windows.Forms.Padding(1);
            this.lvLogFiles.Name = "lvLogFiles";
            this.lvLogFiles.Size = new System.Drawing.Size(622, 201);
            this.lvLogFiles.SmallImageList = this.imageList;
            this.lvLogFiles.TabIndex = 0;
            this.lvLogFiles.UseCompatibleStateImageBehavior = false;
            this.lvLogFiles.UseWaitCursor = true;
            this.lvLogFiles.View = System.Windows.Forms.View.Details;
            this.lvLogFiles.SelectedIndexChanged += new System.EventHandler(this.lvLogFiles_SelectedIndexChanged);
            // 
            // colFileName
            // 
            this.colFileName.Text = "Logfile";
            this.colFileName.Width = 320;
            // 
            // colFileSize
            // 
            this.colFileSize.Text = "Size";
            this.colFileSize.Width = 80;
            // 
            // colFileFrom
            // 
            this.colFileFrom.Text = "Created";
            this.colFileFrom.Width = 120;
            // 
            // colFileTo
            // 
            this.colFileTo.Text = "Last write";
            this.colFileTo.Width = 120;
            // 
            // imageList
            // 
            this.imageList.ColorDepth = System.Windows.Forms.ColorDepth.Depth32Bit;
            this.imageList.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList.ImageStream")));
            this.imageList.TransparentColor = System.Drawing.Color.Transparent;
            this.imageList.Images.SetKeyName(0, "NewDocument 16 n p.png");
            this.imageList.Images.SetKeyName(1, "FolderOpen 16 n p.png");
            this.imageList.Images.SetKeyName(2, "DeleteDocument 16 n p.png");
            // 
            // dlgOpen
            // 
            this.dlgOpen.Filter = "log files|*.log|all files|*.*";
            this.dlgOpen.Multiselect = true;
            // 
            // toolStrip
            // 
            this.toolStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsbFiles,
            this.tsbFolder,
            this.tsbDelete});
            this.toolStrip.Location = new System.Drawing.Point(0, 44);
            this.toolStrip.Name = "toolStrip";
            this.toolStrip.Padding = new System.Windows.Forms.Padding(0);
            this.toolStrip.Size = new System.Drawing.Size(622, 25);
            this.toolStrip.TabIndex = 1;
            // 
            // tsbFiles
            // 
            this.tsbFiles.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbFiles.Name = "tsbFiles";
            this.tsbFiles.Size = new System.Drawing.Size(57, 22);
            this.tsbFiles.Text = "Add files";
            this.tsbFiles.Click += new System.EventHandler(this.tsbAdd_Click);
            // 
            // tsbFolder
            // 
            this.tsbFolder.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbFolder.Name = "tsbFolder";
            this.tsbFolder.Size = new System.Drawing.Size(67, 22);
            this.tsbFolder.Text = "Add folder";
            this.tsbFolder.Click += new System.EventHandler(this.tsbFolder_Click);
            // 
            // tsbDelete
            // 
            this.tsbDelete.Enabled = false;
            this.tsbDelete.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbDelete.Name = "tsbDelete";
            this.tsbDelete.Size = new System.Drawing.Size(54, 22);
            this.tsbDelete.Text = "Remove";
            this.tsbDelete.Click += new System.EventHandler(this.tsbDelete_Click);
            // 
            // lblHeader
            // 
            this.lblHeader.AutoSize = true;
            this.lblHeader.Font = new System.Drawing.Font("Segoe UI", 18F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.lblHeader.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(5)))), ((int)(((byte)(170)))), ((int)(((byte)(219)))));
            this.lblHeader.Location = new System.Drawing.Point(5, 5);
            this.lblHeader.Name = "lblHeader";
            this.lblHeader.Size = new System.Drawing.Size(110, 32);
            this.lblHeader.TabIndex = 0;
            this.lblHeader.Text = "Log files";
            // 
            // panelTop
            // 
            this.panelTop.BackColor = System.Drawing.Color.White;
            this.panelTop.Controls.Add(this.lblHeader);
            this.panelTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelTop.Location = new System.Drawing.Point(0, 0);
            this.panelTop.Name = "panelTop";
            this.panelTop.Size = new System.Drawing.Size(622, 44);
            this.panelTop.TabIndex = 2;
            // 
            // dlgFolder
            // 
            this.dlgFolder.ShowNewFolderButton = false;
            // 
            // LogFileUC
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.lvLogFiles);
            this.Controls.Add(this.toolStrip);
            this.Controls.Add(this.panelTop);
            this.Margin = new System.Windows.Forms.Padding(1);
            this.Name = "LogFileUC";
            this.Size = new System.Drawing.Size(622, 270);
            this.toolStrip.ResumeLayout(false);
            this.toolStrip.PerformLayout();
            this.panelTop.ResumeLayout(false);
            this.panelTop.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListView lvLogFiles;
        private System.Windows.Forms.ColumnHeader colFileName;
        private System.Windows.Forms.ColumnHeader colFileSize;
        private System.Windows.Forms.OpenFileDialog dlgOpen;
        private System.Windows.Forms.ToolStrip toolStrip;
        private System.Windows.Forms.ToolStripButton tsbFiles;
        private System.Windows.Forms.ToolStripButton tsbDelete;
        private System.Windows.Forms.Label lblHeader;
        private System.Windows.Forms.Panel panelTop;
        private System.Windows.Forms.ColumnHeader colFileFrom;
        private System.Windows.Forms.ColumnHeader colFileTo;
        private System.Windows.Forms.ToolStripButton tsbFolder;
        private System.Windows.Forms.FolderBrowserDialog dlgFolder;
        private System.Windows.Forms.ImageList imageList;
    }
}
