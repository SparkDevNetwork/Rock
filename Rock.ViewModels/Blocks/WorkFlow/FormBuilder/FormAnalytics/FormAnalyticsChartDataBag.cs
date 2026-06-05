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

namespace Rock.ViewModels.Blocks.WorkFlow.FormBuilder.FormAnalytics
{
    /// <summary>
    /// KPIs and chart payload for the Form Analytics block. The server emits sparse
    /// series (only buckets with activity) plus the window/unit hints needed by the
    /// client to densify into a continuous x-axis via the chart helpers.
    /// </summary>
    public class FormAnalyticsChartDataBag
    {
        /// <summary>
        /// Gets or sets the total Form Viewed interactions in the requested period.
        /// </summary>
        public int TotalViews { get; set; }

        /// <summary>
        /// Gets or sets the total Form Completed interactions in the requested period.
        /// </summary>
        public int Completions { get; set; }

        /// <summary>
        /// Gets or sets the conversion rate (Completions / TotalViews). Zero when
        /// TotalViews is zero so the value is renderable as a percentage.
        /// </summary>
        public double ConversionRate { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the period contained any view or
        /// completion interactions. False switches the block to its empty state.
        /// </summary>
        public bool HasData { get; set; }

        /// <summary>
        /// Gets or sets the inclusive ISO 8601 start of the chart window. Used by
        /// the client to densify each series into a continuous axis.
        /// </summary>
        public string WindowStart { get; set; }

        /// <summary>
        /// Gets or sets the inclusive ISO 8601 end of the chart window.
        /// </summary>
        public string WindowEnd { get; set; }

        /// <summary>
        /// Gets or sets the bucket size used by the client when filling the window
        /// ("day" or "month"). Matches the server-side group-by granularity.
        /// </summary>
        public string WindowUnit { get; set; }

        /// <summary>
        /// Gets or sets the named series (Views, Completions) to plot. Each series
        /// is sparse: only buckets with activity are included.
        /// </summary>
        public List<FormAnalyticsSeriesBag> Series { get; set; } = new List<FormAnalyticsSeriesBag>();
    }
}
