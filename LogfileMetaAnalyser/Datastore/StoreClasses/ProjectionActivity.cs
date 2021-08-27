using System;
using System.Collections.Generic;
using System.Linq;
 
using LogfileMetaAnalyser.Helpers;

namespace LogfileMetaAnalyser.Datastore
{
    public class ProjectionActivity  
    {
        public List<Projection> projections = new List<Projection>();
        public ProjectionActivity()
        { }


        private int _NumberOfAdHocProjections = -1;
        public int NumberOfAdHocProjections
        {
            get
            {
                if (_NumberOfAdHocProjections < 0)
                    _NumberOfAdHocProjections = projections.Count(t => t.projectionType == ProjectionType.AdHocProvision);

                return _NumberOfAdHocProjections;
            }
        }

        private int _NumberOfSyncProjections = -1;
        public int NumberOfSyncProjections
        {
            get
            {
                if (_NumberOfSyncProjections < 0)
                    _NumberOfSyncProjections = projections.Count(t => t.projectionType != ProjectionType.AdHocProvision);

                return _NumberOfSyncProjections;
            }
        }

        private int _NumberOfJournalSetupTotal = -1;
        public int NumberOfJournalSetupTotal
        {
            get
            {
                if (_NumberOfJournalSetupTotal < 0)
                    _NumberOfJournalSetupTotal = projections
                                                    .Select(p => p.projectionJournal)
                                                    .Where(j => j != null)
                                                    .Sum(j => j.journalSetupElems.Count);  //only the top level
                return _NumberOfJournalSetupTotal;
            }
        }

        private int _NumberOfJournalObjectTotal = -1;
        public int NumberOfJournalObjectTotal
        {
            get
            {
                if (_NumberOfJournalObjectTotal < 0)
                    _NumberOfJournalObjectTotal = projections
                                                    .Select(p => p.projectionJournal)
                                                    .Where(j => j != null)
                                                    .Sum(j => j.journalObjects.Count);  
                return _NumberOfJournalObjectTotal;
            }
        }

        private int _NumberOfJournalMessagesTotal = -1;
        public int NumberOfJournalMessagesTotal
        {
            get
            {
                if (_NumberOfJournalMessagesTotal < 0)
                    _NumberOfJournalMessagesTotal = projections
                                                    .Select(p => p.projectionJournal)
                                                    .Where(j => j != null)
                                                    .Sum(j => j.journalMessages.Count);
                return _NumberOfJournalMessagesTotal;
            }
        }

        private int _NumberOfJournalFailuresTotal = -1;
        public int NumberOfJournalFailuresTotal
        {
            get
            {
                if (_NumberOfJournalFailuresTotal < 0)
                    _NumberOfJournalFailuresTotal = projections
                                                    .Select(p => p.projectionJournal)
                                                    .Where(j => j != null)
                                                    .Sum(j => j.journalFailures.Count);
                return _NumberOfJournalFailuresTotal;
            }
        }

        public string GetUuidByLoggerId(string spid, out ProjectionType ptype)
        {
            ptype = ProjectionType.Unknown;

            if (string.IsNullOrWhiteSpace(spid))
                return null;

            var res = projections
                        .Where(p => GetAllSpidsOfProjection(p).Contains(spid))
                        .FirstOrDefault();

            if (res == null)
                return null;

            ptype = res.projectionType;
            return res.uuid;
        }

        public IEnumerable<string> GetLoggerIdsByUuids(string[] uuidList, bool includeWholeTree = true, ProjectionType projectionType = ProjectionType.Unknown)
        {
            if (uuidList == null || uuidList.Length == 0)
               return new string[] { };

            bool takeAll = (uuidList[0] == "*");

            //ds.projectionActivity.projections[0].uuid
            //ds.projectionActivity.projections[0].systemConnectors[0].uuid
            //ds.projectionActivity.projections[0].projectionSteps[0].uuid
            //ds.projectionActivity.projections[0].specificSqlInformation.sqlSessions[0].uuid
            List<string> result = new List<string>();

            //apply the scope
            var projectionLst = projectionType == ProjectionType.Unknown ?
                                                  projections :
                                                  (
                                                    projectionType == ProjectionType.AdHocProvision ?
                                                    projections.Where(p => p.projectionType == Datastore.ProjectionType.AdHocProvision) :
                                                    projections.Where(p => p.projectionType != Datastore.ProjectionType.AdHocProvision)
                                                  );


            //found in projection
            var selProjections1 = takeAll ? projectionLst : projectionLst.Where(pr => uuidList.Contains(pr.uuid));
            if (includeWholeTree || takeAll)
                result.AddRange(selProjections1.SelectMany(p => GetAllSpidsOfProjection(p)));
            else
                result.AddRange(selProjections1.Select(p => p.loggerSourceId));

            //because this step already collected all children, we can stop here
            if (takeAll)
                return result.Distinct();

            //found somewhere in systemConnectors
            var selProjections2 = projectionLst.Where((Projection pr) => pr.systemConnectors
                                                                        .Any(sc => uuidList.Contains(sc.uuid))
                                                   );
            if (includeWholeTree)
                result.AddRange(selProjections2.SelectMany(p => GetAllSpidsOfProjection(p)));
            else
                result.AddRange(selProjections2.SelectMany(p => p.systemConnectors.Select(sc => sc.loggerSourceId)));


            //found somewhere in projectionSteps
            var selProjections3 = projectionLst.Where((Projection pr) => pr.projectionSteps
                                                                        .Any(sc => uuidList.Contains(sc.uuid))
                                                   );

            if (includeWholeTree)
                result.AddRange(selProjections3.SelectMany(p => GetAllSpidsOfProjection(p)));
            else
                result.AddRange(selProjections3.SelectMany(p => p.projectionSteps.Select(sc => sc.loggerSourceId)));


            //found somewhere in specificSqlInformation.sqlSessions
            var selProjections4 = projectionLst.Where((Projection pr) => pr.specificSqlInformation.sqlSessions
                                                                .Any(sc => uuidList.Contains(sc.uuid))
                                       );

            if (includeWholeTree)
                result.AddRange(selProjections4.SelectMany(p => GetAllSpidsOfProjection(p)));
            else
                result.AddRange(selProjections4.SelectMany(p => p.specificSqlInformation.sqlSessions.Select(sc => sc.loggerSourceId)));


            return result.Distinct();
        }


        private Dictionary<Projection, List<string>> _cache_GetAllSpidsOfProjection = new Dictionary<Projection, List<string>>();

        private List<string> GetAllSpidsOfProjection(Projection proj)
        {
            //search in local cache
            if (_cache_GetAllSpidsOfProjection.ContainsKey(proj))
                return _cache_GetAllSpidsOfProjection[proj];

            //fill the cache and return it afterwards
            _cache_GetAllSpidsOfProjection.Add(proj, new List<string>());

            _cache_GetAllSpidsOfProjection[proj].Add(proj.loggerSourceId);

            _cache_GetAllSpidsOfProjection[proj].AddRange(proj.systemConnectors.Select(sc => sc.loggerSourceId));
            _cache_GetAllSpidsOfProjection[proj].AddRange(proj.projectionSteps.Select(ps => ps.loggerSourceId));
            _cache_GetAllSpidsOfProjection[proj].AddRange(proj.specificSqlInformation.sqlSessions.Select(ss => ss.loggerSourceId));

            return (_cache_GetAllSpidsOfProjection[proj]);
        }

    }
}
