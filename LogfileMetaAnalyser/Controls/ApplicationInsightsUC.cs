using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using LogfileMetaAnalyser.LogReader;

namespace LogfileMetaAnalyser.Controls
{
    public partial class ApplicationInsightsUC : LogReaderControl
    {
        public ApplicationInsightsUC()
        {
            InitializeComponent();
        }

        protected override string GetConnectionString()
		{
			AppInsightsLogReaderConnectionStringBuilder csb = new AppInsightsLogReaderConnectionStringBuilder
				{
					AppId = textAppID.Text,
					ApiKey = textApiKey.Text,
					Query = textQuery.Text
				};

            return csb.ConnectionString;
        }

        public override ILogReader ConnectToReader()
        {
            return new AppInsightsLogReader(ConnectionString);
        }

        private void linkExtended_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            panelExtended.Visible = !panelExtended.Visible;
        }
    }
}
