using System;
using System.Text.Json;
using System.Text.Json.Serialization;

using LogfileMetaAnalyser.Datastore;


namespace LogfileMetaAnalyser
{
    public abstract class ExportSettingBase
    {
        protected DataStore dsref;
        [JsonIgnore(Condition = JsonIgnoreCondition.Always)]

        public DataStore datastore
        {
            set { dsref = value; }
        }

        protected ExportSettingBase(DataStore datastore)
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
