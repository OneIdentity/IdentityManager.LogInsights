using LogfileMetaAnalyser.Helpers;

using System;

using LogfileMetaAnalyser.LogReader;

using Microsoft.Win32;

using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace LogfileMetaAnalyser.Controls
{
    public partial class ApplicationInsightsUC : LogReaderControl
    {
        private const string AppInsightsAppId = nameof(AppInsightsAppId);
        private const string AppInsightsApiKey = nameof(AppInsightsApiKey);

        public ApplicationInsightsUC()
        {
            InitializeComponent();

            try
            {
                textAppID.Text = Config.GetProtected(AppInsightsAppId);
                textApiKey.Text = Config.GetProtected(AppInsightsApiKey);

                cmbTimeSpan.SelectedIndex = 0;

                dtTo.Value = DateTime.UtcNow;
                dtFrom.Value = dtTo.Value.AddHours(-1);
            }
            catch
            {
                // Ignore exceptions
            }
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);

            CheckValid();
        }

        protected override string GetConnectionString()
		{
			var csb = new AppInsightsLogReaderConnectionStringBuilder
				{
					AppId = textAppID.Text,
					ApiKey = textApiKey.Text,
					Query = textQuery.Text
				};

            if (cmbTimeSpan.SelectedIndex > 0)
            {
                csb.TimeSpan = GetTimeSpan();
            }

            return csb.ConnectionString;
        }

        private string GetTimeSpan()
        {
            DateTime dtFrom;
            DateTime dtTo = DateTime.UtcNow;

            /*
            "All",
            "Last 30 minutes",
            "Last hour",
            "Last 4 hours",
            "Last 12 hours",
            "Last 24 hours",
            "Last 2 days",
            "Last 7 days",
            "Custom"});
             */

            switch (cmbTimeSpan.SelectedIndex)
            {
                case 1: dtFrom = dtTo.AddMinutes(-30); break;
                case 2: dtFrom = dtTo.AddHours(-1); break;
                case 3: dtFrom = dtTo.AddHours(-4); break;
                case 4: dtFrom = dtTo.AddHours(-12); break;
                case 5: dtFrom = dtTo.AddHours(-24); break;
                case 6: dtFrom = dtTo.AddDays(-2); break;
                case 7: dtFrom = dtTo.AddDays(-7); break;
                case 8:
                    dtFrom = this.dtFrom.Value;
                    dtTo = this.dtTo.Value;
                    break;

                default: throw new Exception("Unknown time selection");
            }

            return dtFrom.ToString("O", CultureInfo.InvariantCulture) + "/" + dtTo.ToString("O", CultureInfo.InvariantCulture);
        }

        public override ILogReader ConnectToReader()
        {
            return new AppInsightsLogReader(ConnectionString);
        }

        private void text_TextChanged(object sender, EventArgs e)
        {
            CheckValid();
        }

        protected override bool OnCheckValid()
        {
            bool bValid = base.OnCheckValid();

            bValid &= !String.IsNullOrEmpty(textAppID.Text);

            bValid &= !String.IsNullOrEmpty(textApiKey.Text);

            bValid &= !String.IsNullOrEmpty(textQuery.Text);

            return bValid;
        }

        public override void StoreCredentials()
        {
            base.StoreCredentials();

            Config.PutProtected(AppInsightsAppId, textAppID.Text);
            Config.PutProtected(AppInsightsApiKey, textApiKey.Text);
        }

        private void cmbTimeSpan_SelectedIndexChanged(object sender, EventArgs e)
        {
            panelCustom.Visible = cmbTimeSpan.SelectedIndex == 8;
        }

        
    }
}
