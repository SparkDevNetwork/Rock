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
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Rock.Utility
{
    /// <summary>
    /// A JSON Converter that takes care of Deserializing a JSON property is serialized as either a single item or an array of those items
    /// See https://stackoverflow.com/a/32628995/1755417
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <seealso cref="Newtonsoft.Json.JsonConverter" />
    public class SingleOrArrayJsonConverter<T> : JsonConverter
    {
        /// <summary>
        /// Determines whether this instance can convert the specified object type.
        /// </summary>
        /// <param name="objectType">Type of the object.</param>
        /// <returns>
        /// <c>true</c> if this instance can convert the specified object type; otherwise, <c>false</c>.
        /// </returns>
        public override bool CanConvert( Type objectType )
        {
            return ( objectType == typeof( List<T> ) );
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
            JToken token = JToken.Load( reader );
            T[] resultData;
            if ( token.Type == JTokenType.Array )
            {
                resultData = token.ToObject<List<T>>().ToArray();
            }
            else
            {
                resultData = new List<T> { token.ToObject<T>() }.ToArray();
            }

            return resultData;
        }

        /// <summary>
        /// Gets a value indicating whether this <see cref="T:Newtonsoft.Json.JsonConverter" /> can write JSON.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this <see cref="T:Newtonsoft.Json.JsonConverter" /> can write JSON; otherwise, <c>false</c>.
        /// </value>
        public override bool CanWrite
        {
            get { return false; }
        }

        /// <summary>
        /// Writes the JSON representation of the object.
        /// </summary>
        /// <param name="writer">The <see cref="T:Newtonsoft.Json.JsonWriter" /> to write to.</param>
        /// <param name="value">The value.</param>
        /// <param name="serializer">The calling serializer.</param>
        /// <exception cref="NotImplementedException"></exception>
        public override void WriteJson( JsonWriter writer, object value, JsonSerializer serializer )
        {
            throw new NotImplementedException();
        }
    }

}
