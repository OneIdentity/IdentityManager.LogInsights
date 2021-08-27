using System;
using System.Collections.Generic;
using System.Linq;

namespace LogfileMetaAnalyser.Datastore
{
    public enum ProjectionType
    {
        Unknown,
        AdHocProvision,
        SyncGeneral,
        SyncImport,
        SyncExport,
        SyncMixedDirections
    }

    public class Projection: DatastoreBaseDataRange
    {
        public ProjectionType projectionType = ProjectionType.Unknown;
        public string syncStartUpConfig;  //workflow display according to FullProjectionStrategy.cs
        public string adHocObject;
		
		public string conn_TargetSystem;
		public string conn_IdentityManager;

        public List<DatastoreBaseDataPoint> projectionCycles = new List<DatastoreBaseDataPoint>(); 

        public List<ProjectionStep> projectionSteps = new List<ProjectionStep>();

        public List<SystemConnInfo> systemConnectors = new List<SystemConnInfo>();

        public SqlInformation specificSqlInformation = new SqlInformation();

        public DprJournal projectionJournal = null;

        public List<object> maintenance = new List<object>();
#warning RFE#3.a: todo Detector/Datastore: Projection.maintenance auslesen 



        public Projection()
        { }


        public string InterpretType()
        {
            switch (projectionType)
            {
                case ProjectionType.Unknown:
                    return "Unknown projection job";
                case ProjectionType.AdHocProvision:
                    return "AdHoc provisioning job";
                case ProjectionType.SyncGeneral:
                    return "Synchronization job";
                case ProjectionType.SyncExport:
                    return "Synchronization job towards target system";
                case ProjectionType.SyncImport:
                    return "Import synchronization job from target system";
                case ProjectionType.SyncMixedDirections:
                    return "Synchronization job with steps of multiple sync directions";
            }

            return "";
        }

        public override string ToString()
        {
            string c1 = string.IsNullOrEmpty(conn_IdentityManager) ? "" : $"OneIM con: {conn_IdentityManager}\r\n";
            string c2 = string.IsNullOrEmpty(conn_TargetSystem) ? "" : $"Target system con: {conn_TargetSystem}\r\n"; 

            string w1 = string.IsNullOrEmpty(syncStartUpConfig) ? "" : $"Sync workfl: {syncStartUpConfig}\r\n"; 
            string w2 = string.IsNullOrEmpty(adHocObject) ? "" : $"AdHoc obj: {adHocObject}\r\n"; ;

            return $"{InterpretType()}: {c1}{c2}{w1}{w2}";
        }

        public string GetLabel(bool shortDateFormat = false)
        {
            return $"{dtTimestampStart.ToString(shortDateFormat ? "t" : "G")}: {InterpretType()} {conn_TargetSystem}";
        }

        public static Dictionary<Projection, int> GetConcurrentProcessLanes(Projection[] projections)
        {
            Dictionary<Projection, int> res = new Dictionary<Projection, int>();
            int laneId;

            foreach (var proj in projections)
            {
                var freeLaneLst = res.Where(p => !Helpers.DateHelper.DateRangeInterferesWithRange(p.Key.dtTimestampStart, p.Key.dtTimestampEnd, proj.dtTimestampStart, proj.dtTimestampEnd));

                //we got a free lane
                if (freeLaneLst != null && freeLaneLst.Any())                
                    laneId = freeLaneLst.Min(x => x.Value); //take the lane with the lowest laneId                 
                else
                    laneId = res.Count;

                res.Add(proj, laneId);                 
            }

            return (res);
        }
    }
}
