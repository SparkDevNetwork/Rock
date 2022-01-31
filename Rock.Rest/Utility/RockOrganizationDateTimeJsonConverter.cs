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

namespace Rock.Rest.Utility
{
    /// <summary>
    /// JsonConverter that handles ensuring a DateTime is in Rock Organization
    /// time zone.
    /// </summary>
    /// <seealso cref="Newtonsoft.Json.JsonConverter" />
    public class RockOrganizationDateTimeJsonConverter : JsonConverter
    {
        /// <inheritdoc/>
        public override bool CanConvert( Type objectType )
        {
            if ( objectType == typeof( DateTime ) || objectType == typeof( DateTime? ) )
            {
                return true;
            }

            return false;
        }

        /// <inheritdoc/>
        public override object ReadJson( JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer )
        {
            if ( reader.TokenType == JsonToken.Date && reader.Value is DateTimeOffset dateTimeOffset )
            {
                return RockDateTime.ConvertLocalDateTimeToRockDateTime( dateTimeOffset.LocalDateTime );
            }
            else if ( reader.TokenType == JsonToken.Date && reader.Value is DateTime dateTime )
            {
                return RockDateTime.ConvertLocalDateTimeToRockDateTime( dateTime );
            }
            else
            {
                return null;
            }
        }

        /// <inheritdoc/>
        public override void WriteJson( JsonWriter writer, object value, JsonSerializer serializer )
        {
            if ( value is DateTime dateTime )
            {
                writer.WriteValue( dateTime.ToRockDateTimeOffset() );
            }
            else if ( value == null )
            {
                writer.WriteNull();
            }
            else
            {
                throw new ArgumentOutOfRangeException( nameof( value ), "Value was not a DateTime." );
            }
        }
    }
}
