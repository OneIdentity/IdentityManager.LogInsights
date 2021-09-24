using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogInsights.Helpers
{
    class StringHelper
    {  
        public static string TranslateLds(string ldsText)
        {
            //e,.g. #LDS#{0}. Executing synchronization step ({1})\n\tProcessing steps: {2}\n\tExecution time: {3}\n\t{4}| 03 | VerifiedDomain | 3 | 0,09 | s

            if (!ldsText.StartsWith("#LDS#"))
                return ldsText;

            ldsText = ldsText.Substring(5);
            string[] elems = ldsText.Split('|');
            if (elems.Length <= 1)
                return ldsText;

            List<string> parameters = new List<string>();
            for (int i = 0 + 1; i < elems.Length; i++)
                parameters.Add(elems[i]);

            return string.Format(elems[0], parameters.ToArray());
        }

        public static string ShortenText(string text, int maxCol = 150, int maxLines = 22)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            text = text.Replace("\r\n", "\n");
            text = text.Replace("\r\n\r\n", "\r\n");
            text = text.Replace("\n\n", "\n");            

            maxCol = Math.Max(4, maxCol);
            bool ok;
            bool blockSepPushed = false;
            int lno = 0;
            StringBuilder sb = new StringBuilder();
            var spl = text.Split(new string[] { System.Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            int curlen = spl.Length;
            float lowerbound = (maxLines / 2f);
            float upperbound = curlen - (maxLines / 2f);

            foreach (string line in spl)
            {
                if (line == "")
                    continue;

                lno++;

                ok = lno < lowerbound || lno > upperbound;

                if (ok)
                {
                    if (line.Length > maxCol)
                    {
                        string s = line;
                        while (s.Length > 0)
                        {
                            sb.AppendLine(s.Substring(0, Math.Min(s.Length, maxCol)));
                            s = s.Remove(0, Math.Min(s.Length, maxCol));
                            if (s.Length > 0)
                            {
                                lno++;

                                if (lno > maxLines)
                                {
                                    sb.AppendLine("...");
                                    break;
                                }
                            }
                        }
                    }
                    else
                        sb.AppendLine(line);

                }
                else
                    if (!blockSepPushed)
                {
                    sb.AppendLine("...");
                    blockSepPushed = true;
                }
            }

            return (sb.ToString());
        }
    }
}
