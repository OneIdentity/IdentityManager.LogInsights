using System;
using System.Collections.Concurrent;
using System.Linq;

namespace LogfileMetaAnalyser.Datastore
{
    public interface IDataStoreContent
    {
        bool HasData { get; }
    }

    public class DataStore
    {
        private readonly ConcurrentDictionary<Type, IDataStoreContent> _content = new();

        public event EventHandler StoreInvalidation;

        public T GetOrAdd<T>() where T : IDataStoreContent
        {
            return (T)_content.GetOrAdd(
                typeof(T),
                _ => Activator.CreateInstance<T>());
        }

        public void Clear()
        {
            _content.Clear();
            StoreInvalidation?.Invoke(this, EventArgs.Empty);
        }


        public bool HasData()
        {
            return _content.Values.Any(s => s.HasData);
        }

        // Convenience helpers

        public TimetraceReferenceStore TimeTraceRef => GetOrAdd<TimetraceReferenceStore>();

        public JobServiceActivity JobServiceActivities => GetOrAdd<JobServiceActivity>();
        public ProjectionActivity ProjectionActivity => GetOrAdd<ProjectionActivity>();

        public GeneralLogData GeneralLogData => GetOrAdd<GeneralLogData>();
        public SqlInformation GeneralSqlInformation => GetOrAdd<SqlInformation>();

        public StatisticsStore Statistics => GetOrAdd<StatisticsStore>();

    }
}
