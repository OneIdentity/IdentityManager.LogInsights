using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

using LogInsights.Helpers;
using LogInsights.Datastore;
using LogInsights.LogReader;


namespace LogInsights.Detectors
{
    class SyncStepDetailsDetector : DetectorBase, ILogDetector
    {
        //there are 3 main parts of interest
        //p1: query object lists
        //p2: reload objects by their keys after matching (in sync)
        //p3: query object(s) by key/filter/example (for adHoc)


        private Dictionary<string, List<SyncStepDetailBase>> systemConnectionGeneralQueryObjectInfos;  //key == spid


        //log level Debug SystemConnection
        //=================================

        private static string qObj_SysConnection_QueryByKey_Start = @"QueryObject: (?<keyprop>[^ ]*)(?<qtype>=)(?<keyval>.*?)(\n|$)";
        //e.g.: 2017-04-04 11:50:36.8233 DEBUG (SystemConnection ba9c6fe5-0278-4f8a-b455-54d28e7b31c1) : QueryObject: UID_ADSDomain@ADSDomain=cf6b49a0-e35f-4d2f-a5b1-b58559b5c7e3 

        private static string qObj_SysConnection_QueryByExample_Start = @"QueryObject: (?<qtype>Prototype) is (?<objectDisp>.*?) of (?<schemaClass>.*?).*Options: (?<options>.+)";
        //e.g. 2019-05-06 09:54:19.1717 DEBUG (SystemConnection c2c5e174-3ee8-49f8-b7dd-1a095b1473ee) : QueryObject: Prototype is Ralph Grohmann of  (user) 
        //2019-05-06 09:54:19.1717 DEBUG(SystemConnection c2c5e174-3ee8-49f8-b7dd-1a095b1473ee) :    Options: Properties: distinguishedName, objectClass, postOfficeBox, canonicalName, objectGUID, objectSid, vrtcanonicalName, vrtdistinguishedName

        private static string qObj_SysConnection_QueryByFilter_Start = @"QueryObjects: (?<qtype>Querying) objects of (?<schemaClass>.*?)\..*Options: (?<options>.+)";
        //e.g.: 2017-04-04 00:21:24.0385 DEBUG(SystemConnection 0c7724f4-5b9c-47d6-9beb-5a9f861c36a3) : QueryObjects: Querying objects of ADSContainer.
        //2017-04-04 00:21:24.0385 DEBUG (SystemConnection 0c7724f4-5b9c-47d6-9beb-5a9f861c36a3) : 	Options: NativeSystemFilter=False, ObjectDataFilter=False, ObjectFilter=False
        //   Properties: CanonicalName, vrtRevisionDate, XObjectKey, ObjectGUID, DistinguishedName
        //   UseReferenceScope = False

        private static string qObj_SysConnection_QueryByKey_Done = @"QueryObject: Object (not found|found but filtered by scope|found)";
        private static string qObj_SysConnection_QueryByExample_Done = @"QueryObjects result.*?out of scope.*?: (?<objCnt1>\d+).*?(QueryObjects .*?result.*?inside scope.*?: (?<objCnt2>\d+)|$)";
        //private static string qObj_SysConnection_QueryByFilter_Done = @""; //same as qObj_SysConnection_QueryByExample_Done

        private static Regex regex_QueryObject_SystemConnection_Start = new Regex($"{qObj_SysConnection_QueryByKey_Start}|{qObj_SysConnection_QueryByExample_Start}|{qObj_SysConnection_QueryByFilter_Start}", RegexOptions.Compiled | RegexOptions.Singleline);
        private static Regex regex_QueryObject_SystemConnection_Done = new Regex($"{qObj_SysConnection_QueryByKey_Done}|{qObj_SysConnection_QueryByExample_Done}", RegexOptions.Compiled | RegexOptions.Singleline);


        private static Regex regex_ReloadObject_SystemConnection_Start = new Regex(@"ReloadObject: Reloading (?<objCnt>\d+) objects of type (?<schemaClassDisp>.*?) \((?<schemaClass>.*?)\)\.", RegexOptions.Compiled | RegexOptions.Singleline);
        /*eg.:2019-11-05 09:07:32.0934 DEBUG (SystemConnection e6725bce-31f2-42fa-b077-945cc6cc72e7) : ReloadObject: Reloading 1 objects of type LDAP user accounts (LDAPAccount). 
              2019-11-05 09:07:32.0934 DEBUG (SystemConnection e6725bce-31f2-42fa-b077-945cc6cc72e7) :          Options: ExceptionHandling=ContinueOnError
                Properties: CanonicalName, DistinguishedName, ModifyTimeStamp, ObjectGUID             */

        private static Regex regex_ReloadObject_SystemConnection_Done = new Regex(@"ReloadObject-Result: (?<objCnt1>\d+) system objects.", RegexOptions.Compiled | RegexOptions.Singleline);
        //e.g: 2019-11-05 09:07:32.1403 DEBUG (SystemConnection e6725bce-31f2-42fa-b077-945cc6cc72e7) :          ReloadObject-Result: 1 system objects. 




        //log level Debug SystemConnector
        //==================================
        private static string qObj_SysConnector_QueryByKey_Start = @"Querying object (?<qtype>by key).";
        //e.g.: 2018-06-25 08:02:18.6249 DEBUG (SystemConnector 0b3fc0cf-b72b-41b7-8b82-b7d19f6d552d) : Querying object by key. 
        
        private static string qObj_SysConnector_QueryByExample_Start = @"Querying objects..*...(?<qtype>ByExample)..*(?<data>Example:.*Object: (?<objectDisp>.*?)SchemaType: (?<schemaClass>.*?) .*)";
        /*e.g.: 
          2019-05-06 09:54:19.1717 DEBUG (SystemConnector fa100430-27f2-4987-8771-42ecf3fbf2e9) : Querying objects... 
          2019-05-06 09:54:19.1717 DEBUG (SystemConnector fa100430-27f2-4987-8771-42ecf3fbf2e9) : 	...ByExample. 
          2019-05-06 09:54:19.1717 DEBUG(SystemConnector fa100430-27f2-4987-8771-42ecf3fbf2e9) :     Example:
	        Object: Ralph Grohmann
            SchemaType: user
            State: New
            Uid: 8c4ebb12-d1d5-4883-bc91-d581409dc862
            LoadedProperties: 
            ChangedProperties: 
	        IsChanged: False
            IsDifferent: False
            Values:
            ... */

        private static string qObj_SysConnector_QueryByFilter_Start = @"Querying objects..*...*...(?<qtype>ByFilter).";
        /*e.g.: 2019-05-06 09:54:01.0253 DEBUG (SystemConnector fa100430-27f2-4987-8771-42ecf3fbf2e9) : Querying objects... 
                2019-05-06 09:54:01.0253 DEBUG (SystemConnector fa100430-27f2-4987-8771-42ecf3fbf2e9) : 	...ByFilter.          
             */

        private static string qObj_SysConnector_QueryByKey_Done = @"Result: State \((?<state>[A-Za-z ]+)\) Objects \((?<objCntOK>\d+)\) Failures \((?<objCntFailure>\d+)\)";
        //e.g.: 2018-06-25 08:02:18.6249 DEBUG (SystemConnector 0b3fc0cf-b72b-41b7-8b82-b7d19f6d552d) : 	Result: State (Success) Objects (1) Failures (0) 

        private static string qObj_SysConnector_QueryByExample_Done = @"Dumping the first of them:(?<example>..*Object: (?<objDisp>.*)SchemaType.*?($|Performing post filtering.+Filter results in (?<objCntFinallyOK>\d+) objects))"; //additionally to qObj_SysConnector_QueryByKey_Done
        /*e.g.: 2019-05-06 09:54:19.1873 DEBUG (SystemConnector fa100430-27f2-4987-8771-42ecf3fbf2e9) : 	Dumping the first of them:
	        Object: CN=Ralph Grohmann,CN=Users,DC=mentislab,DC=lan
	        SchemaType: user
	        State: Loaded
	        Uid: e9789893-ca98-44a3-9618-4275b50ec741
	        LoadedProperties: vrtparentDn, cn, distinguishedName, objectClass, postOfficeBox, canonicalName, objectGUID, objectSid, vrtcanonicalName, vrtdistinguishedName
	        ChangedProperties: 
	        IsChanged: False
	        IsDifferent: False
	        Values:
            ...
        */
                    
        //private static string qObj_SysConnector_QueryByFilter_Done = @"";  //same as qObj_SysConnector_QueryByKey_Done


        private static Regex regex_QueryObject_SystemConnector_Start = new Regex($"{qObj_SysConnector_QueryByKey_Start}|{qObj_SysConnector_QueryByExample_Start}|{qObj_SysConnector_QueryByFilter_Start}", RegexOptions.Compiled | RegexOptions.Singleline);
        private static Regex regex_QueryObject_SystemConnector_Done = new Regex($"{qObj_SysConnector_QueryByKey_Done}.*?({qObj_SysConnector_QueryByExample_Done}|)", RegexOptions.Compiled | RegexOptions.Singleline);


        private static Regex regex_ReloadObject_SystemConnector_Start = new Regex(@"Reloading objects.*Reloading (?<objCnt>\d+) objects of schema type (?<schemaClass>.*?)\.", RegexOptions.Compiled | RegexOptions.Singleline);
        /*eg.:2019-11-05 09:07:32.0934 DEBUG (SystemConnector 4d8d4581-abea-42a4-b995-23cad433dc83) : Reloading objects. 
            2019-11-05 09:07:32.0934 DEBUG (SystemConnector 4d8d4581-abea-42a4-b995-23cad433dc83) :         Reloading 1 objects of schema type LDAPAccount. */

        private static Regex regex_ReloadObject_SystemConnector_Done = new Regex(@"Result: State \((?<state>[A-Za-z ]+)\) Failures \((?<objCntFailure>\d+)\)", RegexOptions.Compiled | RegexOptions.Singleline);
        //eg.:2019-11-05 09:07:32.1403 DEBUG (SystemConnector 4d8d4581-abea-42a4-b995-23cad433dc83) :         Result: State (Success) Failures (0) 




        public SyncStepDetailsDetector() : base(TextReadMode.StreamGroupMessage)
        { }

        public override string caption
        {
            get
            {
                return "Synchronization step details";
            }
        }

        public override string identifier
        {
            get
            {
                return "#SyncStepDetailsDetector";
            }
        }

        public override string[] requiredParentDetectors
        {
            get
            {
                return new string[] { "#ConnectorsDetector" };
            }
        }

        public void FinalizeDetector()
        {
            logger.Debug("entering FinalizeDetector()");

            //send last msg to close the message group
            isFinalizing = true;
            ProcessMessage(null);
            			
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();

            var projectionActivity = _datastore.GetOrAdd<ProjectionActivity>();

            //for each found sync step we try to find a projection with a sync step that matches our found ones
            foreach (var kp_listSyncStepDetail in systemConnectionGeneralQueryObjectInfos)
            {
                var systemConnectionId = kp_listSyncStepDetail.Key;
                var syncStepDetails = kp_listSyncStepDetail.Value;

                logger.Debug($"handling sync step detail for SystemConnID {systemConnectionId}");

                var potListOfProjections = projectionActivity.Projections
                                                .Where(p => p.systemConnectors.Any(sc => sc.loggerSourceId == systemConnectionId));

                if (potListOfProjections.HasNoData())
                {
                    logger.Warning("potListOfProjections Has No Data!");
                    continue;  // :-(
                }
             
                //only the combination of systemConnectionId + lstSyncStepDetail[i].firstOccurrence is to take to assign this detail to a projection
                Dictionary<Projection, List<SyncStepDetailBase>> projStepDict = SyncStepDetailBase.RelateListOfStepDetailsToProjections(syncStepDetails, potListOfProjections.ToArray());
                                
                if (projStepDict.HasNoData())
                {
                    logger.Warning("projStepDict Has No Data!");
                    continue;  // :-(
                }


                foreach (Projection projection in projStepDict.Keys)
                {
                    foreach (SyncStepDetailBase syncStepDetail in projStepDict[projection])
                    {
                        //find the step
                        var conSide = projection.systemConnectors.Where(c => c.loggerSourceId == systemConnectionId).First().belongsToSide;
                        logger.Trace($"{syncStepDetail}: find the step; conSide = {conSide.ToString()}");

                        var stepListWithoutClasscheck = projection.projectionSteps
                            .Where(s => s.dtTimestampStart.LessThan(syncStepDetail.firstOccurrence) &&
                                        s.dtTimestampEnd.MoreThan(syncStepDetail.lastOccurrence))
                            .ToArray();

                        if (stepListWithoutClasscheck.HasNoData()) //we found object list load jobs outside a step; but we've filtered exactly this in the LINQ above :-o
                        {
                            logger.Trace("stepListWithoutClasscheck Has No Data; we found object list load jobs outside a step; but we've filtered exactly this in the LINQ above :-o");
                            continue;
                        }

                        ProjectionStep[] stepListWithClasscheck = new ProjectionStep[] { };

                        if (!string.IsNullOrWhiteSpace(syncStepDetail.queryObjectInformation.schemaClassName))
                            stepListWithClasscheck = stepListWithoutClasscheck.Where(s => syncStepDetail.queryObjectInformation.schemaClassName ==
                                                                                            (conSide == SystemConnBelongsTo.IdentityManagerSide ?
                                                                                                s.leftSchemaClassName : s.rightSchemaClassName)
                                                                                    ).ToArray();

                        
                        //if we found a step where the object class name matches -> perfect
                        if (stepListWithClasscheck.Length == 1)
                        {
                            logger.Trace($"PutSyncStepDetail stepListWithClasscheck");

                            stepListWithClasscheck[0].syncStepDetail.PutSyncStepDetail(syncStepDetail.queryObjectInformation, conSide);
                            detectorStats.numberOfDetections++;
                            continue;
                        }

                        //still searching but we found exactly one step that matches the timestamp but not the class name -> take it as additional object load job
                        if (stepListWithClasscheck.Length == 0 && stepListWithoutClasscheck.Length == 1)
                        {
                            logger.Trace($"PutSyncStepDetail stepListWithoutClasscheck");

                            stepListWithoutClasscheck[0].syncStepDetail.PutSyncStepDetail(syncStepDetail.queryObjectInformation, conSide);
                            detectorStats.numberOfDetections++;
                            continue;
                        }

                        //still searching, no exact step match, seems to be a retry job or a "phase #2" job and the timestamp is not selective enough
                        if (stepListWithClasscheck.Length > 1 || stepListWithoutClasscheck.Length > 1)
                        {
                            ProjectionStep step = null;

                            //which list is to take
                            if (stepListWithClasscheck.Length > 1)
                                step = stepListWithClasscheck.FirstOrDefault(s => syncStepDetail.queryObjectInformation.schemaClassName ==
                                                                                  (conSide == SystemConnBelongsTo.IdentityManagerSide ?
                                                                                      s.leftSchemaClassName :
                                                                                      s.rightSchemaClassName
                                                                                  ));

                            if (step != null)
                            {
                                logger.Warning("PutSyncStepDetail syncStepDetail");

                                step.syncStepDetail.PutSyncStepDetail(syncStepDetail.queryObjectInformation, conSide);
                                detectorStats.numberOfDetections++;
                                continue;
                            }
                        }                         
                    }
                }
            }

            //stats
            detectorStats.detectorName = $"{GetType().Name} <{identifier}>";
            detectorStats.finalizeDuration = sw.ElapsedMilliseconds;
            _datastore.GetOrAdd<StatisticsStore>().DetectorStatistics.Add(detectorStats);
            logger.Debug(detectorStats.ToString());

            //dispose
            systemConnectionGeneralQueryObjectInfos = null;
        }

        public void InitializeDetector()
        {
            systemConnectionGeneralQueryObjectInfos = new Dictionary<string, List<SyncStepDetailBase>>();   

            detectorStats.Clear();
            isFinalizing = false;
        }

        public void ProcessMessage(LogEntry msg)
        {
            if (!_isEnabled)
                return;
			
			long tcStart = Environment.TickCount64;

            //if (msg != null && (msg.loggerSource != "ProjectorEngine" && !msg.loggerSource.StartsWith("SystemCon")))
            if (msg != null && (msg.Spid == "" || !msg.Logger.StartsWith("SystemCon")))
                return;


            var rt = ProcessMessageBase(ref msg);
            if (rt != null)
                foreach (var xmsg in rt)
                    ProcessMessage(xmsg);
            if (msg == null)
                return;
             

			detectorStats.numberOfLinesParsed += msg.NumberOfLines;


            bool ret = false;
            if (msg.Logger == "SystemConnection")
            {
                ret = _HandleSystemConnectionMessages(msg, regex_QueryObject_SystemConnection_Start, regex_QueryObject_SystemConnection_Done, QueryByEnum.UnknownQueryType);

                if (!ret)
                    ret = _HandleSystemConnectionMessages(msg, regex_ReloadObject_SystemConnection_Start, regex_ReloadObject_SystemConnection_Done, QueryByEnum.ReloadObject);
            }

            if (msg.Logger == "SystemConnector")
            {
                ret = _HandleSystemConnectorMessages(msg, regex_QueryObject_SystemConnector_Start, regex_QueryObject_SystemConnector_Done, QueryByEnum.UnknownQueryType);

                if (!ret)
                    ret = _HandleSystemConnectorMessages(msg, regex_ReloadObject_SystemConnector_Start, regex_ReloadObject_SystemConnector_Done, QueryByEnum.ReloadObject);
            }

            detectorStats.parseDuration += new TimeSpan(Environment.TickCount64 - tcStart).TotalMilliseconds;
        }
        
        
        private bool _HandleSystemConnectionMessages(LogEntry msg, Regex regex_Start, Regex regex_Done, QueryByEnum qtypeInput)
        {
            bool fnd = false;


            //start (request) message
            var rm = regex_Start.Match(msg.FullMessage);
            if (rm.Success)
            {
                logger.Trace($"_HandleSystemConnectionMessages: regex match for rx regex_Start: {regex_Start.ToString()}");


                fnd = true;

                var connectionDetail = systemConnectionGeneralQueryObjectInfos.GetOrAdd(msg.Spid);

                string qObjDisp = "<NoSingleObject>"; 
                QueryByEnum qtypeEnum = QueryByEnum.UnknownQueryType;
                int objectsToRequest = 0;

                //query object part
                if (qtypeInput == QueryByEnum.UnknownQueryType)
                {
                    switch (rm.Groups["qtype"].Value.Trim())
                    {
                        case "=":
                            qtypeEnum = QueryByEnum.QueryByKey;
                            qObjDisp = $"{rm.Groups["keyprop"].Value.Trim()}={rm.Groups["keyval"].Value}";
                            break;

                        case "Querying":
                            qtypeEnum = QueryByEnum.QueryByFilter;
                            break;

                        case "Prototype":
                            qtypeEnum = QueryByEnum.QueryByExample;
                            qObjDisp = rm.Groups["objectDisp"].Value.Trim();
                            break;
                    }
                }
                else  //reload object part
                {
                    qtypeEnum = QueryByEnum.ReloadObject;
                    qObjDisp = "<unspec.>";
                    objectsToRequest = rm.Groups["objCnt"].Value.ToIntSave();
                }

                logger.Debug($"_HandleSystemConnectionMessages: found new connectionDetail for spid {msg.Spid}");
                connectionDetail.Add(new SyncStepDetailBase()
                    {
                        queryObjectInformation = new SyncStepDetailQueryObject()
                        {
                            dtTimestampStart = msg.TimeStamp,
                            queryType = qtypeEnum, 
                            queryObjectDisplay = qObjDisp,
                            systemConnType = SystemConnType.SystemConnection,
                            options = rm.Groups["options"].Value.Trim().Replace("\n", ""),
                            schemaClassName = rm.Groups["schemaClass"].Value.Trim(),                           
                            reloadObjectCountRequested = objectsToRequest,
                            message = msg
                        }
                    });
            }


            //finish (reply) message
            rm = regex_Done.Match(msg.FullMessage);
            if (rm.Success)
            {
                logger.Trace($"_HandleSystemConnectionMessages: regex match for rx regex_Done: {regex_Done.ToString()}");
                
                fnd = true;

                var connectionDetail = systemConnectionGeneralQueryObjectInfos.GetOrAdd(msg.Spid);
                if (connectionDetail.Count > 0)
                {
                    SyncStepDetailBase stepDetail = connectionDetail.Where(n => !n.queryObjectInformation.isDataComplete).LastOrDefault();

                    if (stepDetail == null)
                        return true;

                    int queryResult_NumberOfObjectsBeforeScope = rm.Groups["objCnt1"].Value.ToIntSave();
                    int queryResult_NumberOfObjectsAfterScope = 0;

                    //query object part
                    if (stepDetail.queryObjectInformation.queryType == QueryByEnum.UnknownQueryType || !string.IsNullOrEmpty(rm.Groups["objCnt2"].Value))
                    {
                        queryResult_NumberOfObjectsAfterScope = rm.Groups["objCnt2"].Value.ToIntSave();
                    }
                    else  //reload object part
                    {
                        queryResult_NumberOfObjectsAfterScope = queryResult_NumberOfObjectsBeforeScope;
                    }

                    stepDetail.queryObjectInformation.isSuccessful = msg.Level == LogLevel.Debug;
                    stepDetail.queryObjectInformation.isDataComplete = true;
                    stepDetail.queryObjectInformation.messageEnd = msg;
                    stepDetail.queryObjectInformation.dtTimestampEnd = msg.TimeStamp;
                    stepDetail.queryObjectInformation.queryResult_NumberOfObjectsBeforeScope = queryResult_NumberOfObjectsBeforeScope;
                    stepDetail.queryObjectInformation.queryResult_NumberOfObjectsAfterScope = queryResult_NumberOfObjectsAfterScope;
                }
            }

            return fnd;
        }

        private bool _HandleSystemConnectorMessages(LogEntry msg, Regex regex_Start, Regex regex_Done, QueryByEnum qtypeInput)
        {
            bool fnd = false;


            //start (request) message
            var rm = regex_Start.Match(msg.FullMessage);
            if (rm.Success) 
            {
                logger.Trace($"_HandleSystemConnectorMessages: regex match for rx regex_Start: {regex_Start.ToString()}");

                fnd = true;

                var connectionDetail = systemConnectionGeneralQueryObjectInfos.GetOrAdd(msg.Spid);
                 
                string qObjDisp = "<NoSingleObject>";
                string options = ""; 
                QueryByEnum qtypeEnum = QueryByEnum.UnknownQueryType;
                int objectsToRequest = 0;

                //query object part
                if (qtypeInput == QueryByEnum.UnknownQueryType)
                {
                    switch (rm.Groups["qtype"].Value.Trim())
                    {
                        case "by key":
                            qtypeEnum = QueryByEnum.QueryByKey;
                            break;

                        case "ByFilter":
                            qtypeEnum = QueryByEnum.QueryByFilter;
                            break;

                        case "ByExample":
                            qtypeEnum = QueryByEnum.QueryByExample;
                            qObjDisp = rm.Groups["objectDisp"].Value.Trim();
                            options = rm.Groups["data"].Value.Trim().Replace("\n", "");
                            break;
                    }
                }
                else  //reload object part
                {
                    qtypeEnum = QueryByEnum.ReloadObject;
                    qObjDisp = "<unspec.>";
                    objectsToRequest = rm.Groups["objCnt"].Value.ToIntSave();                    
                }


                logger.Debug($"_HandleSystemConnectorMessages: found new connectionDetail for spid {msg.Spid}");
                connectionDetail.Add(new SyncStepDetailBase()
                {
                    queryObjectInformation = new SyncStepDetailQueryObject()
                    {
                        dtTimestampStart = msg.TimeStamp,
                        queryType = qtypeEnum, 
                        queryObjectDisplay = qObjDisp,
                        systemConnType = SystemConnType.SystemConnector,
                        options = options,
                        schemaClassName = rm.Groups["schemaClass"].Value.Trim(),  
                        message = msg
                    }
                });
            }


            //finish (reply) message
            rm = regex_Done.Match(msg.FullMessage);
            if (rm.Success)
            {
                logger.Trace($"_HandleSystemConnectorMessages: regex match for rx regex_Done: {regex_Done.ToString()}");
                
                fnd = true;

                var connectionDetail = systemConnectionGeneralQueryObjectInfos.GetOrAdd(msg.Spid);
                if (connectionDetail.Count > 0)
                {
                    string qObjDisp = rm.Groups["objectDisp"].Value.Trim();

                    SyncStepDetailBase stepDetail = connectionDetail
                                                        .Where(n => !n.queryObjectInformation.isDataComplete 
                                                                    && (qObjDisp == "" || qObjDisp == n.queryObjectInformation.queryObjectDisplay))
                                                        .LastOrDefault();

                    if (stepDetail == null) //then the query object display does not fit
                        return true;


                    string state = rm.Groups["state"].Value;
                    //string data = rm.Groups["example"].Value.Trim();


                    //reloadObject provides: objCntFailure
                    //QueryByKey + QuerybyFilter provide: objCntOK; objCntFailure
                    //QueryByExample provides: objCntOK; objCntFailure; objCntFinallyOK (optionally)

                    int cntOkAfter = 0;
                    int cntFailure = rm.Groups["objCntFailure"].Value.ToIntSave();
                    int cntOk = rm.Groups["objCntOK"].Value.ToIntSave();


                    if (stepDetail.queryObjectInformation.queryType == QueryByEnum.UnknownQueryType || !string.IsNullOrEmpty(rm.Groups["objCntFinallyOK"].Value))
                    //if (qtypeInput == QueryByEnum.UnknownQueryType) 
                    {
                        string strCntOkFin = rm.Groups["objCntFinallyOK"].Value;
                        cntOkAfter = string.IsNullOrWhiteSpace(strCntOkFin) ? cntOk : strCntOkFin.ToIntSave();
                    }
                    else
                    {
                        if (cntFailure == 0 && state == "Success")
                            cntOkAfter = stepDetail.queryObjectInformation.reloadObjectCountRequested;
                        else
                            cntOkAfter = Math.Max(0, stepDetail.queryObjectInformation.reloadObjectCountRequested - cntFailure);

                        cntOk = cntOkAfter;
                    }

                     

                    stepDetail.queryObjectInformation.isSuccessful = (cntFailure == 0 && state == "Success") || (string.IsNullOrEmpty(state) && msg.Level == LogLevel.Debug);
                    stepDetail.queryObjectInformation.isDataComplete = true;
                    stepDetail.queryObjectInformation.messageEnd = msg;
                    stepDetail.queryObjectInformation.dtTimestampEnd = msg.TimeStamp;
                    stepDetail.queryObjectInformation.queryResult_NumberOfObjectsBeforeScope = cntOk;
                    stepDetail.queryObjectInformation.queryResult_NumberOfObjectsAfterScope = cntOkAfter;
                    stepDetail.queryObjectInformation.queryResult_NumberOfObjectsFailed = cntFailure;
                }
            }

            return fnd;
        }

    }  //class SyncStepDetailsDetector : DetectorBase, ILogDetector


}
