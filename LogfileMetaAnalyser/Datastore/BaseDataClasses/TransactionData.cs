using System;
using LogfileMetaAnalyser.Helpers;


namespace LogfileMetaAnalyser.Datastore
{
    public class TransactionData : DatastoreBaseDataRange
    {
        public bool isSuccessFullTransaction = false;
        public bool isSuspicious = false;

        public TransactionData()
        { }

        public override string ToString()
        {
            return string.Format("{0}Transaction: {1} {2}min", isSuspicious ? "! " : "", isSuccessFullTransaction ? "OK" : "Rolledback", durationMin);
        }

        public string GetLabel()
        {            
            return string.Format("{0} ({1}) {2}",
                    dtTimestampStart.ToHumanTimerange(dtTimestampEnd),
                    DurationString(),
                    isSuccessFullTransaction ? "committed" : "rolled back");
        }
    }
}
