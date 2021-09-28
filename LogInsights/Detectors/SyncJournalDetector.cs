using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using LogInsights.Datastore;
using LogInsights.Helpers;
using LogInsights.LogReader;

namespace LogInsights.Detectors
{
    class SyncJournalDetector : DetectorBase, ILogDetector
    {
        //private static Regex rx_StartNewObject = new Regex(@"(?<tablename>DPRJournal[a-zA-Z]*): Creating new entity", RegexOptions.Compiled);
        //private static Regex rx_ChangeObject = new Regex(@"(?<tablename>DPRJournal[a-zA-Z]*).(?<columnname>[a-zA-Z_]*) = (?<value>.*)($|[\n\r])", RegexOptions.Compiled);

        private static Regex rx_sql_objectInsert = new Regex(@"insert into (?<tablename>DPRJournal[a-z]*) \((?<columns>.*?)\) values \((?<values>.*)\)", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);
        private static Regex rx_sql_objectUpdate = new Regex(@"Update (?<tablename>DPRJournal[a-z]*) set (?<columns>.*?) where .*UID_DPRJournal[a-z]*[ ]?=[ ]?'(?<uid>[a-f0-9-]*)'", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);

        private Dictionary<string, SqlObject> lstDprObject; // key==PK

        public SyncJournalDetector() : base(TextReadMode.SingleMessage)
        { }

        public override string caption => "Synchronization step journal";

        public override string identifier => "#SyncJournalDetector";

        public override string[] requiredParentDetectors => new string[] { "#ConnectorsDetector" };

        public void InitializeDetector()
        {
            lstDprObject = new Dictionary<string, SqlObject>();

            detectorStats.Clear();
            isFinalizing = false;
        }

        public void FinalizeDetector()
        {
            logger.Debug("entering FinalizeDetector()");

            /*
            //send last msg to close the message group
            isFinalizing = true;
            ProcessMessage(null);
            */
                        
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();

            var projectionActivity = _datastore.GetOrAdd<ProjectionActivity>();
            var statisticsStore = _datastore.GetOrAdd<StatisticsStore>();

            /*
             	DPRJournal 
		            <= DPRJournalSetup
			            <= DPRJournalSetup
		            <= DPRJournalObject
			            <= DPRJournalProperty
		            <= DPRJournalMessage
		            <= DPRJournalFailure
			            <= DPRJournalObject
				            <= DPRJournalProperty
			            => DPRProjectionConfigStep
		
		            => DPRProjectionConfig     (per JobRunParam)
		            => DPRProjectionStartInfo  (nur gefüllt für Syncs, nicht für AdHoc)
		            => DPRSystemVariableSet    (per JobRunParam)

             * */


            //===========================================
            //DPRJournal
            //===========================================
            Dictionary<string, DprJournal> dprJournalDict = lstDprObject
                                                                .Where(n => n.Value.tablename == "dprjournal" && n.Value.insertCmdWasCalled)
                                                                .ToDictionarySaveExt<SqlObject, DprJournal>();



            //===========================================
            //DPRJournalSetup
            //===========================================
            Dictionary<string, DprJournalSetupElems> dprJournalSetupDict = lstDprObject
                                                                            .Where(n => n.Value.tablename == "dprjournalsetup")
                                                                            .ToDictionarySaveExt<SqlObject, DprJournalSetupElems>();
            
            foreach (var kp in dprJournalSetupDict)
            {
                var jSetup = kp.Value as DprJournalSetupElems;
                string parent = jSetup.Get("UID_DPRJournalSetupParent");
                if (parent == "")
                {
                    jSetup.isTop = true;
                    string uidJournal = jSetup.Get("UID_DPRJournal");

                    if (uidJournal == "")
                        logger.Warning($"found a DPRJournalSetup ({jSetup.uuid}) without empty UID_DPRJournal!");
                    else
                    {
                        var journal = GetFromDictOrLogWarning(dprJournalDict, uidJournal, "DPRJournalSetup", kp.Value.uuid);
                        journal?.journalSetupElems.Add(jSetup);
                    }
                }
                else
                {
                    var journalSetup = GetFromDictOrLogWarning(dprJournalSetupDict, parent, "DPRJournalSetup", jSetup.uuid, "DPRJournalSetup");
                    journalSetup?.childElems.Add(jSetup);
                }
            }
            

            //===========================================
            // DPRJournalMessage
            //===========================================
            Dictionary<string, DprJournalMessage> dprJournalMsgDict = lstDprObject
                                                                            .Where(n => n.Value.tablename == "dprjournalmessage")
                                                                            .ToDictionarySaveExt<SqlObject, DprJournalMessage>();

            foreach (var kp in dprJournalMsgDict.OrderBy(kp => kp.Value.OrderSequenceId))
            {
                string uidJournal = kp.Value.Get("UID_DPRJournal");


                var journal = GetFromDictOrLogWarning(dprJournalDict, uidJournal, "DPRJournalMessage", kp.Value.uuid);
                journal?.journalMessages.Add(kp.Value);
            }


            //===========================================
            //DPRJournalFailure
            //===========================================
            Dictionary<string, DprJournalFailure> dprJournalFailureDict = lstDprObject
                                                                            .Where(n => n.Value.tablename == "dprjournalfailure")
                                                                            .ToDictionarySaveExt<SqlObject, DprJournalFailure>();
            foreach (var kp in dprJournalFailureDict)
            {
                string uidJournal = kp.Value.Get("UID_DPRJournal");

                var journal = GetFromDictOrLogWarning(dprJournalDict, uidJournal, "DPRJournalFailure", kp.Value.uuid);
                journal?.journalFailures.Add(kp.Value);
            }

            //===========================================
            //DPRJournalObject
            //===========================================
            Dictionary<string, DprJournalObject> dprJournalObjectDict = lstDprObject
                                                                            .Where(n => n.Value.tablename == "dprjournalobject")
                                                                            .ToDictionarySaveExt<SqlObject, DprJournalObject>();
            foreach (var kp in dprJournalObjectDict)
            {
                DprJournalObject dprJObj = kp.Value;

                string uidJournal = dprJObj.Get("UID_DPRJournal");
                string uidJournalFailure = dprJObj.Get("DPRJournalFailure");

                if (uidJournal == "" && uidJournalFailure == "")
                    logger.Warning($"found a DPRJournalObject ({dprJObj.uuid}) which has neither a link to a DPRJournal nor a link to DPRJournalFailure!");
                else
                {
                    //if the parent is a DPRJournalFailure record:
                    if (uidJournalFailure != "")
                    {
                        var journalFailure = GetFromDictOrLogWarning(dprJournalFailureDict, uidJournalFailure, "DPRJournalObject", dprJObj.uuid, "DPRJournalFailure");
                        journalFailure?.dprJournalObjects.Add(dprJObj);
                    }
                    else  //if the parent is a DPRJournal record:
                    {
                        var journal = GetFromDictOrLogWarning(dprJournalDict, uidJournal, "DPRJournalObject", kp.Value.uuid);
                        journal?.journalObjects.Add(dprJObj);
                    }
                }
            }
            
            //===========================================
            //DPRJournalProperty
            //===========================================
            Dictionary<string, DprJournalProperty> dprJournalPropDict = lstDprObject
                                                                            .Where(n => n.Value.tablename == "dprjournalproperty")
                                                                            .ToDictionarySaveExt<SqlObject, DprJournalProperty>();
            foreach (var kp in dprJournalPropDict)
            {
                string uidJournalObject = kp.Value.Get("UID_DPRJournalObject");

                var journalObject = GetFromDictOrLogWarning(dprJournalObjectDict, uidJournalObject, "DPRJournalProperty", kp.Value.uuid, "DPRJournalObject");
                journalObject?.dprJournalProperties.Add(kp.Value);
            }


            //===========================================
            //Finally
            logger.Info($"found {dprJournalDict.Count} DPRJournal objects and {projectionActivity.Projections.Count} projections.");
            TryToAssignDprJournalToAProjection(dprJournalDict);

            
            //and put into datastore
            foreach (var kp in dprJournalDict.Where(kp => kp.Value.belongsToProjectionUuid != ""))
            {
                DprJournal j = kp.Value;
                var projection = projectionActivity.Projections.FirstOrDefault(p => p.uuid == j.belongsToProjectionUuid);

                if (projection != null)
                    projection.projectionJournal = j;
            }


            sw.Stop();
            //stats
            detectorStats.detectorName = string.Format("{0} <{1}>", this.GetType().Name, this.identifier);
            detectorStats.finalizeDuration = sw.ElapsedMilliseconds;
            statisticsStore.DetectorStatistics.Add(detectorStats);
            logger.Debug(detectorStats.ToString());

            //dispose
            lstDprObject = null;
        }


        private T GetFromDictOrLogWarning<T>(Dictionary<string, T> dict, string key, string tablename, string pk, string referencedTablename = "DPRJournal")
        {
            if (dict.ContainsKey(key))
                return dict[key];
            else
                logger.Warning($"found an object {tablename} ({pk}) which refers an object {referencedTablename} ({key}) with was not recorded in this log file!");

            return default(T);
        }


        private void TryToAssignDprJournalToAProjection(Dictionary<string, DprJournal> dprJournalDict)
        {
            //assignment to a sync activity....  -> belongsToProjectionUuid
            var projections = _datastore.GetOrAdd<ProjectionActivity>().Projections;

            foreach (var kp in dprJournalDict)
            {
                DprJournal journal = kp.Value;

                logger.Debug($"TryToAssignDprJournalToAProjection: run for DPRJournal '{journal.uuid}'");


                if (projections.Count == 1)
                {
                    logger.Trace("if we only found one projection, all journal entries must belong to it");
                    journal.belongsToProjectionUuid = projections.First().uuid;
                    continue;
                }


                var projectionsInTimeRange = projections.Where(p => journal.dtTimestampStart.InRange(p.dtTimestampStart, p.dtTimestampEnd, 2000) &&
                                                                    journal.dtTimestampEnd.InRange(p.dtTimestampStart, p.dtTimestampEnd, 10 * 1000))
                    .ToArray();

                switch (projectionsInTimeRange.Length)
                {
                    case 0:
                        logger.Warning($"for DPRJournal '{journal.uuid}' no logged projection was found inside journal lifetime (from {journal.dtTimestampStart} to {journal.dtTimestampEnd}). This DPRJournal cannot be used!");
                        continue;
                    case 1:
                        journal.belongsToProjectionUuid = projectionsInTimeRange.First().uuid;
                        break;
                    default:
                    {
                        //more than one projection spans over this DPRJournal object
                        //first, take all projections away that already have an journal assigned
                        var projectionsInTimeRangeEx = projectionsInTimeRange.Where(p => dprJournalDict.All(x => x.Value.belongsToProjectionUuid != p.uuid)).ToArray();

                        switch (projectionsInTimeRangeEx.Length)
                        {
                            case 0:
                                logger.Warning($"for DPRJournal '{journal.uuid}' no logged projection was found inside journal lifetime (from {journal.dtTimestampStart} to {journal.dtTimestampEnd}). This DPRJournal cannot be used!");
                                continue;
                            case 1:
                                journal.belongsToProjectionUuid = projectionsInTimeRangeEx[0].uuid;
                                break;
                            default:
                            {
                                //more than one unassigned projection spans over this DPRJournal object
                                var projectionsInTimeRangeSameType = projectionsInTimeRangeEx.Where(p => p.projectionType == ProjectionType.Unknown ||
                                                                                                         p.projectionType == ProjectionType.AdHocProvision && journal.isAdHocProjection ||
                                                                                                         p.projectionType != ProjectionType.AdHocProvision && !journal.isAdHocProjection)
                                    .ToArray();

                                switch (projectionsInTimeRangeSameType.Length)
                                {
                                    case 0:
                                        logger.Warning($"for DPRJournal '{journal.uuid}' no logged projection of same projection type ({(journal.isAdHocProjection ? "AdHoc" : "Full")}) was found inside journal lifetime (from {journal.dtTimestampStart} to {journal.dtTimestampEnd}). This DPRJournal cannot be used!");
                                        continue;
                                    case 1:
                                        journal.belongsToProjectionUuid = projectionsInTimeRangeSameType[0].uuid;
                                        break;
                                    default:
                                    {
                                        //well, we still have more than 1 projection where the time range and the type matches

                                        //we can try the match 
                                        //Journal->JournalSetup[n1].OptionContextDisplay == projection.conn_TargetSystem 
                                        //Journal->JournalSetup[n2].OptionContextDisplay == projection.conn_IdentityManager 
                                        // ==> N1 == n2 ? ==> we got a projection, but onlyy if we found exactly 1 projection (count==1)                                        
                                        if (journal.journalSetupElems.Count == 2)
                                        {
                                            string conDisp1 = journal.journalSetupElems[0].Get("OptionContextDisplay").Trim();
                                            string conDisp2 = journal.journalSetupElems[1].Get("OptionContextDisplay").Trim();

                                            var projectionsInTimeRangeSameTypeSameConns = projectionsInTimeRangeSameType
                                                .Where(p => (p.conn_IdentityManager == conDisp1 && p.conn_TargetSystem == conDisp2) 
                                                            || (p.conn_IdentityManager == conDisp2 && p.conn_TargetSystem == conDisp1))
                                                .ToArray();

                                            if (projectionsInTimeRangeSameTypeSameConns.Length > 0)
                                            {
                                                if (projectionsInTimeRangeSameTypeSameConns.Length == 1)
                                                {
                                                    journal.belongsToProjectionUuid = projectionsInTimeRangeSameTypeSameConns[0].uuid;
                                                    continue;
                                                }

                                                //else try again below
                                            }
                                            else
                                            {
                                                logger.Warning($"for DPRJournal '{journal.uuid}' no logged projection of same projection type ({(journal.isAdHocProjection ? "AdHoc" : "Full")}) and equal connection displays was found inside journal lifetime (from {journal.dtTimestampStart} to {journal.dtTimestampEnd}). This DPRJournal cannot be used!");
                                                continue; //no, do not try again below
                                            }
                                        }

                                        //well, lets assign the closest start 
                                        var closestProjectionStart = projectionsInTimeRangeSameType
                                            .OrderBy(p => (p.dtTimestampStart - journal.dtTimestampStart).EnsurePositive())
                                            .First();

                                        journal.belongsToProjectionUuid = closestProjectionStart.uuid;
                                        logger.Warning($"for DPRJournal '{journal.uuid}' only a vague connection to a projection was found due to the closest difference between DPRJournal log entry and projection start log entry");
                                        break;
                                    }
                                }

                                break;
                            }
                        }

                        break;
                    }
                }
            } //foreach DPRJournal object
        }


        public void ProcessMessage(LogEntry msg)
        {
            if (!_isEnabled)
                return;

            long tcStart = Environment.TickCount64;

            //basic filter, we only need the sql log
            if ( msg == null ||
				 string.IsNullOrEmpty(msg.Spid) || 
				 !string.Equals(msg.Logger, "SqlLog", StringComparison.Ordinal) || 
				 msg.Level != LogLevel.Debug && 
				 msg.Level != LogLevel.Error)
                return;


            var rt = ProcessMessageBase(ref msg);
            if (rt != null)
                foreach (var xmsg in rt)
                    ProcessMessage(xmsg);
            if (msg == null)
                return;               
               
            //secondary filter
            if (!msg.FullMessage.Contains("DPRJournal"))
                return;
            
            detectorStats.numberOfLinesParsed += msg.NumberOfLines;


            //new object
            var rm_ObjNew = rx_sql_objectInsert.Match(msg.FullMessage);
            if (rm_ObjNew.Success)
            {
                string objectType = rm_ObjNew.Groups["tablename"].Value.ToLowerInvariant();
                string insertAttributeNames = rm_ObjNew.Groups["columns"].Value;
                string insertAttributeValues = rm_ObjNew.Groups["values"].Value;

                SqlObject dprObject;
                switch (objectType)
                {
                    case "dprjournal":
                        dprObject = new DprJournal();
                        break;

                    case "dprjournalsetup":
                        dprObject = new DprJournalSetupElems();
                        break;

                    case "dprjournalobject":
                        dprObject = new DprJournalObject();
                        break;

                    case "dprjournalfailure":
                        dprObject = new DprJournalFailure();
                        break;

                    case "dprjournalproperty":
                        dprObject = new DprJournalProperty();
                        break;

                    case "dprjournalmessage":
                        dprObject = new DprJournalMessage();
                        break;

                    default:
                        dprObject = new SqlObject(objectType);
                        break;
                }

                dprObject.dtTimestampStart = msg.TimeStamp;
                dprObject.message = msg;
                dprObject.loggerid = msg.Spid;

                dprObject.PutDataFromInsert(insertAttributeNames, insertAttributeValues);

                string pk = dprObject.GetPkValue();
                if (!lstDprObject.ContainsKey(pk))  //it can happen that we read the same NLog message by several logfiles
                {
                    logger.Debug($"new object of type {objectType} with PK = '{pk}' detected");
                    lstDprObject.Add(pk, dprObject);
                }
            } //new dprjournal* object 


            //change object
            var rm_ObjUpdate = rx_sql_objectUpdate.Match(msg.FullMessage);
            if (rm_ObjUpdate.Success)
            {
                string objectType = rm_ObjUpdate.Groups["tablename"].Value.ToLowerInvariant();
                string updatedColumns = rm_ObjUpdate.Groups["columns"].Value;
                string pkuid = rm_ObjUpdate.Groups["uid"].Value;


                if (lstDprObject.TryGetValue(pkuid, out var dprObj))
                {
                    logger.Debug($"found an update of object (type={objectType}; pk={pkuid}");
                    dprObj.PutDataFromUpdate(updatedColumns);

                    //special handling for DprJournal
                    if (objectType == "dprjournal")
                    {
                        dprObj.dtTimestampEnd = msg.TimeStamp;  //for the journal, we need this to have a time span for it

                        var updatedColumnLst = SqlHelper.GetValuePairsFromUpdateCmd(updatedColumns);
                        if (updatedColumnLst.TryGetValue("ProjectionState", out var newProjectionState))
                        {
                            if (string.Compare(newProjectionState, "Running", StringComparison.OrdinalIgnoreCase) == 0)
                                dprObj.dtTimestampStart = msg.TimeStamp;
                        }
                    }
                }                
            }
            ///change object
            

            detectorStats.parseDuration += new TimeSpan(Environment.TickCount64 -tcStart).TotalMilliseconds;
        }

    }
}
