using System.Windows.Forms;

namespace LogfileMetaAnalyser.Controls
{
    partial class ContextLinesUC
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
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.label1 = new System.Windows.Forms.Label();
            this.syntaxEditor1 = new RichTextBox();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.buttonShowInEditor = new System.Windows.Forms.Button();
            this.buttonExport = new System.Windows.Forms.Button();
            this.comboBox_OpenInEditor = new System.Windows.Forms.ComboBox();
            this.button_MessagesJumpBack = new System.Windows.Forms.Button();
            this.button_MessagesJumpForward = new System.Windows.Forms.Button();
            this.tableLayoutPanel1.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.label1, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.syntaxEditor1, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel2, 0, 2);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 3;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(780, 438);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(3, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(41, 20);
            this.label1.TabIndex = 0;
            this.label1.Text = "label1";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // syntaxEditor1
            // 
            this.syntaxEditor1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.syntaxEditor1.HideSelection = true;
            this.syntaxEditor1.Location = new System.Drawing.Point(3, 23);
            this.syntaxEditor1.Name = "syntaxEditor1";
            this.syntaxEditor1.Size = new System.Drawing.Size(774, 387);
            this.syntaxEditor1.TabIndex = 1;
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.ColumnCount = 6;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 150F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 150F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 150F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 45F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 45F));
            this.tableLayoutPanel2.Controls.Add(this.buttonShowInEditor, 2, 0);
            this.tableLayoutPanel2.Controls.Add(this.buttonExport, 1, 0);
            this.tableLayoutPanel2.Controls.Add(this.comboBox_OpenInEditor, 3, 0);
            this.tableLayoutPanel2.Controls.Add(this.button_MessagesJumpBack, 4, 0);
            this.tableLayoutPanel2.Controls.Add(this.button_MessagesJumpForward, 5, 0);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(0, 413);
            this.tableLayoutPanel2.Margin = new System.Windows.Forms.Padding(0);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 1;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(780, 25);
            this.tableLayoutPanel2.TabIndex = 2;
            // 
            // buttonShowInEditor
            // 
            this.buttonShowInEditor.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonShowInEditor.Location = new System.Drawing.Point(406, 1);
            this.buttonShowInEditor.Margin = new System.Windows.Forms.Padding(1);
            this.buttonShowInEditor.Name = "buttonShowInEditor";
            this.buttonShowInEditor.Size = new System.Drawing.Size(133, 23);
            this.buttonShowInEditor.TabIndex = 0;
            this.buttonShowInEditor.Text = "&Open message in editor";
            this.buttonShowInEditor.UseVisualStyleBackColor = true;
            this.buttonShowInEditor.Click += new System.EventHandler(this.buttonShowInEditor_Click);
            // 
            // buttonExport
            // 
            this.buttonExport.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonExport.Location = new System.Drawing.Point(256, 1);
            this.buttonExport.Margin = new System.Windows.Forms.Padding(1);
            this.buttonExport.Name = "buttonExport";
            this.buttonExport.Size = new System.Drawing.Size(133, 23);
            this.buttonExport.TabIndex = 1;
            this.buttonExport.Text = "&Filter\'n\'Export";
            this.buttonExport.UseVisualStyleBackColor = true;
            this.buttonExport.Click += new System.EventHandler(this.buttonExport_Click);
            // 
            // comboBox_OpenInEditor
            // 
            this.comboBox_OpenInEditor.BackColor = System.Drawing.SystemColors.Control;
            this.comboBox_OpenInEditor.Dock = System.Windows.Forms.DockStyle.Fill;
            this.comboBox_OpenInEditor.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox_OpenInEditor.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.comboBox_OpenInEditor.FormattingEnabled = true;
            this.comboBox_OpenInEditor.Location = new System.Drawing.Point(543, 3);
            this.comboBox_OpenInEditor.Name = "comboBox_OpenInEditor";
            this.comboBox_OpenInEditor.Size = new System.Drawing.Size(144, 21);
            this.comboBox_OpenInEditor.TabIndex = 2;
            // 
            // button_MessagesUp
            // 
            this.button_MessagesJumpBack.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.button_MessagesJumpBack.Location = new System.Drawing.Point(691, 1);
            this.button_MessagesJumpBack.Margin = new System.Windows.Forms.Padding(1);
            this.button_MessagesJumpBack.Name = "button_MessagesUp";
            this.button_MessagesJumpBack.Size = new System.Drawing.Size(43, 23);
            this.button_MessagesJumpBack.TabIndex = 3;
            this.button_MessagesJumpBack.Text = "up";
            this.button_MessagesJumpBack.UseVisualStyleBackColor = true;
            this.button_MessagesJumpBack.Click += new System.EventHandler(this.button_MessagesUp_Click);
            // 
            // button_MessagesDn
            // 
            this.button_MessagesJumpForward.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.button_MessagesJumpForward.Location = new System.Drawing.Point(736, 1);
            this.button_MessagesJumpForward.Margin = new System.Windows.Forms.Padding(1);
            this.button_MessagesJumpForward.Name = "button_MessagesDn";
            this.button_MessagesJumpForward.Size = new System.Drawing.Size(43, 23);
            this.button_MessagesJumpForward.TabIndex = 4;
            this.button_MessagesJumpForward.Text = "dn";
            this.button_MessagesJumpForward.UseVisualStyleBackColor = true;
            this.button_MessagesJumpForward.Click += new System.EventHandler(this.button_MessagesDn_Click);
            // 
            // ContextLinesUC
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "ContextLinesUC";
            this.Size = new System.Drawing.Size(780, 438);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.tableLayoutPanel2.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.Button buttonShowInEditor;
        private System.Windows.Forms.Button buttonExport;
        private System.Windows.Forms.ComboBox comboBox_OpenInEditor;
        private RichTextBox syntaxEditor1;
        private System.Windows.Forms.Button button_MessagesJumpBack;
        private System.Windows.Forms.Button button_MessagesJumpForward;
    }
}
