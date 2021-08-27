using System;
using System.Linq;

namespace LogfileMetaAnalyser.Helpers
{
    public enum Loglevels
    {
        Undef = 0,
        Critical = 1,
        Error = 2,
        Warn = 3,
        Info = 4,
        Debug = 5,
        Trace = 6
    }

    public static class Loglevel
    {
        public static byte MostDetailedLevel = 6;
        public static byte FewestDetailedLevel = 1;

        public static byte ConvertFromStringToNumber(string loglevelAsString)
        {
            return (byte)ConvertFromStringToEnum(loglevelAsString);            
        }

        public static Loglevels ConvertFromStringToEnum(string s)
        {
            if (string.IsNullOrWhiteSpace(s))
                return Loglevels.Undef;

            Loglevels res;
            if (Enum.TryParse<Loglevels>(s.Trim(), true, out res))
                return (res);

            return Loglevels.Undef;
        }

        public static string ConvertFromEnumToString(Loglevels level)
        {
            //return level.ToString().ToUpperInvariant();  //performance issue
            switch (level)
            {
                case Loglevels.Critical: return ("CRITICAL");
                case Loglevels.Error: return ("ERROR");
                case Loglevels.Warn: return ("WARN");
                case Loglevels.Info: return ("INFO");
                case Loglevels.Debug: return ("DEBUG");
                case Loglevels.Trace: return ("TRACE");
            }

            return "";
        }

        public static Loglevels ConvertFromNumberToEnum(byte level)
        {
            switch (level)
            {
                case 1: return Loglevels.Critical;
                case 2: return Loglevels.Error;
                case 3: return Loglevels.Warn;
                case 4: return Loglevels.Info;
                case 5: return Loglevels.Debug;
                case 6: return Loglevels.Trace;

                default:
                    return Loglevels.Undef;
            }
        }

        public static byte ConvertFromEnumToNumber(Loglevels level)
        {
            return (byte)Enum.Parse(typeof(Loglevels), level.ToString());           
        }

        public static bool IsGreater(this Loglevels levA, Loglevels LevB)
        {
            return (levA > LevB);
        }

        public static Loglevels GetHighestLevel(Loglevels[] levels)
        {
            return levels.Max(l => l);            
        }
                
    }
}
