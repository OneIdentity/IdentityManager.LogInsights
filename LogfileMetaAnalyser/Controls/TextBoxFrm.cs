using LogfileMetaAnalyser.ExceptionHandling;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using LogfileMetaAnalyser.Helpers;

namespace LogfileMetaAnalyser.Controls
{
    public partial class TextBoxFrm : Form
    {
        public TextBoxFrm()
        {
            InitializeComponent();
            Owner = Application.OpenForms[0];
            Size = new Size(500, 300);
            Location = new Point(10, 10);
            ShowIcon = false;
            ShowInTaskbar = false;
            MinimizeBox = false;
            StartPosition = FormStartPosition.CenterScreen;

            textBox1.Multiline = true;
            textBox1.WordWrap = false;
            textBox1.ReadOnly = true;
            textBox1.ScrollBars = ScrollBars.Both;
            textBox1.Dock = DockStyle.Fill;
            textBox1.Font = new Font("Consolas", 10); 
            textBox1.SelectedText = "";
            textBox1.Select(0, 0);

            checkBox1.Checked = textBox1.WordWrap;
            checkBox1.CheckedChanged += new EventHandler((object o, EventArgs e) => {
                textBox1.WordWrap = checkBox1.Checked;
            });

            SetupLabel("");

            button1.Focus();
        }

        public void SetupLabel(string label)
        {
            Text = label;
            button1.Focus();
        }

        public void SetupData(string dataText)
        {
            textBox1.Text = dataText;
            textBox1.SelectedText = "";
            textBox1.Select(0, 0);

            button1.Focus();
            button2.Text = $"copy all ({(1f*dataText.Length/1024f).Int()} KB)";
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                if (!string.IsNullOrEmpty(textBox1.Text))
                    Clipboard.SetText(textBox1.Text);
            }
            catch (Exception exception)
            {
                ExceptionHandler.Instance.HandleException(exception);
            }
        }
    }
}
