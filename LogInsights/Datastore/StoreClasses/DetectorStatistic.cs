using System;


namespace LogInsights.Datastore
{ 
    public class DetectorStatistic
    {
        public string detectorName = "N/A";
        public int numberOfLinesParsed;
        public int numberOfDetections;
        public double parseDuration;  //unit is ms
        public double finalizeDuration; //unit is ms

        public DetectorStatistic()
        {
            Clear();
        }

        public override string ToString()
        {
            return $"stats for {detectorName}: lines parsed: {numberOfLinesParsed}; detections: {numberOfDetections}; parseDuration: {parseDuration} ms; finalizeDuration: {finalizeDuration} ms";
        }

        public void Clear()
        {
            numberOfLinesParsed = 0;
            numberOfDetections = 0;
            parseDuration = -1d;
            finalizeDuration = -1d;
        }
    }
}
