using System;
using System.Collections.Generic;
using System.Linq;

using LogfileMetaAnalyser.Helpers;


namespace LogfileMetaAnalyser.Datastore
{
    public class SqlInformation
    {
        public int numberOfSqlSessions = 0;
        public bool isSuspicious = false;
        public int threshold_suspicious_duration_SqlCommand_msec = -1;
        public int threshold_suspicious_duration_SqlTransaction_sec = -1;
        public List<SqlSession> sqlSessions = new List<SqlSession>();

        public SqlInformation()
        { }

        public override string ToString()
        {
            return string.Format("{0}{1} session(s)", isSuspicious ? "! " : "", numberOfSqlSessions);
        }

        public IEnumerable<string> GetLoggerIdsByUuids(string[] uuidList)
        {
            if (uuidList == null || uuidList.Length == 0)
                return new string[] { };

            if (uuidList[0] == "*")
                return sqlSessions.Select(ss => ss.loggerSourceId);
            else
                return sqlSessions
                            .Where(ss => uuidList.Contains(ss.uuid))
                            .Select(ss => ss.loggerSourceId);                    
        }
 
    }


       
    public enum SQLCmdType
    {
        Insert, Update, Delete, Select, Other
    }
}
