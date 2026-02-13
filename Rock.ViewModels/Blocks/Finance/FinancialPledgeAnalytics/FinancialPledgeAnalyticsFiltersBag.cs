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

namespace Rock.ViewModels.Blocks.Finance.FinancialPledgeAnalytics
{
    /// <summary>
    /// The filter options for the Communication Saturation Report block.
    /// </summary>
    public class FinancialPledgeAnalyticsFiltersBag
    {
        #region Filter Options

        /// <summary>
        /// Gets or sets the selected accounts.
        /// </summary>
        /// <value>
        /// The selected accounts.
        /// </value>
        public List<ListItemBag> PledgeAccounts { get; set; } = new List<ListItemBag>();

        /// <summary>
        /// String representing a range of time to filter pledge results by.
        /// </summary>
        public string PledgeDateRangeDelimitedString { get; set; } = "Current||Year||";

        /// <summary>
        /// String representing a range of time to filter giving results by.
        /// </summary>
        public string GivingDateRangeDelimitedString { get; set; } = "";

        /// <summary>
        /// Gets or sets the pledge amount range filter in a delimited string format.
        /// </summary>
        public string PledgeAmountRange { get; set; }

        /// <summary>
        /// Gets or sets the given amount range filter in a delimited string format.
        /// </summary>
        public string GivenAmountRange { get; set; }

        /// <summary>
        /// Gets or sets the percent complete range filter in a delimited string format.
        /// </summary>
        public string PercentCompleteRange { get; set; }

        /// <summary>
        /// Gets or sets the inclusion options for which pledges should be shown.
        /// </summary>
        public string IncludeOptions { get; set; }

        #endregion
    }
}
