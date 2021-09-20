using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogfileMetaAnalyser.LogReader
{
    public class Locator
    {
        public Locator(/*ILogReader reader, */int entryNumber, int position, string source)
        {
            Position = position;
            Source = source;
            EntryNumber = entryNumber;
            //  Reader = reader;
        }

        public int Position { get; }

        public string Source { get; }

        public int EntryNumber { get; }

        //public ILogReader Reader { get; }
    }
}
