// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
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
