using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogInsights.Datastore
{
    public enum SystemConnBelongsTo
    {
        Unknown,
        TargetsystemSide,
        IdentityManagerSide
    }

    public enum SystemConnType
    {
        SystemConnector,
        SystemConnection,
        Undef
    }

    public class SystemConnInfo : DatastoreBaseDataRange
    {
        public SystemConnBelongsTo belongsToSide = SystemConnBelongsTo.Unknown;
        public SystemConnType systemConnType = SystemConnType.Undef;
        public List<string> belongsToProjectorId = new List<string>();

        public string systemConnectionSpid = ""; //refers to SystemConnection.uuid

        //will be used in ConnectorsDetector to assign the SystemConnections/SystemConnectors to a Sync
        public List<DateTime> connectTimestamp = new List<DateTime>(); 
        public List<DateTime> disconnectTimestamp = new List<DateTime>();


        public SystemConnInfo()
        { }

        public override string ToString()
        {
            string dataRangeInfo = base.ToString();

            return $"{systemConnType} (side {belongsToSide}; id {systemConnectionSpid}) - {dataRangeInfo}"; 
        }
    }
}
