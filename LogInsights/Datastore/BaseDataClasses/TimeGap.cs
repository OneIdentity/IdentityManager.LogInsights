﻿using System;
using LogInsights.Helpers;

namespace LogInsights.Datastore
{
    public class TimeGap : DatastoreBaseDataRange
    {
        public TimeGap()
        { }

        public int GetGapInSecods()
        {
            return Convert.ToInt32(Math.Floor((dtTimestampEnd - dtTimestampStart).TotalSeconds));
        }

        public override string ToString()
        {
            return $"gap {dtTimestampStart.ToHumanTimerange(dtTimestampEnd)} which is {GetGapInSecods().ToString()} s; id = {loggerSourceId}";
        }
    }
}
