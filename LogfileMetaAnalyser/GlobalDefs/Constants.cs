using System.Text.RegularExpressions;


namespace LogfileMetaAnalyser
{
    class Constants
    {
        //https://docs.microsoft.com/en-us/dotnet/standard/base-types/regular-expression-language-quick-reference

        public static Regex regexTimeStampAtLinestart =
            //    new Regex(@"^(<.>){0,2}(?<Timestamp>20\d\d[:.-]?\d\d[:.-]?\d\d[ :.-]?\d\d[:.-]?\d\d([:.-]\d\d([.]\d+)?)?)", RegexOptions.Compiled);
            new Regex(@"^(<.>){0,2}(?<Timestamp>20\d\d-\d\d-\d\d \d\d:\d\d(:\d\d(\.\d+)?)?)", RegexOptions.Compiled);

        public static Regex regexMessageMetaDataNLogDefault =
            new Regex(@"^(?<Timestamp>20\d\d-\d\d-\d\d \d\d:\d\d(:\d\d(\.\d+)?)?) (?<NLevel>CRITICAL|ERROR|WARN|INFO|DEBUG|TRACE) (?<NSourceExt2>[a-zA-Z0-9\.-]+ )?\((?<NSource>[0-9A-Za-z_.]+)(:(?<PID>\d+))? ((?<SID>[0-9a-fA-F.-]+)|(?<NSourceExt>[^)]+)|)\)([ ]*:)?(?<Payload>.*)", 
                            RegexOptions.Compiled | RegexOptions.Singleline);

        public static Regex regexMessageMetaDataNLogDefaultTestDebug =
            new Regex(@" (?<NLevel>CRITICAL|ERROR|WARN|INFO|DEBUG|TRACE) \((?<NSource>[0-9A-Za-z_.]+)(:(?<PID>\d+))? ((?<SID>[0-9a-fA-F.-]+)|(?<NSourceExt>[^)]+)|)\)([ ]*:)?(?<Payload>.*)",
                            RegexOptions.Compiled | RegexOptions.Singleline);

        public static Regex regexMessageMetaDataJobservice =
            new Regex(@"^(?<tag><.>(<.>)?){0,2}(?<Timestamp>20\d\d-\d\d-\d\d \d\d:\d\d(:\d\d(\.\d+)?)?(?<TimeOffset>\s+[+-]\d\d:\d\d)?)(.*?(?<SID>[-0-9a-zA-Z]{36}))?([ ]*:)?(?<Payload>.*)", 
                            RegexOptions.Compiled | RegexOptions.Singleline);

        public static string str_FormatPlaceholder = "{.*?}";
        public static string str_SingleQuote = @"N?'.*?[^'\\]'(?!['\\])";
        public static string str_DoubleQuote = "\".*?[^\\\\]\"";
        public static string str_Guid = "[a-zA-Z0-9]{8}-[a-zA-Z0-9]{4}-[a-zA-Z0-9]{4}-[a-zA-Z0-9]{4}-[a-zA-Z0-9]{12,14}";  //yes :( a-z and length either 36 (guid) or 38 ("VI" guid)

        public static Regex regexLiterals = new Regex(string.Format("{0}|{1}|{2}|{3}", str_FormatPlaceholder, str_SingleQuote, str_DoubleQuote, str_Guid),
                            RegexOptions.Compiled | RegexOptions.Singleline);


        public static System.Drawing.Color treenodeBackColorNormal = System.Drawing.Color.Transparent;
        public static System.Drawing.Color treenodeBackColorSuspicious = System.Drawing.Color.Orange;

        public static string[] logSourcesOfInterest = new string[]
            {
                "Jobservice",
                "SqlLog",
                "ObjectLog",
                "JobGenLog",

                "ProjectorComponent",

                "ProjectorEngine",
                "Projector",

                "SystemConnection",
                "SystemConnector",
                "SystemObjectData",

                "StopWatch",

                "SystemScope",
                "RevisionStore",

                "SystemMappingRule",
                "SchemaElement",

                "Scripting",
            };

        public static int NumberOfContextMessages = 24;

        public static System.Drawing.Color contextLinesUcHighlightColor = System.Drawing.Color.FromArgb(255, 252, 210);

        public static string messageInsignificantStopTermRegexString = @"Loading strings from|Creating SystemObjectData based on entity|TRACE .SchemaElement static";


        public static bool AdaptiveStatisticalStopKeyAnalyser_isEnabled = true;
        public static int AdaptiveStatisticalStopKeyAnalyser_statisticalWindowWidth = 2000; //calc the moving avg of the last measured n samples to have a statistical base
        public static int AdaptiveStatisticalStopKeyAnalyser_recalcSampleRateFrequency = 100; //every n measured samples the sample rate is recalculated
        /*
        p:\Projects\_nn\xMyDevelopment\LogfileMetaAnalyser\TestData\20190506_StdioProcessor_80_Trace_pid4776.log

        enabled = 0; statisticalWindowWidth = 1000; recalcSampleRateFrequency = 100 => 9.7 : 19.4
        enabled = 1; statisticalWindowWidth =  700; recalcSampleRateFrequency =  70 => 9.6 : 19.4  (gleich schnell wie ohne stats)
        enabled = 1; statisticalWindowWidth = 1000; recalcSampleRateFrequency = 100 => 9.5 : 19.2
        enabled = 1; statisticalWindowWidth = 2000; recalcSampleRateFrequency = 100 => 9.6 : 19.1
        enabled = 1; statisticalWindowWidth = 1000; recalcSampleRateFrequency = 500 => 9.7 : 19.4  (gleich schnell wie ohne stats)
        enabled = 1; statisticalWindowWidth = 5000; recalcSampleRateFrequency = 200 => 9.9 : 19.4  (langsamer als ohne stats)
        */



    }
}
