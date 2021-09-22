using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

using LogfileMetaAnalyser.Helpers;

namespace LogfileMetaAnalyser.LogReader
{
    public class NLogReader : LogReader
    {
        private enum LogFormat
        {
            Unknown,
            NLog,
            JobService
        }

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

        protected override  async IAsyncEnumerable<LogEntry> OnReadAsync([EnumeratorCancellation] CancellationToken ct)
        {
            StringBuilder sb = new StringBuilder(1024);

            // todo read order of files?
            foreach (var file in m_FileNames)
            {
                var logFormat = await _TryDetectFileFormatAsync(file).ConfigureAwait(false);

                Regex regex = null;
                switch (logFormat)
                {
                    case LogFormat.NLog:
                        regex = Constants.regexMessageMetaDataNLogDefault;
                        break;
                    case LogFormat.JobService:
                        regex = Constants.regexMessageMetaDataJobservice;
                        break;
                    default:
                        continue; // todo log message or other user notification
                }


                using var reader = new StreamReader(file, m_Encoding, true);

                sb.Length = 0;
                int lineNumberTotal = 0;
                int lineNumber = 1;
                int entryNumber = 0;

                await foreach(var line in _ReadAsync(reader, ct).ConfigureAwait(false))
                {
                    string entry = null;
                    lineNumberTotal++;

                    bool isStart = line == null || _IsLineStart(line, logFormat);
                    if (!isStart)
                    {
                        sb.AppendLine();
                        sb.Append(line);
                        continue;
                    }

                    entry = sb.ToString();
                    sb.Clear();

                    // the last one is <null>
                    if (line != null)
                        sb.Append(line);

                    if (entry.Length == 0)
                        continue;

                    var match = regex.Match(entry);
                    if (!match.Success)
                        continue;

                    var spid = string.Format("{1}{0}", 
                        match.Groups["SID"].Value,
                        match.Groups["NSourceExt"].Value.Length > 0 ? match.Groups["NSourceExt"].Value : match.Groups["NSourceExt2"].Value.Trim());

                    var logger = match.Groups["NSource"].Value;
                    if (logFormat == LogFormat.JobService && string.IsNullOrEmpty(logger))
                        logger = "Jobservice";

                    var logEntry = new LogEntry(new Locator(++entryNumber, lineNumber, file), 
                        lineNumber.ToString(),
                        DateTime.TryParse(match.Groups["Timestamp"].Value, out var timeStamp) ? timeStamp : DateTime.MinValue,
                        _GetLogLevel(match.Groups["NLevel"].Value),
                        0,
                        match.Groups["Payload"].Value,
                        logger, 
                        "",
                        match.Groups["PID"].Value,
                        spid);

                    lineNumber = lineNumberTotal;

                    yield return logEntry;
                }
            }
        }

        private async Task<LogFormat> _TryDetectFileFormatAsync(string file)
        {
            using (StreamReader reader = new StreamReader(file))
            {
                string line;
                int idx = 0;
                while ((line = await reader.ReadLineAsync().ConfigureAwait(false)) != null)
                {
                    if (Constants.regexMessageMetaDataNLogDefault.IsMatch(line))
                        return LogFormat.NLog;

                    if (Constants.regexMessageMetaDataJobservice.IsMatch(line))
                        return LogFormat.JobService;

                    if (idx++ == 32)
                        break;
                }
            }

            return LogFormat.Unknown;
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


        private static async IAsyncEnumerable<string> _ReadAsync(StreamReader reader, [EnumeratorCancellation] CancellationToken ct)
        {
            string line;

            while ((line = await reader.ReadLineAsync()) != null)
                yield return line;

            // we have to add null in order to handle the last entry correctly
            yield return null;
        }


        private static bool _IsLineStart(string line, LogFormat format)
        {
            switch (format)
            {
                case LogFormat.NLog:
                    return line.Length > 30
                           && line[4] == '-'
                           && line[7] == '-'
                           && m_LineStartRegexNLog.IsMatch(line);

                case LogFormat.JobService:
                    return line.Length > 30
                           && line[0] == '<'
                           && line[2] == '>'
                           && line[7] == '-'
                           && m_LineStartRegexJobService.IsMatch(line);
            }

            return false;
        }

        /// <summary>
        /// Gets a short display of the reader and it's data.
        /// </summary>
        public override string Display => $"NLog Reader - {m_FileNames.Length} files";

        private readonly string[] m_FileNames;
        private readonly Encoding m_Encoding;

        private static readonly Regex m_LineStartRegexNLog = new (@"^\d\d\d\d-\d\d-\d\d\s+\d\d:\d\d:\d\d", RegexOptions.CultureInvariant | RegexOptions.Compiled);
        private static readonly Regex m_LineStartRegexJobService = new(@"^<\w>\d\d\d\d-\d\d-\d\d\s+\d\d:\d\d:\d\d", RegexOptions.CultureInvariant | RegexOptions.Compiled);
    }
}
