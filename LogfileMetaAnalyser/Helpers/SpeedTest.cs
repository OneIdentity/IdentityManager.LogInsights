using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LogfileMetaAnalyser;

namespace LogfileMetaAnalyser.Helpers
{
    public class SpeedTest
    {
        static string teststring1 = @"2019-08-16 06:24:21.2558 DEBUG (    SqlLog ) : (< 1 ms) - select count(*) from ShoppingCartItem where ((not isnull(UID_ShoppingCartOrder, '') = '') and (UID_PersonInserted = 'e31ac7ed-a78e-45fc-aaec-5f55bd27745b'))";
        static string teststring2 = "bla bla bla -- bla bla bla -- ";

        public static void Meth1()
        {
            Meth1(teststring1);
            Meth1(teststring2);
        }

        public static void Meth2()
        {
            Meth2(teststring1);
            Meth2(teststring2);
        }


        public static  void   Meth1(string sx)
        {
            DateTime dt;
            string s;

            //p1
            var x = Constants.regexTimeStampAtLinestart.IsMatch(sx);
            if (!x)
                return;

            //p2
            var rm = Constants.regexMessageMetaDataNLogDefault.Match(sx);
            if (rm.Success)
            {
                if (!DateTime.TryParse(rm.Groups["Timestamp"].Value, out dt))
                    dt = DateTime.MinValue;               

                s = rm.Groups["NLevel"].Value;
                s = rm.Groups["NSource"].Value;
                s = rm.Groups["NSourceExt"].Value;
                s = rm.Groups["PID"].Value;
                s = rm.Groups["SID"].Value;
                s = rm.Groups["Payload"].Value;                
            }
        }

        public static  void   Meth2(string sx)
        {
            DateTime dt;
            string s;


            //p1
            var rm1 = Constants.regexTimeStampAtLinestart.Match(sx);
            if (!DateTime.TryParse(rm1.Groups["Timestamp"].Value, out dt))
                return;


            //p2
            var rm = Constants.regexMessageMetaDataNLogDefault.Match(sx);
            if (rm.Success)
            {
                s = rm.Groups["NLevel"].Value;
                s = rm.Groups["NSource"].Value;
                s = rm.Groups["NSourceExt"].Value;
                s = rm.Groups["PID"].Value;
                s = rm.Groups["SID"].Value;
                s = rm.Groups["Payload"].Value;
            }
        }

    }
}
