using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

using LogInsights.Datastore;


namespace LogInsights
{
    public class ExportSetting_InputOutput : ExportSettingBase, IExportSetting
    {
        //Profile relevant
        public string filenamePostfix = "_filtered";
        public bool mergeFiles = false;  //not yet supported

        //Non-Profile relevant
        [JsonIgnore(Condition = JsonIgnoreCondition.Always)]
        public List<string> includeFiles = new List<string>();

        [JsonIgnore(Condition = JsonIgnoreCondition.Always)]
        public string outputFolder = ".";



        //constructor
        public ExportSetting_InputOutput(DataStore datastore) : base(datastore)
        {}
        
     
        public MessageMatchResult IsMessageMatch(TextMessage msg, object additionalData)
        {
            return MessageMatchResult.filterNotApplied;
        }

        public void Prepare()
        {
            // -> nothing to prepare
        }

       
    }
}
