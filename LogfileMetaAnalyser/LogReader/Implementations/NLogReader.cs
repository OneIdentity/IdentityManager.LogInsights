using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

using LogfileMetaAnalyser.Helpers;

namespace LogfileMetaAnalyser.LogReader
{
    public class NLogReader : LogReader
    {
        public NLogReader(string[] fileNames, Encoding encoding = null)
        {
            // check the parameters
            m_FileNames = fileNames ?? throw new ArgumentNullException(nameof(fileNames));
            m_Encoding = encoding ?? Encoding.Default;
        }

		public NLogReader(NLogReaderConnectionStringBuilder connectionString)
			: this(connectionString?.FileNames, connectionString?.Encoding)
		{}

        public NLogReader(string connectionString)
			: this(new NLogReaderConnectionStringBuilder(connectionString))
        {}

        protected override  async IAsyncEnumerable<LogEntry> OnReadAsync(CancellationToken ct)
        {
            StringBuilder sb = new StringBuilder(1024);

            // todo read order of files?
            foreach (var file in m_FileNames)
            {
                var regex = await _TryDetectFileFormatAsync(file).ConfigureAwait(false);
                if (regex == null) break; // todo log message or other user notification

                using var reader = new StreamReader(file, m_Encoding, true);

                sb.Length = 0;
                int lineNumber = 0;
                int entryNumber = 0;

                await foreach(var line in _ReadAsync(reader, ct).ConfigureAwait(false))
                {
                    string entry = null;
                    lineNumber++;

                    bool isStart = line == null || _IsLineStart(line);
                    if (!isStart)
                    {
                        sb.AppendLine(line);
                        continue;
                    }

                    entryNumber++;
                    entry = sb.ToString();
                    sb.Clear();

                    // the last one is <null>
                    if (line != null)
                        sb.AppendLine(line);

                    if (entry.Length == 0)
                        continue;

                    var match = Constants.regexMessageMetaDataNLogDefault.Match(entry);
                    if (!match.Success)
                        continue;

                    var spid = string.Format("{1}{0}", 
                        match.Groups["SID"].Value,
                        match.Groups["NSourceExt"].Value.Length > 0 ? match.Groups["NSourceExt"].Value : match.Groups["NSourceExt2"].Value.Trim());
                    
                    var logEntry = new LogEntry(new Locator(entryNumber, lineNumber, file), 
                        lineNumber.ToString(),
                        DateTime.TryParse(match.Groups["Timestamp"].Value, out var timeStamp) ? timeStamp : DateTime.MinValue,
                        _GetLogLevel(match.Groups["NLevel"].Value),
                        0,
                        match.Groups["Payload"].Value,
                        match.Groups["NSource"].Value, 
                        "",
                        match.Groups["PID"].Value,
                        spid);

                    yield return logEntry;
                }
            }
        }

        private async Task<Regex> _TryDetectFileFormatAsync(string file)
        {
            using (StreamReader reader = new StreamReader(file))
            {
                string line;
                int idx = 0;
                while ((line = await reader.ReadLineAsync().ConfigureAwait(false)) != null)
                {
                    if (Constants.regexMessageMetaDataNLogDefault.IsMatch(line))
                        return Constants.regexMessageMetaDataNLogDefault;

                    if (Constants.regexMessageMetaDataJobservice.IsMatch(line))
                        return Constants.regexMessageMetaDataJobservice;

                    if (idx++ == 32)
                        break;
                }
            }

            return null;
        }

        private LogLevel _GetLogLevel(string value)
        {
            if (value.Length == 0)
                return LogLevel.Critical;

            switch (value[0])
            {
                case 'D':
                case 'd':
                    return LogLevel.Debug;
                case 'T':
                case 't':
                    return LogLevel.Trace;
                case 'W':
                case 'w':
                    return LogLevel.Warn;
                case 'E':
                case 'e':
                    return LogLevel.Error;
                case 'C':
                case 'c':
                    return LogLevel.Critical;
                case 'I':
                case 'i':
                    return LogLevel.Info;
                default:
                    return LogLevel.Critical;
            }
        }


        private static async IAsyncEnumerable<string> _ReadAsync(StreamReader reader, CancellationToken ct)
        {
            string line;

            while ((line = await reader.ReadLineAsync()) != null)
                yield return line;

            // we have to add null in order to handle the last entry correctly
            yield return null;
        }


        private static bool _IsLineStart(string line)
        {
            return line.Length > 30
                   && line[4] == '-'
                   && line[7] == '-'
                   && m_LineStartRegex.IsMatch(line);
        }

        /// <summary>
        /// Gets a short display of the reader and it's data.
        /// </summary>
        public override string Display => $"NLog Reader - {m_FileNames.Length} files";

        private readonly string[] m_FileNames;
        private readonly Encoding m_Encoding;

        private static readonly Regex m_LineStartRegex = new (@"^\d\d\d\d-\d\d-\d\d\s+\d\d:\d\d:\d\d", RegexOptions.CultureInvariant | RegexOptions.Compiled);
    }
}
