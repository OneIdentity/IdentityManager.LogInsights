using System;
using System.Collections.Generic;


namespace LogfileMetaAnalyser.Datastore
{
    public class SqlLongRunningStatement : DatastoreBaseDataRange
    {
        public uint durationMsec;
        public SQLCmdType sqlCmdType;
        public List<string> assignedTablenames = new List<string>();
        public string statementText;
        public bool isPayloadTableInvolved;

        public SqlLongRunningStatement()
        { }

        public override string ToString()
        {
            return string.Format("{0}: {1} sec: {2}", sqlCmdType, durationMsec, statementText);
        }

        public string GetLabel()
        {
            return $"at {dtTimestampStart.ToString("G")}: {sqlCmdType}: {durationMsec} ms: {statementText}";
        }
    }
}
