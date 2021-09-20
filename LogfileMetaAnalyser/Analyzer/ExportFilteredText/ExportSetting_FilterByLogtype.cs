using System;
using System.Collections.Generic;
using System.Linq;
using System.Text; 
using Newtonsoft.Json;

using LogfileMetaAnalyser.Datastore;
using LogfileMetaAnalyser.Helpers;

namespace LogfileMetaAnalyser 
{
    public class ExportSetting_FilterByLogtype : IExportSetting
    {
        private DatastoreStructure dsref;
        private static string[] jsonExportTakeAttributeLst = new string[] { "logLevelFilters", "logSourceFilters" };


        //Profile relevant
        public Dictionary<LogLevel, bool> logLevelFilters = new();
        public Dictionary<string, bool> logSourceFilters = new();


        //Non-Profile relevant
        private bool _startDate_valid = false;
        private DateTime _startDate;
        public DateTime startDate
        {
            set { _startDate = value; _startDate_valid = false; }


            get {
                if (_startDate_valid)
                    return _startDate;

                if (_startDate.IsNull())
                    _startDate = dsref.generalLogData.logDataOverallTimeRange_Start;

                _startDate_valid = true;

                return _startDate;
            }
            
        }

        private bool _endDate_valid = false;
        private DateTime _endDate;
        public DateTime endDate
        {
            set { _endDate = value; _endDate_valid = false; }
            get {
                if (_endDate_valid)
                    return _endDate;

                if (_endDate.IsNull())
                    _endDate = dsref.generalLogData.logDataOverallTimeRange_Finish;

                _endDate_valid = true;
                return _endDate;
            }
        }

        public DatastoreStructure datastore
        {
            set { dsref = value; }
        }

        private bool logLevelFilters_passUnseen = false;  //do not check, pass the filter
        private bool logSourceFilters_passUnseen = false; //do not check, pass the filter



        public ExportSetting_FilterByLogtype(DatastoreStructure datastore)
        {
            dsref = datastore;
            _startDate = DateTime.MinValue;
            _endDate = DateTime.MaxValue;            
        }


        public void Prepare()
        {
            //let's check: If ALL filters are enabled or disabled => disable filter
            int checkedLLFilters = logLevelFilters.Count(di => di.Value);
            if (checkedLLFilters == 0 || checkedLLFilters == LogLevelTools.MostDetailedLevel) //none or all are checked
                logLevelFilters_passUnseen = true;

            int checkedLSFilters = logSourceFilters.Where(di => di.Value).Count();
            if (checkedLSFilters == 0 | !logSourceFilters.Any(di => !di.Value)) //none or all are checked (not any one option is not checked)
                logSourceFilters_passUnseen = true;
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
            if (msg.messageTimestamp < startDate || msg.messageTimestamp > endDate)
                return MessageMatchResult.negative;

            if (!logLevelFilters_passUnseen && !logLevelFilters[msg.loggerLevel])
                return MessageMatchResult.negative;

            if (!logSourceFilters_passUnseen && (!logSourceFilters.ContainsKey(msg.loggerSource) || !logSourceFilters[msg.loggerSource]))
                return MessageMatchResult.negative;

            return MessageMatchResult.positive;
        }
    }
}
