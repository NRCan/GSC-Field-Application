using Newtonsoft.Json.Serialization;
using System.Reflection;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace GSCFieldApp.Services
{
    //helper to ignore some properties from serialization
    public class IgnorePropertiesResolver : DefaultContractResolver
    {
        private HashSet<string> _propsToIgnore;

        public IgnorePropertiesResolver(IEnumerable<string> propNamesToIgnore)
        {
            _propsToIgnore = new HashSet<string>(propNamesToIgnore);
        }

        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            JsonProperty property = base.CreateProperty(member, memberSerialization);

            if (_propsToIgnore.Contains(property.PropertyName))
            {
                property.ShouldSerialize = (x) => { return false; };
            }

            return property;
        }
    }
}