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

namespace Rock.ViewModels.Blocks.Finance.FinancialBatchList
{
    /// <summary>
    /// 
    /// </summary>
    public class FinancialBatchListOptionsBag
    {
        /// <summary>
        /// Determines if the organization is configured with multiple active
        /// campuses or not.
        /// </summary>
        public bool HasMultipleCampuses { get; set; }

        /// <summary>
        /// Determines if the accounts column should be displayed.
        /// </summary>
        public bool ShowAccountsColumn { get; set; }

        /// <summary>
        /// Determines if the accounting system code column should be displayed.
        /// </summary>
        public bool ShowAccountingSystemCodeColumn { get; set; }

        /// <summary>
        /// The transaction types that can be used to filter the results.
        /// </summary>
        public List<ListItemBag> TransactionTypes { get; set; }

        /// <summary>
        /// The sources that can be used to filter the results.
        /// </summary>
        public List<ListItemBag> Sources { get; set; }

        /// <summary>
        /// Gets or sets the CurrencyInfo
        /// </summary>
        public CurrencyInfoBag CurrencyInfo { get; set; }
    }
}
