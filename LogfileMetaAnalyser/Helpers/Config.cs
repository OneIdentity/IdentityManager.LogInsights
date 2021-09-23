using Microsoft.Win32;

using System;
using System.Security.Cryptography;
using System.Text;

namespace LogfileMetaAnalyser.Helpers
{
    public static class Config
    {
        private const string RegistryPath = @"Software\One Identity\One Identity Manager\LogInsights";

        public static string Get(string name)
        {
            try
            {
                using var key = Registry.CurrentUser.OpenSubKey(RegistryPath);

                if (key == null)
                    return "";

                var data = key.GetValue(name) as string;
                return data ?? "";
            }
            catch
            {
                return "";
            }
        }

        public static string GetProtected(string name)
        {
            try
            {
                var data = Get(name);

                if (string.IsNullOrEmpty(data))
                    return "";

                var protectedData = Convert.FromBase64String(data);
                var unprotectedData = ProtectedData.Unprotect(protectedData, null, DataProtectionScope.CurrentUser);

                return Encoding.UTF8.GetString(unprotectedData);
            }
            catch
            {
                return "";
            }
        }

        public static void Put(string name, string value)
        {
            try
            {
                using var key = Registry.CurrentUser.CreateSubKey(RegistryPath);

                if (key == null)
                    return;

                key.SetValue(name, value);
            }
            catch
            {
                // Ignore errors
            }
        }

        public static void PutProtected(string name, string value)
        {
            try
            {
                var unprotectedData = Encoding.UTF8.GetBytes(value);
                var protectedData = ProtectedData.Protect(unprotectedData, null, DataProtectionScope.CurrentUser);
                var base64 = Convert.ToBase64String(protectedData);

                Put(name, base64);
            }
            catch
            {
                // Ignore errors
            }
        }
    }
}
