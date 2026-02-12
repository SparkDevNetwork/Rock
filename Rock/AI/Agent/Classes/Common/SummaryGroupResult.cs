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
using System.Text.Json.Serialization;

namespace Rock.AI.Agent.Classes.Common
{
    /// <summary>
    /// Represents a single grouping of summary data in a <see cref="SummaryResult"/>
    /// object. Each grouping can also optionally contain child groupings
    /// that further break down the results.
    /// </summary>
    internal class SummaryGroupResult
    {
        /// <summary>
        /// The entity id. This will not be show in the JSON output.
        /// </summary>
        [JsonIgnore]
        internal int? Id { get; set; }

        /// <summary>
        /// Internal identifier of the item.
        /// </summary>
        public string IdKey => Id?.AsIdKey();

        /// <summary>
        /// The name of the item at this level.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The total number of items at this level of the summary, including
        /// all child groups. This usually reflects the total number of items
        /// represented by <see cref="Source"/>.
        /// </summary>
        public int Total { get; set; }

        /// <summary>
        /// Child summary groups that further break down the summary results.
        /// </summary>
        public List<SummaryGroupResult> Groups { get; set; }

        /// <summary>
        /// The object that contains the source data for this group. This
        /// will be typically be used by further grouping operations to
        /// slice this data into smaller groups.
        /// </summary>
        [JsonIgnore]
        public object Source { get; set; }
    }

}
