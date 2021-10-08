using System;
using System.Collections.Concurrent;
using System.Text.Json;
using System.Linq;
using System.Text.Encodings.Web;

namespace LogInsights.Datastore
{
    public interface IDataStoreContent
    {
        bool HasData { get; } 
    }
    
    public class DataStore
    {
        private readonly ConcurrentDictionary<Type, IDataStoreContent> _content = new();

        public event EventHandler StoreInvalidation;

        public T GetOrAdd<T>() where T : IDataStoreContent, new()
        {
            return (T)_content.GetOrAdd(
                typeof(T),
                _ => Activator.CreateInstance<T>());
        }

        public void Clear()
        {
            _content.Clear();
            StoreInvalidation?.Invoke(this, EventArgs.Empty);
        }


        public bool HasData()
        {
            return _content.Values.Any(s => s.HasData);
        }

        public string AsJson()
        {
            var opts = new JsonSerializerOptions()
            {
                PropertyNameCaseInsensitive = true,
                MaxDepth = 60,
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                IncludeFields = true, //include public fields even without an explicit getter/setter
                WriteIndented = true, //write pretty formatted text
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
            };

            //collect the single "modules" from inside the datastore
            var jsonList = _content.Select(c => _FormatJson(
                                                    c.Key.FullName,
                                                    JsonSerializer.Serialize(_content[c.Key], Type.GetType(c.Key.FullName), opts)
                                                ));

            string listSep = $",{Environment.NewLine}";
            return $"[{Environment.NewLine}{string.Join(listSep, jsonList)}]";
        }

        private static string _FormatJson(string param, string value)
        {
            string nl = Environment.NewLine;
            return "\t{" + nl + 
                $"\t\t\"Type\": \"{param}\",{nl}" +
                $"\t\t\"Content\": {value}{nl}" +
                "\t}" + nl;
        }
    }
}
