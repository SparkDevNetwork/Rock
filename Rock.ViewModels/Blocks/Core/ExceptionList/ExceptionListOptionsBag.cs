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

namespace Rock.ViewModels.Blocks.Core.ExceptionList
{
    /// <summary>
    /// The additional configuration options for the Communication List block.
    /// </summary>
    public class ExceptionListOptionsBag
    {
        /// <summary>
        /// Gets or sets the list of site items the individual can filter by.
        /// </summary>
        public List<ListItemBag> SiteItems { get; set; }

        /// <summary>
        /// Gets or sets the number of days represented by the subset count grid column.
        /// </summary>
        public int SubsetCountDays { get; set; }

        /// <summary>
        /// Gets or sets whether to show the "Clear All Exceptions" button.
        /// </summary>
        public bool ShowClearAllExceptionsButton { get; set; }

        /// <summary>
        /// Gets or sets whether to show the chart legend.
        /// </summary>
        public bool ShowChartLegend { get; set; }

        /// <summary>
        /// Gets or sets the position of the chart legend.
        /// </summary>
        public string ChartLegendPosition { get; set; }

    }
}
