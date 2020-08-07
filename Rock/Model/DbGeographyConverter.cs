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
using System.Data.Entity.Spatial;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Rock.Model
{
    /// <summary>
    /// 
    /// </summary>
    public class DbGeographyConverter : JsonConverter
    {
        /// <summary>
        /// Determines whether this instance can convert the specified object type.
        /// </summary>
        /// <param name="objectType">Type of the object.</param>
        /// <returns>
        ///   <c>true</c> if this instance can convert the specified object type; otherwise, <c>false</c>.
        /// </returns>
        public override bool CanConvert( Type objectType )
        {
            return objectType.IsAssignableFrom( typeof( string ) );
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
            if (reader.TokenType == JsonToken.Null)
            {
                return null;
            }

            JObject location = JObject.Load( reader );
            JToken wktToken = location["Geography"]["WellKnownText"];
            string wkt = wktToken.ToString();

            DbGeography converted = DbGeography.FromText( wkt, DbGeography.DefaultCoordinateSystemId );

            return converted;
        }

        /// <summary>
        /// Writes the JSON representation of the object.
        /// </summary>
        /// <param name="writer">The <see cref="T:Newtonsoft.Json.JsonWriter" /> to write to.</param>
        /// <param name="value">The value.</param>
        /// <param name="serializer">The calling serializer.</param>
        public override void WriteJson( JsonWriter writer, object value, JsonSerializer serializer )
        {
            serializer.Serialize( writer, value );
        }
    }
}