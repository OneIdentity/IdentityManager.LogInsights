
namespace LogInsights.Controls
{
    partial class LogReaderForm
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(LogReaderForm));
            this.panelButtom = new System.Windows.Forms.Panel();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnOk = new System.Windows.Forms.Button();
            this.ctlLogFile = new LogInsights.Controls.LogFileUC();
            this.ctlAppInsights = new LogInsights.Controls.ApplicationInsightsUC();
            this.tabControl = new System.Windows.Forms.TabControl();
            this.tabPageLogFiles = new System.Windows.Forms.TabPage();
            this.tabPageAppInsights = new System.Windows.Forms.TabPage();
            this.imageList = new System.Windows.Forms.ImageList(this.components);
            this.panelButtom.SuspendLayout();
            this.tabControl.SuspendLayout();
            this.tabPageLogFiles.SuspendLayout();
            this.tabPageAppInsights.SuspendLayout();
            this.SuspendLayout();
            // 
            // panelButtom
            // 
            this.panelButtom.Controls.Add(this.btnCancel);
            this.panelButtom.Controls.Add(this.btnOk);
            this.panelButtom.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelButtom.Location = new System.Drawing.Point(0, 363);
            this.panelButtom.Name = "panelButtom";
            this.panelButtom.Size = new System.Drawing.Size(758, 53);
            this.panelButtom.TabIndex = 0;
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(632, 11);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(118, 30);
            this.btnCancel.TabIndex = 1;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // btnOk
            // 
            this.btnOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOk.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnOk.Location = new System.Drawing.Point(499, 11);
            this.btnOk.Name = "btnOk";
            this.btnOk.Size = new System.Drawing.Size(127, 30);
            this.btnOk.TabIndex = 0;
            this.btnOk.Text = "Ok";
            this.btnOk.UseVisualStyleBackColor = true;
            // 
            // ctlLogFile
            // 
            this.ctlLogFile.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ctlLogFile.IsValid = false;
            this.ctlLogFile.Location = new System.Drawing.Point(3, 3);
            this.ctlLogFile.Margin = new System.Windows.Forms.Padding(1);
            this.ctlLogFile.Name = "ctlLogFile";
            this.ctlLogFile.Size = new System.Drawing.Size(645, 312);
            this.ctlLogFile.TabIndex = 1;
            this.ctlLogFile.IsValidChanged += new System.EventHandler(this.SelectedProvider_IsValidChanged);
            // 
            // ctlAppInsights
            // 
            this.ctlAppInsights.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ctlAppInsights.IsValid = false;
            this.ctlAppInsights.Location = new System.Drawing.Point(3, 3);
            this.ctlAppInsights.Margin = new System.Windows.Forms.Padding(2, 1, 2, 1);
            this.ctlAppInsights.Name = "ctlAppInsights";
            this.ctlAppInsights.Size = new System.Drawing.Size(645, 349);
            this.ctlAppInsights.TabIndex = 0;
            this.ctlAppInsights.IsValidChanged += new System.EventHandler(this.SelectedProvider_IsValidChanged);
            // 
            // tabControl
            // 
            this.tabControl.Alignment = System.Windows.Forms.TabAlignment.Left;
            this.tabControl.Controls.Add(this.tabPageLogFiles);
            this.tabControl.Controls.Add(this.tabPageAppInsights);
            this.tabControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl.ImageList = this.imageList;
            this.tabControl.Location = new System.Drawing.Point(0, 0);
            this.tabControl.Multiline = true;
            this.tabControl.Name = "tabControl";
            this.tabControl.SelectedIndex = 0;
            this.tabControl.Size = new System.Drawing.Size(758, 363);
            this.tabControl.TabIndex = 4;
            this.tabControl.SelectedIndexChanged += new System.EventHandler(this.tabControl_SelectedIndexChanged);
            // 
            // tabPageLogFiles
            // 
            this.tabPageLogFiles.Controls.Add(this.ctlLogFile);
            this.tabPageLogFiles.ImageIndex = 0;
            this.tabPageLogFiles.Location = new System.Drawing.Point(103, 4);
            this.tabPageLogFiles.Name = "tabPageLogFiles";
            this.tabPageLogFiles.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageLogFiles.Size = new System.Drawing.Size(651, 318);
            this.tabPageLogFiles.TabIndex = 0;
            this.tabPageLogFiles.ToolTipText = "Log files";
            this.tabPageLogFiles.UseVisualStyleBackColor = true;
            // 
            // tabPageAppInsights
            // 
            this.tabPageAppInsights.Controls.Add(this.ctlAppInsights);
            this.tabPageAppInsights.ImageIndex = 1;
            this.tabPageAppInsights.Location = new System.Drawing.Point(103, 4);
            this.tabPageAppInsights.Name = "tabPageAppInsights";
            this.tabPageAppInsights.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageAppInsights.Size = new System.Drawing.Size(651, 355);
            this.tabPageAppInsights.TabIndex = 1;
            this.tabPageAppInsights.ToolTipText = "Application Insights";
            this.tabPageAppInsights.UseVisualStyleBackColor = true;
            // 
            // imageList
            // 
            this.imageList.ColorDepth = System.Windows.Forms.ColorDepth.Depth32Bit;
            this.imageList.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList.ImageStream")));
            this.imageList.TransparentColor = System.Drawing.Color.Transparent;
            this.imageList.Images.SetKeyName(0, "LogFilest.png");
            this.imageList.Images.SetKeyName(1, "ApplicationInsights.png");
            // 
            // LogReaderForm
            // 
            this.AcceptButton = this.btnOk;
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(758, 416);
            this.Controls.Add(this.tabControl);
            this.Controls.Add(this.panelButtom);
            this.MinimumSize = new System.Drawing.Size(610, 320);
            this.Name = "LogReaderForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Connect to log source";
            this.panelButtom.ResumeLayout(false);
            this.tabControl.ResumeLayout(false);
            this.tabPageLogFiles.ResumeLayout(false);
            this.tabPageAppInsights.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panelButtom;
        private System.Windows.Forms.Button btnOk;
        private System.Windows.Forms.Button btnCancel;
        private LogFileUC ctlLogFile;
        private ApplicationInsightsUC ctlAppInsights;
        private System.Windows.Forms.TabControl tabControl;
        private System.Windows.Forms.TabPage tabPageLogFiles;
        private System.Windows.Forms.TabPage tabPageAppInsights;
        private System.Windows.Forms.ImageList imageList;
    }
}