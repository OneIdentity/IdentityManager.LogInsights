using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;


namespace LogfileMetaAnalyser.Helpers
{
    public static class RegistryHelper
    {
        public static string GetApplicationForFileExt(string fileext)
        {
            if (string.IsNullOrEmpty(fileext))
                throw new ArgumentNullException("empty file extention passed!");

            if (!fileext.StartsWith("."))
                fileext = "." + fileext;

            string regData = "";
            string[] prefixes = new string[] { @"HKEY_CLASSES_ROOT", @"HKEY_CURRENT_USER\Software\Classes\", @"HKEY_LOCAL_MACHINE\Software\Classes\" };
            
            
            //https://stackoverflow.com/questions/2681878/associate-file-extension-with-application

            //1.) HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\FileExts\.txt\UserChoice -> ProgId == Applications\notepad++.exe
            //2.) *\Software\Classes\Applications\notepad++.exe\shell\open\command -> default
            if (regData == "")
                if (TryGetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\FileExts\" + fileext + @"\UserChoice", "ProgId", out regData))
                {
                    string handlerApp = regData;
                    regData = "";
                    foreach (string rt in prefixes)
                    {
                        if (!TryGetValue(rt + handlerApp + @"\shell\open\command", "", out regData))
                            regData = "";

                        if (regData != "")
                            break;
                    }
                }


            //1.)HKEY_CLASSES_ROOT\.txt -> default = "blabla"
            //2.)HKEY_CLASSES_ROOT\blabla\shell\open\command -> default

            //1.)HKEY_CURRENT_USER\Software\Classes\.txt -> default = "blabla"
            //2.)HKEY_CURRENT_USER\Software\Classes\blabla\shell\open\command -> default
            if (regData == "")
                foreach (var prefix in prefixes)
                {
                    if (regData != "")
                        break;

                    if (TryGetValue(prefix + fileext, "", out regData))
                        if (!TryGetValue(prefix + regData + @"\shell\open\command", "", out regData))
                            regData = "";
                }

            return regData;
        }

        public static bool TryGetValue(string regPath, string regValue, out string result)
        {
            result = "";
            try
            {
                result = Registry.GetValue(regPath, regValue, "#?#").ToString();
            }
            catch
            {
                return false;
            }

            if (result == "#?#")
            {
                result = "";
                return false;
            }
            else
                return true;
        }
    }
}
