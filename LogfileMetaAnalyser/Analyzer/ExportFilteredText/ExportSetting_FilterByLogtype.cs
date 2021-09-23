using System;
using System.Collections.Generic;
using System.Linq;
using System.Text; 
using System.Text.Json;
using System.Text.Json.Serialization;

using LogfileMetaAnalyser.Datastore;
using LogfileMetaAnalyser.Helpers;

namespace LogfileMetaAnalyser 
{
    public class ExportSetting_FilterByLogtype : ExportSettingBase, IExportSetting
    {
        //Profile relevant
        public Dictionary<LogLevel, bool> logLevelFilters = new();
        public Dictionary<string, bool> logSourceFilters = new();


        //Non-Profile relevant
        private bool _startDate_valid = false;
        
        private DateTime _startDate = DateTime.MinValue;
        [JsonIgnore(Condition = JsonIgnoreCondition.Always)]
        public DateTime startDate
        {
            set { _startDate = value; _startDate_valid = false; }


            get {
                if (_startDate_valid)
                    return _startDate;

                if (_startDate.IsNull())
                    _startDate = dsref.GeneralLogData.LogDataOverallTimeRangeStart;

                _startDate_valid = true;

                return _startDate;
            }
            
        }

        private bool _endDate_valid = false;

        private DateTime _endDate = DateTime.MaxValue; 
        [JsonIgnore(Condition = JsonIgnoreCondition.Always)]
        public DateTime endDate
        {
            set { _endDate = value; _endDate_valid = false; }
            get {
                if (_endDate_valid)
                    return _endDate;

                if (_endDate.IsNull())
                    _endDate = dsref.GeneralLogData.LogDataOverallTimeRangeFinish;

                _endDate_valid = true;
                return _endDate;
            }
        }

        private bool logLevelFilters_passUnseen = false;  //do not check, pass the filter
        private bool logSourceFilters_passUnseen = false; //do not check, pass the filter


        //constructor
        public ExportSetting_FilterByLogtype(DataStore datastore): base(datastore)
        {}


        public void Prepare()
        {
            //let's check: If ALL filters are enabled or disabled => disable filter
            int checkedLLFilters = logLevelFilters.Count(di => di.Value);
            if (checkedLLFilters == 0 || checkedLLFilters == LogLevelTools.MostDetailedLevel) //none or all are checked
                logLevelFilters_passUnseen = true;

            int checkedLSFilters = logSourceFilters.Count(di => di.Value);
            if (checkedLSFilters == 0 || logSourceFilters.All(di => di.Value)) //none or all are checked (not any one option is not checked)
                logSourceFilters_passUnseen = true;
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
