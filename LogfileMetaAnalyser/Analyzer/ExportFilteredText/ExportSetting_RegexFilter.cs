using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Text.Json;
using System.Text.Json.Serialization;

using LogfileMetaAnalyser.Datastore;


namespace LogfileMetaAnalyser
{
    public class ExportSetting_RegexFilter : ExportSettingBase, IExportSetting
    {
        //Profile relevant
        public List<ExportRegex> rxFilters { get; private set; }

        //Non-Profile relevant
        private List<ExportRegexInstance> filterByRegex_AppliedAtStart_RegexLst = new List<ExportRegexInstance>();
        private List<ExportRegexInstance> filterByRegex_AppliedAtEnd_RegexLst = new List<ExportRegexInstance>();

        private bool isEnabledAtStart = false;
        private bool isEnabledAtEnd = false;


        //constructor
        public ExportSetting_RegexFilter(DatastoreStructure datastore) : base(datastore)
        {
            rxFilters = new List<ExportRegex>();
            for (int i = 1; i <= 10; i++)
                rxFilters.Add(new ExportRegex());
        }

        
        public void Prepare()
        {
            filterByRegex_AppliedAtStart_RegexLst.Clear();
            filterByRegex_AppliedAtEnd_RegexLst.Clear();

            foreach (var rf in rxFilters.Where(t => t.enabled && t.regexText.Trim() != ".+" && t.regexText.Trim() != ".*" && t.regexText.Trim() != "."))
            {
                ExportRegexInstance rxi = new ExportRegexInstance()
                {
                    regex = new Regex(rf.regexText, RegexOptions.Multiline | (rf.ignoreCase ? (RegexOptions.IgnoreCase | RegexOptions.Singleline) : RegexOptions.Singleline)),
                    hasToMatch = rf.isMatch
                };

                if (rf.isAppliedAtStart)
                    filterByRegex_AppliedAtStart_RegexLst.Add(rxi);
                else
                    filterByRegex_AppliedAtEnd_RegexLst.Add(rxi);
            }

            isEnabledAtStart = filterByRegex_AppliedAtStart_RegexLst.Count > 0;
            isEnabledAtEnd = filterByRegex_AppliedAtEnd_RegexLst.Count > 0;
        }

        

        public MessageMatchResult IsMessageMatch(TextMessage msg, object additionalData)
        {
            bool isCheckAtBeginning = (bool)(additionalData ?? true);

            //the algorithm is as follows: all MustMatch filters are OR connected, all NotMatch filters are OR connected: (NotMatch1 AND NotMatch2 AND NotMatch3) + (Match1 OR Match2)
            //if we have one NotMatch filter, and the input does not match it also has not to match all other NotMatch filters to finally have MessageMatchResult.positive
            //if we have one Match filter and the input does not match, it might match another Match filter. If one Match filter finds the desired pattern, we have MessageMatchResult.positive
            //if all Match filters returns MessageMatchResult.negative, the whole regex filter is MessageMatchResult.negative
            //if the whole chain of NotMatch filters is negative, the whole regex filter is negative

            if (isCheckAtBeginning)
            {
                if (!isEnabledAtStart)
                    return MessageMatchResult.filterNotApplied;

                return isFilterChainMatch(filterByRegex_AppliedAtStart_RegexLst, msg.messageText);                
            }
            else  //check at the end of the filter chain
            {
                if (!isEnabledAtEnd)
                    return MessageMatchResult.filterNotApplied;

                return isFilterChainMatch(filterByRegex_AppliedAtEnd_RegexLst, msg.messageText);
            }

        }


        private MessageMatchResult isFilterChainMatch(List<ExportRegexInstance> regexFilterLst, string msgText)
        {
            var regexFilterLst_isToMatch    = regexFilterLst.Where(f => f.hasToMatch);
            var regexFilterLst_isNotToMatch = regexFilterLst.Where(f => !f.hasToMatch);

            //first eval the ToMatch filter
            MessageMatchResult r1 = handleFilterForToMatch(regexFilterLst_isToMatch, msgText);

            //if this was negative, exit here negativly
            if (r1 == MessageMatchResult.negative)
                return MessageMatchResult.negative;

            //eval NotMatch filters
            MessageMatchResult r2 = handleFilterForNotToMatch(regexFilterLst_isNotToMatch, msgText);

            return r2;                
        }

        private MessageMatchResult handleFilterForNotToMatch(IEnumerable<ExportRegexInstance> regexFilterLst, string msgText)
        {
            //we need to validate all filters, because they are AND connected:  (NotMatch1 AND NotMatch2 AND NotMatch3)
            //but as soon as one filter says it matches the whole filter exists with false
            
            foreach (var rxInst in regexFilterLst)            
                if (rxInst.regex.IsMatch(msgText))  //means: the input text contains a pattern, which is not wanted 
                    return MessageMatchResult.negative;            

            return MessageMatchResult.positive;  //no violation: the input text was not found by the regex, which was demanded
        }

        private MessageMatchResult handleFilterForToMatch(IEnumerable<ExportRegexInstance> regexFilterLst, string msgText)
        {
            //we need to validate all filters until one says it matches            
            foreach (var rxInst in regexFilterLst)            
                if (rxInst.regex.IsMatch(msgText))
                    return MessageMatchResult.positive;            

            return MessageMatchResult.negative; //violation: the text was not found by the regex, which was not demanded
        }
    }


    public class ExportRegex
    {
        public bool enabled = false;
        public string regexText = ".*";
        public bool ignoreCase = false;
        public bool isMatch = true;
        public bool isAppliedAtStart = true;

        public ExportRegex()
        { }
    }


    public class ExportRegexInstance
    {
        public Regex regex;
        public bool hasToMatch;

        public ExportRegexInstance()
        { }
    }
}
