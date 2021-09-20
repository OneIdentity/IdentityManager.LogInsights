using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using LogfileMetaAnalyser.LogReader;

namespace LogfileMetaAnalyser.LogReader
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
    }
}
