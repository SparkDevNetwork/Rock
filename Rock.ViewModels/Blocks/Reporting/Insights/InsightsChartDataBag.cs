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
using Rock.ViewModels.Reporting;

namespace Rock.ViewModels.Blocks.Reporting.Insights
{
    /// <summary>
    /// Data Bag for the Insights block charts.
    /// </summary>
    public class InsightsChartDataBag
    {
        /// <summary>
        /// Gets or sets the category of the insight.
        /// </summary>
        public string InsightCategory { get; set; }

        /// <summary>
        /// Gets or sets the subcategory of the insight.
        /// </summary>
        public string InsightSubcategory { get; set; }

        /// <summary>
        /// Gets or sets the chart bag containing the chart's data and configuration.
        /// </summary>
        public ChartBag ChartBag { get; set; }
    }
}
