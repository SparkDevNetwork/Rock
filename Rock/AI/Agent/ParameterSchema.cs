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

using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Nodes;

using Microsoft.SemanticKernel;

using Rock.Enums.Core.AI.Agent;

namespace Rock.AI.Agent
{
    /// <summary>
    /// Defines a single parameter to be used with an <see cref="AgentFunction"/>.
    /// </summary>
    internal class ParameterSchema
    {
        #region Fields

        /// <summary>
        /// The cached kernel parameter metadata. This is only craeted when
        /// first requested. Subsequent requests will return the cached instance.
        /// </summary>
        private KernelParameterMetadata _metadata;

        /// <summary>
        /// The text that will be appended to the usage hint for a collection.
        /// This helps the LLM understand that multiple value sshould be passed
        /// in a single call instead of making multiple calls.
        /// </summary>
        internal const string CollectionUsageHint = " CRITICAL: If multiple values are to be used, then they must all be passed in a single function call. Never call this function twice because of multiple values.";

        #endregion

        #region Properties

        /// <summary>
        /// The name of the parameter that will be passed to the Lava prompt.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The type of data allowed in the parameter.
        /// </summary>
        public ParameterSchemaDataType DataType { get; set; }

        /// <summary>
        /// A concise, but descriptive, hint to the language model that provides
        /// context about how to fill in this parameter.
        /// </summary>
        public string UsageHint { get; set; }

        /// <summary>
        /// Indicates that the parameter is a collection of values. If true, the
        /// DataType represents the type of each item in the collection.
        /// </summary>
        public bool IsCollection { get; set; }

        /// <summary>
        /// Indicates that this parameter is required.
        /// </summary>
        public bool IsRequired { get; set; }

        /// <summary>
        /// A list of allowed values for the parameter. Only valid if DataType
        /// is set to String.
        /// </summary>
        public List<string> AllowedValues { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// Gets the JSON Schema representation of the data type and allowed
        /// values. This is used to create the KernelParameterMetadata.
        /// </summary>
        /// <returns>An instance of <see cref="KernelJsonSchema"/>.</returns>
        private KernelJsonSchema GetSchemaJson()
        {
            var json = new JsonObject();
            JsonObject typeDefinition;
            var type = GetDataTypeName();
            var format = GetDataFormatName();
            JsonArray enumValues = null;

            if ( AllowedValues != null && AllowedValues.Count > 0 )
            {
                enumValues = new JsonArray();

                foreach ( var allowedValue in AllowedValues )
                {
                    enumValues.Add( allowedValue );
                }
            }

            if ( IsCollection )
            {
                typeDefinition = new JsonObject();

                json["type"] = "array";
                json["items"] = typeDefinition;

                if ( UsageHint.IsNotNullOrWhiteSpace() )
                {
                    json["description"] = UsageHint + CollectionUsageHint;
                }
            }
            else
            {
                typeDefinition = json;

                if ( UsageHint.IsNotNullOrWhiteSpace() )
                {
                    json["description"] = UsageHint;
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
        /// <returns>The JSON schema type name to use for the data type.</returns>
        private string GetDataTypeName()
        {
            switch ( DataType )
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
        /// <returns>The JSON schema format name to use for the data type.</returns>
        private string GetDataFormatName()
        {
            switch ( DataType )
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
        /// Gets the JSON Schema data type name that corresponds to the specified
        /// parameter configuration values.
        /// </summary>
        /// <returns>An instance of <see cref="KernelParameterMetadata"/> that represents this parameter schema.</returns>
        public KernelParameterMetadata GetKernelParameterMetadata()
        {
            if ( _metadata == null )
            {
                _metadata = new KernelParameterMetadata( Name )
                {
                    Description = UsageHint,
                    IsRequired = IsRequired,
                    Schema = GetSchemaJson(),
                };
            }

            return _metadata;
        }

        #endregion
    }
}
