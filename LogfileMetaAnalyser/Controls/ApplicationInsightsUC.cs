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
            DbConnectionStringBuilder csb = new DbConnectionStringBuilder();

            csb.Add("AppID", textAppID.Text);
            csb.Add("ApiKey", textApiKey.Text);

            return csb.ConnectionString;
        }

        public override ILogReader ConnectToReader()
        {
            return new AppInsightsLogReader(ConnectionString);
        }

        private void textAppID_TextChanged(object sender, EventArgs e)
        {
            CheckValid();
        }

        private void textApiKey_TextChanged(object sender, EventArgs e)
        {
            CheckValid();
        }

        protected override bool OnCheckValid()
        {
            bool bValid = base.OnCheckValid();

            bValid &= !String.IsNullOrEmpty(textAppID.Text);

            bValid &= !String.IsNullOrEmpty(textApiKey.Text);

            return bValid;
        }
    }
}
