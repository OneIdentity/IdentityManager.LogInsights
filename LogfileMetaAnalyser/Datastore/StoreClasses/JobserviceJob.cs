using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using LogfileMetaAnalyser.Helpers;


namespace LogfileMetaAnalyser.Datastore
{
    public class JobserviceJob
    {
        public List<JobserviceJobattempt> jobserviceJobattempts = new List<JobserviceJobattempt>();

        public string uidJob;
        public string taskname = "";
        public string componentname = "";
        public string queuename = "";
        public string parameters = "";

        public string taskfull
        {
            get
            {
                string component;
                if (componentname == "")
                    component = "<noComponent>";
                else
                    component = componentname.Substring(componentname.LastIndexOf(".") + 1);

                return string.Format("{0}.{1}", component, string.IsNullOrEmpty(taskname) ? "<noTask>" : taskname);
            }
        }

        public bool isSmoothExecuted
        {
            get
            {
                if (jobserviceJobattempts.Count >= 100)  //100+ retries is suspicious!
                    return false;

                if (jobserviceJobattempts.Any(j => j.jobExecutionState == JobserviceJobExecutionState.Error))  //one attempt was returned as error; warnings might be ok for retries
                    return false;

                if (string.IsNullOrEmpty(uidJob) && jobserviceJobattempts.Count == 1 && jobserviceJobattempts[0].jobExecutionState == JobserviceJobExecutionState.Unknown)  //this can happen when we got a partially formatted finish message but without the start message
                    return false;

                if (jobserviceJobattempts.Any(j => j.durationMin > 60 * 24))  //longer than 24h
                    return false;

                return true;
            }
        }

        public string finaleStatus
        {
            get
            {
                var lastAttempt = jobserviceJobattempts.GetLastOrNull();

                if (lastAttempt == null)
                    return "unknown";

                return lastAttempt.jobExecutionState.ToString();
            }
        }

        public bool hasMissingStartOrFinishMessages
        {
            get
            {
                return jobserviceJobattempts.Any(a => a.hasMissingStartOrFinishMessage);
            }
        }

      
        public override string ToString()
        {
            if (jobserviceJobattempts.Count == 0)
                return ("empty"); //not possible

            string info;
            string paramsDisplay = parameters?.Length > 200 ? parameters.Substring(0, 200) : parameters;

            if (jobserviceJobattempts.Count == 1)
                info = string.Format("{0} till {1} ({2}) with {3}", jobserviceJobattempts[0].dtTimestampStart.ToString("yyyy-MM-dd hh:mm:ss"),
                                                                    jobserviceJobattempts[0].dtTimestampEnd.ToString("yyyy-MM-dd hh:mm:ss"),
                                                                    jobserviceJobattempts[0].DurationString(true),
                                                                    jobserviceJobattempts[0].jobExecutionState);
            else
                info = string.Format("{0} executions: #1: {1} till {2} ({3}) with {4} .. #{0}: {5} till {6} ({7}) with {8}",
                                    jobserviceJobattempts.Count,
                                    jobserviceJobattempts[0].dtTimestampStart.ToString("yyyy-MM-dd hh:mm:ss"),
                                    jobserviceJobattempts[0].dtTimestampEnd.ToString("yyyy-MM-dd hh:mm:ss"),
                                    jobserviceJobattempts[0].DurationString(true),
                                    jobserviceJobattempts[0].jobExecutionState,
                                    jobserviceJobattempts.Last().dtTimestampStart.ToString("yyyy-MM-dd hh:mm:ss"),
                                    jobserviceJobattempts.Last().dtTimestampEnd.ToString("yyyy-MM-dd hh:mm:ss"),
                                    jobserviceJobattempts.Last().DurationString(true),
                                    jobserviceJobattempts.Last().jobExecutionState);

            return $"{info}\nParameters: {paramsDisplay}";
        }

        public string GetLabel()
        {
            return $"job {taskfull} on {queuename} with {finaleStatus}";
        }

        public JobserviceJob()
        { }
    }

       

    public enum JobserviceJobExecutionState
    {
        Unknown,
        Error,
        Warning,
        Success       
    }
}
