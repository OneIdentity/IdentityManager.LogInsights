using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;


namespace LogInsights.Helpers
{
    public static class SqlHelper
    {
        public static Dictionary<string, string> GetValuePairsFromInsertCmd(string columnNames, string columnValues)
        {
            var columnNameLst = columnNames.Contains("\"") ? SplitSqlValues(columnNames, ",", "\"") : columnNames.Split(',').ToList();
            var columnValueLst = SplitSqlValues(columnValues);

            if (columnNameLst.Count != columnValueLst.Count)
                throw new ArgumentException($"count elements of columnNames ({columnNameLst.Count}) does not match number of elements in columnValues ({columnValueLst.Count})!");

            Dictionary<string, string> result = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);

            for (int pos = 0; pos < columnNameLst.Count; pos++)
                result.Add(columnNameLst[pos].Trim(), columnValueLst[pos]);
            
            return result;
        }

        public static Dictionary<string, string> GetValuePairsFromUpdateCmd(string updateList)  //updateList = A='A,B,C', B=N'x=ü', C=99, "Z"='z'
        {
            Dictionary<string, string> result = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);

            if (string.IsNullOrEmpty(updateList))
                return result;

            var pairs = SplitSqlValues(updateList, ",", "'", true);  // { A='A,B,C'}, {B=N'x=ü'}, {C=99}, {"Z"='z'}
            foreach (string attrNameValue in pairs)
            {
                //attrNameValue  e.g.: A=N'X'
                //we assume and require the attribute name does not contain a = in it
                string attrName = "";

                var pos = attrNameValue.IndexOf("=");
                if (pos > 0)
                    attrName = attrNameValue.Substring(0, pos).Trim().TrimStart('"').TrimEnd('"');
                                

                if (attrName != "")
                {
                    var attrValue = SplitSqlValues(attrNameValue.Substring(pos+1));
                    string attrValueNormed = attrValue[0];

                    result.AddOrUpdate(attrName, attrValueNormed);
                }
            }

            return result;
        }

        public static List<string> SplitSqlValues(string valuelist, string listSepChars = ",", string elemTextChars = "'", bool takeAllTokens = false)
        {
            // 'test, ok','ja' => {"test, ok"; "ja"}
            // 'D''Arc' ==> "D'Arc"
            // '' => ""
            // null => ""
            // N'test' => "test"

            List<string> result = new List<string>();
            Regex rx_number = new Regex(@"[0-9E.-]", RegexOptions.Compiled);

            if (string.IsNullOrEmpty(valuelist))
                return result;

            valuelist = valuelist.Trim();
            if (valuelist.StartsWith("(") && valuelist.EndsWith(")"))
                valuelist = valuelist.Substring(1, valuelist.Length - 2);

            if (string.IsNullOrEmpty(valuelist))
                return result;

            StringBuilder sb = new StringBuilder();
            bool instring = false;

            for (int pos = 0; pos < valuelist.Length; pos++)
            {
                char curr = valuelist[pos];

                //a new element begins, because the list seperator (,) is outside a string
                if (!instring && listSepChars.Contains(curr))
                {
                    result.Add(sb.ToString());
                    sb.Clear();
                    continue;
                }


                //a text begins
                if (elemTextChars.Contains(curr))
                {
                    if (!instring)
                    {
                        instring = true;

                        if (takeAllTokens)
                            sb.Append(curr);

                        continue;    
                    }

                    if (instring)
                    {
                        if (pos < valuelist.Length - 1 && valuelist[pos + 1] == curr) //it's a masking char 'D''Arc'
                        {
                            sb.Append(curr);
                            pos++;                        
                        }
                        else
                            instring = false;

                        if (takeAllTokens)
                            sb.Append(curr);

                        continue;
                    }
                }  //curr == elemTextChar


                if (takeAllTokens || instring || (rx_number.IsMatch(curr.ToString())))
                    sb.Append(curr);
            }

            if (valuelist.Length > 0)
                result.Add(sb.ToString());

            return result;
        }
    }
}
