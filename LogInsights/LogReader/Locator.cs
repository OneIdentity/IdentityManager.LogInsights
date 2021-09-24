namespace LogInsights.LogReader
{
    public class Locator
    {
        public Locator(int entryNumber, int position, string source)
        {
            Position = position;
            Source = source;
            EntryNumber = entryNumber;
        }

        public int Position { get; }

        public string Source { get; }

        public int EntryNumber { get; }
    }
}
