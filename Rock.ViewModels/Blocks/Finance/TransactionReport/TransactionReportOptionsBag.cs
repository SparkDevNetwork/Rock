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

using Rock.ViewModels.Core.Grid;
using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Finance.TransactionReport
{
    /// <summary>
    /// The configuration and display options for the Transaction Report block. These values are
    /// set once when the block initializes and do not change as the individual adjusts the filter.
    /// </summary>
    public class TransactionReportOptionsBag
    {
        /// <summary>
        /// Gets or sets the singular label used to describe a transaction (e.g. "Gift"). Used as the
        /// grid item term, which the grid pluralizes for its row-count message.
        /// </summary>
        public string TransactionLabel { get; set; }

        /// <summary>
        /// Gets or sets the label shown on the account filter picker.
        /// </summary>
        public string AccountLabel { get; set; }

        /// <summary>
        /// Gets or sets the accounts configured as the viewable whitelist for the block. These pre-select
        /// the account filter on first load. An empty list means the block is not restricted to specific
        /// accounts and all accounts the person contributed to are shown.
        /// </summary>
        public List<ListItemBag> Accounts { get; set; }

        /// <summary>
        /// Gets or sets the message shown in the grid when no transactions match the current filter.
        /// </summary>
        public string EmptyDataText { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the transaction code column is shown in the grid.
        /// </summary>
        public bool ShowTransactionCode { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the foreign key column is shown in the grid.
        /// </summary>
        public bool ShowForeignKey { get; set; }

        /// <summary>
        /// Gets or sets the organization currency formatting details used to render amounts on the client.
        /// </summary>
        public CurrencyInfoBag CurrencyInfo { get; set; }

        /// <summary>
        /// Gets or sets the column definition for the transactions grid.
        /// </summary>
        public GridDefinitionBag GridDefinition { get; set; }

        /// <summary>
        /// Gets or sets the default lower (start) date applied to the date range filter on first load,
        /// formatted as an ISO date string.
        /// </summary>
        public string DefaultLowerDate { get; set; }

        /// <summary>
        /// Gets or sets the default upper (end) date applied to the date range filter on first load,
        /// formatted as an ISO date string.
        /// </summary>
        public string DefaultUpperDate { get; set; }
    }
}
