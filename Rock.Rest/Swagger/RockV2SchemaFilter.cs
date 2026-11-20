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
using Newtonsoft.Json.Serialization;

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
            AddEnumNames( schema, type );

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
        /// Enum properties are described inline as a list of integer values
        /// with no names. This adds the member names as "x-enumNames" so that
        /// client generators can produce named enum members. Values stay
        /// integers, which is what the API sends and accepts.
        /// </summary>
        /// <param name="schema">The schema object.</param>
        /// <param name="type">The object type this schema represents.</param>
        private void AddEnumNames( Schema schema, Type type )
        {
            if ( schema?.properties == null )
            {
                return;
            }

            var contractResolver = GetSerializerSettings().ContractResolver ?? new DefaultContractResolver();

            if ( !( contractResolver.ResolveContract( type ) is JsonObjectContract contract ) )
            {
                return;
            }

            foreach ( var property in contract.Properties )
            {
                if ( property.PropertyName == null || !schema.properties.TryGetValue( property.PropertyName, out var propertySchema ) )
                {
                    continue;
                }

                var enumType = Nullable.GetUnderlyingType( property.PropertyType ) ?? property.PropertyType;

                if ( enumType.IsEnum )
                {
                    AddEnumNamesToEnumSchema( propertySchema, enumType );
                }
                else if ( propertySchema.items != null )
                {
                    var elementType = GetEnumerableElementType( property.PropertyType );

                    if ( elementType != null )
                    {
                        AddEnumNamesToEnumSchema( propertySchema.items, Nullable.GetUnderlyingType( elementType ) ?? elementType );
                    }
                }
            }
        }

        /// <summary>
        /// Adds "x-enumNames" to a single inline enum schema. A value that has
        /// more than one name (an obsolete alias) is listed once, under its
        /// current name, so generated enums don't get duplicate members.
        /// </summary>
        /// <param name="enumSchema">The inline schema describing the enum values.</param>
        /// <param name="enumType">The enum type.</param>
        private static void AddEnumNamesToEnumSchema( Schema enumSchema, Type enumType )
        {
            if ( !enumType.IsEnum || enumSchema.@enum == null || enumSchema.@enum.Count == 0 || enumSchema.type != "integer" )
            {
                return;
            }

            var fields = enumType.GetFields( BindingFlags.Public | BindingFlags.Static );
            var values = new List<object>();
            var names = new List<string>();
            var seenValues = new HashSet<long>();

            foreach ( var value in enumSchema.@enum )
            {
                var number = Convert.ToInt64( value );

                if ( !seenValues.Add( number ) )
                {
                    continue;
                }

                var matchingFields = fields.Where( f => Convert.ToInt64( f.GetRawConstantValue() ) == number ).ToList();
                var field = matchingFields.FirstOrDefault( f => f.GetCustomAttribute<ObsoleteAttribute>() == null ) ?? matchingFields.FirstOrDefault();

                values.Add( value );
                names.Add( field?.Name ?? number.ToString() );
            }

            enumSchema.@enum = values;
            enumSchema.vendorExtensions = enumSchema.vendorExtensions ?? new Dictionary<string, object>();
            enumSchema.vendorExtensions["x-enumNames"] = names;
        }

        /// <summary>
        /// Gets the element type of an array or generic collection type.
        /// </summary>
        /// <param name="type">The collection type.</param>
        /// <returns>The element type, or <c>null</c> if it isn't a collection.</returns>
        private static Type GetEnumerableElementType( Type type )
        {
            if ( type.IsArray )
            {
                return type.GetElementType();
            }

            return type.GetInterfaces()
                .Concat( new[] { type } )
                .Where( t => t.IsGenericType && t.GetGenericTypeDefinition() == typeof( IEnumerable<> ) )
                .Select( t => t.GetGenericArguments()[0] )
                .FirstOrDefault();
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
