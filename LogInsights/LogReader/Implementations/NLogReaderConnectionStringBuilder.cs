using System;
using System.Data.Common;
using System.Text;

namespace LogInsights.LogReader
{
    public class NLogReaderConnectionStringBuilder : DbConnectionStringBuilder
    {
        public NLogReaderConnectionStringBuilder()
        {
        }

        public NLogReaderConnectionStringBuilder(string connectionString)
        {
            ConnectionString = connectionString;
        }

        public string[] FileNames
        {
            get => TryGetValue(nameof(FileNames), out var tmp) && tmp is string fileNames
                ? fileNames.Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                : Array.Empty<string>();

            set => this[nameof(FileNames)] = value != null ? string.Join('|', value) : string.Empty;
        }

        public Encoding Encoding
        {
            get => TryGetValue(nameof(Encoding), out var tmp) && tmp is string encoding && !string.IsNullOrEmpty(encoding)
                ? Encoding.GetEncoding(encoding)
                : Encoding.UTF8;

            set => this[nameof(Encoding)] = value?.WebName ?? string.Empty;
        }
    }
}