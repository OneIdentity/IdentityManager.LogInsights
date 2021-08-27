using System;
using System.Collections.Generic;
using System.Linq; 

using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;


namespace LogfileMetaAnalyser
{
    class ExportSettingsJsonContractResolver : DefaultContractResolver
    {
        private string[] attributesToExport = new string[] { };
        private string[] attributesNotToExport = new string[] { };
        public ExportSettingsJsonContractResolver(string[] attributesToExport = null, string[] attributesNotToExport = null)
        {
            this.attributesToExport = attributesToExport == null ? new string[] { } : attributesToExport;
            this.attributesNotToExport = attributesNotToExport == null ? new string[] { } : attributesNotToExport;
        }

        protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
        {
            IList<JsonProperty> properties = base.CreateProperties(type, memberSerialization);

            properties = properties.Where(p =>
                                                (attributesToExport.Length == 0 || attributesToExport.Contains(p.PropertyName)) 
                                                &&
                                                !attributesNotToExport.Contains(p.PropertyName)
                                          )
                                          .ToList();

            return properties;
        }
    }
}
