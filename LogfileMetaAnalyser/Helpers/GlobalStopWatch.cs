using System.Linq;
using System.Collections.Generic;


using System.Diagnostics;

namespace LogfileMetaAnalyser.Helpers
{
    class GlobalStopWatch
    {
        private static Dictionary<string, Stopwatch> dictWatches = new Dictionary<string, Stopwatch>();
        public static Dictionary<string, float> results = new Dictionary<string, float>();

        public static void StartWatch(string label)
        {
#if DEBUG
            if (!dictWatches.ContainsKey(label))
                dictWatches.Add(label, new Stopwatch());


            dictWatches[label].Start();
#endif
        }

        public static void StopWatch(string label)
        {
#if DEBUG
            if (dictWatches.ContainsKey(label))
                dictWatches[label].Stop();
            else
                dictWatches.Add(label, new Stopwatch());
#endif 
        }

        public static Dictionary<string, float> GetResult()
        {
            if (results.Count == 0)
            {
                foreach (var kp in dictWatches.OrderBy(x => x.Key))
                {
                    StopWatch(kp.Key);
                    results.Add(kp.Key, kp.Value.ElapsedMilliseconds);
                }
            }

            return results;
        }
    }
}
