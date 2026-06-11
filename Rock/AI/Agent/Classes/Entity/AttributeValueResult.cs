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

using System.Text.Json.Serialization;

namespace Rock.AI.Agent.Classes.Entity
{
    /// <summary>
    /// POCO result for attribute values.
    /// </summary>
    internal class AttributeValueResult
    {
        /// <summary>
        /// The identifier of the attribute. This is not sent to the agent but
        /// must be included for internal processing such as security checks.
        /// </summary>
        [JsonIgnore]
        public int AttributeId { get; set; }

        /// <summary>
        /// The attribute name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The attribute value.
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// The formatted text value of the attribute. Should be null if it is the
        /// same as the raw value.
        /// </summary>
        public string TextValue { get; set; }

        /// <summary>
        /// The attribute key.
        /// </summary>
        public string Key { get; set; }
    }
}
