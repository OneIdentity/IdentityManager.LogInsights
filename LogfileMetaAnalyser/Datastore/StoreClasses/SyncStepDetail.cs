using System;
using System.Collections.Generic;
using System.Linq;

using LogfileMetaAnalyser.Helpers;

namespace LogfileMetaAnalyser.Datastore
{
    public class SyncStepDetail //: SyncStepDetailBase
    {     
        private Dictionary<string, SyncStepDetailQueryObject> loadingObjectList_left = new Dictionary<string, SyncStepDetailQueryObject>();  //key == uuid
        private Dictionary<string, SyncStepDetailQueryObject> loadingObjectList_right = new Dictionary<string, SyncStepDetailQueryObject>(); //key == uuid


        public SyncStepDetail() { }

        public int Count()
        {
            return
                    loadingObjectList_left.Count +
                    loadingObjectList_right.Count;
        }

        public bool IsSuspicious
        {
            get
            {
                if (loadingObjectList_left.Any(t => t.Value.IsSuspicious))
                    return true;

                return loadingObjectList_right.Any(t => t.Value.IsSuspicious);
            }
        }

        public override string ToString()
        {
            return string.Format("left objList: {0}\nadditional # of left objectList:{2}\nright objList: {1}\nadditional # of right objectList:{2}",
                                    loadingObjectList_left.Any() ? loadingObjectList_left.First().Value.schemaClassName : " - ",
                                    loadingObjectList_right.Any() ? loadingObjectList_right.First().Value.schemaClassName : " - ", 
                                    loadingObjectList_left.Count,
                                    loadingObjectList_right.Count
                                    );
        }


        public IEnumerable<SyncStepDetailQueryObject> GetSyncStepDetails()
        {
            foreach (var elem in loadingObjectList_left
                                    .Union(loadingObjectList_right)
                                    .Select(t => t.Value)
                                    .OrderBy(t => t.dtTimestampStart))
                yield return elem;

            yield break;
        }

        public void PutSyncStepDetail(SyncStepDetailQueryObject detailItem, SystemConnBelongsTo side)
        {
            detailItem.connectionSide = side;

            switch (side)
            {
                case SystemConnBelongsTo.IdentityManagerSide:
                        loadingObjectList_left.Add(detailItem.uuid, detailItem);

                    break;

                case SystemConnBelongsTo.TargetsystemSide:
                        loadingObjectList_right.Add(detailItem.uuid, detailItem);
                    break;
            }
        }


        public SyncStepDetailQueryObject GetSyncStepDetailByUuid(string uuid)
        {
            if (loadingObjectList_left.ContainsKey(uuid))
                return loadingObjectList_left[uuid];

            if (loadingObjectList_right.ContainsKey(uuid))
                return loadingObjectList_right[uuid];

            return null;
        }
    }




    public class SyncStepDetailBase
    {
        public SyncStepDetailQueryObject queryObjectInformation = new SyncStepDetailQueryObject();

        public DateTime firstOccurrence  
        {
            get { return queryObjectInformation.dtTimestampStart; }
        }

        public DateTime lastOccurrence  
        {
            get { return queryObjectInformation.dtTimestampEnd; }
        }

        public SyncStepDetailBase() { }

        public override string ToString()
        {
            return string.Format("{0}: {1} ({2})", queryObjectInformation.queryType, queryObjectInformation.systemConnType, queryObjectInformation.connectionSide);
        }

        public static Dictionary<Projection, List<SyncStepDetailBase>> RelateListOfStepDetailsToProjections(List<SyncStepDetailBase> stepDetails, Projection[] potentialProjections)
        {
            Dictionary<Projection, List<SyncStepDetailBase>> res = new Dictionary<Projection, List<SyncStepDetailBase>>();

            //some pre checks
            if (potentialProjections.Length == 0)
                return null;

            if (potentialProjections.Length == 1)
            {
                res.Add(potentialProjections[0], stepDetails);
                return res;
            }


            //seperate the projections regarding their presence (put overlapping projections into different lanes)
            Dictionary<Projection, int> concurrentProcessLanes = Projection.GetConcurrentProcessLanes(potentialProjections);
            int numOfdiffLanes = concurrentProcessLanes.GroupBy(x => x.Value).Count();

            if (numOfdiffLanes > 1)
            {
                //check each step detail and put in into zero, one or more potential projections
                Dictionary<Projection, List<SyncStepDetailBase>> projectionsWithPotentialSteps = new Dictionary<Projection, List<SyncStepDetailBase>>();

                foreach (var stepDetail in stepDetails)
                {
                    //if a stepdetail fits more projections regarding the timestamp, we can define that all of the stepDetails must belong to non-overlapping projections
                    //e.g. projection 1 and 3 were executed by Process1; projection 2 was executed by Process2, proj1 overlapps with proj2 and proj2 overlapps with proj3
                    //  => ALL stepDetails must belong to EITHER process1 (proj1, proj3) OR process2 (proj2)

                    var potListOfProjections = potentialProjections.Where(p => p.dtTimestampStart <= stepDetail.firstOccurrence &&
                                                           p.dtTimestampEnd >= stepDetail.lastOccurrence &&
                                                           p.projectionSteps.Count >= 0 &&
                                                           p.projectionSteps.Min(step => step.dtTimestampStart).LessThan(stepDetail.firstOccurrence) &&
                                                           p.projectionSteps.Max(step => step.dtTimestampEnd).MoreThan(stepDetail.lastOccurrence)
                                                      );

                    if (!potListOfProjections.Any())
                        continue; // :(

                    foreach (var p in potListOfProjections)
                        projectionsWithPotentialSteps.AddOrUpdate(p, stepDetail);
                }


                //analysis result
                Dictionary<int, int> laneSteps = new Dictionary<int, int>();  //key = laneID, value = number of assigned pot. step details
                foreach (var p in potentialProjections)
                {
                    int laneID = concurrentProcessLanes[p];
                    laneSteps.AddOrIncrease(laneID);
                }

                //get the lane with the most steps
                int bestLane = laneSteps.OrderByDescending(s => s.Value).FirstOrDefault().Key;

                //if there are more than one "best lane", we've failed
                if (laneSteps.Count(s => s.Value == laneSteps[bestLane]) != 1)
                    return null;  //:(


                //now we have the lane, we can reduce our potentialProjections list 
                potentialProjections = concurrentProcessLanes.Where(x => x.Value == bestLane).Select(x => x.Key).ToArray();
            }

            //now we need to assign each step detail to a projection
            foreach (var stepDetail in stepDetails)
            {                
                var potListOfProjections = potentialProjections.Where(p => p.dtTimestampStart <= stepDetail.firstOccurrence &&
                                                        p.dtTimestampEnd >= stepDetail.lastOccurrence &&
                                                        p.projectionSteps.Count >= 0 &&
                                                        p.projectionSteps.Min(step => step.dtTimestampStart).LessThan(stepDetail.firstOccurrence) &&
                                                        p.projectionSteps.Max(step => step.dtTimestampEnd).MoreThan(stepDetail.lastOccurrence)
                                                    );

                if (potListOfProjections.Count() != 1)
                    continue; // :(
                
                res.AddOrUpdate(potListOfProjections.First(), stepDetail);
            }


            //finally, we are done
            return res;
        }
    }


}
