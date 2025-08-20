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

namespace Rock.AI.Agent.Classes.Common
{
    /// <summary>
    /// POCO result for attributes.
    /// </summary>
    public class AttributeResult
    {
        #region Ignored Properties
        /// <summary>
        /// The attribute id. This will not be show in the JSON output.
        /// </summary>
        [JsonIgnore]
        public int Id { get; set; }
        #endregion

        /// <summary>
        /// The attribute identifier key.
        /// </summary>
        public string AttributeIdKey
        {
            get
            {
                return this.Id.AsIdKey();
            }
        }

        /// <summary>
        /// The attribute name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The attribute value.
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// The attribute key.
        /// </summary>
        public string Key { get; set; }
    }
}
