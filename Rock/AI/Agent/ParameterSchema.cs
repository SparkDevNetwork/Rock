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

using Rock.Enums.AI.Agent;

namespace Rock.AI.Agent
{
    /// <summary>
    /// Defines a single parameter to be used with an <see cref="AgentTool"/>.
    /// </summary>
    internal class ParameterSchema
    {
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
        /// The default value to use for the parameter if no value is provided.
        /// </summary>
        public string DefaultValue { get; set; }

        /// <summary>
        /// A concise, but descriptive, hint to the language model that provides
        /// context about how to fill in this parameter.
        /// </summary>
        public string Instructions { get; set; }

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
    }
}
