using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;



namespace LogfileMetaAnalyser.Helpers
{
    public class Closeness<T>
    {
        private Dictionary<T, int> numsOfCandidates = new Dictionary<T, int>();
        public Closeness()
        { }

        public void AddCandidate(T obj)
        {
            if (numsOfCandidates.ContainsKey(obj))
                numsOfCandidates[obj]++;
            else
                numsOfCandidates.Add(obj, 1);
        }

        public Dictionary<T,float> GetLikeliness()
        {
            Dictionary<T,float> res = new Dictionary<T,float>();

            int numOfEntries = numsOfCandidates.Sum(kp => kp.Value);

            foreach (var kp in numsOfCandidates.OrderByDescending(k => k.Value))
                res.Add(kp.Key, 1f * kp.Value / numOfEntries);

            return res;
        }

        public T GetMostLikeliness()
        {
            Dictionary<T, float> res = GetLikeliness();
            if (res.Count == 0)
                return default(T);
            else
                return res.OrderByDescending(k => k.Value).First().Key;
        }

       
        private static int ComputeStringDistance(string s, string t)
        {
            int n = s.Length;
            int m = t.Length;
            int[,] d = new int[n + 1, m + 1];

            // Step 1
            if (n == 0)
            {
                return m;
            }

            if (m == 0)
            {
                return n;
            }

            // Step 2
            for (int i = 0; i <= n; d[i, 0] = i++)
            {
            }

            for (int j = 0; j <= m; d[0, j] = j++)
            {
            }

            // Step 3
            for (int i = 1; i <= n; i++)
            {
                //Step 4
                for (int j = 1; j <= m; j++)
                {
                    // Step 5
                    int cost = (t[j - 1] == s[i - 1]) ? 0 : 1;

                    // Step 6
                    d[i, j] = Math.Min(
                        Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
                        d[i - 1, j - 1] + cost);
                }
            }
            // Step 7
            return d[n, m];
        }

        public static KeyValuePair<Ty, int> GetKeyOfBestMatch<Ty>(string testtext, Dictionary<Ty, string> matchMatrix)
        {
            if (matchMatrix == null || matchMatrix.Count == 0)
                throw new ArgumentException("match matrix must be passed!");

            Dictionary<Ty, int> distancePerKey = new Dictionary<Ty, int>();
            foreach (var kp in matchMatrix)
                distancePerKey.Add(kp.Key, ComputeStringDistance(testtext, kp.Value));

            return distancePerKey.OrderBy(kp => kp.Value).First();
        }

        public static KeyValuePair<int, double> GetKeyOfBestMatch(TimeRangeMatchCandidate rangeToInspect, List<TimeRangeMatchCandidate> matchMatrix, 
                                                                    double weightOnStart=10d, double weightOnEnd=5d, double weightOnTextmatch=7d)
        {
            if (rangeToInspect == null)
                throw new ArgumentNullException("rangeToInspect must not be null!");

            if (matchMatrix == null || matchMatrix.Count == 0)
                throw new ArgumentException("match matrix must be passed!");


            Dictionary<int, double> distanceToMatch = new Dictionary<int, double>();  //key = candidate position; value = text distance (the lower the more similar they texts are)
            for (int i = 0; i < matchMatrix.Count; i++)
            {
                var rangeCandidate = matchMatrix[i];

                if (!rangeCandidate.Includes(rangeToInspect))
                    distanceToMatch.Add(i, int.MaxValue);
                else
                {
                    var projTimeRange = Math.Max(0.001, (rangeCandidate.starttime - rangeCandidate.endtime).TotalMilliseconds);  //ensure > 0

                    var avgTextLen1 = (rangeCandidate.textmatchcandidate1.Length + rangeToInspect.textmatchcandidate1.Length) / 2d;
                    var avgTextLen2 = (rangeCandidate.textmatchcandidate2.Length + rangeToInspect.textmatchcandidate2.Length) / 2d;


                    var startDist = weightOnStart * ((rangeToInspect.starttime - rangeCandidate.starttime).TotalMilliseconds / projTimeRange).EnsureRange(0d, 1d);
                    var endDist   = weightOnEnd   * ((rangeCandidate.endtime   - rangeToInspect.endtime).TotalMilliseconds   / projTimeRange).EnsureRange(0d, 1d);

                    var textDist1  = weightOnTextmatch * (ComputeStringDistance(rangeToInspect.textmatchcandidate1, rangeCandidate.textmatchcandidate1) / avgTextLen1);
                    var textDist2  = weightOnTextmatch * (ComputeStringDistance(rangeToInspect.textmatchcandidate2, rangeCandidate.textmatchcandidate2) / avgTextLen2);

                    distanceToMatch.Add(i, startDist + endDist + Math.Min(textDist1, textDist2));
                }
            }

            return distanceToMatch.OrderBy(d => d.Value).First();
        }

        public static TimeRangeMatchCandidate[] GetKeyOfBestMatch(PeriodsMatchCandidate baseline, List<TimeRangeMatchCandidate> matchMatrix)
        {
            /*
            - aus matchMatrix nur die nehmen, wo baseline ein Connect Timestamp drin hat
            - wenn dann nur eines über bleibt -> raus damit
            - wenn mehrere übrig bleiben, könnten mehrere raus oder auch nur einer, d.h. wir müssen dann die Texte vergleichen
                - allerdings weiß ich nicht, wo man die Grnze zieht
                - Bsp: es bleiben noch 2 AD AdHoc Jobs und ein Notes Sync übrig, rausgegeben werden SOLL 2xAdHoc
            */

            if (matchMatrix == null || matchMatrix.Count == 0)
                throw new ArgumentException("match matrix must be passed!");

            Dictionary<TimeRangeMatchCandidate, double> res = new Dictionary<TimeRangeMatchCandidate, double>();            
            Dictionary<int, double> distanceToMatch = new Dictionary<int, double>();

            foreach (var rangeCandidate in matchMatrix)
            {                
                //condition 1: baseline must have at least ONE connect timestamp, which lies inside a matrix range candidate
                if (baseline.StartsInsideOf(rangeCandidate.starttime, rangeCandidate.endtime))
                {                
                    //condition 2: well, if we have e.g. 2 timerangematch candidates running in parallel (e.g. 2 syncs), and baselines starts inside of both, we need to decide which one is the right one
                    double avgTextLen1 = (rangeCandidate.textmatchcandidate1.Length + baseline.textmatchcandidate1.Length) / 2d;
                    double avgTextLen2 = (rangeCandidate.textmatchcandidate2.Length + baseline.textmatchcandidate2.Length) / 2d;

                    double textDist1 = (ComputeStringDistance(baseline.textmatchcandidate1, rangeCandidate.textmatchcandidate1)) / avgTextLen1;
                    double textDist2 = (ComputeStringDistance(baseline.textmatchcandidate2, rangeCandidate.textmatchcandidate2)) / avgTextLen2;

                    //the lower the better the texts matches; we need to ensure an ADS Connection is not assigned to an SAP projection
                    res.Add(rangeCandidate, Math.Min(textDist1, textDist2));
                }
            }

            if (res.Count <= 1)
                return res.Select(c => c.Key).ToArray();
            else
            {
                //hmm, we have multiple candidates
                //if we filter with .Where(c => c.Value < 0.75) then it can result in an empty list, which is impossible bcause at least one candidate MUST have caused the matrix data
                var res_filtered = res.Where(c => c.Value < 0.75).Select(c => c.Key).ToArray();

                if (res_filtered.Any())
                    return res_filtered;

                return res.OrderBy(c => c.Value).Take(1).Select(c => c.Key).ToArray();
            }
        }
    }

    public class TimeRangeMatchCandidate
    {
        public DateTime starttime;
        public DateTime endtime;
        public string textmatchcandidate1;
        public string textmatchcandidate2;
        public string key;

        public TimeRangeMatchCandidate() { }
        public TimeRangeMatchCandidate(string key, DateTime Starttime, DateTime Endtime, string Textmatchcandidate1, string Textmatchcandidate2 = null)
        {
            this.key = key;
            starttime = Starttime;
            endtime = Endtime;
            textmatchcandidate1 = Textmatchcandidate1;
            textmatchcandidate2 = Textmatchcandidate2 == null ? Textmatchcandidate1 : Textmatchcandidate2;
        }

        public bool Includes(TimeRangeMatchCandidate subRangeToTestForInclude)
        {
            return (starttime <= subRangeToTestForInclude.starttime) &&
                   (endtime >= subRangeToTestForInclude.endtime);
        }
    }

    public class PeriodsMatchCandidate
    {
        public string textmatchcandidate1;
        public string textmatchcandidate2;
        public List<DateTime> PeriodeStarts;
        public List<DateTime> PeriodsEnds;

        private List<Datastore.DatastoreBaseDataRange> timePeriods = new List<Datastore.DatastoreBaseDataRange>();

        public PeriodsMatchCandidate(List<DateTime> PeriodeStarts, List<DateTime> PeriodsEnds, string Textmatchcandidate1, string Textmatchcandidate2 = null)
        {
            
            this.PeriodeStarts = PeriodeStarts;
            this.PeriodsEnds = PeriodsEnds;
            textmatchcandidate1 = Textmatchcandidate1;
            textmatchcandidate2 = Textmatchcandidate2 == null ? Textmatchcandidate1 : Textmatchcandidate2;

            if (PeriodeStarts.Count == 0) // || PeriodsEnds.Count == 0)
                throw new ArgumentException("Time periods must not be empty");

            bool tb = PeriodeStarts.Count > PeriodsEnds.Count;
            while (tb)
            {
                PeriodsEnds.Add(new DateTime(2099, 12, 31));
                tb = PeriodeStarts.Count > PeriodsEnds.Count;
            }

            tb = PeriodeStarts.Count < PeriodsEnds.Count;
            while (tb)
            {
                PeriodeStarts.Add(new DateTime(2099, 12, 31));
                tb = PeriodeStarts.Count > PeriodsEnds.Count;
            }

            for (int i = 0; i < PeriodeStarts.Count; i++)
                timePeriods.Add(new Datastore.DatastoreBaseDataRange() { dtTimestampStart = PeriodeStarts[i], dtTimestampEnd = PeriodsEnds[i] });
        }

        public bool IncludeEventBlock(DateTime start, DateTime end)
        {
            return timePeriods.Any(t => Helpers.DateHelper.DateRangeIncludesRange(t.dtTimestampStart, t.dtTimestampEnd, start, end));
        }

        public bool StartsInsideOf(DateTime start, DateTime end)
        {
            return timePeriods.Any(t => t.dtTimestampStart.InRange(start, end)); 
        }

    public DateTime FindClosestPeriodStart(DateTime eventStart)
        {
            if (PeriodeStarts.Count == 1)
                return PeriodeStarts[0];

            var list = PeriodeStarts.Select(p => new Tuple<DateTime, double>(p, (p - eventStart).TotalMilliseconds));

            return list.OrderByDescending(p => p.Item2).FirstOrDefault().Item1;
        }

        public DateTime FindClosestPeriodEnd(DateTime eventStart)
        {
            if (PeriodsEnds.Count == 1)
                return PeriodsEnds[0];

            var list = PeriodsEnds.Select(p => new Tuple<DateTime, double>(p, (p - eventStart).TotalMilliseconds));

            return list.OrderByDescending(p => p.Item2).FirstOrDefault().Item1;
        }
    }
}
