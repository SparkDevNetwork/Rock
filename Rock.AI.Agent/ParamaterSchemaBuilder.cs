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

using System.Text.Json;
using System.Text.Json.Nodes;

using Microsoft.SemanticKernel;

using Rock.Enums.Core.AI.Agent;

namespace Rock.AI.Agent
{
    /// <summary>
    /// Provides functionality to build metadata for kernel parameters,
    /// including JSON Schema representations and other configuration details.
    /// </summary>
    internal class ParamaterSchemaBuilder
    {
        #region Fields

        /// <summary>
        /// The text that will be appended to the usage hint for a collection.
        /// This helps the LLM understand that multiple value sshould be passed
        /// in a single call instead of making multiple calls.
        /// </summary>
        internal const string CollectionUsageHint = " CRITICAL: If multiple values are to be used, then they must all be passed in a single function call. Never call this function twice because of multiple values.";

        #endregion

        #region Methods

        /// <summary>
        /// Gets the JSON Schema representation of the data type and allowed
        /// values. This is used to create the KernelParameterMetadata.
        /// </summary>
        /// <param name="parameter">The parameter to be built up into a Semantic Kernel object.</param>
        /// <returns>An instance of <see cref="KernelJsonSchema"/>.</returns>
        private KernelJsonSchema GetSchemaJson( ParameterSchema parameter )
        {
            var json = new JsonObject();
            JsonObject typeDefinition;
            var type = GetDataTypeName( parameter );
            var format = GetDataFormatName( parameter );
            JsonArray enumValues = null;

            if ( parameter.AllowedValues != null && parameter.AllowedValues.Count > 0 )
            {
                enumValues = new JsonArray();

                foreach ( var allowedValue in parameter.AllowedValues )
                {
                    enumValues.Add( allowedValue );
                }
            }

            if ( parameter.IsCollection )
            {
                typeDefinition = new JsonObject();

                json["type"] = "array";
                json["items"] = typeDefinition;

                if ( parameter.UsageHint.IsNotNullOrWhiteSpace() )
                {
                    json["description"] = parameter.UsageHint + CollectionUsageHint;
                }
            }
            else
            {
                typeDefinition = json;

                if ( parameter.UsageHint.IsNotNullOrWhiteSpace() )
                {
                    json["description"] = parameter.UsageHint;
                }
            }

            typeDefinition["type"] = type;

            if ( enumValues != null )
            {
                typeDefinition["enum"] = enumValues;
            }

            if ( format != null )
            {
                typeDefinition["format"] = format;
            }

            return KernelJsonSchema.Parse( JsonSerializer.SerializeToElement( json ).GetRawText() );
        }

        /// <summary>
        /// Gets the JSON Schema data type name that corresponds to the specified
        /// <see cref="DataType"/>.
        /// </summary>
        /// <param name="parameter">The parameter to be built up into a Semantic Kernel object.</param>
        /// <returns>The JSON schema type name to use for the data type.</returns>
        private string GetDataTypeName( ParameterSchema parameter )
        {
            switch ( parameter.DataType )
            {
                case ParameterSchemaDataType.Number:
                    return "number";

                case ParameterSchemaDataType.Boolean:
                    return "boolean";

                default:
                    return "string";
            }
        }

        /// <summary>
        /// Gets the JSON Schema data format name that corresponds to the specified
        /// <see cref="DataType"/>.
        /// </summary>
        /// <param name="parameter">The parameter to be built up into a Semantic Kernel object.</param>
        /// <returns>The JSON schema format name to use for the data type.</returns>
        private string GetDataFormatName( ParameterSchema parameter )
        {
            switch ( parameter.DataType )
            {
                case ParameterSchemaDataType.Date:
                    return "date";

                case ParameterSchemaDataType.DateTime:
                    return "date-time";

                default:
                    return null;
            }
        }

        /// <summary>
        /// Builds the JSON Schema data type name that corresponds to the specified
        /// parameter configuration values.
        /// </summary>
        /// <param name="parameter">The parameter to be built up into a Semantic Kernel object.</param>
        /// <returns>An instance of <see cref="KernelParameterMetadata"/> that represents this parameter schema.</returns>
        public KernelParameterMetadata BuildKernelParameterMetadata( ParameterSchema parameter )
        {
            return new KernelParameterMetadata( parameter.Name )
            {
                Description = parameter.UsageHint,
                IsRequired = parameter.IsRequired,
                Schema = GetSchemaJson( parameter ),
                DefaultValue = parameter.DefaultValue,
            };
        }

        #endregion
    }
}
