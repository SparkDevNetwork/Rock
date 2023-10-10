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
using System.Linq;

using Newtonsoft.Json;

using Rock.Rest.Utility;

using Swashbuckle.Swagger;

namespace Rock.Rest.Swagger
{
    /// <summary>
    /// Swagger filter for schema objects in the v2 namespace. This ensures
    /// the generated schema is correct for each object.
    /// </summary>
    internal class RockV2SchemaFilter : ISchemaFilter
    {
        private static JsonSerializerSettings _serializerSettings;

        /// <summary>
        /// Applies any custom changes to the schema object.
        /// </summary>
        /// <param name="schema">The schema object.</param>
        /// <param name="schemaRegistry">The registry for looking up other schemas.</param>
        /// <param name="type">The object type this schema represents.</param>
        public void Apply( Schema schema, SchemaRegistry schemaRegistry, Type type )
        {
            if ( typeof( Rock.Data.IEntity ).IsAssignableFrom( type ) )
            {
                try
                {
                    // Little trick to properly convert and encode everything
                    // with the correct serializer settings.
                    var exampleObject = Activator.CreateInstance( type );
                    var exampleJson = JsonConvert.SerializeObject( exampleObject, GetSerializerSettings() );

                    schema.example = JsonConvert.DeserializeObject( exampleJson );
                }
                catch
                {
                    // Just in case we get an exception, have the example just be object. This will
                    // cause the example JSON to be empty.
                    schema.example = new object();
                }

                schema.properties.Remove( "Attributes" );
                schema.properties.Remove( "AttributeValues" );
            }

            if ( schema.properties != null )
            {
                schema.properties = schema.properties
                    .Where( p =>
                    {
                        try
                        {
                            var propertyType = type.GetProperty( p.Key )?.PropertyType;

                            if ( propertyType == null )
                            {
                                return false;
                            }

                            return !ExcludeNavigationPropertiesContractResolver.IsNavigationPropertyType( propertyType );
                        }
                        catch
                        {
                            return false;
                        }
                    } )
                    .ToDictionary( a => ToCamelCase( a.Key ), a => a.Value );
            }

            if ( schema.required != null )
            {
                schema.required = schema.required
                    .Select( a => ToCamelCase( a ) )
                    .Where( a => schema.properties == null || schema.properties.ContainsKey( a ) )
                    .ToList();
            }
        }

        /// <summary>
        /// Gets the serializer settings to use when generting sample data.
        /// </summary>
        /// <returns>JSON serializer settings.</returns>
        private JsonSerializerSettings GetSerializerSettings()
        {
            if ( _serializerSettings == null )
            {
                _serializerSettings = Utility.ApiPickerJsonMediaTypeFormatter.CreateV2Formatter().SerializerSettings;
                _serializerSettings.NullValueHandling = NullValueHandling.Ignore;
            }

            return _serializerSettings;
        }

        /// <summary>
        /// Convert a string into camel case.
        /// </summary>
        /// <remarks>Originally from https://github.com/JamesNK/Newtonsoft.Json/blob/01e1759cac40d8154e47ed0e11c12a9d42d2d0ff/Src/Newtonsoft.Json/Utilities/StringUtils.cs#L155</remarks>
        /// <param name="value">The string to be converted.</param>
        /// <returns>A string in camel case.</returns>
        private static string ToCamelCase( string value )
        {
            if ( string.IsNullOrEmpty( value ) || !char.IsUpper( value[0] ) )
            {
                return value;
            }

            var chars = value.ToCharArray();

            for ( int i = 0; i < chars.Length; i++ )
            {
                if ( i == 1 && !char.IsUpper( chars[i] ) )
                {
                    break;
                }

                bool hasNext = ( i + 1 < chars.Length );
                if ( i > 0 && hasNext && !char.IsUpper( chars[i + 1] ) )
                {
                    // if the next character is a space, which is not considered uppercase 
                    // (otherwise we wouldn't be here...)
                    // we want to ensure that the following:
                    // 'FOO bar' is rewritten as 'foo bar', and not as 'foO bar'
                    // The code was written in such a way that the first word in uppercase
                    // ends when if finds an uppercase letter followed by a lowercase letter.
                    // now a ' ' (space, (char)32) is considered not upper
                    // but in that case we still want our current character to become lowercase
                    if ( char.IsSeparator( chars[i + 1] ) )
                    {
                        chars[i] = char.ToLower( chars[i] );
                    }

                    break;
                }

                chars[i] = char.ToLower( chars[i] );
            }

            return new string( chars );
        }
    }
}
