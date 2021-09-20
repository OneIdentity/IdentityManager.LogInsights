using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LogfileMetaAnalyser.LogReader
{
    public class AppInsidesReader : LogReader
    {
        protected override IAsyncEnumerable<LogEntry> OnReadAsync(CancellationToken ct)
        {
            return null;
        }
    }
}
