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
using System.IO.Enumeration;

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
            m_FileNames = ResolveAndSortFiles(fileNames ?? throw new ArgumentNullException(nameof(fileNames))).ToArray();
            m_Encoding = encoding ?? Encoding.Default;
        }

		public NLogReader(NLogReaderConnectionStringBuilder connectionString)
			: this(connectionString?.FileNames, connectionString?.Encoding)
		{}

        public NLogReader(string connectionString)
			: this(new NLogReaderConnectionStringBuilder(connectionString))
        {}

        public static IEnumerable<string> ResolveAndSortFiles(string[] filesAndDirectories)
        {
            return _ResolveAndSortFiles(filesAndDirectories)
                .Select(f => _TryDetectFileFormatAsync(f.FullName).Result)
                .Where(f =>f.Format != LogFormat.Unknown)
                .OrderBy(f=>f.FirstTimeStamp) // sort by the first timestamp and NOT by the creation time, because the log may lost it's creation time when send by email
                .Select(f => f.FileName);
        }

        private static IEnumerable<FileInfo> _ResolveAndSortFiles(string[] filesAndDirectories)
        {
            foreach (var ford in filesAndDirectories ?? Enumerable.Empty<string>())
            {
                var file = new FileInfo(ford);
                if (file.Exists)
                    yield return file;

                var directory = new DirectoryInfo(ford);
                if (!directory.Exists)
                    continue;

                foreach (var f in directory.GetFiles("*.log", SearchOption.AllDirectories))
                    yield return f;
                
                foreach (var f in directory.GetFiles("*.txt", SearchOption.AllDirectories))
                    yield return f;
            }
        }

        protected override  async IAsyncEnumerable<LogEntry> OnReadAsync([EnumeratorCancellation] CancellationToken ct)
        {
            StringBuilder sb = new StringBuilder(1024);

            foreach (var file in m_FileNames)
            {
                var data = await _TryDetectFileFormatAsync(file).ConfigureAwait(false);
                var logFormat = data.Format;
                
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

                    var logEntry = new LogEntry {
                        Locator = new Locator(++entryNumber, lineNumber, file),
                        Id = lineNumber.ToString(),
                        TimeStamp = DateTime.TryParse(match.Groups["Timestamp"].Value, out var timeStamp)
                            ? timeStamp
                            : DateTime.MinValue,
                        Level = logFormat == LogFormat.JobService
                            ? _GetServiceLogLevel(match.Groups["tag"].Value)
                            : _GetNLogLevel(match.Groups["NLevel"].Value),
                        Message = match.Groups["Payload"].Value,
                        Logger = logger,
                        Pid = match.Groups["PID"].Value,
                        Spid = spid
                    };

                    lineNumber = lineNumberTotal;

                    yield return logEntry;
                }
            }
        }

        
        private static async Task<(string FileName, LogFormat Format, DateTime FirstTimeStamp)> _TryDetectFileFormatAsync(string file)
        {
            using (StreamReader reader = new StreamReader(file))
            {
                string line;
                int idx = 0;
                while ((line = await reader.ReadLineAsync().ConfigureAwait(false)) != null)
                {
                    var match = Constants.regexMessageMetaDataNLogDefault.Match(line);
                    if (match.Success)
                        return (file, LogFormat.NLog, DateTime.TryParse(match.Groups["Timestamp"].Value, out var timeStamp) ? timeStamp : DateTime.MinValue);

                    match = Constants.regexMessageMetaDataJobservice.Match(line);
                    if (match.Success)
                        return (file, LogFormat.JobService, DateTime.TryParse(match.Groups["Timestamp"].Value, out var timeStamp) ? timeStamp : DateTime.MinValue);

                    if (idx++ == 32)
                        break;
                }
            }

            return (file, LogFormat.Unknown, DateTime.MinValue);
        }

        private LogLevel _GetNLogLevel(string value)
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

        private LogLevel _GetServiceLogLevel(string value)
        {
            if (value.Length < 3)
                return LogLevel.Critical;

            // fast path for single tags^<.>
            switch (value[1])
            {
                case 'I':
                case 'i':
                    return LogLevel.Info;

                case 'W':
                case 'w':
                    return LogLevel.Warn;

                case 'E':
                case 'e':
                    return LogLevel.Error;
                
                // r=serious
                case 'R':
                case 'r':
                    return LogLevel.Critical;
            }

            // slow path for multiple tags ^<x><i>....
            if (value.IndexOf("<i>", StringComparison.OrdinalIgnoreCase)>=0)
                return LogLevel.Info;

            if (value.IndexOf("<w>", StringComparison.OrdinalIgnoreCase) >= 0)
                return LogLevel.Warn;

            if (value.IndexOf("<e>", StringComparison.OrdinalIgnoreCase) >= 0)
                return LogLevel.Error;

            if (value.IndexOf("<r>", StringComparison.OrdinalIgnoreCase) >= 0)
                return LogLevel.Critical;

            return LogLevel.Info;
        }

        private static async IAsyncEnumerable<string> _ReadAsync(StreamReader reader, [EnumeratorCancellation] CancellationToken ct)
        {
            string line;

            while ((line = await reader.ReadLineAsync().ConfigureAwait(false)) != null)
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
        private static readonly Regex m_LineStartRegexJobService = new(@"^(<\w>)+\d\d\d\d-\d\d-\d\d\s+\d\d:\d\d:\d\d", RegexOptions.CultureInvariant | RegexOptions.Compiled);
    }
}
