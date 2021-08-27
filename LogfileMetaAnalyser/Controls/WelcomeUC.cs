using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LogfileMetaAnalyser.Controls
{
    public partial class WelcomeUC : UserControl
    {
        public WelcomeUC()
        {
            InitializeComponent();
            this.Dock = DockStyle.Fill;
            label1.ForeColor = Color.FromArgb(5, 170, 219);
            label2.ForeColor = Color.FromArgb(5, 170, 219);
        }
    }
}
