using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Diagnostics;


namespace LogInsights.Helpers
{
    public static class ProcessExec
    {
        private static Dictionary<string, string> extentionAppCache = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
        public static void OpenFileInEditor(string Filename, long FilePosition=-1)
        {
            string appTxt = GetAppForFileExt("txt");
            if (appTxt == "")
                appTxt = GetAppForFileExt("log");

            if (appTxt == "")
                return;

            Tuple<string, string> appTxtData = SplitDataIntoExeAndParams(appTxt);

            string paramaters = appTxtData.Item2.Replace("%1", Filename);
            if (FilePosition > 0 && !appTxtData.Item1.Contains(@"\notepad.exe"))
                paramaters += " -n" + FilePosition.ToString();

            ProcessStartInfo PSI = new ProcessStartInfo(appTxtData.Item1, paramaters);
            PSI.WindowStyle = ProcessWindowStyle.Maximized;

            Process.Start(PSI);
        }

        private static string GetAppForFileExt(string ext)
        {
            if (!extentionAppCache.ContainsKey(ext))
            {
                string appTxt = Helpers.RegistryHelper.GetApplicationForFileExt("txt");
                extentionAppCache.Add(ext, appTxt);
            }

            return extentionAppCache[ext];
        }


        public static Tuple<string, string> SplitDataIntoExeAndParams(string s)
        {
            Regex rx = new Regex("^(?<exe>\"?.*?\\.exe\"?)(?<params>.*)", RegexOptions.Compiled);

            var match = rx.Match(s);
            if (match.Success)
                return new Tuple<string, string>(match.Groups["exe"].Value, match.Groups["params"].Value);

            return new Tuple<string, string>(s, "");
        }

    }
}
