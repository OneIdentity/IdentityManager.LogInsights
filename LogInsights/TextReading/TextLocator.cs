using System; 

namespace LogInsights
{
    public class TextLocator
    {
        public string Source = "";
        public long Position;
        public long EntryNumber;
        

        public TextLocator(string source = "", long position = -1,  long entryNumber = -1)
        {
            this.Source = source;
            this.Position = position;
            this.EntryNumber = entryNumber;
        }
    }
}
