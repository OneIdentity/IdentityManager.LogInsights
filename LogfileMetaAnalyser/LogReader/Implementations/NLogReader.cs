using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace LogfileMetaAnalyser.LogReader
{
    public class NLogReader : LogReader
    {
        public NLogReader(string[] fileNames, Encoding encoding)
        {
            // check the parameters
            m_FileNames = fileNames ?? throw new ArgumentNullException(nameof(fileNames));
            m_Encoding = encoding ?? throw new ArgumentNullException(nameof(encoding));
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
                using var reader = new StreamReader(file, m_Encoding, true);

                sb.Length = 0;
                int lineNumber = 0;

                await foreach(var line in _ReadAsync(reader, ct))
                {
                    string entry = null;
                    lineNumber++;

                    bool isStart = line == null || _IsLineStart(line);
                    if (!isStart)
                    {
                        sb.AppendLine(line);
                        continue;
                    }

                    entry = sb.ToString();
                    sb.Clear();

                    // the last one is <null>
                    if (line != null)
                        sb.AppendLine(line);

                    if (entry.Length == 0)
                        continue;

                    // TODO
                    var logEntry = new LogEntry(new Locator(lineNumber, file), 
                        lineNumber.ToString(),
                        DateTime.Now,
                        LogLevel.Info,
                        0, entry, "", "");

                    yield return logEntry;
                }
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

        private readonly string[] m_FileNames;
        private readonly Encoding m_Encoding;

        private static readonly Regex m_LineStartRegex = new (@"^\d\d\d\d-\d\d-\d\d\s+\d\d:\d\d:\d\d", RegexOptions.CultureInvariant | RegexOptions.Compiled);
    }
}
