using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

using LogfileMetaAnalyser.Datastore;
using LogfileMetaAnalyser.Helpers;


namespace LogfileMetaAnalyser.Detectors
{
    class ConnectorsDetector : DetectorBase, ILogDetector
    {
        private Dictionary<string, Datastore.SystemConnInfo> systemConnectorsAndConnections;
        private Dictionary<string, string> systemConnectorsHasSchemaDisplay;

        private Dictionary<string, DatastoreBaseDataPoint> projectorSessions;

        private static Regex regex_SystemConnTimestampConnect = new Regex(@"SystemConnection.+Connecting to |SystemConnector.+Connecting target system", RegexOptions.Compiled);
        private static Regex regex_SystemConnTimestampDisconnect = new Regex(@"SystemConnection.+Disconnecting connection|SystemConnector.+Disconnecting target system", RegexOptions.Compiled);

        private static Regex regex_SystemConnSchemaDetect_DetermineSide1 = new Regex(@"Settings schema \((?<schemaname>.*)\)", RegexOptions.Compiled);
        private static Regex regex_SystemConnSchemaDetect_DetermineSide2 = new Regex(@"Connection disconnected successfully \((?<schemaname>.*)\)", RegexOptions.Compiled);
        private static Regex regex_SystemConnSchemaDetect_DetermineSide3 = new Regex(@"Disconnecting connection .*?\((?<schemaname>.*)\)", RegexOptions.Compiled);

        private static Regex regex_SystemConnector_DetermineSide1 = new Regex(@"Connecting to (?<sidetype>.*)\.{3}", RegexOptions.Compiled);

        private static Regex regex_SystemConnect_Indicator_RightSide = new Regex(@"cn=|dc=|ou=|o=|[a-z]:(?!1433([a-z]|$))\d{2,5}|Database \([a-z\.:]+:[ ]|Connector for [^O]|Cross-Domain Identity Management|Universal Cloud Interface|PoshNet40|\.csvsys|Oracle E-Business System", RegexOptions.Compiled | RegexOptions.CultureInvariant);

        private static Regex[] regex_SystemConnSchemaDetect_DetermineSide =
        {
            regex_SystemConnSchemaDetect_DetermineSide1,
            regex_SystemConnSchemaDetect_DetermineSide2,
            regex_SystemConnSchemaDetect_DetermineSide3
        };

        private static Regex[] regex_SystemConnSideDetect_DetermineSide =
        {
            regex_SystemConnector_DetermineSide1
        };

        public ConnectorsDetector() : base(TextReadMode.SingleMessage)
        { }

        public override string caption => "Found OneIM connector communication";
            
        public override string identifier =>"#ConnectorsDetector";

        public override string[] requiredParentDetectors => new string[] {"#SyncStructureDetector"};
                  

       

        public void FinalizeDetector()
        {
            logger.Debug("entering FinalizeDetector()");
			DateTime finStartpoint = DateTime.Now;

            
            //some post work on the collected system connector information
            foreach (var c in systemConnectorsAndConnections)
            {
                c.Value.isDataComplete = true;
                if (c.Value.connectTimestamp.Count == 0)
                    c.Value.connectTimestamp.Add(c.Value.dtTimestampStart);
                if (c.Value.disconnectTimestamp.Count == 0)
                    c.Value.disconnectTimestamp.Add(c.Value.dtTimestampEnd);
            }


            //determine whether the systemconnector works for the target system connector or for the database connector
            DetectSystemConnectorSide();  //fill systemConnectorsAndConnections.Value.belongsToSide attrib

            //try to find for each SystemConnection its SystemConnector and vice versa
            TryToAssignAConnectionToConnectors();  //fill systemConnectorsAndConnections.Values.systemConnectionSpid attrib

            //try to find out to which projectorEngine Id the system connectors belong
            TryMatchConnectorIDToProjectorID();  //fill systemConnectorsAndConnections.Values.belongsToProjectorId attrib


            //for each projection we need to put the system connector information in
            var projections = _datastore.projectionActivity.projections;
            var projectionsToGetConnData = projections.Where(p => systemConnectorsAndConnections.Any(con => con.Value.belongsToProjectorId.Contains(p.loggerSourceId)));

            foreach (var proj in projectionsToGetConnData)
                foreach (var connectorInfo in systemConnectorsAndConnections.Where(c => c.Value.belongsToProjectorId.Contains(proj.loggerSourceId)))
                {
                    proj.systemConnectors.Add(connectorInfo.Value);
                    logger.Debug($"pushing to ds: systemConnectors <= {connectorInfo.Value.ToString()}");

                    detectorStats.numberOfDetections++;
                }


            //well, some logging
            bool[] fail;
            foreach (var conn in systemConnectorsAndConnections.Values)
            {
                fail = new bool[3] { false, false, false };

                fail[0] = conn.belongsToSide == SystemConnBelongsTo.Unknown;
                fail[1] = conn.systemConnType == SystemConnType.SystemConnector && string.IsNullOrEmpty(conn.systemConnectionSpid);
                fail[2] = conn.belongsToProjectorId.Count == 0;

                if (fail.Any(f => f))
                {
                    string s = string.Format("Unable to determine information for {0} {1} => {2}{3}{4}",
                                        conn.systemConnType.ToString(),
                                        conn.loggerSourceId,                                
                                        fail[0] ? "unknown side (OneIM or TS); " : "",
                                        fail[1] ? "unknown link to SystemConnection; " : "",
                                        fail[2] ? "unknown link to projection" : ""
                                    );

                    logger.Warning(s);
                }
                    
            }

            //stats
            detectorStats.detectorName = string.Format("{0} <{1}>", this.GetType().Name, this.identifier);
            detectorStats.finalizeDuration = (DateTime.Now - finStartpoint).TotalMilliseconds;
            _datastore.statistics.detectorStatistics.Add(detectorStats);
            logger.Debug(detectorStats.ToString());

            //dispose
            systemConnectorsAndConnections = null;
            projectorSessions = null;
            systemConnectorsHasSchemaDisplay = null;
        }


        public void InitializeDetector()
        {
            systemConnectorsAndConnections = new Dictionary<string, Datastore.SystemConnInfo>();            
            projectorSessions = new Dictionary<string, DatastoreBaseDataPoint>();
            systemConnectorsHasSchemaDisplay = new Dictionary<string, string>();
			detectorStats.Clear();
        }


        public void ProcessMessage(TextMessage msg)
        {
            if (!_isEnabled)
                return;
                       

            var rt = ProcessMessageBase(ref msg);
            if (rt != null)
                foreach (var xmsg in rt)
                    ProcessMessage(xmsg);
            if (msg == null)
                return;


            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();

            detectorStats.numberOfLinesParsed += msg.numberOfLines;    

            if (msg.spid != "" && 
                (msg.loggerSource == "SystemConnector" || msg.loggerSource == "SystemConnection"))
            {
                if (systemConnectorsAndConnections.ContainsKey(msg.spid))
                    systemConnectorsAndConnections[msg.spid].dtTimestampEnd = msg.messageTimestamp;
                else
                {
                    Datastore.SystemConnInfo data = new Datastore.SystemConnInfo()
                    {
                        dtTimestampStart = msg.messageTimestamp,
                        systemConnType = msg.loggerSource == "SystemConnector" ? SystemConnType.SystemConnector : SystemConnType.SystemConnection,
                        message = msg
                    };
                    systemConnectorsAndConnections.Add(msg.spid, data);
                    logger.Trace($"new systemConn info {data.systemConnType.ToString()} for spid {msg.spid}");
                }

                //try to assign the schema display to an system connector/connection
                if (!systemConnectorsHasSchemaDisplay.ContainsKey(msg.spid))
                    foreach (var regexGetSchema in regex_SystemConnSchemaDetect_DetermineSide)
                    {                     
                        var rm_SystemConnector_DetermineSide = regexGetSchema.Match(msg.messageText);
                        if (rm_SystemConnector_DetermineSide.Success)
                        {
                            logger.Trace($"regex match for rx regexGetSchema: {regexGetSchema.ToString()}");

                            string schemaname = rm_SystemConnector_DetermineSide.Groups["schemaname"].Value;
                            systemConnectorsHasSchemaDisplay.Add(msg.spid, schemaname);
                        }
                    }

                //trying to specify the side (left or right)
                if (systemConnectorsAndConnections[msg.spid].belongsToSide == SystemConnBelongsTo.Unknown && msg.loggerSource == "SystemConnection")
                    foreach(var regexSide in regex_SystemConnSideDetect_DetermineSide)
                    {
                        var rm_SystemConnector_DetermineSide = regexSide.Match(msg.messageText);
                        if (rm_SystemConnector_DetermineSide.Success)
                        {
                            logger.Trace($"regex match for rx regexSide: {regexSide.ToString()}");

                            string side = rm_SystemConnector_DetermineSide.Groups["sidetype"].Value;

                            if (side.Contains("One Identity Manager"))
                                systemConnectorsAndConnections[msg.spid].belongsToSide = SystemConnBelongsTo.IdentityManagerSide;
                            else
                                systemConnectorsAndConnections[msg.spid].belongsToSide = SystemConnBelongsTo.TargetsystemSide;
                        }
                    }

                //get timestamps for connecting and disconnecting
                if (systemConnectorsAndConnections[msg.spid].connectTimestamp.Count == systemConnectorsAndConnections[msg.spid].disconnectTimestamp.Count)
                    if (regex_SystemConnTimestampConnect.IsMatch(msg.messageText))
                    {
                        logger.Trace($"regex match for rx regex_SystemConnTimestampConnect: {regex_SystemConnTimestampConnect.ToString()}");
                        systemConnectorsAndConnections[msg.spid].connectTimestamp.Add(msg.messageTimestamp);
                    }

                if (systemConnectorsAndConnections[msg.spid].connectTimestamp.Count > systemConnectorsAndConnections[msg.spid].disconnectTimestamp.Count)
                    if (regex_SystemConnTimestampDisconnect.IsMatch(msg.messageText))
                    {
                        logger.Trace($"regex match for rx regex_SystemConnTimestampDisconnect: {regex_SystemConnTimestampDisconnect.ToString()}");
                        systemConnectorsAndConnections[msg.spid].disconnectTimestamp.Add(msg.messageTimestamp);
                    }
            }

            sw.Stop();
            detectorStats.parseDuration += sw.ElapsedMilliseconds;
        }


        private void TryToAssignAConnectionToConnectors()
        {
            foreach (var systemConnection in systemConnectorsAndConnections.Values
                                                                                .Where(t => t.systemConnType == SystemConnType.SystemConnection && 
                                                                                            t.connectTimestamp.Count > 0 && 
                                                                                            t.disconnectTimestamp.Count > 0))
            {
                var systemConnectorCandidates = systemConnectorsAndConnections.Values
                                                    .Where(t =>
                                                        t.systemConnType == SystemConnType.SystemConnector &&
                                                        t.belongsToSide == systemConnection.belongsToSide &&
                                                        t.systemConnectionSpid == "" &&
                                                        t.connectTimestamp.Count > 0 &&
                                                        t.disconnectTimestamp.Count > 0
                                                    ).ToArray();

                //match, when systemConnection has at least one connect/disconnect pair with a Connector
                for (int connectNum = 0; connectNum < systemConnection.connectTimestamp.Count; connectNum++)
                {
                    foreach (var candi in systemConnectorCandidates)
                        for (int candiConnectNum = 0; candiConnectNum < candi.connectTimestamp.Count; candiConnectNum++)
                            if (systemConnection.connectTimestamp[connectNum].AlmostEqual(candi.connectTimestamp[candiConnectNum], 1500) &&
                                systemConnection.disconnectTimestamp[connectNum].AlmostEqual(candi.disconnectTimestamp[candiConnectNum], 1500)
                               )
                            {
                                candi.systemConnectionSpid = systemConnection.loggerSourceId;

                                logger.Trace($"TryToAssignAConnectionToConnectors: found systemConnectionSpid {candi.systemConnectionSpid} for {candi.ToString()}");
                                break;
                            }
                }

            }
        }


        private void TryMatchConnectorIDToProjectorID()   
        {
            var projections = _datastore.projectionActivity.projections;

            //good luck case: if we only found one projection, all log entries must belong to it            
            if (projections.Count == 1)
            {
                foreach (var conn in systemConnectorsAndConnections.Values)
                    conn.belongsToProjectorId.Add(projections[0].loggerSourceId);

                logger.Trace("TryMatchConnectorIDToProjectorID: if we only found one projection, all log entries must belong to it");
                return;
            }


            //hmm, we have more than 1 projections and several SystemConnector log entries to get matched to the projections

            
            //find connections that where created outside a projection (reused connections)

            List<TimeRangeMatchCandidate> projectionCanditateMatrix = projections
                                                                        .Where(p => !string.IsNullOrEmpty(p.conn_IdentityManager) && !string.IsNullOrEmpty(p.conn_TargetSystem))
                                                                        .Select(p => new TimeRangeMatchCandidate(p.loggerSourceId,
                                                                                                                 p.dtTimestampStart, 
                                                                                                                 p.dtTimestampEnd, 
                                                                                                                 p.conn_IdentityManager, 
                                                                                                                 p.conn_TargetSystem))
                                                                        .ToList();

            logger.Trace($"TryMatchConnectorIDToProjectorID: projectionCanditateMatrix.Count = {projectionCanditateMatrix.Count}");
            string errorString = $"Unable to find a projection job where this SystemConnection:";

            if (projectionCanditateMatrix.Count > 0)
                foreach (var conn in systemConnectorsAndConnections.Values.Where(t => t.systemConnType == SystemConnType.SystemConnection))
                {
                    if (conn.connectTimestamp.Count == 0) //well, either this connection was used to work on something without connecting to the systems or the event was outside this logfile
                    {
                        logger.Trace($"TryMatchConnectorIDToProjectorID: {errorString} '{conn.ToString()}'\nBad :(  well, either this connection was used to work on something without connecting to the systems or the event was outside this logfile");
                        continue;
                    }

                    if (!systemConnectorsHasSchemaDisplay.ContainsKey(conn.loggerSourceId))
                    {
                        logger.Warning($"TryMatchConnectorIDToProjectorID: {errorString} '{conn.ToString()}'\nNo further information was found in the logfile. Either at runtime the log level was not set to debug or trace or the start of the projection is outside the log file(s)");
                        continue;
                    }

                    Helpers.PeriodsMatchCandidate systemConnectionTotest = new PeriodsMatchCandidate(conn.connectTimestamp,
                                                                                                     conn.disconnectTimestamp,
                                                                                                     systemConnectorsHasSchemaDisplay[conn.loggerSourceId]);

                    var allCandidateMatches = Helpers.Closeness<int>.GetKeyOfBestMatch(systemConnectionTotest, projectionCanditateMatrix);

                    conn.belongsToProjectorId.AddRange(allCandidateMatches.Select(t => t.key));

                    //also push this result to the connectors
                    foreach (var connectors in systemConnectorsAndConnections.Values.Where(t => t.systemConnectionSpid == conn.loggerSourceId))
                        connectors.belongsToProjectorId = conn.belongsToProjectorId;
                }
            else
                logger.Warning($"TryMatchConnectorIDToProjectorID: {errorString}\nNo canditate string compare matrix found to assign a SystemConn to a projector id (connectors.belongsToProjectorId)");

            /*
            //#2: find connection that where created inside a projection
            List<TimeRangeMatchCandidate> projectionCanditateMatrix2 = projections
                                                                        .Select(p => new TimeRangeMatchCandidate(p.loggerSourceId, 
                                                                                                                 p.firstOccurrence,
                                                                                                                 p.lastOccurrence,
                                                                                                                 p.conn_IdentityManager,
                                                                                                                 p.conn_TargetSystem))
                                                                        .ToList();
            if (projectionCanditateMatrix2.Count > 0)
                foreach (var conn in systemConnectorsAndConnections.Values.Where(t => t.systemConnType == SystemConnType.SystemConnection))
                {
                    Helpers.TimeRangeMatchCandidate tr = new TimeRangeMatchCandidate("", conn.firstOccurrence,
                                                                                     conn.lastOccurrence,
                                                                                     systemConnectorsHasSchemaDisplay.GetOrReturnDefault(conn.loggerSourceId));

                    var bestMatch = Helpers.Closeness<int>.GetKeyOfBestMatch(tr, projectionCanditateMatrix2, 10d, 5d, 12d);

                    conn.belongsToProjectorId.Add(projections.ElementAt(bestMatch.Key).loggerSourceId);

                    FireOnDatastoreChangedEvent(conn.firstOccurrence, conn.loggerSourceId, "SYSTEMCON.belongsToProjectorId set");
                }
                */
        }

        private void DetectSystemConnectorSide()
        {
            if (!systemConnectorsAndConnections.Any(t => t.Value.belongsToSide == SystemConnBelongsTo.Unknown))
                return;

            Dictionary<string, string> matchMatrix_Left = new Dictionary<string, string>();
            Dictionary<string, string> matchMatrix_Right = new Dictionary<string, string>();

            foreach (var proj in _datastore.projectionActivity.projections)
                foreach (var projstep in proj.projectionSteps)
                {
                    matchMatrix_Left.AddOrUpdate(proj.loggerSourceId, projstep.leftConnection);
                    matchMatrix_Right.AddOrUpdate(proj.loggerSourceId, projstep.rightConnection);
                }
                        
                        
            foreach (var conn in systemConnectorsAndConnections.Where(c => c.Value.belongsToSide == SystemConnBelongsTo.Unknown))
            {
                if (!systemConnectorsHasSchemaDisplay.ContainsKey(conn.Key))  //hmm
                {
                    logger.Warning($"DetectSystemConnectorSide: for SystemConn {conn.Key} there was no schema display found!");
                    continue;
                }

                //positive keys that indicate a system connection to the target system
                if (regex_SystemConnect_Indicator_RightSide.IsMatch(systemConnectorsHasSchemaDisplay[conn.Key]))
                {
                    logger.Trace($"DetectSystemConnectorSide: positive keys that indicate a system connection to the target system: '{systemConnectorsHasSchemaDisplay[conn.Key]}' for {conn.Value.ToString()}");
                    conn.Value.belongsToSide = SystemConnBelongsTo.TargetsystemSide;
                    continue;
                }

                //we need a string compare with the projection's connection display
                logger.Trace($"DetectSystemConnectorSide: we need a string compare with the projection's connection display for {conn.Value.ToString()} - matchMatrix_Left.Count = {matchMatrix_Left.Count}; matchMatrix_Right.Count = {matchMatrix_Right.Count}");
                if (matchMatrix_Left.Count > 0 && matchMatrix_Right.Count > 0)
                    try
                    {
                        KeyValuePair<string, int> likeliness_connector_left = Closeness<string>.GetKeyOfBestMatch<string>(systemConnectorsHasSchemaDisplay[conn.Key], matchMatrix_Left);
                        KeyValuePair<string, int> likeliness_connector_right = Closeness<string>.GetKeyOfBestMatch<string>(systemConnectorsHasSchemaDisplay[conn.Key], matchMatrix_Right);

                        if (likeliness_connector_left.Value < likeliness_connector_right.Value)  //SystemConnector is more likely OneIM side
                            conn.Value.belongsToSide = SystemConnBelongsTo.IdentityManagerSide;
                        else
                            conn.Value.belongsToSide = SystemConnBelongsTo.TargetsystemSide;
                    }
                    catch { }
            }

        }
                 
    }
}
