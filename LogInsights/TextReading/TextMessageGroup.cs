using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LogInsights
{
    public class TextMessageGroup
    {
        private List<TextMessage> buffer = new List<TextMessage>();
        private int bufferLevel; //optimized replacement for buffer.Count
        private int maxLines;
        private bool groupClosed = false;

        public TextMessageGroup(TextMessage firstLine = null, int maxLines = 26)
        {
            if (maxLines < 1 || maxLines > 10240)
                throw new ArgumentOutOfRangeException("maxLines");

            this.maxLines = maxLines;

            bufferLevel = 0;
            if (firstLine != null)
                AppendMessage(firstLine);            
        }

        public void AppendMessage(TextMessage msg)
        {
            //this passed msg belongs to the group when a) the current group is able to get one more item and b) the msg meta data are the same with the group's meta data
            bool msgBelongsToGroup = bufferLevel == 0 //this is the very first msg in this group
                                     || (
                                        bufferLevel >= 1 && //we already have buffered msgs
                                        !IsGroupClosed() && //the current group is still open
                                        buffer[bufferLevel - 1].Locator.messageNumber + 10 >= msg.Locator.messageNumber &&   //we allow a gap of 10 messages, we cannot infinitely wait for group closure
                                        buffer[bufferLevel - 1].EqualMetaData(msg, 1250)  //equal meta data incl. timestamp (+- tolerance of 0 ms!)
                                     );
            if (msgBelongsToGroup)
            {                
                buffer.Add(msg); //.Clone());
                bufferLevel++;
            }
            else
                groupClosed = true;
        }

        public bool IsGroupClosed()
        {
            return bufferLevel == maxLines || groupClosed;
        }

        public TextMessage GetGroupMessage()
        {
            if (bufferLevel == 0)
                return null;

            TextMessage res = buffer[0];

            return res.Merge(buffer, 1, buffer.Count - 1); 
        }
    }
}
