using System.Collections.Generic;
using System.Linq;


namespace LogfileMetaAnalyser.Datastore
{
    public class SqlInformation : IDataStoreContent
    {
        public int numberOfSqlSessions = 0;
        public bool isSuspicious = false;
        public int ThresholdSuspiciousDurationSqlCommandMsec = -1;
        public int ThresholdSuspiciousDurationSqlTransactionSec = -1;
        public List<SqlSession> SqlSessions { get; } = new();

        public override string ToString()
        {
            return string.Format("{0}{1} session(s)", isSuspicious ? "! " : "", numberOfSqlSessions);
        }

        public IEnumerable<string> GetLoggerIdsByUuids(string[] uuidList)
        {
            if (uuidList == null || uuidList.Length == 0)
                return new string[] { };

            if (uuidList[0] == "*")
                return SqlSessions.Select(ss => ss.loggerSourceId);
            else
                return SqlSessions
                            .Where(ss => uuidList.Contains(ss.uuid))
                            .Select(ss => ss.loggerSourceId);                    
        }

        public bool HasData => numberOfSqlSessions > 0 || 
                               ThresholdSuspiciousDurationSqlCommandMsec > -1 ||
                               ThresholdSuspiciousDurationSqlTransactionSec > -1 ||
                               SqlSessions.Count > 0;
    }


       
    public enum SQLCmdType
    {
        Other,
        Insert, 
        Update, 
        Delete, 
        Select,
    }
}
