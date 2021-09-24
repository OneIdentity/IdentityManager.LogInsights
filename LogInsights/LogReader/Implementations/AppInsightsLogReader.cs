using LogInsights.Helpers;
using Microsoft.Azure.ApplicationInsights.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading;

// ReSharper disable UnusedMember.Local

namespace LogInsights.LogReader
{
    public class AppInsightsLogReader : LogReader
    {
        private readonly ApplicationInsightsDataClient _client;
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


        public AppInsightsLogReader(string connectionString)
            : this(new AppInsightsLogReaderConnectionStringBuilder(connectionString))
        {
        }

        public AppInsightsLogReader(AppInsightsLogReaderConnectionStringBuilder connectionString)
        {
            _connString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));

            var appId = connectionString.AppId;
            var apiKey = connectionString.ApiKey;

            if ( string.IsNullOrEmpty(appId) )
                throw new Exception($"Missing {nameof(appId)} value.");
            if ( string.IsNullOrEmpty(apiKey) )
                throw new Exception($"Missing {nameof(apiKey)} value.");

            _client = new ApplicationInsightsDataClient(new ApiKeyClientCredentials(apiKey));
        }

        protected override void OnDispose()
        {
            _client.Dispose();
        }

        protected override async IAsyncEnumerable<LogEntry> OnReadAsync(
            [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            var events = new Events(_client);

            var result = await events.Client.Query.ExecuteAsync(
                    _connString.AppId,
                    _connString.Query,
                    !string.IsNullOrEmpty(_connString.TimeSpan) ? _connString.TimeSpan : null,
                    cancellationToken: cancellationToken)
                .ConfigureAwait(false);

            if ( result.Tables.Count < 1 )
                throw new Exception("Missing result table");

            var table = result.Tables[0];

            var timestampIdx = -1;
            var messageIdx = -1;
            var severityIdx = -1;
            var itemIdIdx = -1;
            var customDimensionsIdx = -1;
            var outerMessageIdx = -1;
            var detailsIdx = -1;

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

            if ( timestampIdx < 0 || messageIdx < 0 || itemIdIdx < 0 )
                throw new Exception("Missing columns");

            var entryNo = 1;
            var lineNo = 1;
            foreach (var row in table.Rows.OrderBy(r => (DateTime)r[timestampIdx]))
            {
                var locator = new Locator(entryNo++, lineNo, "AppInsights");

                var id = (string)row[itemIdIdx];
                var timeStamp = (DateTime)row[timestampIdx];
                var message = (string)row[messageIdx];
                var severity = severityIdx > -1 ? (long)row[severityIdx] : 1L;

                var logger = "";
                var appName = "";

                if ( customDimensionsIdx > 0 )
                {
                    try
                    {
                        var customDimensions = (string)row[customDimensionsIdx];
                        if ( !string.IsNullOrEmpty(customDimensions) )
                        {
                            var r = JsonSerializer.Deserialize<_CustomDimensions>(customDimensions, _jsonOptions);

                            if ( r != null )
                            {
                                logger = r.LoggerName;
                                appName = r.AppName;
                            }
                        }
                    }
                    catch
                    {
                        // Ignore exception
                    }
                }

                _ExceptionDetail[] exceptionDetails = null;
                if ( detailsIdx > -1 )
                {
                    try
                    {
                        var details = (string)row[detailsIdx];
                        if ( !string.IsNullOrEmpty(details) )
                        {
                            exceptionDetails = JsonSerializer.Deserialize<_ExceptionDetail[]>(details, _jsonOptions);
                        }
                    }
                    catch
                    {
                        // Ignore exception
                    }
                }

                if ( string.IsNullOrEmpty(message) && exceptionDetails != null && exceptionDetails.Length > 0 )
                    message = string.Join(Environment.NewLine, exceptionDetails.Select(d => d.Message));

                if ( string.IsNullOrEmpty(message) && outerMessageIdx > -1 )
                    message = (string)row[outerMessageIdx];

                lineNo += _NumberOfLines(message);

                yield return new LogEntry 
                {
                    Locator = locator,
                    Id = id,
                    TimeStamp = timeStamp,
                    Level = _LevelFromSeverity(severity),
                    Message = message,
                    Logger = logger,
                    AppName = appName
                };
            }
        }

        /// <summary>
        /// Gets a short display of the reader and it's data.
        /// </summary>
        public override string Display => $"App Insights Reader - {_client.BaseUri.OriginalString}";

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
