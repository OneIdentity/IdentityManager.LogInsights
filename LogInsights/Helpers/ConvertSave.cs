using System;
using System.Collections.Generic;
using System.Linq;


namespace LogInsights.Helpers
{
    public static class ConvertSave
    {

        public static int ConvertToInt(string s)
        {
            if (string.IsNullOrWhiteSpace(s))
                return -1;

            int res = -1;
            try
            {
                res = System.Convert.ToInt32(s.Trim());
            }
            catch { }

            return (res);
        }

        public static uint ConvertToUInt(string s)
        {
            if (string.IsNullOrWhiteSpace(s))
                return 0;

            uint res = 0;
            try
            {
                res = System.Convert.ToUInt32(s.Trim());
            }
            catch { }

            return (res);
        }
    }
}
