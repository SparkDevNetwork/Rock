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

namespace Rock.ViewModels.Blocks.Finance.TransactionFeeReport
{
    /// <summary>
    /// Restored filter state and display configuration for the Transaction Fee Report block.
    /// </summary>
    public class TransactionFeeReportOptionsBag
    {
        /// <summary>
        /// Gets or sets the accounts currently selected in the filter, restored from person preferences.
        /// </summary>
        public List<ListItemBag> SelectedAccounts { get; set; } = new List<ListItemBag>();

        /// <summary>
        /// Gets or sets the delimited sliding-date-range value currently selected in the filter.
        /// </summary>
        public string DateRangeDelimitedValue { get; set; }

        /// <summary>
        /// Gets or sets the organization currency formatting details used to render the KPI amounts.
        /// </summary>
        public CurrencyInfoBag CurrencyInfo { get; set; }
    }
}
