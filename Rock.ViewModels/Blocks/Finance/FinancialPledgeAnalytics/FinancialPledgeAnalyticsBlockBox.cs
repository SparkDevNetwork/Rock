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

using Rock.ViewModels.Core.Grid;
using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Finance.FinancialPledgeAnalytics
{
    /// <summary>
    /// Contains the configuration, filters, and supporting data for the Financial Pledge Analytics block.
    /// </summary>
    public class FinancialPledgeAnalyticsBlockBox : BlockBox
    {
        /// <summary>
        /// The primary options and filter values for the block.
        /// </summary>
        public FinancialPledgeAnalyticsFiltersBag Filters { get; set; } = new FinancialPledgeAnalyticsFiltersBag();

        /// <summary>
        /// The grid configuration for the pledges grid.
        /// </summary>
        public GridDefinitionBag PledgeGridBox { get; set; }

        /// <summary>
        /// Gets or sets the currency information.
        /// </summary>
        public CurrencyInfoBag CurrencyInfo { get; set; }
    }
}
