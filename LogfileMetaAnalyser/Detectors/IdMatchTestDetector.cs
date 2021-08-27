using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

using LogfileMetaAnalyser.Helpers;

namespace LogfileMetaAnalyser.Detectors
{
    class IdMatchTestDetector : DetectorBase, ILogDetector
    {
        private static Regex regex_Uid = new Regex(@"(?<uid>[A-Fa-f0-9]{8}-[A-Fa-f0-9]{4}-[A-Fa-f0-9]{4}-[A-Fa-f0-9]{4}-[A-Fa-f0-9]{12})", RegexOptions.Compiled);

        private Dictionary<string, IdMatches> idmatches;
        private List<string> idmatchChain;

        public override string caption
        {
            get
            {
                return "UID correlations";
            }
        }


        public override string identifier
        {
            get
            {
                return "#IdMatchTestDetector";
            }
        }
        public IdMatchTestDetector() : base(TextReadMode.GroupMessage)
        { }

        public void InitializeDetector()
        {
            idmatches = new Dictionary<string, IdMatches>();
            idmatchChain = new List<string>(); 
			detectorStats.Clear();
            isFinalizing = false;
        }
             

        private IEnumerable<string> GetFollower(string xuid1)
        {
            foreach (IdMatches m in idmatches.Values.Where(m => m.uid1 == xuid1 /*&& !m.touched*/))
            {
                m.touched = true;
                yield return string.Format("[{0}: {1} -> {2}] -> {3}]", m.loggerSrc, m.uid1, m.uid2, GetFollower(m.uid2));
            }
            yield break;
        }

        public void FinalizeDetector()
        {
            logger.Debug("entering FinalizeDetector()");
            isFinalizing = true;
            ProcessMessage(null);
			
			DateTime finStartpoint = DateTime.Now;

            foreach (IdMatches m in idmatches.Values)
            {
                if (m.touched)
                    continue;

                m.touched = true;

                foreach (var id in GetFollower(m.uid2))
                    idmatchChain.Add(string.Format("[{0}: {1} -> {2}] -> {3}]", m.loggerSrc, m.uid1, m.uid2, id));
            }
            
            //stats
            detectorStats.detectorName = string.Format("{0} <{1}>", this.GetType().Name, this.identifier);
            detectorStats.finalizeDuration = (DateTime.Now - finStartpoint).TotalMilliseconds;
            detectorStats.numberOfDetections = 0;
            _datastore.statistics.detectorStatistics.Add(detectorStats);

            //dispose
            //idmatches = null;
            //idmatchChain = null;
        }

        public void ProcessMessage(TextMessage msg)
        {
            if (!_isEnabled)
                return;

			DateTime procMsgStartpoint = DateTime.Now;
			
            var rt = ProcessMessageBase(ref msg);
            if (rt != null)
                foreach (var xmsg in rt)
                    ProcessMessage(xmsg);
            if (msg == null)
                return;

			detectorStats.numberOfLinesParsed += msg.numberOfLines;    
			
            var matches = regex_Uid.Matches(msg.messageText);
            if (matches.Count > 1)            
            {
                List<string> uids = new List<string>();
                foreach (Match rm in matches)
                    uids.Add(rm.Groups["uid"].Value);

                uids.Sort();
                uids = uids.Distinct().ToList();

                //for (int i=0; i<uids.Count -1; i++)
                foreach (string uid in uids)
                {
                    if (msg.spid == uid)
                        continue;

                    string key = msg.spid + "=>" + uid;
                    if (!idmatches.ContainsKey(key))
                        idmatches.Add(key, new IdMatches()
                        {
                            uid1 = msg.spid,
                            uid2 = uid,
                            text = msg.messageText,
                            loggerSrc = msg.loggerSource
                        });
                }
                
            }
			
			detectorStats.parseDuration += (DateTime.Now - procMsgStartpoint).TotalMilliseconds;
        }

        internal class IdMatches
        {           
            public string uid1;
            public string uid2;
            public string text;
            public string loggerSrc;
            public bool touched;

            public IdMatches()
            { }

            public override string ToString()
            {
                return string.Format("{0}: \"{1}\"-\"{2}\" <- {3}", loggerSrc, uid1, uid2, text);
            }
        }
        
    }
}
