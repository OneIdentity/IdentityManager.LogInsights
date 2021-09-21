using System;
using System.Text.Json;
using System.Text.Json.Serialization;

using LogfileMetaAnalyser.Datastore;


namespace LogfileMetaAnalyser
{
    public abstract class ExportSettingBase
    {
        protected DatastoreStructure dsref;
        [JsonIgnore(Condition = JsonIgnoreCondition.Always)]

        public DatastoreStructure datastore
        {
            set { dsref = value; }
        }

        public ExportSettingBase(DatastoreStructure datastore)
        {
            dsref = datastore;
        }


        public string ExportAsJson()
        {
            return JsonSerializer.Serialize(this, new JsonSerializerOptions()
            {
                PropertyNameCaseInsensitive = true,
                IncludeFields = true, //include public fields even without an explicit getter/setter
                WriteIndented = true, //write pretty formatted text
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
            });
        }
    }
}
