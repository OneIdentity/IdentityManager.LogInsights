using System;
using System.Linq;

using LogfileMetaAnalyser.Helpers;

namespace LogfileMetaAnalyser.Datastore
{
    public class DatastoreStructure
    {
        public TimetraceReferenceStore timetraceRef;

        public JobserviceActivity jobserviceActivities;
        public ProjectionActivity projectionActivity;

        public GeneralLogData generalLogData;
        public SqlInformation generalSqlInformation;

        public StatisticsStore statistics;

        public event EventHandler storeInvalidation;


        public DatastoreStructure()
        {            
            Clear();
        }

        public void Clear()
        {
            timetraceRef = null;
            timetraceRef = new TimetraceReferenceStore();

            jobserviceActivities = null;
            jobserviceActivities = new JobserviceActivity();

            projectionActivity = null;
            projectionActivity = new ProjectionActivity();

            generalLogData = null;
            generalLogData = new GeneralLogData();

            generalSqlInformation = null;
            generalSqlInformation = new SqlInformation();

            statistics = null;
            statistics = new StatisticsStore();

            storeInvalidation?.Invoke(this, null);
        }


        public bool HasData()
        {
            return
                    !generalLogData.logDataOverallTimeRange_Start.IsNull() &&
                    !generalLogData.logDataOverallTimeRange_Finish.IsNull() &&
                    generalLogData.numberOflogSources.Count > 0;
        }

    }
}
