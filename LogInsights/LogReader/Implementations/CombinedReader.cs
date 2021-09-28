using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace LogInsights.LogReader
{
    public class CombinedReader : ILogReader
    {
        private readonly ILogReader[] _readers;

        public CombinedReader(ILogReader[] readers)
        {
            _readers = readers ?? throw new ArgumentNullException(nameof(readers));
        }

        public void Dispose()
        {
            foreach (ILogReader reader in _readers) 
                reader.Dispose();
        }

        public async IAsyncEnumerable<LogEntry> ReadAsync(CancellationToken ct = default)
        {
            foreach (var reader in _readers)
            await foreach (var entry in reader.ReadAsync(ct).ConfigureAwait(false))
                yield return entry;
        }

        public string Display => "Combination of files";
    }
}