using System;
using System.Collections.Generic;
using System.Linq;
 

namespace LogfileMetaAnalyser.Helpers
{ 
    public static class DateHelper
    {
        public static bool DateRangeInterferesWithRange(DateTime clipStart, DateTime clipEnd, DateTime timeblockToCheck_Start, DateTime timeblockToCheck_End)
        {
            if (clipStart > clipEnd || timeblockToCheck_Start > timeblockToCheck_End)
                throw new ArgumentException("invalid date time ranges");

            return (!(timeblockToCheck_Start > clipEnd || timeblockToCheck_End < clipStart));
        }

        public static bool DateRangeIncludesRange(DateTime clipStart, DateTime clipEnd, DateTime timeblockToCheck_Start, DateTime timeblockToCheck_End)
        {
            if (clipStart > clipEnd || timeblockToCheck_Start > timeblockToCheck_End)
                throw new ArgumentException("invalid date time ranges");

            return (timeblockToCheck_Start >= clipStart && timeblockToCheck_End <= clipEnd);
        }

        public static DateTime MinDt(DateTime dt1, DateTime dt2)
        {
            if (dt1 < dt2)
                return (dt1);
            else
                return (dt2);
        }

        public static DateTime MaxDt(DateTime dt1, DateTime dt2)
        {
            if (dt1 > dt2)
                return (dt1);
            else
                return (dt2);
        }

        public static DateTime IfNull(DateTime dt1, DateTime dt2)
        {
            return (dt1.IsNull()) ? dt2 : dt1;
        }

        public static DateTime[] GetMeaningfulDateTimePoints(DateTime minDate, DateTime maxDate, int numOfPointsGoal = 10)
        {
            if (minDate > maxDate || minDate == DateTime.MinValue || maxDate == DateTime.MaxValue)
                throw new ArgumentException("invalid date objects");

            if (numOfPointsGoal < 2 || numOfPointsGoal > 1000)
                throw new ArgumentException("numOfPointsGoal must be in range 2..1000");


            List<DateTime> res = new List<DateTime>();
            var span = maxDate - minDate;
            var oneBlockRaw_sec = span.TotalSeconds / numOfPointsGoal;
            int oneBlockReal_sec;

            //Part 1: retrieve a way to round the first date to a human readable date
            var firstDt = minDate.AddSeconds(oneBlockRaw_sec); //goal: hh:m[05]:00 -> hh:m[02468]:00 -> hh:mm:00 -> hh:mm:s0
            Dictionary<byte, double> dictRoundHuman = new Dictionary<byte, double>();
            for (byte i = 0; i <= 9; i++)
                dictRoundHuman.Add(i, Math.Abs((firstDt - firstDt.RoundHuman(i)).TotalSeconds));

            double fivepercentLimit = (span.TotalSeconds * 0.05f);
            var dictColRoundHuman = dictRoundHuman.Where(kp => kp.Value <= fivepercentLimit).OrderByDescending(kp => kp.Value).ToArray();
            if (dictColRoundHuman.Length == 0)
                return res.ToArray(); //return no result

            byte bestfit = dictColRoundHuman[0].Key;
            //firstDt = firstDt.RoundHuman(bestfit);
            firstDt = minDate.RoundHuman(bestfit);


            //Part 2: Get a good human readable gap between two date points
            int[] secCandidates = {2, 10, 30, 60, //sec
                                   60*2, 60*5, 60*10, 60*15, 60*30, //minutes 10,15,30
                                   3600*1, 3600*2, 3600*3, 3600*4, 3600*6, 3600*10, 3600*12, 3600*15, 3600*18, 3600*20, //hours
                                   3600 *24*1 //days
                };

            oneBlockReal_sec = secCandidates.Select(c => new Tuple<int, double>(c, Math.Abs((span.TotalSeconds / c) - (1 + numOfPointsGoal))))  //item1 == seconds candidate; item2 == default difference to goal
                                   .OrderBy(o => o.Item2)
                                   .First().Item1;

            if (oneBlockReal_sec == 3600 * 24 * 1) //we are in range of days or higher, so we take the RawSeconds value as days
                oneBlockReal_sec = Convert.ToInt32(oneBlockRaw_sec / (3600f * 24)) * 3600 * 24;


            //Part 3: Calulate the single date points
            do
            {
                if (firstDt.InRange(minDate, maxDate))
                    res.Add(firstDt);

                firstDt = firstDt.AddSeconds(oneBlockReal_sec);
            } while (firstDt <= maxDate);


            //Part 4: check and return result
            if (res.Count <= numOfPointsGoal / 2f) //failed :(
                return res.ToArray(); //return no result

            return res.ToArray();
        }

       
    }

}
