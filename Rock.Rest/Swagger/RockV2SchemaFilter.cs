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
using System.Reflection;

using Newtonsoft.Json;

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
            var enumType = Nullable.GetUnderlyingType( type ) ?? type;

            if ( enumType.IsEnum )
            {
                ApplyEnumNames( schema, enumType );
                return;
            }

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
            }
        }

        /// <summary>
        /// Adds the enum member names to an integer enum schema so that client
        /// code generators can create named members instead of _0, _1, etc.
        /// </summary>
        /// <param name="schema">The inline schema that describes the enum.</param>
        /// <param name="enumType">The enum type being described.</param>
        private static void ApplyEnumNames( Schema schema, Type enumType )
        {
            // Enums with a string converter are already described by name.
            if ( schema.type != "integer" )
            {
                return;
            }

            /*
                11/25/26 - CLAUDE

                Some enums have more than one name for the same value, usually
                an obsolete name kept for compatibility (AttendanceRule has
                AlreadyBelongs and AlreadyEnrolledInGroup both = 2). Swashbuckle
                lists such a value twice, which makes generated clients declare
                a duplicate member and fail to compile. We list each value once
                and prefer the name that is not obsolete.

                The x-enumNames extension is read by NSwag and x-enum-varnames
                by OpenAPI Generator. Both must line up with the enum list.

                Reason: Give REST v2 client generators usable enum member names.
            */
            var members = enumType.GetFields( BindingFlags.Public | BindingFlags.Static )
                .Select( f => new
                {
                    f.Name,
                    Value = Convert.ChangeType( f.GetRawConstantValue(), Enum.GetUnderlyingType( enumType ) ),
                    IsObsolete = f.GetCustomAttribute<ObsoleteAttribute>() != null
                } )
                .GroupBy( m => m.Value )
                .Select( g => g.OrderBy( m => m.IsObsolete ).First() )
                .ToList();

            var names = members.Select( m => m.Name ).ToList();

            schema.@enum = members.Select( m => m.Value ).ToList();

            if ( schema.vendorExtensions == null )
            {
                schema.vendorExtensions = new Dictionary<string, object>();
            }

            schema.vendorExtensions["x-enumNames"] = names;
            schema.vendorExtensions["x-enum-varnames"] = names;
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
    }
}
