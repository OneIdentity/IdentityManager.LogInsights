using System;
using System.Linq;
using System.Text;

using LogInsights.Helpers;


namespace LogInsights.Datastore
{
    public class JobserviceJobattempt : DatastoreBaseDataRange
    {
        public JobserviceJobExecutionState jobExecutionState = JobserviceJobExecutionState.Unknown;

        public TextMessage endmessage;

        public string resultmessagetext;

        private bool _hasMissingStartOrFinishMessageCache = true; //initially we assume we have not all information
        public bool hasMissingStartOrFinishMessage
        {
            get
            {
                if (!_hasMissingStartOrFinishMessageCache)  //once this is false it cannot be true again, so it's final
                    return false;

                _hasMissingStartOrFinishMessageCache = dtTimestampStart.IsNull() || dtTimestampEnd.IsNull();

                return _hasMissingStartOrFinishMessageCache;
            }
        }




        public string SuccessInformation
        {
            get
            {
                bool b1 = dtTimestampStart.IsNull();
                bool b2 = dtTimestampEnd.IsNull();

                StringBuilder sb = new StringBuilder();

                if (b1)
                    sb.Append("start event missing; ");

                if (b2)
                    sb.Append("finish event missing; ");

                sb.Append(jobExecutionState.ToString());

                return sb.ToString();
            }
        }


        public JobserviceJobattempt()
        { }
    }
}
