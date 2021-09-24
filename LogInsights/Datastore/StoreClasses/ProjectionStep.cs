using System;
using System.Text.RegularExpressions;



namespace LogInsights.Datastore
{
    public class ProjectionStep: DatastoreBaseDataRange
    {

        private static Regex regex_SchemaClass = new Regex(@"(?<SchemaClassDisplay>.*) \((?<SchemaClassName>.*?)\)", RegexOptions.Compiled);
        private static Regex regex_SchemaType = new Regex(@"^(?<SchemaTypeGuess>[A-z]{3}[A-Za-z_0-9]+?)[ _\(]", RegexOptions.Compiled);
        private static Regex regex_Map = new Regex(@"(?<Map>.*) \((?<ID>[-a-fA-F0-9]*)\)", RegexOptions.Compiled);        

        public string stepId;
        public int stepNr;
        public string direction;
        public string useRevision;
        public string adHocObject;
        public string leftConnection;
        public string rightConnection;
        public SyncStepDetail syncStepDetail = new SyncStepDetail();

        private string _map;
        public string map
        {
            get { return _map; }
            set
            {
                _map = value;
                var mx = regex_Map.Match(value);
                if (mx.Success)
                    _map = mx.Groups["Map"].Value;
            }
        }

        private string _leftSchemaClassRaw;
        public string leftSchemaClassDisp { get; private set; }
        public string leftSchemaClassName { get; private set; }
        public string leftSchemaTypeGuess { get; private set; }
        public string leftSchemaClassRaw
        {
            get { return _leftSchemaClassRaw; }
            set
            {
                _leftSchemaClassRaw = value;

                var mx = regex_SchemaClass.Match(value);
                if (mx.Success)
                {
                    leftSchemaClassDisp = "\"" + mx.Groups["SchemaClassDisplay"].Value + "\"";
                    leftSchemaClassName = mx.Groups["SchemaClassName"].Value;
                }
                else
                    leftSchemaClassDisp = value;

                mx = regex_SchemaType.Match(mx.Groups["SchemaClassName"].Value == "" ? leftSchemaClassDisp : mx.Groups["SchemaClassName"].Value);
                if (mx.Success)
                    leftSchemaTypeGuess = mx.Groups["SchemaTypeGuess"].Value;
            }
        }

        private string _rightSchemaClassRaw;
        public string rightSchemaClassDisp { get; private set; }
        public string rightSchemaClassName { get; private set; }
        public string rightSchemaTypeGuess { get; private set; }
        public string rightSchemaClassRaw
        {
            get { return _rightSchemaClassRaw; }
            set
            {
                _rightSchemaClassRaw = value;

                var mx = regex_SchemaClass.Match(value);
                if (mx.Success)
                {
                    rightSchemaClassDisp = "\"" + mx.Groups["SchemaClassDisplay"].Value + "\"";
                    rightSchemaClassName = mx.Groups["SchemaClassName"].Value;
                }
                else
                    rightSchemaClassDisp = value;

                mx = regex_SchemaType.Match(mx.Groups["SchemaClassName"].Value == "" ? rightSchemaClassDisp : mx.Groups["SchemaClassName"].Value);
                if (mx.Success)
                    rightSchemaTypeGuess = mx.Groups["SchemaTypeGuess"].Value;
            }
        }


        public ProjectionStep()
        { }

        public override string ToString()
        {
            return string.Format("step '{0}', from {1} to {2}\r\nmap: {3}\r\nschema class left:{4}\r\nschema class right:{5}", stepId, dtTimestampStart, dtTimestampEnd, map, leftSchemaClassDisp, rightSchemaClassDisp);
        }
    }
}
