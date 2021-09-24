using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LogInsights.Controls
{
    public partial class QuestionFrm : Form
    {
        public string QuestionDataText
        {
            get { return textBox1.Text; }
        }

        public QuestionFrm()
        {
            InitializeComponent();

            textBox1.TextChanged += new EventHandler((object o, EventArgs args) =>
               {
                   button_ok.Enabled = textBox1.Text != "";
               });
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        public void SetupLabel(string label)
        {
            this.label1.Text = label;
        }

        public void SetupData(string dataText)
        {
            textBox1.Text = dataText;
        }
    }
}
