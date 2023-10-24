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
    }
}
