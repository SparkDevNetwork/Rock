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

namespace Rock.ViewModels.Blocks.Crm.PersonDetail.GivingOverview
{
    /// <summary>
    /// The total giving amount for a single month, used by the Giving Overview
    /// block to render the giving by month chart.
    /// </summary>
    public class MonthlyGivingBag
    {
        /// <summary>
        /// Gets or sets the display label for the month (e.g. "Mar 2025").
        /// </summary>
        public string Label { get; set; }

        /// <summary>
        /// Gets or sets the total amount given in the month. Used to compute
        /// the relative bar height in the chart.
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// Gets or sets the amount formatted as currency for the bar tooltip.
        /// </summary>
        public string FormattedAmount { get; set; }
    }
}
