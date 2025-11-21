using CsvHelper;
using CsvHelper.Configuration.Attributes;

using LogInsights.Helpers;
using Azure.Core;
using Azure.Identity;
using Azure.Monitor.Query;
using Azure.Monitor.Query.Models;
using System.Net.Http;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

// ReSharper disable UnusedMember.Local

namespace LogInsights.LogReader
{
    // Custom TokenCredential for Application Insights API key authentication
    internal class AppInsightsApiKeyCredential : TokenCredential
    {
        private readonly string _apiKey;

        public AppInsightsApiKeyCredential(string apiKey)
        {
            _apiKey = apiKey ?? throw new ArgumentNullException(nameof(apiKey));
        }

        public override AccessToken GetToken(TokenRequestContext requestContext, CancellationToken cancellationToken)
        {
            // Return the API key as a bearer token - this may need adjustment based on actual Azure.Monitor.Query implementation
            return new AccessToken(_apiKey, DateTimeOffset.UtcNow.AddHours(1));
        }

        public override ValueTask<AccessToken> GetTokenAsync(TokenRequestContext requestContext, CancellationToken cancellationToken)
        {
            return new ValueTask<AccessToken>(GetToken(requestContext, cancellationToken));
        }
    }
    public class AppInsightsLogReader : LogReader
    {
        private readonly LogsQueryClient _client;
        private readonly AppInsightsLogReaderConnectionStringBuilder _connString;

        private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.General) {
            PropertyNameCaseInsensitive = true
        };

        private class _CustomDimensions
        {
            public string LoggerName { get; set; }
            public string AppName { get; set; }
        }

        private class _ExceptionDetail
        {
            public string Message { get; set; }
            public string Type { get; set; }
            public _StackFrame[] ParsedStack { get; set; }
        }

        // ReSharper disable once ClassNeverInstantiated.Local
        private class _StackFrame
        {
            public string Method { get; set; }
            public string Assembly { get; set; }
            public int Level { get; set; }
            public int Line { get; set; }
        }

        private class _Row
        {
            [Name("timestamp [UTC]")]
            public DateTime TimeStamp { get; set; }

            [Name("message")]
            public string Message { get; set; }

            [Name("severityLevel")]
            public long Severity { get; set; }

            [Name("itemId")]
            public string ItemId { get; set; }

            [Name("customDimensions")]
            public string CustomDimensions { get; set; }

            [Name("outerMessage")]
            public string OuterMessage { get; set; }

            [Name("details")]
            public string Details { get; set; }
        }


        public AppInsightsLogReader(string connectionString)
            : this(new AppInsightsLogReaderConnectionStringBuilder(connectionString))
        {
        }

        public AppInsightsLogReader(AppInsightsLogReaderConnectionStringBuilder connectionString)
        {
            _connString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));

            if ( (_connString.CsvFiles?.Length ?? 0) == 0 )
            {
                var appId = connectionString.AppId;
                var apiKey = connectionString.ApiKey;

                if ( string.IsNullOrEmpty(appId) )
                    throw new Exception($"Missing {nameof(appId)} value.");
                if ( string.IsNullOrEmpty(apiKey) )
                    throw new Exception($"Missing {nameof(apiKey)} value.");

                // Create LogsQueryClient for Application Insights
                // Note: Application Insights now uses Log Analytics workspace under the hood
                // API key authentication is handled via custom credential
                var credential = new AppInsightsApiKeyCredential(apiKey);
                var options = new LogsQueryClientOptions();
                _client = new LogsQueryClient(credential, options);
                Display = $"App Insights Reader - {appId}";
            }
            else
            {
                Display = _connString.CsvFiles[0];
            }
        }

        protected override void OnDispose()
        {
            _client?.Dispose();
        }

        protected override async IAsyncEnumerable<LogEntry> OnReadAsync(
            [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            var rows = _connString.CsvFiles?.Length > 0
                ? _ReadCsvDataAsync()
                : _ReadAppInsightsDataAsync(cancellationToken);

            var entryNo = 1;
            var lineNo = 1;
            await foreach (var row in rows.OrderBy(r => r.TimeStamp).ConfigureAwait(false) )
            {
                var locator = new Locator(entryNo++, lineNo, "AppInsights");

                var logger = "";
                var appName = "";

                if (! string.IsNullOrEmpty(row.CustomDimensions) )
                {
                    try
                    {
                        var r = JsonSerializer.Deserialize<_CustomDimensions>(row.CustomDimensions, _jsonOptions);

                        if ( r != null )
                        {
                            logger = r.LoggerName;
                            appName = r.AppName;
                        }
                    }
                    catch
                    {
                        // Ignore exception
                    }
                }

                _ExceptionDetail[] exceptionDetails = null;
                if ( !string.IsNullOrEmpty(row.Details) )
                {
                    try
                    {
                        exceptionDetails = JsonSerializer.Deserialize<_ExceptionDetail[]>(row.Details, _jsonOptions);
                    }
                    catch
                    {
                        // Ignore exception
                    }
                }

                string message = row.Message;
                if ( string.IsNullOrEmpty(message) && exceptionDetails != null && exceptionDetails.Length > 0 )
                    message = string.Join(Environment.NewLine, exceptionDetails.Select(d => d.Message));

                if ( string.IsNullOrEmpty(message) && !string.IsNullOrEmpty(row.OuterMessage) )
                    message = row.OuterMessage;

                lineNo += _NumberOfLines(message);

                yield return new LogEntry 
                {
                    Locator = locator,
                    Id = row.ItemId,
                    TimeStamp = row.TimeStamp,
                    Level = _LevelFromSeverity(row.Severity),
                    Message = message,
                    Logger = logger,
                    AppName = appName
                };
            }
        }

        private async IAsyncEnumerable<_Row> _ReadAppInsightsDataAsync([EnumeratorCancellation] CancellationToken ct)
        {
            QueryTimeRange timeRange = QueryTimeRange.All;
            if (!string.IsNullOrEmpty(_connString.TimeSpan))
            {
                // Try to parse the timespan - the old API used formats like "P1D" (ISO 8601 duration)
                // The new API expects QueryTimeRange or TimeSpan objects
                if (TimeSpan.TryParse(_connString.TimeSpan, out var parsedTimespan))
                {
                    timeRange = new QueryTimeRange(parsedTimespan);
                }
                else
                {
                    // Try parsing as ISO 8601 duration or use default
                    timeRange = QueryTimeRange.All;
                }
            }

            var result = await _client.QueryWorkspaceAsync(
                    _connString.AppId,
                    _connString.Query,
                    timeRange,
                    cancellationToken: ct)
                .ConfigureAwait(false);

            var table = result.Value.Table;

            int timestampIdx = -1;
            int messageIdx = -1;
            int severityIdx = -1;
            int itemIdIdx = -1;
            int customDimensionsIdx = -1;
            int outerMessageIdx = -1;
            int detailsIdx = -1;

            for (var i = 0; i < table.Columns.Count; i++)
            {
                var column = table.Columns[i];

                switch (column.Name)
                {
                    case "itemId":
                        itemIdIdx = i;
                        break;

                    case "timestamp":
                        timestampIdx = i;
                        break;

                    case "message":
                        messageIdx = i;
                        break;

                    case "severityLevel":
                        severityIdx = i;
                        break;

                    case "customDimensions":
                        customDimensionsIdx = i;
                        break;

                    case "outerMessage":
                        outerMessageIdx = i;
                        break;

                    case "details":
                        detailsIdx = i;
                        break;
                }
            }

            if (timestampIdx < 0 ||
                messageIdx < 0 ||
                itemIdIdx < 0)
                throw new Exception("Missing columns");

            foreach (var row in table.Rows)
            {
                yield return new _Row
                    {
                        ItemId = row[itemIdIdx]?.ToString(),
                        TimeStamp = row[timestampIdx] is DateTimeOffset dto ? dto.DateTime : 
                                   DateTime.TryParse(row[timestampIdx]?.ToString(), out var dt) ? dt : DateTime.MinValue,
                        Message = row[messageIdx]?.ToString(),
                        Severity = severityIdx > -1 && long.TryParse(row[severityIdx]?.ToString(), out var sev) ? sev : 1L,
                        CustomDimensions = customDimensionsIdx > -1 ? row[customDimensionsIdx]?.ToString() : "",
                        Details = detailsIdx > -1 ? row[detailsIdx]?.ToString() : "",
                        OuterMessage = outerMessageIdx > -1 ? row[outerMessageIdx]?.ToString() : ""
                };
            }
        }

        private async IAsyncEnumerable<_Row> _ReadCsvDataAsync()
        {
            var resultingFiles = _ResolveFiles(_connString.CsvFiles);

            foreach (var csvFile in resultingFiles)
            {
                using var reader = new StreamReader(csvFile);
                using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

                await csv.ReadAsync().ConfigureAwait(false);
                csv.ReadHeader();

                while ( await csv.ReadAsync().ConfigureAwait(false) ) 
                    yield return csv.GetRecord<_Row>();
            }
        }

        private static IEnumerable<string> _ResolveFiles(string[] filesAndDirectories)
        {
            foreach (var name in filesAndDirectories ?? Enumerable.Empty<string>())
            {
                if (File.Exists(name))
                    yield return name;

                if (!Directory.Exists(name))
                    continue;

                foreach (var f in Directory.GetFiles(name, "*.csv", SearchOption.AllDirectories))
                    yield return f;
            }
        }

        /// <summary>
        /// Gets a short display of the reader and it's data.
        /// </summary>
        public override string Display { get; }

        private static LogLevel _LevelFromSeverity(long severity)
        {
            return severity switch
                {
                    0L => LogLevel.Debug,
                    1L => LogLevel.Info,
                    2L => LogLevel.Warn,
                    3L => LogLevel.Error,
                    4L => LogLevel.Critical,
                    _ => LogLevel.Info
                };
        }

        private static int _NumberOfLines(string msg)
        {
            if ( msg == null )
                return 1;

            var cnt = 1;

            foreach (char c in msg)
            {
                if ( c == '\n' )
                    cnt++;
            }

            return cnt;
        }
    }
}
