using System;

using LogfileMetaAnalyser.LogReader;

using Windows.Security.Credentials;

namespace LogfileMetaAnalyser.Controls
{
    public partial class ApplicationInsightsUC : LogReaderControl
    {
        private const string VaultResource = "LogInsights_ApplicationInsights";

        public ApplicationInsightsUC()
        {
            InitializeComponent();

            try
            {
                var vault = new PasswordVault();
                var entry = _TryGetCredential(vault);

                if ( entry != null )
                {
                    entry.RetrievePassword();
                    textAppID.Text = entry.UserName;
                    textApiKey.Text = entry.Password;
                }

                CheckValid();
            }
            catch
            {
                // Ignore exceptions
            }
        }

        protected override string GetConnectionString()
		{
			var csb = new AppInsightsLogReaderConnectionStringBuilder
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

        private void text_TextChanged(object sender, EventArgs e)
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

        public override void StoreCredentials()
        {
            base.StoreCredentials();

            var vault = new PasswordVault();
            var entry = _TryGetCredential(vault);

            if ( entry != null )
            {
                entry.UserName = textAppID.Text;
                entry.Password = textApiKey.Text;

                entry.RetrievePassword();
            }
            else
            {
                entry = new PasswordCredential(VaultResource, textAppID.Text, textApiKey.Text);
                vault.Add(entry);
            }
        }

        private static PasswordCredential _TryGetCredential(PasswordVault vault)
        {
            try
            {
                var entries = vault.FindAllByResource(VaultResource);
                return entries.Count > 0 ? entries[0] : null;
            }
            catch
            {
                return null;
            }
        }
    }
}
