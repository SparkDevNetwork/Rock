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

using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Reporting.MetricValueList
{
    /// <summary>
    /// Options sent to the metric value list block on initialization.
    /// </summary>
    public class MetricValueListOptionsBag
    {
        /// <summary>
        /// Gets or sets the metric IdKey, used by the client to scope the partition
        /// filter person preference to the current metric.
        /// </summary>
        public string MetricIdKey { get; set; }

        /// <summary>
        /// Gets or sets the items shown in the Goal/Measure filter dropdown.
        /// </summary>
        public List<ListItemBag> MetricValueTypeItems { get; set; }

        /// <summary>
        /// Gets or sets one filter descriptor for each entity-typed partition the
        /// metric defines. Empty when the metric has no partitions.
        /// </summary>
        public List<MetricPartitionFilterBag> PartitionFilters { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the partitions column should be
        /// shown in the grid. Mirrors whether the metric defines any entity-typed
        /// partitions.
        /// </summary>
        public bool IsPartitionsColumnVisible { get; set; }
    }
}
