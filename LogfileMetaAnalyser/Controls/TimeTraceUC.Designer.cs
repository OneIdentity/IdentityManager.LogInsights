namespace LogfileMetaAnalyser.Controls
{
    partial class TimeTraceUC
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
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Vom Komponenten-Designer generierter Code

        /// <summary> 
        /// Erforderliche Methode für die Designerunterstützung. 
        /// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
        /// </summary>
        private void InitializeComponent()
        {
            this.thePanel = new System.Windows.Forms.Panel();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.hScrollBar1 = new System.Windows.Forms.HScrollBar();
            this.vScrollBar1 = new System.Windows.Forms.VScrollBar();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.button_zoomIn = new System.Windows.Forms.Button();
            this.button_zoomAll = new System.Windows.Forms.Button();
            this.button_zoomOut = new System.Windows.Forms.Button();
            this.button_ShowAll = new System.Windows.Forms.Button();
            this.label_title = new System.Windows.Forms.Label();
            this.button_exportGraph = new System.Windows.Forms.Button();
            this.tableLayoutPanel1.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // thePanel
            // 
            this.thePanel.BackColor = System.Drawing.Color.White;
            this.thePanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.thePanel.Location = new System.Drawing.Point(3, 21);
            this.thePanel.Name = "thePanel";
            this.thePanel.Size = new System.Drawing.Size(536, 303);
            this.thePanel.TabIndex = 0;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 18F));
            this.tableLayoutPanel1.Controls.Add(this.thePanel, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.hScrollBar1, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.vScrollBar1, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel2, 0, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(1);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 3;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 18F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 99F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 18F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(560, 345);
            this.tableLayoutPanel1.TabIndex = 1;
            // 
            // hScrollBar1
            // 
            this.hScrollBar1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.hScrollBar1.Location = new System.Drawing.Point(0, 327);
            this.hScrollBar1.Name = "hScrollBar1";
            this.hScrollBar1.Size = new System.Drawing.Size(542, 18);
            this.hScrollBar1.TabIndex = 1;
            // 
            // vScrollBar1
            // 
            this.vScrollBar1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.vScrollBar1.Location = new System.Drawing.Point(542, 18);
            this.vScrollBar1.Name = "vScrollBar1";
            this.vScrollBar1.Size = new System.Drawing.Size(18, 309);
            this.vScrollBar1.TabIndex = 2;
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.ColumnCount = 6;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 45F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 45F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 45F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 45F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 45F));
            this.tableLayoutPanel2.Controls.Add(this.button_zoomIn, 3, 0);
            this.tableLayoutPanel2.Controls.Add(this.button_zoomAll, 2, 0);
            this.tableLayoutPanel2.Controls.Add(this.button_zoomOut, 1, 0);
            this.tableLayoutPanel2.Controls.Add(this.button_ShowAll, 4, 0);
            this.tableLayoutPanel2.Controls.Add(this.label_title, 0, 0);
            this.tableLayoutPanel2.Controls.Add(this.button_exportGraph, 5, 0);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel2.Margin = new System.Windows.Forms.Padding(0);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 1;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(542, 18);
            this.tableLayoutPanel2.TabIndex = 3;
            // 
            // button_zoomIn
            // 
            this.button_zoomIn.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button_zoomIn.Location = new System.Drawing.Point(407, 0);
            this.button_zoomIn.Margin = new System.Windows.Forms.Padding(0);
            this.button_zoomIn.Name = "button_zoomIn";
            this.button_zoomIn.Size = new System.Drawing.Size(45, 18);
            this.button_zoomIn.TabIndex = 0;
            this.button_zoomIn.Text = "+";
            this.button_zoomIn.UseVisualStyleBackColor = true;
            // 
            // button_zoomAll
            // 
            this.button_zoomAll.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button_zoomAll.Location = new System.Drawing.Point(362, 0);
            this.button_zoomAll.Margin = new System.Windows.Forms.Padding(0);
            this.button_zoomAll.Name = "button_zoomAll";
            this.button_zoomAll.Size = new System.Drawing.Size(45, 18);
            this.button_zoomAll.TabIndex = 2;
            this.button_zoomAll.Text = "<>";
            this.button_zoomAll.UseVisualStyleBackColor = true;
            // 
            // button_zoomOut
            // 
            this.button_zoomOut.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button_zoomOut.Location = new System.Drawing.Point(317, 0);
            this.button_zoomOut.Margin = new System.Windows.Forms.Padding(0);
            this.button_zoomOut.Name = "button_zoomOut";
            this.button_zoomOut.Size = new System.Drawing.Size(45, 18);
            this.button_zoomOut.TabIndex = 1;
            this.button_zoomOut.Text = "-";
            this.button_zoomOut.UseVisualStyleBackColor = true;
            // 
            // button_ShowAll
            // 
            this.button_ShowAll.Location = new System.Drawing.Point(452, 0);
            this.button_ShowAll.Margin = new System.Windows.Forms.Padding(0);
            this.button_ShowAll.Name = "button_ShowAll";
            this.button_ShowAll.Size = new System.Drawing.Size(45, 18);
            this.button_ShowAll.TabIndex = 3;
            this.button_ShowAll.Text = "*";
            this.button_ShowAll.UseVisualStyleBackColor = true;
            // 
            // label_title
            // 
            this.label_title.AutoSize = true;
            this.label_title.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label_title.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label_title.Location = new System.Drawing.Point(3, 0);
            this.label_title.Name = "label_title";
            this.label_title.Size = new System.Drawing.Size(311, 18);
            this.label_title.TabIndex = 4;
            this.label_title.Text = "label_title";
            this.label_title.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // button_exportGraph
            // 
            this.button_exportGraph.Font = new System.Drawing.Font("Microsoft Sans Serif", 6F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button_exportGraph.Location = new System.Drawing.Point(497, 0);
            this.button_exportGraph.Margin = new System.Windows.Forms.Padding(0);
            this.button_exportGraph.Name = "button_exportGraph";
            this.button_exportGraph.Size = new System.Drawing.Size(45, 18);
            this.button_exportGraph.TabIndex = 5;
            this.button_exportGraph.Text = "export";
            this.button_exportGraph.UseVisualStyleBackColor = true;
            // 
            // TimeTraceUC
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "TimeTraceUC";
            this.Size = new System.Drawing.Size(560, 345);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel thePanel;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.HScrollBar hScrollBar1;
        private System.Windows.Forms.VScrollBar vScrollBar1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.Button button_zoomIn;
        private System.Windows.Forms.Button button_zoomOut;
        private System.Windows.Forms.Button button_zoomAll;
        private System.Windows.Forms.Button button_ShowAll;
        private System.Windows.Forms.Label label_title;
        private System.Windows.Forms.Button button_exportGraph;
    }
}
