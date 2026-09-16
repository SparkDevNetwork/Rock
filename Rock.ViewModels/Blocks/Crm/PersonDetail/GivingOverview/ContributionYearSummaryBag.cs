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

namespace Rock.ViewModels.Blocks.Crm.PersonDetail.GivingOverview
{
    /// <summary>
    /// The contribution totals for a single year, broken down by account, used
    /// by the Giving Overview block's yearly summary section.
    /// </summary>
    public class ContributionYearSummaryBag
    {
        /// <summary>
        /// Gets or sets the calendar year being summarized.
        /// </summary>
        public int Year { get; set; }

        /// <summary>
        /// Gets or sets the per-account contribution totals for the year,
        /// ordered by account order then name.
        /// </summary>
        public List<AccountContributionSummaryBag> Accounts { get; set; }

        /// <summary>
        /// Gets or sets the total amount contributed in the year formatted as currency.
        /// </summary>
        public string FormattedTotalAmount { get; set; }
    }
}
