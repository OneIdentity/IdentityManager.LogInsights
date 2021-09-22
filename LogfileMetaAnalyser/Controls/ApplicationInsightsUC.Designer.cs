
namespace LogfileMetaAnalyser.Controls
{
    partial class ApplicationInsightsUC
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
            this.lblAplicationID = new System.Windows.Forms.Label();
            this.textAppID = new System.Windows.Forms.TextBox();
            this.textApiKey = new System.Windows.Forms.TextBox();
            this.labelApiKey = new System.Windows.Forms.Label();
            this.labelQueury = new System.Windows.Forms.Label();
            this.panelTop = new System.Windows.Forms.Panel();
            this.lblHeader = new System.Windows.Forms.Label();
            this.textQuery = new System.Windows.Forms.TextBox();
            this.panelTop.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblAplicationID
            // 
            this.lblAplicationID.Location = new System.Drawing.Point(8, 52);
            this.lblAplicationID.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblAplicationID.Name = "lblAplicationID";
            this.lblAplicationID.Size = new System.Drawing.Size(399, 15);
            this.lblAplicationID.TabIndex = 0;
            this.lblAplicationID.Text = "Application ID";
            // 
            // textAppID
            // 
            this.textAppID.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textAppID.Location = new System.Drawing.Point(8, 70);
            this.textAppID.Margin = new System.Windows.Forms.Padding(2, 1, 2, 1);
            this.textAppID.Name = "textAppID";
            this.textAppID.Size = new System.Drawing.Size(480, 23);
            this.textAppID.TabIndex = 1;
            this.textAppID.TextChanged += new System.EventHandler(this.text_TextChanged);
            // 
            // textApiKey
            // 
            this.textApiKey.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textApiKey.Location = new System.Drawing.Point(8, 120);
            this.textApiKey.Margin = new System.Windows.Forms.Padding(2, 1, 2, 1);
            this.textApiKey.Name = "textApiKey";
            this.textApiKey.Size = new System.Drawing.Size(480, 23);
            this.textApiKey.TabIndex = 3;
            this.textApiKey.UseSystemPasswordChar = true;
            this.textApiKey.TextChanged += new System.EventHandler(this.text_TextChanged);
            // 
            // labelApiKey
            // 
            this.labelApiKey.Location = new System.Drawing.Point(8, 101);
            this.labelApiKey.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.labelApiKey.Name = "labelApiKey";
            this.labelApiKey.Size = new System.Drawing.Size(399, 15);
            this.labelApiKey.TabIndex = 2;
            this.labelApiKey.Text = "API Key";
            // 
            // labelQueury
            // 
            this.labelQueury.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.labelQueury.Location = new System.Drawing.Point(8, 154);
            this.labelQueury.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.labelQueury.Name = "labelQueury";
            this.labelQueury.Size = new System.Drawing.Size(435, 17);
            this.labelQueury.TabIndex = 5;
            this.labelQueury.Text = "Query definition";
            // 
            // panelTop
            // 
            this.panelTop.BackColor = System.Drawing.Color.White;
            this.panelTop.Controls.Add(this.lblHeader);
            this.panelTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelTop.Location = new System.Drawing.Point(0, 0);
            this.panelTop.Name = "panelTop";
            this.panelTop.Size = new System.Drawing.Size(496, 44);
            this.panelTop.TabIndex = 8;
            // 
            // lblHeader
            // 
            this.lblHeader.AutoSize = true;
            this.lblHeader.Font = new System.Drawing.Font("Segoe UI", 18F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.lblHeader.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(5)))), ((int)(((byte)(170)))), ((int)(((byte)(219)))));
            this.lblHeader.Location = new System.Drawing.Point(5, 5);
            this.lblHeader.Name = "lblHeader";
            this.lblHeader.Size = new System.Drawing.Size(249, 32);
            this.lblHeader.TabIndex = 0;
            this.lblHeader.Text = "Application insights ";
            // 
            // textQuery
            // 
            this.textQuery.AcceptsReturn = true;
            this.textQuery.AcceptsTab = true;
            this.textQuery.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textQuery.Location = new System.Drawing.Point(8, 176);
            this.textQuery.Margin = new System.Windows.Forms.Padding(2, 1, 2, 1);
            this.textQuery.Multiline = true;
            this.textQuery.Name = "textQuery";
            this.textQuery.Size = new System.Drawing.Size(480, 101);
            this.textQuery.TabIndex = 6;
            this.textQuery.TextChanged += new System.EventHandler(this.text_TextChanged);
            // 
            // ApplicationInsightsUC
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.textQuery);
            this.Controls.Add(this.labelQueury);
            this.Controls.Add(this.panelTop);
            this.Controls.Add(this.textApiKey);
            this.Controls.Add(this.labelApiKey);
            this.Controls.Add(this.textAppID);
            this.Controls.Add(this.lblAplicationID);
            this.Margin = new System.Windows.Forms.Padding(2, 1, 2, 1);
            this.Name = "ApplicationInsightsUC";
            this.Size = new System.Drawing.Size(496, 292);
            this.panelTop.ResumeLayout(false);
            this.panelTop.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblAplicationID;
        private System.Windows.Forms.TextBox textAppID;
        private System.Windows.Forms.TextBox textApiKey;
        private System.Windows.Forms.Label labelApiKey;
        private System.Windows.Forms.Label labelQueury;
        private System.Windows.Forms.Panel panelTop;
        private System.Windows.Forms.Label lblHeader;
        private System.Windows.Forms.TextBox textQuery;
    }
}
