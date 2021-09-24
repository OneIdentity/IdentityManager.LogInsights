using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LogInsights.LogReader
{
    public interface ILogReader : IDisposable
    {
        IAsyncEnumerable<LogEntry> ReadAsync(CancellationToken ct = default);

        /// <summary>
        /// Gets a short display of the reader and it's data.
        /// </summary>
        string Display { get; }
    }
}
