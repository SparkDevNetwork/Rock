using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Rock.Utility
{
    /// <summary>
    /// Use with JSONConvert options in cases where you don't need UrlEncodedKey to be serialized (UrlEncodedKey takes a while to serialize since it has to encrypt stuff first)
    /// </summary>
    public class IgnoreUrlEncodedKeyContractResolver : DefaultContractResolver
    {
        /// <summary>
        /// Creates properties for the given <see cref="T:Newtonsoft.Json.Serialization.JsonContract" />.
        /// </summary>
        /// <param name="type">The type to create properties for.</param>
        /// <param name="memberSerialization">The member serialization mode for the type.</param>
        /// <returns>
        /// Properties for the given <see cref="T:Newtonsoft.Json.Serialization.JsonContract" />.
        /// </returns>
        protected override IList<JsonProperty> CreateProperties( Type type, MemberSerialization memberSerialization )
        {
            var result = base.CreateProperties( type, memberSerialization );

            return result.Where( a => a.PropertyName != "UrlEncodedKey" ).ToList();
        }
    }
}
