using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LogfileMetaAnalyser.LogReader
{
    public interface ILogReader : IDisposable
    {
        IAsyncEnumerable<LogEntry> ReadAsync(CancellationToken ct = default);
    }
}
