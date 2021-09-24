using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;

namespace LogInsights.Composition
{
    public static class Composer
    {
        private const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        public static IEnumerable<T> BuildRepo<T>(object[] parms, string externalAssemblyFileMask = null)
        {
            var mainAsm = typeof(T).Assembly;
            var path = Path.GetDirectoryName(mainAsm.Location);
            var assemblies = new List<Assembly> { mainAsm };

            if (!string.IsNullOrWhiteSpace(externalAssemblyFileMask))
            {
                var files = Directory.GetFiles(path!, externalAssemblyFileMask, SearchOption.TopDirectoryOnly);
                foreach (var file in files)
                    assemblies.Add(Assembly.LoadFile(file));
            }

            var types =  assemblies.SelectMany(a => a.GetTypes())
                .Where(t => typeof(T).IsAssignableFrom(t))
                .Where(t => !t.IsAbstract);

            foreach (var type in types) 
                yield return CreateInstance<T>(type, parms);
        }

        private static T CreateInstance<T>(Type t, object[] parms)
        {
            return (T)Activator.CreateInstance(
                t, 
                flags,
                default(Binder),
                parms,
                default(CultureInfo));
        }        
    }
}