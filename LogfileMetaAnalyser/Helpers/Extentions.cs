using System;
using System.Collections.Generic;
using System.Linq;


namespace LogfileMetaAnalyser.Helpers
{
    //=======================================================================================================================================
    public static class Extentions_IEnnumerable
    {
        public static Dictionary<string, Tnew> ToDictionarySaveExt<Told, Tnew>(this IEnumerable<KeyValuePair<string, Told>> dict, StringComparer sc = null) where Told : class where Tnew : class
        {
            Dictionary<string, Tnew> result;
            
            if (sc == null)
                result = new Dictionary<string, Tnew>();
            else
                result = new Dictionary<string, Tnew>(sc);

            foreach (var elem in dict)
                result.Add(elem.Key, elem.Value as Tnew);

            return result;
        }


        /// <summary>
        /// similar function to IEnumerable.Take(count), but does not take the first found elements. Instead an equal distributed amount of elements all over the full range of the list is taken
        /// </summary>
        /// <typeparam name="T">type of the enumeration</typeparam>
        /// <param name="lst">the enumeration</param>
        /// <param name="count">number of elements to take at maximum</param>
        /// <param name="KeySelector">function which returns a dictionary where the key is an long typed attribute by this function spreads the elements all over the full range of the list</param>
        /// <returns></returns>
        public static IEnumerable<T> TakeDistributed<T>(this IEnumerable<T> lst, int count, Func<IEnumerable<T>, Dictionary<long,T>> KeySelector)
        {
            if (lst == null)
                throw new ArgumentNullException("lst");

            if (count <= 0)
                return default(IEnumerable<T>);

            //if more elements are required as available, return all available
            if (lst.Count() <= count)
                return lst;

            
            var keySelectorData = KeySelector(lst);

            //if the key selector function returned less elements than required, invalidate the function result and return the first reqired elements instead
            if (keySelectorData.Count < count)
                return lst.Take(count);


            //use a distribution algorithm
            List<T> res = new List<T>();
            long blockMin;
            long blockMax;
            long blockCenter;
            long elemKey = 0;
            bool fnd = false;
            long rangeMin = keySelectorData.Keys.Min();
            long rangeMax = keySelectorData.Keys.Max();

            var blocksize = (rangeMax-rangeMin) / count;

            
            for (long blockNr=0; blockNr < count; blockNr++)
            {
                blockMin = rangeMin + blockNr * blocksize;
                blockMax = rangeMin + (1+blockNr) * blocksize;

                //first try to find an elem within the block
                fnd = false;
                try
                {
                    var elem = keySelectorData.FirstOrDefault(e => MathHelper<long>.isWithin(e.Key, blockMin, blockMax));
                    if (keySelectorData.ContainsKey(elem.Key))
                    {
                        elemKey = elem.Key;
                        fnd = true;
                    }
                }
                catch { }

                //secondary try to find an element which is as close to the current block as possible
                if (!fnd)
                {
                    blockCenter = (blockMax - blockMin) / 2;
                    
                    elemKey = keySelectorData.Keys.OrderBy(x => Math.Abs(x - blockCenter)).First();
                }

                res.Add(keySelectorData[elemKey]);
                keySelectorData.Remove(elemKey);
            }

            return res;
        }

        public static bool HasNoData<T>(this IEnumerable<T> lst)
        {
            return (lst == null || !lst.Any());
        }
    }

    
    //=======================================================================================================================================
    public static class Extentions_Dictionary
    {
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dict"></param>
        /// <param name="key"></param>
        /// <returns>returns true in case the element was created</returns>
        public static bool AddOrIncrease<T>(this Dictionary<T, long> dict, T key)
        {
            if (dict.ContainsKey(key))
            {
                dict[key]++;
                return false;
            }
            else
            {
                dict.Add(key, 1);
                return true;
            }
        }

        public static bool AddOrIncrease<T>(this Dictionary<T, int> dict, T key)
        {
            if (dict.ContainsKey(key))
            {
                dict[key]++;
                return false;
            }
            else
            {
                dict.Add(key, 1);
                return true;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dict"></param>
        /// <param name="key"></param>
        /// <returns>returns true in case the element was just added to the dictionary</returns>
        public static bool EnableKey<T>(this Dictionary<T, bool> dict, T key)
        {
            if (dict.ContainsKey(key))
            {
                dict[key] = true;
                return false;
            }
            else
            {
                dict.Add(key, true);
                return true;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dict"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns>returns true in case the element value was just added to the dictionary</returns>
        public static bool AddOrUpdate<T, P>(this Dictionary<T, P> dict, T key, P value)
        {
            if (dict.ContainsKey(key))
            {
                dict[key] = value;
                return false;
            }
            else
            {
                dict.Add(key, value);
                return true;
            }
        }

        public static bool AddOrUpdate<T, P>(this Dictionary<T, List<P>> dict, T key, P value)
        {
            if (dict.ContainsKey(key))
            {
                List<P> lst = dict[key];
                lst.Add(value);
                dict[key] = lst;
                return false;
            }
            else
            {
                List<P> lst = new List<P>();
                lst.Add(value);
                dict.Add(key, lst);
                return true;
            }
        }

        public static K AddRange<K, V>(this Dictionary<K, V> dict,  K[] keys, V[] values)
        {
            if (keys.Length == 0)
                return default(K);

            if (keys.Length != values.Length)
                throw new ArgumentOutOfRangeException("list of values does not have the same number of elements than the list of values!");

            K lastAddedKey = default(K);
            for (int i = 0; i < keys.Length; i++)
            {
                if (!dict.ContainsKey(keys[i]))
                {
                    dict.Add(keys[i], values[i]);
                    lastAddedKey = keys[i];
                }
            }

            return lastAddedKey;
        }

        public static T GetOrAdd<T>(this Dictionary<string, T> dict, string key) where T : new()
        {
            if (!dict.ContainsKey(key))
                dict.Add(key, new T());

            return (dict[key]);
        }

        public static bool GetBoolOrAdd<T>(this Dictionary<T, bool> dict, T key, bool defaultValue = false)
        {
            if (!dict.ContainsKey(key))
                dict.Add(key, defaultValue);

            return (dict[key]);
        }

        public static TVal GetOrReturnDefault<TKey, TVal>(this Dictionary<TKey, TVal> dict, TKey key)
        {
            if (!dict.ContainsKey(key))
                return (default(TVal));

            return (dict[key]);
        }

        public static KeyValuePair<int, T> GetCenterElem<T>(this Dictionary<int, T> dict)
        {
            if (dict.Count == 0)
                return (new KeyValuePair<int, T>(0, default(T)));

            int c = Convert.ToInt32(dict.Count / 2f);

            return dict.ElementAt(c);
        }
    }


    //=======================================================================================================================================
    public static class Extentions_List
    {    
        public static T GetLastOrNull<T>(this List<T> list)
        {
            if (list.Any())
                return list.Last();
            else
                return default(T);
        }

        public static IEnumerable<T> EmptyIfNull<T>(this IEnumerable<T> arr)
        {
            if (arr == null)
                return new T[] { } as IEnumerable<T>;
            else
                return arr;
        }

        public static void AddIfNotPresent<T>(this List<T> list, T elem)
        {
            if (list == null)
                list = new List<T>();

            if (list.Contains(elem))
                list.Add(elem);
        }
    }


    //=======================================================================================================================================
    public static class Extentions_Array
    {
        public static T GetFirstElemOrDefault<T>(this T[] theArray, T theDefault)
        {
            if (theArray == null || theArray.Length == 0)
                return theDefault;

            return theArray[0];
        }

        public static T GetLastElemOrDefault<T>(this T[] theArray, T theDefault)
        {
            if (theArray == null || theArray.Length == 0)
                return theDefault;

            return theArray.Last();
        }

    }

    //=======================================================================================================================================
    public static class Extentions_DateTimeSpan
    {

        public static TimeSpan EnsurePositive(this TimeSpan timespan)
        {
            if (timespan.TotalMilliseconds >= 0)
                return timespan;

            return (new TimeSpan(0));
        }

        public static string ToHumanString(this TimeSpan timespan)
        {
            // 1d 1h:1m:1s.1ms
            
            if (timespan.TotalMilliseconds < 1000)
                return string.Format("{0} ms", timespan.TotalMilliseconds);

            if (timespan.TotalMilliseconds < 1000 * 60) //less 60sec
            {
                var ms = timespan.ToString("%f");
                ms = (ms == "0") ? "" : System.Globalization.CultureInfo.CurrentCulture.NumberFormat.CurrencyDecimalSeparator + ms;
                return string.Format("{0}{1} sec", timespan.ToString("%s"), ms);
            }

            if (timespan.TotalMilliseconds < 1000 * 60 * 60) //less 60min
                return string.Format("{0} min {1} sec", timespan.ToString("%m"), timespan.ToString("%s"));
            
            if (timespan.TotalMilliseconds < 1000 * 60 * 60 * 24) //less 24h
                return string.Format("{0} h {1} min {2} sec", timespan.ToString("%h"), timespan.ToString("%m"), timespan.ToString("%s"));

            return string.Format("{0} d {1} h {2} min {3} sec", timespan.ToString("dd"), timespan.ToString("%h"),  timespan.ToString("%m"), timespan.ToString("%s"));
        }


        /// <summary>
        /// compare 2 date objects with a 1 sec tollerance
        /// </summary>
        /// <param name="thisDt"></param>
        /// <param name="dt"></param>
        /// <returns></returns>
        public static bool AlmostEqual(this DateTime thisDt, DateTime dt, int tolleranceMs = 1001)
        {
            return Math.Abs((thisDt - dt).TotalMilliseconds) <= tolleranceMs;
        }

        /// <summary>
        /// returns true, when the current datetime object is lower|earlier (incl. tollerance) than the passed datetime
        /// </summary>
        /// <param name="thisdt"></param>
        /// <param name="dt"></param>
        /// <param name="tolleranceMs"></param>
        /// <returns></returns>
        public static bool LessThan(this DateTime thisdt, DateTime dt, int tolleranceMs = 1001)
        {
            DateTime baseDt = thisdt.AddMilliseconds(-1 * tolleranceMs);

            return (baseDt < dt);
        }

        /// <summary>
        /// returns true, when the current datetime object is higher|later (incl. tollerance) than the passed datetime
        /// </summary>
        /// <param name="thisdt"></param>
        /// <param name="dt"></param>
        /// <param name="tolleranceMs"></param>
        /// <returns></returns>
        public static bool MoreThan(this DateTime thisdt, DateTime dt, int tolleranceMs = 1001)
        {
            DateTime baseDt = thisdt.AddMilliseconds(tolleranceMs);

            return (baseDt > dt);
        }

        public static bool IsNull(this DateTime dt)
        {
            return dt == DateTime.MinValue || dt == DateTime.MaxValue;
        }

        public static bool InRange(this DateTime dt, DateTime rangeStart, DateTime rangeEnd, int tolleranceMsInsideRange = 0)
        {
            return dt.MoreThan(rangeStart, tolleranceMsInsideRange) 
                && dt.LessThan(rangeEnd, tolleranceMsInsideRange);
        }

        public static string ToHumanTimerange(this DateTime dt_from, DateTime dt_to, string pattern = "from {0} to {1}")
        {
            bool sameDay =  dt_from.Day == dt_to.Day && 
                            dt_from.Month == dt_to.Month && 
                            dt_from.Year == dt_to.Year;

            if (sameDay)
                return string.Format(pattern, dt_from.ToString("HH:mm:ss"), dt_to.ToString("HH:mm:ss"));
            else
                return string.Format(pattern, dt_from.ToString("g"), dt_to.ToString("g"));
        }        

        public static DateTime RoundHuman(this DateTime dt, byte level)  //hh:m[05]:00 -> hh:m[02468]:00 -> hh:mm:00 -> hh:mm:s0
        {
            int sec;
            int min;
            int hr;

            switch (level)
            {
                case 0: //hh:mm:ss:n00
                    int milli = Convert.ToInt32(Math.Round(1d * dt.Millisecond / 100d) * 100d).EnsureRange(0,999);
                    return new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second, milli);

                case 1: //hh:mm:s0
                    sec = Convert.ToInt16(Math.Round(dt.Second / 10f, 0) * 10);
                    if (sec >= 60)
                        return new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, 00).AddMinutes(1);
                    else
                        return new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, sec);

                case 2: //hh:mm:00
                    sec = dt.Second;
                    if (sec >= 30)
                        return new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, 00).AddMinutes(1);
                    else
                        return new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, 00);

                case 3: //hh:m[02468]:00
                    min = Convert.ToInt32(Math.Round((60f * dt.Minute + dt.Second) / 60f, 0));
                    if (min % 2 != 0)
                        min++;

                    if (min > 59)
                        return new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, 00, 00).AddHours(1);
                    else
                        return new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, min, 00);

                case 4: //hh:m[05]:00
                    min = Convert.ToInt32(Math.Round((60f * dt.Minute + dt.Second) / 60f, 0));
                    if (min % 5 != 0)
                    {
                        min -= 2;
                        while (min % 5 != 0)
                            min++;
                    }
                    if (min > 59)
                        return new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, 00, 00).AddHours(1);
                    else
                        return new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, min, 00);

                case 5: //hh:m0:00
                    min = Convert.ToInt32(Math.Round((60f * dt.Minute + dt.Second) / 60f, 0));
                    if (min % 10 != 0)
                    {
                        min -= 5;
                        while (min % 10 != 0)
                            min++;
                    }
                    if (min > 59)
                        return new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, 00, 00).AddHours(1);
                    else
                        return new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, min, 00);

                case 6: //hh:[03]0:00
                    min = Convert.ToInt32(Math.Round((60f * dt.Minute + dt.Second) / 60f, 0));
                    if (min % 30 != 0)
                    {
                        min -= 15;
                        while (min % 30 != 0)
                            min++;
                    }
                    if (min > 59)
                        return new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, 00, 00).AddHours(1);
                    else
                        return new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, min, 00);

                case 7: //hh:00:00
                    min = Convert.ToInt32(Math.Round((60f * dt.Minute + dt.Second) / 60f, 0));
                    if (min % 60 != 0)
                    {
                        min -= 30;
                        while (min % 60 != 0)
                            min++;
                    }
                    if (min > 59)
                        return new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, 00, 00).AddHours(1);
                    else
                        return new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, min, 00);

                case 8: //h[24680]:00:00
                    hr = Convert.ToInt32(Math.Round((60 * dt.Hour + dt.Minute) / 60f, 0));
                    if (hr % 2 != 0)
                    {
                        while (hr % 60 != 0)
                            hr++;
                    }
                    if (hr > 23)
                        return new DateTime(dt.Year, dt.Month, dt.Day, 00, 00, 00).AddHours(24);
                    else
                        return new DateTime(dt.Year, dt.Month, dt.Day, hr, 00, 00);

                case 9: //yyy-mm-dd 00:00:00
                    hr = dt.Hour;

                    if (hr >= 12)
                        return new DateTime(dt.Year, dt.Month, dt.Day, 00, 00, 00).AddDays(1);
                    else
                        return new DateTime(dt.Year, dt.Month, dt.Day, 00, 00, 00);
            }

            return dt;
        }

        /// <summary>
        /// transfers the DateTime into a string while the difference to the reference date is taken into account. The higher the difference the less important are second and ms parts and vice versa
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="dtref"></param>
        /// <param name="dateStringAtStart"></param>
        /// <returns></returns>
        public static string ToStringByReferenceDate(this DateTime dt, DateTime dtref, bool dateStringAtStart = false)
        {
            TimeSpan span = (dt > dtref) ? dt - dtref : dtref - dt;
            string dateString = dateStringAtStart ? $"{dt.ToString("D")} " : "";

            //basically the time data is printed with                              hh:mm
            //when the difference between dt and dtref < 60 minutes, the format is hh:mm:ss
            //when the difference between dt and dtref < 10 seconds, the format is hh:mm:ss.n
            //when the difference between dt and dtref <  2 seconds, the format is hh:mm:ss.nnn

            if (span.TotalMilliseconds < 2000)
                return $"{dateString}{dt.ToString("HH:mm:ss.fff")}";

            if (span.TotalMilliseconds < 10000)
                return $"{dateString}{dt.ToString("HH:mm:ss.f")}";

            if (span.TotalMinutes < 60)
                return $"{dateString}{dt.ToString("HH:mm:ss")}";

            return $"{dateString}{dt.ToString("HH:mm")}";
        }


    }


    //=======================================================================================================================================
    public static class Extentions_String
    {        
        public static int ToIntSave(this string s)
        {
            if (string.IsNullOrWhiteSpace(s))
                return 0;

            try
            {
                return Convert.ToInt32(s);
            }
            catch
            {
                return 0;
            }
        }

        public static string DefaultIfEmpty(this string s, string defaultValue)
        {
            if (string.IsNullOrEmpty(s))
                return defaultValue;
            else
                return s;
        }

        public static string TrimStart(this string s, string trimText)
        {
            if (string.IsNullOrEmpty(s))
                return "";

            if (string.IsNullOrEmpty(trimText) || !s.StartsWith(trimText))
                return s;

            return s.Substring(trimText.Length);
        }

    }


    //=======================================================================================================================================
    public static class Extentions_Numbers
    {
        public static int Int(this float number)
        {
            return Convert.ToInt32(number);
        }

        public static int Int(this double number)
        {
            return Convert.ToInt32(number);
        }

        public static int Int(this long number)
        {
            return Convert.ToInt32(number);
        }

        public static int IntDown(this float number)
        {
            return Convert.ToInt32(Math.Floor(number));
        }
 
        public static int IntDown(this double number)
        {
            return Convert.ToInt32(Math.Floor(number));
        }

        public static bool InRange(this int number, int rangeStart, int rangeEnd)
        {
            return (number >= rangeStart && number <= rangeEnd);
        }

        public static bool InRange(this float number, float rangeStart, float rangeEnd)
        {
            return (number >= rangeStart && number <= rangeEnd);
        }

        public static int EnsureRange(this int number, int lowerLimit, int upperLimit)
        {
            return Math.Min(Math.Max(number, lowerLimit), upperLimit);
        }

        public static double EnsureRange(this double number, double lowerLimit, double upperLimit)
        {
            return Math.Min(Math.Max(number, lowerLimit), upperLimit);
        }

        public static string ToString(this float number, int precesion)
        {
            double rnd = Math.Round(number, precesion);
            int rndDn = rnd.IntDown();

            if (rnd == rndDn)  // 13.0 == 13
                return rndDn.ToString();

            return Math.Round(number, precesion).ToString();
        }
    }



    //=======================================================================================================================================
    public static class Extentions_ColorAndDrawing
    {
        public static System.Drawing.Rectangle EnsureMinSize(this System.Drawing.Rectangle rect, int minWidth, int minHeight = -1)
        {
            if (minWidth > 0 && rect.Width < minWidth)
            {
                int  minWidthHalf = System.Convert.ToInt32(1f * minWidth / 2f);
                rect = new System.Drawing.Rectangle(rect.X - minWidthHalf, rect.Y, minWidth, rect.Height);
            }

            if (minHeight > 0 && rect.Height < minHeight)
            {
                int minHeightHalf = System.Convert.ToInt32(1f * minHeight / 2f);
                rect = new System.Drawing.Rectangle(rect.X, rect.Y - minHeightHalf, rect.Width, minHeight);
            }

            return rect;
        }

        public static System.Drawing.Color Complement(this System.Drawing.Color col)
        {
            if (col.GetBrightness() > 0.5f) //bright
                return System.Drawing.Color.Black;
            else
                return System.Drawing.Color.White;
        }

        public static System.Drawing.Color Darken(this System.Drawing.Color col, int level)
        {
            int r = col.R;
            int g = col.G;
            int b = col.B;

            r = Math.Max(0, Math.Min(255, r - level));
            g = Math.Max(0, Math.Min(255, g - level));
            b = Math.Max(0, Math.Min(255, b - level));
            
            return (System.Drawing.Color.FromArgb(col.A, r, g, b));
        }

        public static System.Drawing.Point Move(this System.Drawing.Point pt, int offsetX, int offsetY = int.MaxValue)
        {
            if (offsetY == int.MaxValue)
                offsetY = offsetX;

            return new System.Drawing.Point(pt.X + offsetX, pt.Y + offsetY);
        }

        public static System.Drawing.Rectangle GetUnion(this System.Drawing.Rectangle rect, System.Drawing.Rectangle rect2)
        {
            System.Drawing.Rectangle res = rect;
            res.Intersect(rect2);
            return res;
        }
    }
}
