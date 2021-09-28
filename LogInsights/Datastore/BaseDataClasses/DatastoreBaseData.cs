using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogInsights.Datastore
{
    public abstract class DatastoreBaseData
    {
        public string uuid;  //because loggerSourceId is not unique when StdIoprocessor is reused for another job
        public bool isDataComplete;
        public string metaData;

        public TextMessage message;

        public string loggerSourceId  //NLog SPID / sessionID / loggerID
        {
            get { return message?.Spid; }
        }

        public long logfilePosition
        {
            get { return message == null ? -1 : message.Locator.Position; }
        }

        public string logfileName
        {
            get { return message == null ? "" : message.Locator.Source; }
        }



        public DatastoreBaseData()
        {
            uuid = Guid.NewGuid().ToString();
        }

        public virtual string GetEventLocator()
        {
            if (message == null)
                return "";


            string logfilenameShort = System.IO.Path.GetFileName(message.Locator.Source);            

            return ($"{logfilenameShort}@+{message.Locator.Position.ToString()}");
        }

    }
}
