using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading;

using Microsoft.Azure.ApplicationInsights.Query;

namespace LogfileMetaAnalyser.LogReader
{
	public class AppInsightsLogReader : LogReader
	{
		private readonly ApplicationInsightsDataClient _client;
		private readonly AppInsightsLogReaderConnectionStringBuilder _connString;

		private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.General)
		{
			PropertyNameCaseInsensitive = true
		};

		private class _CustomDimensions
		{
			public string LoggerName { get; set; }
			public string AppName { get; set; }
		}

		public AppInsightsLogReader(string connectionString)
			: this(new AppInsightsLogReaderConnectionStringBuilder(connectionString))
		{ }

		public AppInsightsLogReader(AppInsightsLogReaderConnectionStringBuilder connectionString)
		{
			_connString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));

			var appId = connectionString.AppId;
			var apiKey = connectionString.ApiKey;

			if (string.IsNullOrEmpty(appId))
				throw new Exception($"Missing {nameof(appId)} value.");
			if (string.IsNullOrEmpty(apiKey))
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
					cancellationToken: cancellationToken)
				.ConfigureAwait(false);

			if (result.Tables.Count < 1)
				throw new Exception("Missing result table");

			var table = result.Tables[0];

			var timestampIdx = -1;
			var messageIdx = -1;
			var severityIdx = -1;
			var itemIdIdx = -1;
			var itemTypeIdx = -1;
			var customDimensionsIdx = -1;

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

					case "itemType":
						itemTypeIdx = i;
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
				}
			}

			if (timestampIdx < 0 || messageIdx < 0 || itemIdIdx < 0)
				throw new Exception("Missing columns");

			var pos = 0;
			foreach (var row in table.Rows)
			{
				var locator = new Locator(pos++, _connString.Query);

				var id = (string) row[itemIdIdx];
				var timeStamp = (DateTime) row[timestampIdx];
				var message = (string) row[messageIdx];
				var severity = severityIdx > -1 ? (long) row[severityIdx] : 0;
				var itemTypeStr = itemTypeIdx > -1 ? (string) row[itemTypeIdx] : null;

				if ( !Enum.TryParse(itemTypeStr, out LogEntryType type) )
					type = LogEntryType.Info;

				var logger = "";
				var appName = "";

				if ( customDimensionsIdx > 0 )
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

				yield return new LogEntry(locator, id, timeStamp, type, (int)severity, message, logger, appName);
			}
		}
	}
}
