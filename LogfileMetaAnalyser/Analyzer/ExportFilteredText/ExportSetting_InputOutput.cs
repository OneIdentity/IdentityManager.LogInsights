using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

using LogfileMetaAnalyser.Datastore;


namespace LogfileMetaAnalyser
{
    public class ExportSetting_InputOutput : IExportSetting
    {

        private DatastoreStructure dsref;
        private static string[] jsonExportTakeAttributeLst = new string[] { "filenamePostfix", "mergeFiles", "includeFileType_NLog", "includeFileType_JSLog"  };
        

        //Profile relevant
        public string filenamePostfix = "_filtered";
        public bool mergeFiles = false;  //not yet supported
        public bool includeFileType_NLog = false;
        public bool includeFileType_JSLog = false;
        
        //Non-Profile relevant
        public List<string> includeFiles = new List<string>();
        public string outputFolder = ".";

        public ExportSetting_InputOutput(DatastoreStructure datastore)
        {
            dsref = datastore;
        }
        

        public string ExportAsJson()
        {
            var jssett = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                Formatting = Formatting.Indented,
                ContractResolver = new ExportSettingsJsonContractResolver(jsonExportTakeAttributeLst, null)
            };

            return JsonConvert.SerializeObject(this, jssett);
        }

     
        public MessageMatchResult IsMessageMatch(TextMessage msg, object additionalData)
        {
            return MessageMatchResult.filterNotApplied;
        }

        public void Prepare()
        {
            // -> nothing to prepare
        }

        public DatastoreStructure datastore
        {
            set {dsref = value;}
        }
    }
}
