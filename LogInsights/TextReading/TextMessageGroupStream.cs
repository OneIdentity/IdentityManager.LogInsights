using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using LogInsights.Helpers;
using LogInsights.LogReader;

namespace LogInsights
{
    public class TextMessageGroupStream
    {
        private int maxLines = 99;
        private Dictionary<string, TextMessageGroup> streams = new Dictionary<string, TextMessageGroup>();
        private string lastStreamKey = "";


        public TextMessageGroupStream(int maxLines = 26)
        {
            if (maxLines < 1 || maxLines > 10240)
                throw new ArgumentOutOfRangeException("maxLines");
            
            this.maxLines = maxLines;            
        }

        public bool IsGroupClosed(LogEntry msgStreamMessage = null)
        {
            string streamKeyToCheck = msgStreamMessage == null ? lastStreamKey : GetStreamKey(msgStreamMessage);

            if (!streams.ContainsKey(streamKeyToCheck))
                return false;

            return streams[streamKeyToCheck].IsGroupClosed();
        }

        public void Clear(LogEntry msgStreamMessage = null)
        {
            string streamKeyToCheck = msgStreamMessage == null ? lastStreamKey : GetStreamKey(msgStreamMessage);

            if (!streams.ContainsKey(streamKeyToCheck))
                streams.Add(streamKeyToCheck, new TextMessageGroup());
            else
                streams[streamKeyToCheck] = new TextMessageGroup();
        }

        public LogEntry GetGroupMessage(LogEntry msgStreamMessage = null)
        {
            string streamKeyToCheck = msgStreamMessage == null ? lastStreamKey : GetStreamKey(msgStreamMessage);

            if (!streams.ContainsKey(streamKeyToCheck))
                return null;
            else
                return streams[streamKeyToCheck].GetGroupMessage();
        }

        public LogEntry[] GetAllGroupMessages()
        {
            List<LogEntry> res = new List<LogEntry>();

            foreach (var grp in streams.Values)
            {
                LogEntry tm = grp.GetGroupMessage();
                if (tm != null)
                    res.Add(tm);
            }

            return res.Count == 0 ? null : res.ToArray();
        }

        public void AppendMessage(LogEntry msg)
        {
            lastStreamKey = GetStreamKey(msg);

            if (!streams.ContainsKey(lastStreamKey))
                streams.Add(lastStreamKey, new TextMessageGroup(msg, this.maxLines));
            else
                streams[lastStreamKey].AppendMessage(msg);
        }

        private string GetStreamKey(LogEntry msg)
        {
            return string.Format("{0}|{1}", msg.Logger, msg.Spid);
        }
    }
}
