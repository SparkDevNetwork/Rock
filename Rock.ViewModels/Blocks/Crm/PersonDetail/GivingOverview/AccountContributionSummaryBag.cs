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
    /// The contribution total for a single account within one year of the
    /// Giving Overview block's yearly summary.
    /// </summary>
    public class AccountContributionSummaryBag
    {
        /// <summary>
        /// Gets or sets the name of the financial account.
        /// </summary>
        public string AccountName { get; set; }

        /// <summary>
        /// Gets or sets the amount contributed to the account formatted as currency.
        /// </summary>
        public string FormattedAmount { get; set; }
    }
}
