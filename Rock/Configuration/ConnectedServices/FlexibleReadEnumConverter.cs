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

using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Rock.Configuration.ConnectedServices
{
    /// <summary>
    /// A <see cref="JsonConverter{T}"/> that can read both string and numeric
    /// enumerations for enum types. When writing, it always writes the numeric
    /// representation of the enum value.
    /// </summary>
    /// <typeparam name="TEnum"></typeparam>
    internal class FlexibleReadEnumConverter<TEnum> : JsonConverter<TEnum>
        where TEnum : struct, Enum
    {
        /// <inheritdoc/>
        public override TEnum Read( ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options )
        {
            if ( reader.TokenType == JsonTokenType.Number )
            {
                var raw = reader.GetInt64();
                return ( TEnum ) Enum.ToObject( typeof( TEnum ), raw );
            }

            // string name (case-insensitive)
            var str = reader.GetString();

            return ( TEnum ) Enum.Parse( typeof( TEnum ), str, ignoreCase: true );
        }

        /// <inheritdoc/>
        public override void Write( Utf8JsonWriter writer, TEnum value, JsonSerializerOptions options )
        {
            writer.WriteNumberValue( Convert.ToInt64( value ) );
        }
    }
}
