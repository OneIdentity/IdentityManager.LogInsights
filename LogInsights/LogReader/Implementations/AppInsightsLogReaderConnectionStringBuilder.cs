using System.Data.Common;

namespace LogInsights.LogReader
{
    public class AppInsightsLogReaderConnectionStringBuilder : DbConnectionStringBuilder
    {
        public AppInsightsLogReaderConnectionStringBuilder()
        {
        }

        public AppInsightsLogReaderConnectionStringBuilder(string connectionString)
        {
            ConnectionString = connectionString;
        }

        public string AppId
        {
            get => TryGetValue(nameof(AppId), out var ret) ? ret as string : string.Empty;
            set => this[nameof(AppId)] = value;
        }

        public string ApiKey
        {
            get => TryGetValue(nameof(ApiKey), out var ret) ? ret as string : string.Empty;
            set => this[nameof(ApiKey)] = value;
        }

        public string Query
        {
            get => TryGetValue(nameof(Query), out var ret) ? ret as string : string.Empty;
            set => this[nameof(Query)] = value;
        }

        public string TimeSpan
        {
            get => TryGetValue(nameof(TimeSpan), out var ret) ? ret as string : string.Empty;
            set => this[nameof(TimeSpan)] = value;
        }
    }
}