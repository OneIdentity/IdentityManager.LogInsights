using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

using LogfileMetaAnalyser.Datastore;


namespace LogfileMetaAnalyser
{
    public class ExportSetting_InputOutput : ExportSettingBase, IExportSetting
    {
        //Profile relevant
        public string filenamePostfix = "_filtered";
        public bool mergeFiles = false;  //not yet supported
        public bool includeFileType_NLog = false;
        public bool includeFileType_JSLog = false;

        //Non-Profile relevant
        [JsonIgnore(Condition = JsonIgnoreCondition.Always)]
        public List<string> includeFiles = new List<string>();

        [JsonIgnore(Condition = JsonIgnoreCondition.Always)]
        public string outputFolder = ".";



        //constructor
        public ExportSetting_InputOutput(DatastoreStructure datastore) : base(datastore)
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
