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

namespace Rock.ViewModels.Blocks.Reporting.ServiceMetricsEntry
{
    /// <summary>
    /// A bag that contains the metric item information.
    /// </summary>
    public class ServiceMetricsEntryMetricItemBag
    {
        /// <summary>
        /// Gets or sets the metric item identifier.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the metric item name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the metric item value.
        /// </summary>
        public decimal? Value { get; set; }

        /// <summary>
        /// Gets or sets the category identifiers to which this metric belongs.
        /// </summary>
        public List<int> CategoryIds { get; set; }
    }
}
