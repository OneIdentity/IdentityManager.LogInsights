using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using LogInsights.LogReader;

namespace LogInsights.LogReader
{
    public abstract class LogReader : ILogReader
    {
        protected LogReader()
        {
        }

        public void Dispose()
        {
            OnDispose();
        }

        public IAsyncEnumerable<LogEntry> ReadAsync(CancellationToken ct = default)
        {
            return OnReadAsync(ct);
        }

        protected virtual void OnDispose()
        { }

        protected abstract IAsyncEnumerable<LogEntry> OnReadAsync(CancellationToken ct);

        /// <summary>
        /// Gets a short display of the reader and it's data.
        /// </summary>
        public abstract string Display { get; }
    }
}
