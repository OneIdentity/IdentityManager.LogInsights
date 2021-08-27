using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace LogfileMetaAnalyser.TextReading
{
    class TextMessageJsonContractResolver : DefaultContractResolver
    {
        private static string[] attributesToSkip = new string[] { "contextMsgBefore", "contextMsgAfter" };
        public TextMessageJsonContractResolver()
        { }

        protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
        {
            if (type != typeof(TextMessage))
                return base.CreateProperties(type, memberSerialization);

            IList<JsonProperty> properties = base.CreateProperties(type, memberSerialization);
            
            properties = properties.Where(p => !attributesToSkip.Contains(p.PropertyName)).ToList();

            return properties;
        }
    }
}
