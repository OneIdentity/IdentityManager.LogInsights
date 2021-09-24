using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LogInsights.Controls
{
    public partial class WelcomeUC : UserControl
    {
        public event EventHandler StartAnalysis;

        public WelcomeUC()
        {
            InitializeComponent();

            this.Dock = DockStyle.Fill;
            label1.ForeColor = Color.FromArgb(5, 170, 219);
            labelHelp.ForeColor = Color.FromArgb(5, 170, 219);

        }

        private void labelHelp_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            StartAnalysis?.Invoke(this, EventArgs.Empty);
        }
    }
}
