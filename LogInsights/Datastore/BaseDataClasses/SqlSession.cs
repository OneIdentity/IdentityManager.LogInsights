using System;
using System.Collections.Generic;



namespace LogInsights.Datastore
{
    public class SqlSession : DatastoreBaseDataRange
    {
        public bool isSuspicious = false;
        public List<TransactionData> transactions = new List<TransactionData>();
        public List<SqlLongRunningStatement> longRunningStatements = new List<SqlLongRunningStatement>();

        public SqlSession()
        { }

        public override string ToString()
        {
            return string.Format("{0}id={1}; {2} transactions; {3} long running statements", isSuspicious ? "! " : "", loggerSourceId, transactions.Count, longRunningStatements.Count);
        }
    }
}
