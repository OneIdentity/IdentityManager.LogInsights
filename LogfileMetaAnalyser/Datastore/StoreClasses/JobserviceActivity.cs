using System.Collections.Generic;
using System.Linq;


namespace LogfileMetaAnalyser.Datastore
{
    public class JobServiceActivity : IDataStoreContent
    {
        public List<JobserviceJob> JobServiceJobs { get; } = new();

        private List<string> _distinctTaskFull;
        public List<string> DistinctTaskFull => _distinctTaskFull ??= JobServiceJobs.Select(j => j.taskfull).Distinct().ToList();

        private List<string> _distinctQueueName;
        public List<string> DistinctQueueName => _distinctQueueName ??= JobServiceJobs.Select(j => j.queuename).Distinct().ToList();

        public bool HasData => JobServiceJobs?.Count > 0;
    }
}
