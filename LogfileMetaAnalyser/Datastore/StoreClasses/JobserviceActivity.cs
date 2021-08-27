using System;
using System.Collections.Generic;
using System.Linq;

using LogfileMetaAnalyser.Helpers;


namespace LogfileMetaAnalyser.Datastore
{
    public class JobserviceActivity 
    {
        public List<JobserviceJob> jobserviceJobs = new List<JobserviceJob>();

        public JobserviceActivity()
        { }

        private List<string> _distinctTaskfull = null;
        public List<string> distinctTaskfull
        {
            get
            {
                if (_distinctTaskfull == null)
                    _distinctTaskfull = jobserviceJobs.Select(j => j.taskfull).Distinct().ToList();

                return _distinctTaskfull;
            }
        }

        private List<string> _distinctQueuename = null;
        public List<string> distinctQueuename
        {
            get
            {
                if (_distinctQueuename == null)
                    _distinctQueuename = jobserviceJobs.Select(j => j.queuename).Distinct().ToList();

                return _distinctQueuename;
            }
        }

    }
    
}
