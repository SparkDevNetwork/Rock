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

namespace Rock.UniversalSearch
{
    /// <summary>
    /// Search Result Object
    /// </summary>
    public class ModelFieldFilterConfig
    {
        /// <summary>
        /// Gets or sets the filter values.
        /// </summary>
        /// <value>
        /// The filter values.
        /// </value>
        public List<string> FilterValues { get; set; } = new List<string>();

        /// <summary>
        /// Gets or sets the filter label.
        /// </summary>
        /// <value>
        /// The filter label.
        /// </value>
        public string FilterLabel { get; set; }

        /// <summary>
        /// Gets or sets the filter field.
        /// </summary>
        /// <value>
        /// The filter field.
        /// </value>
        public string FilterField { get; set; }
    }
}
