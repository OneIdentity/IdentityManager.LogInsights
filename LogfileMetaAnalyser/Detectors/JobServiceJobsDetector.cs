﻿using System;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using LogfileMetaAnalyser.Helpers;
using LogfileMetaAnalyser.Datastore;

namespace LogfileMetaAnalyser.Detectors
{
    class JobServiceJobsDetector : DetectorBase, ILogDetector
    {
        private static Regex regex_JobStart_TypeJS = new Regex(@"<p>.*?( - (?<queue>(\\[^ ]+) - )| - )Process step parameter (?<jobid>[-0-9a-zA-Z]{36,38}).*?ComponentClass=(?<classname>.*?)Task=(?<taskname>.*?)Executiontype.*?(?<params>\[Parameters\].+)", RegexOptions.Compiled | RegexOptions.Singleline);
        private static Regex regex_JobStart_TypeNLog = new Regex(@"Process step parameter (?<jobid>[-0-9a-zA-Z]{36,38}).*?ComponentClass=(?<classname>.*?)Task=(?<taskname>.*?)Executiontype.*?(?<params>\[Parameters\].+)", RegexOptions.Compiled | RegexOptions.Singleline);
        private static Regex regex_JobStart_TypeNLog_SQL = new Regex(@"exec QBM_PJobUpdateState N?'(?<jobid>[-0-9a-zA-Z]{36,38})', N'PROCESSING'", RegexOptions.Compiled | RegexOptions.Singleline);

        private static Regex regex_JobFinish_TypeJS = new Regex(@"<[swrxe]>.*?(?<queue>\\.+?) - .*?(?<jobid>[-0-9a-fA-Z]{36,38}): (?<jobstate>[a-zA-Z ]+)(?<msg>.*)?$|<[swrxe]>.*?(?<jobid>[-0-9a-fA-Z]{36,38}): (?<jobstate>[a-zA-Z ]+)(?<msg>.*)?$|<[swrxe]>.*?(?<queue>\\.+?) - Process step output parameter (?<jobid>[-0-9a-fA-Z]{36,38}):(?<msg>.*)?$", RegexOptions.Compiled | RegexOptions.Singleline);
        private static Regex regex_JobFinish_TypeNLog = new Regex(@"(?<jobid>[-0-9a-fA-Z]{36,38}): (?<jobstate>[a-zA-Z ]+)(?<msg>.*)?", RegexOptions.Compiled | RegexOptions.Singleline);
        private static Regex regex_JobFinish_TypeNLog_SQL = new Regex(@"exec QBM_PJobUpdateState.*@uid = (?<jobid>[-0-9a-fA-Z]{36,38}).*@state = (?<jobstate>[a-zA-Z ]+).*@messages = (?<msg>.*)?", RegexOptions.Compiled | RegexOptions.Singleline);

        private Dictionary<string, JobserviceJob> jobs;


        public override string caption
        {
            get
            {
                return "Found Jobservice jobs";
            }
        }

        public override string identifier
        {
            get
            {
                return "#JobServiceJobsDetector";
            }
        }


        public JobServiceJobsDetector ()
        { }

        public void InitializeDetector()
        {
            jobs = new Dictionary<string, JobserviceJob>();
            jobs = _datastore.jobserviceActivities?.jobserviceJobs?.ToDictionary(j => j.uidJob);
                        
            detectorStats.Clear();
        }

        public void FinalizeDetector()
        {
            logger.Debug("entering FinalizeDetector()");
            DateTime finStartpoint = DateTime.Now;

            var jobLst = jobs.Values.Where(j => j.jobserviceJobattempts.Any());
            int cnt = jobLst.Count();
            foreach (var job in jobLst)
            {
                if (cnt < 100)
                    logger.Debug($"pushing to ds: jobserviceActivities: {job.ToString()}");
                _datastore.jobserviceActivities.jobserviceJobs.Add(job);
            }

            //stats
            detectorStats.detectorName = string.Format("{0} <{1}>", this.GetType().Name, this.identifier);
            detectorStats.numberOfDetections = _datastore.jobserviceActivities.jobserviceJobs.Count;
            detectorStats.finalizeDuration = (DateTime.Now - finStartpoint).TotalMilliseconds;
            _datastore.statistics.detectorStatistics.Add(detectorStats);
            logger.Debug(detectorStats.ToString());

            //dispose
            jobs = null;
        }

        public void ProcessMessage(TextMessage msg)
        {
            if (!_isEnabled)
                return;
                   
			DateTime procMsgStartpoint = DateTime.Now;

            if (
                (msg.messageLogfileType == LogfileType.Jobservice && msg.spid == "") ||
                (msg.messageLogfileType != LogfileType.Jobservice && msg.loggerSource != "Jobservice" && msg.loggerSource != "SqlLog")
               )
                return;


            var rt = ProcessMessageBase(ref msg);
            if (rt != null)
                foreach (var xmsg in rt)
                    ProcessMessage(xmsg);
            if (msg == null)
                return;

			detectorStats.numberOfLinesParsed += msg.numberOfLines;


            string jobId;
            JobserviceJob job;
            JobserviceJobattempt jobAttend;

            try
            {
                //job start
                Match rm_JobStart = null;

                if (msg.messageLogfileType == LogfileType.Jobservice)
                    rm_JobStart = regex_JobStart_TypeJS.Match(msg.messageText);
                else if (msg.loggerSource == "Jobservice")
                    rm_JobStart = regex_JobStart_TypeNLog.Match(msg.messageText);
                else if (msg.loggerSource == "SqlLog" && msg.loggerLevel == "DEBUG")
                    rm_JobStart = regex_JobStart_TypeNLog_SQL.Match(msg.messageText);


                if (rm_JobStart != null && rm_JobStart.Success)
                {
                    jobId = rm_JobStart.Groups["jobid"].Value;
                    if (string.IsNullOrEmpty(jobId))
                        jobId = msg.spid;

                    if (jobs.ContainsKey(jobId))
                    {
                        job = jobs[jobId];
                        logger.Debug($"found job start event for known job: {jobId}");
                    }
                    else
                    {
                        logger.Trace($"found job start event for new job: {jobId}");
                        job = new JobserviceJob();                        
                        jobs.Add(jobId, job);
                    }


                    //if this JS job was added previously by importing from a Jobservice.log, we already have this job incl. QueueName which is available in the Jobservice NLog file :(
                    //or we just found this job for the very first time
                    //in both cases: try to fill in data
                    if (string.IsNullOrEmpty(job.queuename))
                    {
                        job.uidJob = jobId;
                        job.queuename = rm_JobStart.Groups["queue"]?.Value?.Trim();
                        job.componentname = rm_JobStart.Groups["classname"].Value.Trim();
                        job.taskname = rm_JobStart.Groups["taskname"].Value.Trim();
                        job.parameters = rm_JobStart.Groups["params"].Value.Trim();

                        if (string.IsNullOrEmpty(job.queuename))
                            job.queuename = "<unrecognized>";
                    }

                    //avoid adding the same job from different log files into the list
                    if (!job.jobserviceJobattempts.Any(j => j.dtTimestampStart.AlmostEqual(msg.messageTimestamp)))
                        job.jobserviceJobattempts.Add(new JobserviceJobattempt()
                        {
                            dtTimestampStart = msg.messageTimestamp,
                            message = msg
                        });

                    return;
                }

               
                //job finish
                Match rm_JobFinish = null;

                if (msg.messageLogfileType == LogfileType.Jobservice)
                    rm_JobFinish = regex_JobFinish_TypeJS.Match(msg.messageText);
                else if (msg.loggerSource == "Jobservice")
                    rm_JobFinish = regex_JobFinish_TypeNLog.Match(msg.messageText);
                else if (msg.loggerSource == "SqlLog" && msg.loggerLevel == "DEBUG")
                    rm_JobFinish = regex_JobFinish_TypeNLog_SQL.Match(msg.messageText);

                if (rm_JobFinish != null && rm_JobFinish.Success)
                {
                    jobId = rm_JobFinish.Groups["jobid"].Value;

                    logger.Trace($"found job finish event for job id {jobId}");

                    if (string.IsNullOrEmpty(jobId))
                        jobId = msg.spid;

                    job = jobs.GetOrAdd(jobId);                    

                    string jstate = rm_JobFinish.Groups["jobstate"].Value?.TrimEnd();


                    //did we miss the beginning? This can happen if we got only logfile(s) where the end was logged but the start was reported ages ago :(
                    if (string.IsNullOrEmpty(job.taskname) || job.jobserviceJobattempts.Count == 0)
                    {
                        job.jobserviceJobattempts.Add(new JobserviceJobattempt()
                        {
                            dtTimestampStart = DateTime.MinValue,
                            message = null
                        });
                    }


                    //try to find the last non-complete job attempt even there are more than two (e.g. when the 1st attempt was killed and the 2nd was finished)
                    jobAttend = job.jobserviceJobattempts.Where(n => !n.isDataComplete).OrderByDescending(n => n.dtTimestampStart).Take(1).FirstOrDefault();
                    
                    //the following can happen
                    //1.) we have 2 logs which recorded the same job and therefor the start event was ignored 
                    // -> in this case we do not have an incomplete JobServiceAttempt
                    // -> but if we are here still having no JSAttempt and the code above reconstructed one in case we do not have the start event, that means now: 
                    //   we found the start, the end and a secods start (which is ignored due to duplicate log; or missing) and this end event
                    //2.) we have only one logfile which recorded the end event, so the start is really missing
                    if (jobAttend == null)
                    {
                        jobAttend = job.jobserviceJobattempts.Where(n => n.isDataComplete && n.dtTimestampEnd.AlmostEqual(msg.messageTimestamp)).OrderByDescending(n => n.dtTimestampStart).Take(1).FirstOrDefault();

                        if (jobAttend != null) //a duplicate log event
                            return;

                        job.jobserviceJobattempts.Add(new JobserviceJobattempt()
                        {
                            dtTimestampStart = DateTime.MinValue,
                            message = null
                        });

                        jobAttend = job.jobserviceJobattempts.Where(n => !n.isDataComplete).OrderByDescending(n => n.dtTimestampStart).Take(1).FirstOrDefault();
                    }


                    jobAttend.isDataComplete = true;
                    jobAttend.dtTimestampEnd = msg.messageTimestamp;
                    jobAttend.endmessage = msg;
                    jobAttend.resultmessagetext = rm_JobFinish.Groups["msg"]?.Value.TrimStart('\r', '\n');
                    jobAttend.jobExecutionState =
                        jstate.Contains("Error") ? JobserviceJobExecutionState.Error :
                        jstate.Contains("Warning") ? JobserviceJobExecutionState.Warning : JobserviceJobExecutionState.Success;

                    return;
                }                
            }
            finally
            {
                detectorStats.parseDuration += (DateTime.Now - procMsgStartpoint).TotalMilliseconds;
            }
        }
    }
}

