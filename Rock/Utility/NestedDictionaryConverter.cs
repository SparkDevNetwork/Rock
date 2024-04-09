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
using Newtonsoft.Json;

namespace Rock.Utility
{
    /// <summary>
    /// Custom JSON converter that can be used to read JSON and deserialize
    /// it into either a Dictionary&lt;string, object&gt; or a
    /// List&lt;object&gt;. All values will be returned as either a primitive
    /// or one of the two previously mentioned types.
    /// </summary>
    internal class NestedDictionaryConverter : JsonConverter
    {
        /// <inheritdoc/>
        public override bool CanWrite => false;

        /// <inheritdoc/>
        public override bool CanConvert( Type objectType )
        {
            return objectType == typeof( Dictionary<string, object> ) || objectType == typeof( List<object> );
        }

        /// <inheritdoc/>
        public override object ReadJson( JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer )
        {
            if ( objectType == typeof( Dictionary<string, object> ) )
            {
                var dict = new Dictionary<string, object>();

                reader.Read();

                while ( reader.TokenType == JsonToken.PropertyName )
                {
                    var key = ( string ) reader.Value;
                    object value;

                    reader.Read();

                    if ( reader.TokenType == JsonToken.StartObject )
                    {
                        value = serializer.Deserialize<Dictionary<string, object>>( reader );
                    }
                    else if ( reader.TokenType == JsonToken.StartArray )
                    {
                        value = serializer.Deserialize<List<object>>( reader );
                    }
                    else
                    {
                        value = serializer.Deserialize( reader );
                    }

                    dict.Add( key, value );

                    reader.Read();
                }

                return dict;
            }
            else if ( objectType == typeof( List<object> ) )
            {
                var list = new List<object>();

                reader.Read();

                while ( reader.TokenType != JsonToken.EndArray )
                {
                    object value;

                    if ( reader.TokenType == JsonToken.StartObject )
                    {
                        value = serializer.Deserialize<Dictionary<string, object>>( reader );
                    }
                    else if ( reader.TokenType == JsonToken.StartArray )
                    {
                        value = serializer.Deserialize<List<object>>( reader );
                    }
                    else
                    {
                        value = serializer.Deserialize( reader );
                    }

                    list.Add( value );

                    reader.Read();
                }

                return list;
            }

            return null;
        }

        /// <inheritdoc/>
        public override void WriteJson( JsonWriter writer, object value, JsonSerializer serializer )
        {
            throw new NotImplementedException();
        }
    }
}
