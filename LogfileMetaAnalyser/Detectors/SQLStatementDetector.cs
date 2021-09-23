using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

using LogfileMetaAnalyser.Datastore;
using LogfileMetaAnalyser.Helpers;


namespace LogfileMetaAnalyser.Detectors
{
    class SQLStatementDetector : DetectorBase, ILogDetector
    {
        private static int threshold_suspicious_duration_SqlCommand_msec = 1000 * 2;
        private static int threshold_suspicious_duration_SqlTransaction_sec = 90;

        private static Regex regex_TransactionBegin = new Regex(@" BEGIN TRANSACTION", RegexOptions.Compiled);
        private static Regex regex_TransactionEnd = new Regex(@" (?<cmd>COMMIT|ROLLBACK) TRANSACTION", RegexOptions.Compiled);

        private static Regex regex_NonSystemTable = new Regex(@"^(?!(DPR|QBM|dbo.QBM|Dialog|Job|RAW|Mon))", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static Regex regex_BasicStatement = new Regex(@"\((?<dura>\d+) ms\) .*?((?<cmdSelect>Select.*from.+)|(?<cmdUpdate>Update (?<cmdUpdateTable>[^ \r\t\n\(\)\""']+) .+)|(?<cmdInsert>Insert into (?<cmdInsertTable>[^ \r\t\n\(\)\""']+).+)|(?<cmdDelete>Delete (from )?(?<cmdDeleteTable>[^ \r\t\n\(\)\""']+).+)|(?<cmdExec>exec \w{3}_.+))", RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.IgnoreCase);
        private static Regex regex_SelectTableStatement = new Regex(@"((from|join) (?'cmdSelectTable'[^ \r\t\n\(\)\""']+))+", RegexOptions.Singleline | RegexOptions.Compiled);
        

        
        private Dictionary<string, SqlSessionIntern> sqlSessionInfo;
        

        public override string caption
        {
            get
            {
                return "Found suspicious SQL commands";
            }
        }

        public override string identifier
        {
            get
            {
                return "#SQLStatementDetector";
            }
        }

        public override string[] requiredParentDetectors
        {
            get
            {
                return new string[] { "#SyncStructureDetector" };
            }
        }


        public SQLStatementDetector()
        { }


        public void InitializeDetector()
        {
            sqlSessionInfo = new Dictionary<string, SqlSessionIntern>();
			detectorStats.Clear();
        }


        public void FinalizeDetector()
        {
            logger.Debug("entering FinalizeDetector()");

            DateTime finStartpoint = DateTime.Now;

            var generalSqlInformation = _datastore.GetOrAdd<SqlInformation>();
            var projectionActivity = _datastore.GetOrAdd<ProjectionActivity>();
            var statisticsStore = _datastore.GetOrAdd<StatisticsStore>();


            //1.) if flag sqlSessionInfo[].touchesDprModuleTables = true but we have no projection assignment yet, and there is only one, we can safely assign it
            if (projectionActivity.Projections.Count == 1)
            {
                logger.Debug("we only have 1 projection: if flag sqlSessionInfo[].touchesDprModuleTables = true but we have no projection assignment yet, and there is only one, we can safely assign it");

                foreach (var sqlsession in sqlSessionInfo.Where(kp => kp.Value.touchesDprModuleTables))
                    sqlsession.Value.belongsToProjectionId.Add(projectionActivity.Projections[0].uuid);
            }


            //2) if we have only one sql session with touchesDprModuleTables==true but several projections, we can add each projection id to the sql session
            if (sqlSessionInfo.Count(t => t.Value.touchesDprModuleTables) == 1)
            {
                logger.Debug("we have only one sql session with touchesDprModuleTables==true but several projections, so we can add each projection id to the sql session");

                sqlSessionInfo.First(t => t.Value.touchesDprModuleTables).Value.belongsToProjectionId.AddRange(projectionActivity.Projections.Select(p => p.uuid));
            }


            //3.) a sql session likely belongs to a specific projection if those tables were handled which are part of projection's step's left schema class
            if (projectionActivity.Projections.Count > 1)
            {
                logger.Debug("a sql session likely belongs to a specific projection if those tables were handled which are part of projection's step's left schema class");

                foreach (var sqlsesion in sqlSessionInfo.Where(kp => kp.Value.belongsToProjectionId.Count == 0))
                    foreach (var projection in projectionActivity
                                                            .Projections
                                                            .Where(p => p.dtTimestampEnd.InRange(sqlsesion.Value.dtTimestampStart, sqlsesion.Value.dtTimestampEnd)))
                    {
                        var steps = projection.projectionSteps;
                        int stepcnt = steps.Count;
                        int matchcnt = 0;

                        foreach (var step in steps)
                        {
                            if (sqlsesion.Value.nonSystemTables.Any(t => string.Compare(t, step.leftSchemaTypeGuess, true) == 0))
                                matchcnt++;
                        }

                        if (matchcnt == stepcnt)
                        {
                            logger.Debug($"\tsql session {sqlsesion.Value.loggerSourceId} belongs to projection id {projection.uuid}");
                            sqlsesion.Value.belongsToProjectionId.Add(projection.uuid);
                        }
                    }
            }

            //4.)
            logger.Debug("try to assign a sql session to a projection with help of statements inside the time range of a projection");
            foreach (var sqlsesion in sqlSessionInfo.Where(kp => kp.Value.belongsToProjectionId.Count == 0))
                foreach (var projection in projectionActivity.Projections)
                    if (sqlsesion.Value.longRunningStatements.Any(lr => lr.isPayloadTableInvolved && lr.dtTimestampStart.InRange(projection.dtTimestampStart, projection.dtTimestampEnd)))  //at least one payload statement within the lifetime of a projection
                    {
                        logger.Debug($"\tsql session {sqlsesion.Value.loggerSourceId} belongs to projection id {projection.uuid}");
                        sqlsesion.Value.belongsToProjectionId.Add(projection.uuid);
                    }


            //4.) there is no other way to connect a projection with a sql session as a sql session is too loose
            //    maybe we can execute an ID match algorithm, but this is too much and would produce too many false-positives



            //respect threshold_suspicious_duration_SqlCommand_msec
            foreach (var se in sqlSessionInfo.Values)
            {
                se.longRunningStatements = se.longRunningStatements.Where(lr => lr.durationMsec >= threshold_suspicious_duration_SqlCommand_msec).ToList();
                se.isSuspicious = (se.longRunningStatements.Count > 0 || se.transactions.Any(t => t.isSuspicious));
            }


            //finally assign results to datastore
            //a) general information            
            generalSqlInformation.ThresholdSuspiciousDurationSqlCommandMsec = threshold_suspicious_duration_SqlCommand_msec;
            generalSqlInformation.ThresholdSuspiciousDurationSqlTransactionSec  = threshold_suspicious_duration_SqlTransaction_sec;
            generalSqlInformation.numberOfSqlSessions = sqlSessionInfo.Count;
            generalSqlInformation.isSuspicious = sqlSessionInfo.Any(s => s.Value.isSuspicious);
            generalSqlInformation.SqlSessions.AddRange(sqlSessionInfo.Select(i => i.Value.AsSqlSession(DateTime.MinValue, DateTime.MaxValue)));
            logger.Debug($"pushing to ds: generalSqlInformation generally");

            //b) each projection
            foreach (var sqlsession in sqlSessionInfo.Values.Where(x => x.belongsToProjectionId.Count > 0))
                foreach (var projuuid in sqlsession.belongsToProjectionId.Distinct())
                {
                    var proj = projectionActivity.Projections.FirstOrDefault(p => p.uuid == projuuid);

                    proj.specificSqlInformation.ThresholdSuspiciousDurationSqlCommandMsec = threshold_suspicious_duration_SqlCommand_msec;
                    proj.specificSqlInformation.ThresholdSuspiciousDurationSqlTransactionSec = threshold_suspicious_duration_SqlTransaction_sec;

                    proj.specificSqlInformation.numberOfSqlSessions = sqlSessionInfo.Count(x => x.Value.belongsToProjectionId.Any(a => a == proj.uuid));
                    proj.specificSqlInformation.isSuspicious = sqlSessionInfo.Where(x => x.Value.belongsToProjectionId.Any(a => a == proj.uuid)).Any(s => s.Value.isSuspicious);
                    proj.specificSqlInformation.SqlSessions.Add(sqlsession.AsSqlSession(proj.dtTimestampStart, proj.dtTimestampEnd));

                    logger.Debug($"pushing to ds: generalSqlInformation {sqlsession.loggerSourceId} for projection {proj.loggerSourceId}");
                }



            //stats
            detectorStats.detectorName = string.Format("{0} <{1}>", this.GetType().Name, this.identifier);
            detectorStats.numberOfDetections += 4 + generalSqlInformation.SqlSessions.Count;
            detectorStats.finalizeDuration = (DateTime.Now - finStartpoint).TotalMilliseconds;
            statisticsStore.DetectorStatistics.Add(detectorStats);
            logger.Debug(detectorStats.ToString());

            //dispose
            sqlSessionInfo = null;
        }


        public void ProcessMessage(TextMessage msg)
        {
            if (!_isEnabled)
                return;

			DateTime procMsgStartpoint = DateTime.Now;
			
            if (msg.loggerSource != "SqlLog" || msg.spid == "")
                return;

            var rt = ProcessMessageBase(ref msg);
            if (rt != null)
                foreach (var xmsg in rt)
                    ProcessMessage(xmsg);
            if (msg == null)
                return;

			detectorStats.numberOfLinesParsed += msg.numberOfLines;    

            //general session information
            if (!sqlSessionInfo.ContainsKey(msg.spid))
            {
                sqlSessionInfo.Add(msg.spid, new SqlSessionIntern()
                {
                    dtTimestampStart = msg.messageTimestamp,
                    message = msg
                });

                logger.Trace($"found new sql session {msg.spid}");
            }
            else
                sqlSessionInfo[msg.spid].dtTimestampEnd = msg.messageTimestamp;


            if (msg.loggerLevel != LogLevel.Debug)
                return;
            
                        
            //transaction start
            if (regex_TransactionBegin.IsMatch(msg.payloadMessage))
            {
                logger.Trace($"regex match for rx regex_TransactionBegin: {regex_TransactionBegin.ToString()}");
                
                var transList = sqlSessionInfo.GetOrAdd(msg.spid).transactions;

                transList.Add(new TransactionData()
                {
                    dtTimestampStart= msg.messageTimestamp, 
                    message = msg
                });

                logger.Debug($"found new sql transaction {msg.spid} opening");

                return;
            }
                
            //transaction end
            var rm = regex_TransactionEnd.Match(msg.payloadMessage);
            if (rm.Success)
            {
                logger.Trace($"regex match for rx regex_TransactionEnd: {regex_TransactionEnd.ToString()}");

                var transList = sqlSessionInfo.GetOrAdd(msg.spid).transactions;

                if (transList.Count == 0) //we ignore open transactions where the start was not logged
                    return;
                                
                var tran = transList.LastOrDefault(t => t.dtTimestampEnd.IsNull() && !t.dtTimestampStart.IsNull());
                if (tran != null)
                {
                    tran.dtTimestampEnd = msg.messageTimestamp; 
                    tran.messageEnd = msg;                    
                    tran.isSuccessFullTransaction = rm.Groups["cmd"].Value == "COMMIT";                    
                    tran.isSuspicious = !tran.isSuccessFullTransaction || tran.durationSec >= threshold_suspicious_duration_SqlTransaction_sec;

                    logger.Debug($"found new sql transaction {tran.loggerSourceId} closing");
                }

                return;
            }

            //basic statement
            rm = regex_BasicStatement.Match(msg.payloadMessage);
            if (rm.Success)
            {
                logger.Trace($"regex match for rx regex_BasicStatement: {regex_BasicStatement.ToString()}");

                SqlLongRunningStatement sqlcmd = new SqlLongRunningStatement()
                {
                    durationMsec = ConvertSave.ConvertToUInt(rm.Groups["dura"].Value),
                    dtTimestampStart = msg.messageTimestamp,
                    isDataComplete = true,
                    message = msg
                };

                if (!string.IsNullOrEmpty(rm.Groups["cmdInsert"].Value))
                {
                    sqlcmd.assignedTablenames.Add(rm.Groups["cmdInsertTable"].Value);
                    sqlcmd.sqlCmdType = SQLCmdType.Insert;
                    sqlcmd.statementText = rm.Groups["cmdInsert"].Value;
                }
                else if (!string.IsNullOrEmpty(rm.Groups["cmdUpdate"].Value))
                {
                    sqlcmd.assignedTablenames.Add(rm.Groups["cmdUpdateTable"].Value);
                    sqlcmd.sqlCmdType = SQLCmdType.Update;
                    sqlcmd.statementText = rm.Groups["cmdUpdateTable"].Value;
                }
                else if (!string.IsNullOrEmpty(rm.Groups["cmdDelete"].Value))
                {
                    sqlcmd.assignedTablenames.Add(rm.Groups["cmdDeleteTable"].Value);
                    sqlcmd.sqlCmdType = SQLCmdType.Delete;
                    sqlcmd.statementText = rm.Groups["cmdDeleteTable"].Value;
                }
                else if (!string.IsNullOrEmpty(rm.Groups["cmdSelect"].Value))
                {
                    var rms = regex_SelectTableStatement.Matches(msg.payloadMessage);

                    foreach (Match m in rms)
                        if (sqlcmd.assignedTablenames.All(c => c != m.Groups["cmdSelectTable"].Value))
                            sqlcmd.assignedTablenames.Add(m.Groups["cmdSelectTable"].Value);

                    sqlcmd.sqlCmdType = SQLCmdType.Select;
                    sqlcmd.statementText = rm.Groups["cmdSelect"].Value;
                }
                else
                {
                    sqlcmd.sqlCmdType = SQLCmdType.Other;
                    sqlcmd.statementText = msg.payloadMessage;
                }

                //can we assign this sql session to a projection anyway?
                if (!sqlSessionInfo[msg.spid].touchesDprModuleTables)
                    if (sqlcmd.assignedTablenames.Any(t => t.ToUpper().StartsWith("DPR")))
                        sqlSessionInfo[msg.spid].touchesDprModuleTables = true;

                //assign non system table to this sql session, so we can match it to the projection step schema class name
                var payloadTables = sqlcmd.assignedTablenames.Where(t => regex_NonSystemTable.IsMatch(t)).ToArray();
                sqlcmd.isPayloadTableInvolved = payloadTables.Length > 0;

                foreach (string tabname in payloadTables)
                    if (!sqlSessionInfo[msg.spid].nonSystemTables.Any(t => string.Compare(t, tabname, true) == 0))
                        sqlSessionInfo[msg.spid].nonSystemTables.Add(tabname);

                //store a sql command 
                // when long running or 
                if (sqlcmd.durationMsec >= threshold_suspicious_duration_SqlCommand_msec || sqlcmd.isPayloadTableInvolved)
                    sqlSessionInfo[msg.spid].longRunningStatements.Add(sqlcmd); 
            }
			
			detectorStats.parseDuration += (DateTime.Now - procMsgStartpoint).TotalMilliseconds;			
		}


       

        internal class SqlSessionIntern: Datastore.SqlSession
        {
            public List<string> belongsToProjectionId = new List<string>();
            public bool touchesDprModuleTables;
            public List<string> nonSystemTables = new List<string>();

            public SqlSessionIntern()
            { }

            public SqlSession AsSqlSession(DateTime projectionStart, DateTime projectionEnd)
            {
                return (new SqlSession()
                {                    
                    dtTimestampStart= this.dtTimestampStart,
                    dtTimestampEnd = this.dtTimestampEnd,
                    isDataComplete = true,
                    isSuspicious = this.isSuspicious,
                    longRunningStatements = this.longRunningStatements.Where(l => l.dtTimestampStart >= projectionStart && l.dtTimestampEnd<= projectionEnd).ToList(),
                    transactions = this.transactions.Where(t => t.dtTimestampStart >= projectionStart &&t.dtTimestampEnd <= projectionEnd).ToList(),
                    message = this.message
                }
                );
            }
        }
         
    }
}
