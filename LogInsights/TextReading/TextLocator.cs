using System; 

namespace LogInsights
{
    public class TextLocator
    {
        public string fileName = "";
        public long fileLinePosition;
        public long fileStreamOffset;
        public long messageNumber;
        

        public TextLocator(string fileName = "", long fileStreamOffset = -1, long fileLinePosition = -1,  long messageNumber = -1)
        {
            this.fileName = fileName;
            this.fileLinePosition = fileLinePosition;
            this.fileStreamOffset = fileStreamOffset;
            this.messageNumber = messageNumber;
        }
    }
}
