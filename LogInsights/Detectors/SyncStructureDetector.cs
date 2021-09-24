using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

using LogInsights.Datastore;
using LogInsights.Helpers;

namespace LogInsights.Detectors
{
    /// <summary>
    /// this detector has basically the job to find projection activities in the logfile and determine the structure
    /// - check if this is an AdHoc Provisioning job or FullProjection/Sync => activity
    /// - assign SystemConnection and SystemConnector messages to such an activity  => connection
    /// - try to to determine the type of targetsystem the connection is linked to => target system (type)
    /// </summary>
    public class SyncStructureDetector : DetectorBase, ILogDetector
    {
        //log level Info:
        private static Regex regex_SyncActivity = new Regex(@"Running synchronization configuration \((?<SyncStartUpConfig>.+)\)|Processing sync configuration \((?<SyncStartUpConfig>.+)\)|Performing an adhoc projection on.*Object: (?<AdHocObject>(.+))$", RegexOptions.Compiled|RegexOptions.Singleline);

        //log level Debug:
        private static Regex regex_SyncActivityDone = new Regex(@"Sync done.", RegexOptions.Compiled);

        //log level Debug:
        private static Regex regex_SyncActivityCycleRun = new Regex(@"Starting execution cycle #\d+", RegexOptions.Compiled);

        //log level Debug:
        private static Regex regex_SyncStep = new Regex(@"Executing projection step \((?<stepid>.*?)\)$|Executing projection step \((?<stepid>.*?)\)\.[ ]*[\r\n].*?Direction \(effective\): (?<direction>[a-zA-Z]+).*[ ]{0,3}[\r\n].*?(AdHocObject|Object to project): (?<AdHocObject>.*)[ ]{0,3}[\r\n].*?UseRevision \(effective\): (?<UseRevicion>(False|True)).*LeftConnection: (?<LeftCon>.+)[ ]{0,3}[\r\n].*?SchemaClassLeft: (?<LeftSchemaClass>.+)[ ]{0,3}[\r\n].*?RightConnection: (?<RightCon>.+)[ ]{0,3}[\r\n].*?SchemaClassRight: (?<RightSchemaClass>.+)[ ]{0,3}[\r\n].*?Map: (?<Map>.*?)[ ]{0,3}[\r\n](IsFiltered)?", RegexOptions.Compiled|RegexOptions.Singleline);

        //log level Debug:
        private static Regex regex_SyncFinalizeInitiated = new Regex(@"Performing finalization tasks", RegexOptions.Compiled);



        private Dictionary<string, List<Projection>> projections;


        public override string caption
        {
            get
            {
                return "Found synchronization structure";
            }
        }
           

        public override string identifier
        {
            get
            {
                return "#SyncStructureDetector";
            }
        }

        public override string[] requiredParentDetectors
        {
            get
            {
                return new string[] { "#TimeRangeDetector" };
            }        
        }


        public void FinalizeDetector()
        {
            logger.Debug("entering FinalizeDetector()");

            //to close a probably unclosed group message
            isFinalizing = true;
            ProcessMessage(null);

            long tcStart = Environment.TickCount64;

            var generalLogData = _datastore.GetOrAdd<GeneralLogData>();
            var projectionActivity = _datastore.GetOrAdd<ProjectionActivity>();
            var statisticsStore = _datastore.GetOrAdd<StatisticsStore>();

            //correct the projections start time
            foreach (Projection proj in projections.Values.SelectMany(p => p).Where(p => p.dtTimestampStart.IsNull()))
                proj.dtTimestampStart = generalLogData.LogDataOverallTimeRangeStart;

            //finish the projections
            foreach (Projection proj in projections.Values.SelectMany(p => p).OrderBy(o => o.dtTimestampStart))
            {
                //if (!proj.isDataComplete)
                //    proj.isDataComplete = true;

                //check if we can specify the projection type (Import, Export) regarding the step directions
                if (proj.projectionType == ProjectionType.SyncGeneral)
                {
                    Dictionary<string, int> direction = new Dictionary<string, int>(2);
                    direction.Add("ToTheRight", 0);
                    direction.Add("ToTheLeft",  0);

                    //count the direction modes
                    proj.projectionSteps.ForEach(step => direction[step.direction]++);

                    if (direction["ToTheRight"] == 0 && direction["ToTheLeft"] > 0)
                        proj.projectionType = ProjectionType.SyncImport;
                    else if (direction["ToTheRight"] > 0 && direction["ToTheLeft"] == 0)
                        proj.projectionType = ProjectionType.SyncExport;
                    else if (direction["ToTheRight"] > 0 && direction["ToTheLeft"] > 0)
                        proj.projectionType = ProjectionType.SyncMixedDirections;
                }

                //get the connection from the steps up to the projection objects
                if (proj.projectionSteps.Count>0)
                {
                    proj.conn_TargetSystem = proj.projectionSteps[0].rightConnection;
                    proj.conn_IdentityManager = proj.projectionSteps[0].leftConnection;
                }
                else
                {
                    logger.Warning($"projection {proj.ToString()} does not seems to have any projection step. Therefor the connection id and display to target system and to OneIM cannot be determined!");

                    proj.conn_TargetSystem = $"unknown {proj.loggerSourceId}";  //make it unique so other detectors cannot find it; this value is meaningless and shall not be used for anything else as for display
                    proj.conn_IdentityManager = $"unknown {proj.loggerSourceId}";
                }

                //put in store if not already in it (can happen when a log file was analyed twice (a copy))
                if (!projectionActivity.Projections.Any(p => (p.dtTimestampStart.AlmostEqual(proj.dtTimestampStart) && p.logfileName != proj.logfileName) 
                                                                        || 
                                                                        p.loggerSourceId == proj.loggerSourceId))
                {
                    logger.Debug($"pushing to ds: projectionActivity.projections.Add: {proj.ToString()}");
                    projectionActivity.Projections.Add(proj);

                    detectorStats.numberOfDetections++;
                    detectorStats.numberOfDetections += proj.projectionSteps.Count;
                    detectorStats.numberOfDetections += proj.projectionCycles.Count;
                }
            }

            //stats
            detectorStats.detectorName = string.Format("{0} <{1}>", this.GetType().Name, this.identifier);
            detectorStats.finalizeDuration = new TimeSpan(Environment.TickCount64 - tcStart).TotalMilliseconds;
            statisticsStore.DetectorStatistics.Add(detectorStats);
            logger.Debug(detectorStats.ToString());

            //dispose
            projections = null;
        }


        public void InitializeDetector()
        {
            projections = new Dictionary<string, List<Projection>>();
            detectorStats.Clear();
            isFinalizing = false;
        }

        public void ProcessMessage(TextMessage msg)
        {
            if (!_isEnabled)
                return;

            long tcStartpoint = Environment.TickCount64;
            
            //we are either interessted in Projector msgs or all other stuff that might close our group message
            if ( (msg != null) &&
                (msg.loggerSource != "ProjectorEngine" || msg.spid == "")  //the message is not in-focus
                //&&(msgGroup == null || msgGroup.IsGroupClosed())  //we don't already have a started group message, which now may be closed to parse its in-focus message
               )
                return;

            var rt = ProcessMessageBase(ref msg);
            if (rt != null)
                foreach (var xmsg in rt)
                    ProcessMessage(xmsg);
            if (msg == null)
                return;


            bool messageProcessed = false;
            detectorStats.numberOfLinesParsed += msg.numberOfLines;          


            //new sync/projection activity?
            if (!messageProcessed)
            {
                var rm_Act = regex_SyncActivity.Match(msg.messageText);

                if (rm_Act.Success)
                {
                    logger.Trace($"regex match for rx regex_SyncActivity: {regex_SyncActivity.ToString()}");

                    CreateNewProjection(rm_Act, msg);
                    messageProcessed = true;
                }
            }  //new projection


            //sync/projection cycle
            //if (!messageProcessed)
            {
                if (regex_SyncActivityCycleRun.IsMatch(msg.messageText))
                {
                    logger.Trace($"regex match for rx regex_SyncActivityCycleRun: {regex_SyncActivityCycleRun.ToString()}");

                    if (projections.ContainsKey(msg.spid))
                        projections[msg.spid].Where(p => !p.isDataComplete).Last().projectionCycles.Add(new DatastoreBaseDataPoint()
                        {
                            dtTimestamp = msg.messageTimestamp,
                            isDataComplete = true,
                            message = msg
                        });

                    //messageProcessed = true;
                }
            }


            //sync/projection done
            if (!messageProcessed)
            {
                //we have a life sign of this projection id
                var unfinishedProjections = projections.Where(kp => kp.Key == msg.spid).SelectMany(x => x.Value).Where(p => !p.isDataComplete).ToArray();
                foreach (var p in unfinishedProjections)
                    p.dtTimestampEnd = msg.messageTimestamp;

                //in debug log level only
                if (regex_SyncActivityDone.IsMatch(msg.messageText))                
                {
                    logger.Trace($"regex match for rx regex_SyncActivityDone: {regex_SyncActivityDone.ToString()}");

                    foreach (var p in unfinishedProjections)
                    {
                        p.dtTimestampEnd = msg.messageTimestamp;
                        p.isDataComplete = true;
                    }
                    messageProcessed = true;  //only in this regex block
                }
            }


            //sync steps  
            //if (!messageProcessed && projections.ContainsKey(msg.spid))
            //if (projections.ContainsKey(msg.spid))
            {
                //var unfinishedProjectionsOfThisSpid = projections[msg.spid].Where(p => !p.isDataComplete);
                //if (unfinishedProjectionsOfThisSpid.Any())
                {

                    var rm_Step = regex_SyncStep.Match(msg.messageText);
                    if (rm_Step.Success)
                    {
                        logger.Trace($"regex match for rx regex_SyncStep: {regex_SyncStep.ToString()}");

                        //try to find the last unfinished projection of the same logger id
                        var projection = projections.ContainsKey(msg.spid) ? projections[msg.spid].LastOrDefault(p => !p.isDataComplete) : null;

                        //oh no, we got information about a projection which never started in the log file(s)
                        if (projection == null)
                        {
                            CreateNewProjection(null, msg);
                            projection = projections[msg.spid].LastOrDefault(p => !p.isDataComplete);
                        }

                        //create current step info, take the data from the group msg
                        var step = new Datastore.ProjectionStep()
                        {
                            stepId              = rm_Step.Groups["stepid"].Value.TrimEnd().Replace("\n", ""),
                            stepNr              = 1 + projection.projectionSteps.Count,
                            direction           = rm_Step.Groups["direction"].Value.TrimEnd().Replace("\n", ""),
                            adHocObject         = rm_Step.Groups["AdHocObject"].Value.TrimEnd().Replace("\n", ""),
                            useRevision         = rm_Step.Groups["UseRevicion"].Value,
                            leftConnection      = rm_Step.Groups["LeftCon"].Value.TrimEnd().Replace("\n", ""),
                            leftSchemaClassRaw  = rm_Step.Groups["LeftSchemaClass"].Value.TrimEnd().Replace("\n", ""),
                            rightConnection     = rm_Step.Groups["RightCon"].Value.TrimEnd().Replace("\n", ""),
                            rightSchemaClassRaw = rm_Step.Groups["RightSchemaClass"].Value.TrimEnd().Replace("\n", ""),
                            map                 = rm_Step.Groups["Map"].Value.TrimEnd().Replace("\n", ""),
                            dtTimestampStart    = msg.messageTimestamp,
                            message             = msg
                        };

                        logger.Trace($"new projection step: {step}");

                        //if there is a last step, we can close/finish it
                        var lastStep = projection.projectionSteps.GetLastOrNull();

                        if (lastStep != null)
                        {
                            lastStep.dtTimestampEnd = msg.messageTimestamp;
                            lastStep.isDataComplete = true;
                        }

                        //add the new step into the store
                        if (!projection.projectionSteps.Any(st => st.dtTimestampStart.AlmostEqual(step.dtTimestampStart, 0)
                                                               && st.stepId == step.stepId
                                                               && st.map == step.map
                           ))
                        {
                            projection.projectionSteps.Add(step);
                        }

                        messageProcessed = true;
                    }
                }
            } //starting sync step


            //if the last step is still unfinished, check if we can close it now
            //if (syncSteps.Count > 0 && syncSteps.Any(v => v.Key == xmsg.spid && v.Value[v.Value.Count-1].lastOccurrence == DateTime.MinValue))
            if (!messageProcessed &&
                projections.ContainsKey(msg.spid) &&
                projections[msg.spid].Where(p => !p.isDataComplete).Any(p => p.projectionSteps
                                                                                        .Any(k => k.loggerSourceId == msg.spid &&
                                                                                                    k.dtTimestampEnd.IsNull())
                                                                       )
                )
            {
                if (regex_SyncFinalizeInitiated.IsMatch(msg.messageText))
                {
                    //we can close all open steps of all open prpjections
                    var openProjections = projections[msg.spid].Where(p => !p.isDataComplete);

                    foreach (var projection in openProjections)
                    {
                        var unfiniStepsOfThisProjection = projection
                                                            .projectionSteps
                                                            .Where(k => k.loggerSourceId == msg.spid && k.dtTimestampEnd.IsNull());

                        foreach (var step in unfiniStepsOfThisProjection) //in general this should be only one or no step
                        {
                            step.dtTimestampEnd = msg.messageTimestamp;
                            step.isDataComplete = true;
                        }
                    }
                }
            } //ending sync step


            detectorStats.parseDuration += new TimeSpan(Environment.TickCount64 - tcStartpoint).TotalMilliseconds;
        }

        private void CreateNewProjection(Match rm_Act, TextMessage msg)
        {
            Datastore.Projection newProjection = null;

            if (rm_Act != null)
            {
                //the type cannot be perfectly specified yet, but we can check for AdHoc or SyncGeneral
                Datastore.ProjectionType prType = ProjectionType.Unknown;
                if (rm_Act.Groups["SyncStartUpConfig"].Value == "" && rm_Act.Groups["AdHocObject"].Value != "")
                    prType = ProjectionType.AdHocProvision;
                else if (rm_Act.Groups["SyncStartUpConfig"].Value != "")
                    prType = ProjectionType.SyncGeneral;

                newProjection = new Projection()
                {
                    dtTimestampStart = msg.messageTimestamp,
                    syncStartUpConfig = rm_Act.Groups["SyncStartUpConfig"].Value,
                    adHocObject = rm_Act.Groups["AdHocObject"].Value,
                    projectionType = prType,
                    message = msg
                };

                logger.Debug($"found new projection: type {prType.ToString()}; id {msg.spid}");                
            }
            else
            {
                logger.Warning($"Found projection information without a projection start. Will create a dummy start information.");
                newProjection = new Projection()
                {
                    dtTimestampStart = DateTime.MinValue,
                    syncStartUpConfig = "n/a",
                    adHocObject = "",
                    projectionType = ProjectionType.Unknown,
                    message = msg
                };
                logger.Debug($"found new projection: type {ProjectionType.Unknown.ToString()}; id {msg.spid}");
            }

            //finish the last sync with the same spid
            var unfinishedProjections = projections.Where(kp => kp.Key == msg.spid).SelectMany(x => x.Value).Where(p => !p.isDataComplete);
            foreach (var p in unfinishedProjections)
            {
                logger.Trace($"finish the last sync with the same spid: {p.loggerSourceId}");
                p.isDataComplete = true;
            }
            
            //store in cache
            if (newProjection != null)
                projections.GetOrAdd(msg.spid).Add(newProjection);
        }

        public SyncStructureDetector() : base(TextReadMode.GroupMessage)
        {}
        
        
    }
}
