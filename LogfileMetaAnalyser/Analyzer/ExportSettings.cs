using System;
using System.Collections;
using System.Linq;

using System.Text.Json;


namespace LogfileMetaAnalyser
{
    public class ExportSettings
    {
        //1.) Input/Output file options
        public ExportSetting_InputOutput inputOutputOptions;

        //2.) Filter by Activity
        public ExportSetting_FilterByActivity filterByActivity;

        //3.) Filter by log type
        public ExportSetting_FilterByLogtype filterByLogtype;

        //4.) Filter by RegEx
        public ExportSetting_RegexFilter filterByRegex;

        public IExportSetting[] exportSettingsMods
        {
            get
            {
                return new IExportSetting[]
                {
                    inputOutputOptions,
                    filterByLogtype,
                    filterByActivity,
                    filterByRegex
                };
            }
        }
        private Datastore.DataStore ds;


        public ExportSettings(Datastore.DataStore datastore)
        {
            ds = datastore;

            inputOutputOptions =  new ExportSetting_InputOutput(ds);
            filterByLogtype = new ExportSetting_FilterByLogtype(ds);
            filterByActivity = new ExportSetting_FilterByActivity(ds);
            filterByRegex = new ExportSetting_RegexFilter(ds);            
        }

        public void PutDatastoreRef(Datastore.DataStore datastore)
        {
            ds = datastore;

            foreach (var exportSettingsMod in exportSettingsMods)
                exportSettingsMod.datastore = datastore;
        }

        public void PrepareForFilterAndExport() 
        {
            //take the export settings and prepare objects that can be easily and in a fast way be used by IsMessageMatch()

            //1.) Input/Output file options            
            
            //2.) Filter by Activity
            // -> collect all SPIDs (not uuid) and put it in a hash table
            
            //3.) Filter by log type
            // -> the only thing to prepare is to define fast readable variables if a filter is necessary or not
            
            //4.) Filter by RegEx
            // -> prepare 2 fast accessable collections (before filters; after filters) for all enables and MEANINGFULL regex (".*" is not meaningfull)           
            

            foreach (var e in exportSettingsMods)
                e.Prepare();
        }

        public bool IsMessageMatch(TextMessage msg)
        {
            MessageMatchResult filterResponse;


            //apply regex of type "Before all other filters"
            filterResponse = filterByRegex.IsMessageMatch(msg, true);
            if (filterResponse == MessageMatchResult.negative)
                return false;
            if (filterResponse == MessageMatchResult.positive)
                return true;


            //filter by Logtype (Startdate, Enddate, Loglevel, LogSource)
            filterResponse = filterByLogtype.IsMessageMatch(msg, null);
            if (filterResponse == MessageMatchResult.negative)
                return false;


            //filter by log activity         
            filterResponse = filterByActivity.IsMessageMatch(msg, null);
            if (filterResponse == MessageMatchResult.negative)
                return false;


            //regex filters afterwards     
            filterResponse = filterByRegex.IsMessageMatch(msg, false);
            if (filterResponse == MessageMatchResult.negative)
                return false;
            if (filterResponse == MessageMatchResult.positive)
                return true;


            //finally if all filters had the chance to say NO, if we are here the message can pass to get into the result
            return true;
        }

        public string AsJson()
        {
            //transfer each IExportSetting submodule object into a json string
            var dataToExport = exportSettingsMods.Select(e => e.ExportAsJson()).ToArray();

            //put all strings into a string list and create a new json
            return JsonSerializer.Serialize(dataToExport, new JsonSerializerOptions()
            {
                PropertyNameCaseInsensitive = true,
                IncludeFields = true, //include public fields even without an explicit getter/setter
                WriteIndented = true, //write pretty formatted text
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
            });
        }

        public static ExportSettings FromJson(string jsonText)
        {
            string[] exportsettingImport = null;
            ExportSettings result = new ExportSettings(null); 

            exportsettingImport = JsonSerializer.Deserialize<string[]>(jsonText);

            if (exportsettingImport != null && exportsettingImport.Length == result.exportSettingsMods.Length)
            {
                for (int i = 0; i < exportsettingImport.Length; i++)
                {
                    string jsonTextForSubModule = exportsettingImport[i] ?? "";

                    switch (i)
                    {
                        case 0:
                            result.inputOutputOptions = jsonTextForSubModule == "" ?
                                new ExportSetting_InputOutput(null) :
                                JsonSerializer.Deserialize<ExportSetting_InputOutput>(jsonTextForSubModule);
 
                            break;
                        case 1:
                            result.filterByLogtype = jsonTextForSubModule == "" ?
                                    new ExportSetting_FilterByLogtype(null) :
                                    JsonSerializer.Deserialize<ExportSetting_FilterByLogtype>(jsonTextForSubModule);
                            break;
                        case 2:
                            result.filterByActivity = jsonTextForSubModule == "" ?
                                    new ExportSetting_FilterByActivity(null) :
                                    JsonSerializer.Deserialize<ExportSetting_FilterByActivity>(jsonTextForSubModule);
                            break;
                        case 3:
                            result.filterByRegex = jsonTextForSubModule == "" ?
                                    new ExportSetting_RegexFilter(null) :
                                    JsonSerializer.Deserialize<ExportSetting_RegexFilter>(jsonTextForSubModule);
                            break;
                    }
                }
            }
            else
            {
                string subM = "JSON parsing error";

                if (exportsettingImport.Length != result.exportSettingsMods.Length)
                    subM = $"unexpected number of sub modules read (expected {result.exportSettingsMods.Length}; got {exportsettingImport.Length}";

                throw new Exception($"cannot read from json to build up a new export settings structure! {subM}");
            }

            return result;
        }



    }
   
}
