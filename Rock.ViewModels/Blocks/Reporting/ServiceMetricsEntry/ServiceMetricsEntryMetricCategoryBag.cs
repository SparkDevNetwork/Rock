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
    /// A bag that contains the metric category information.
    /// </summary>
    public class ServiceMetricsEntryMetricCategoryBag
    {
        /// <summary>
        /// Gets or sets the category identifier.
        /// </summary>
        public int CategoryId { get; set; }

        /// <summary>
        /// Gets or sets the metric category name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the child metric categories.
        /// </summary>
        public List<ServiceMetricsEntryMetricCategoryBag> ChildMetricCategories { get; set; }
    }
}
