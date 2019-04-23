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

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using Rock.Web.Cache;

namespace Rock.Utility
{
    /// <summary>
    /// Serializes only the specified fields depending on the LoadAttributes parameter of a REST call
    /// if the parameter value is 'simple' or True, only the specified fields will be specified
    /// if the parameter value is 'expanded', the object will be serialized normally
    /// </summary>
    public class AttributeCacheJsonConverter : SimpleModeJsonConverter<AttributeCache>
    {
        /// <summary>
        /// Gets the properties to serialize in simple mode.
        /// </summary>
        /// <value>
        /// The properties to serialize in simple mode.
        /// </value>
        public override string[] PropertiesToSerializeInSimpleMode
        {
            get
            {
                return new string[] { "Id", "Key", "Name" };
            }
        }

        /// <summary>
        /// Gets a value indicating whether this
        /// <see cref="T:Newtonsoft.Json.JsonConverter" /> can read JSON.
        /// </summary>
        /// <value>
        /// <c>true</c> if this <see cref="T:Newtonsoft.Json.JsonConverter" /> can read JSON; otherwise, <c>false</c>.
        /// </value>
        public override bool CanRead
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Reads the JSON representation of the object.
        /// </summary>
        /// <param name="reader">The <see cref="T:Newtonsoft.Json.JsonReader" /> to read from.</param>
        /// <param name="objectType">Type of the object.</param>
        /// <param name="existingValue">The existing value of object being read.</param>
        /// <param name="serializer">The calling serializer.</param>
        /// <returns>
        /// The object value.
        /// </returns>
        public override object ReadJson( JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer )
        {
            var rockJsonReader = reader as RockJsonTextReader;
            var deserializeInSimpleMode = rockJsonReader != null && rockJsonReader.DeserializeInSimpleMode;

            var jsonObject = JObject.Load( reader );
            var target = Create( objectType, jsonObject );
            serializer.Populate( jsonObject.CreateReader(), target );

            var attributeCache = target as AttributeCache;

            if ( deserializeInSimpleMode && attributeCache != null && attributeCache.Id > 0 )
            {
                return AttributeCache.Get( attributeCache.Id );
            }

            return attributeCache;
        }

        private object Create( Type objectType, JObject jsonObject )
        {
            return new AttributeCache();
        }
    }
}