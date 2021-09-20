using System;
using System.Linq;

namespace LogfileMetaAnalyser.Helpers
{
    public enum LogLevel
    {
        Undef = 0,
        Critical = 1,
        Error = 2,
        Warn = 3,
        Info = 4,
        Debug = 5,
        Trace = 6
    }

    public static class LogLevelTools
    {
        public static byte MostDetailedLevel = 6;
        public static byte FewestDetailedLevel = 1;

        public static byte ConvertFromStringToNumber(string loglevelAsString)
        {
            return (byte)ConvertFromStringToEnum(loglevelAsString);            
        }

        public static LogLevel ConvertFromStringToEnum(string s)
        {
            if (string.IsNullOrWhiteSpace(s))
                return LogLevel.Undef;

			if (Enum.TryParse(s.Trim(), true, out LogLevel res))
                return res;

            return LogLevel.Undef;
        }

		public static string ConvertFromEnumToString(LogLevel level)
		{
			//return level.ToString().ToUpperInvariant();  //performance issue
			switch (level)
			{
				case LogLevel.Critical: return ("CRITICAL");
				case LogLevel.Error: return ("ERROR");
				case LogLevel.Warn: return ("WARN");
				case LogLevel.Info: return ("INFO");
				case LogLevel.Debug: return ("DEBUG");
				case LogLevel.Trace: return ("TRACE");
			}

			return "";
		}

        public static LogLevel ConvertFromNumberToEnum(byte level)
		{
			return (LogLevel) level;
		}

        public static byte ConvertFromEnumToNumber(this LogLevel level)
        {
            return (byte)Enum.Parse(typeof(LogLevel), level.ToString());           
        }

        public static bool IsGreater(this LogLevel levA, LogLevel LevB)
        {
            return (levA > LevB);
        }

        public static LogLevel GetHighestLevel(LogLevel[] levels)
        {
            return levels.Max(l => l);            
        }
                
    }
}
