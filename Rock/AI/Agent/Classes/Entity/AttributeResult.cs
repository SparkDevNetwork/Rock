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

namespace Rock.AI.Agent.Classes.Entity
{
    /// <summary>
    /// Represents a single attribute to the agent.
    /// </summary>
    internal class AttributeResult
    {
        /// <summary>
        /// The name of the attribute.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The key that identifies this specific attribute in requests and responses.
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// Describes the format of the values for this attribute.
        /// </summary>
        public string ValueFormat { get; set; }

        /// <summary>
        /// Indicates if this attribute is required.
        /// </summary>
        public bool IsRequired { get; set; }

        /// <summary>
        /// Indicates that this attribute's values are read-only and can't be changed.
        /// </summary>
        public bool IsReadOnly { get; set; }
    }
}
