using System; 
using LogfileMetaAnalyser.Helpers;
 

namespace LogfileMetaAnalyser.Datastore
{
    /// <summary>
    /// represents an event with a single occurrence, it has a specific time point only and no duration
    /// </summary>
    public class DatastoreBaseDataPoint : DatastoreBaseData
    {
        public DateTime dtTimestamp = DateTime.MinValue;
                 

        public DatastoreBaseDataPoint () : base()
        { }

        public override string ToString()
        {
            return $"at {dtTimestamp.ToString("yyyy-MM-dd hh:mm:ss")} {(isDataComplete ? "" : "?")}";
        }


    }
}
