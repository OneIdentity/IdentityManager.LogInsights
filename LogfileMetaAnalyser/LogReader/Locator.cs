using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogfileMetaAnalyser.LogReader
{
    public class Locator
    {
        public Locator(/*ILogReader reader, */int position, string source)
        {
            Position = position;
            Source = source;
          //  Reader = reader;
        }

        public int Position { get; }

        public string Source { get; }

        //public ILogReader Reader { get; }
    }
}
