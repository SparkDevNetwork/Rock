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

namespace Rock.ViewModels.Blocks.Finance.TransactionList
{
    /// <summary>
    /// The additional configuration options for the Transaction List block.
    /// </summary>
    public class TransactionListOptionsBag
    {

        /// <summary>
        /// Contains the entity attribute fields for FinancialTransaction
        /// </summary>
        public List<AttributeFieldDefinitionBag> TransactionAttributeOptions { get; set; }

        /// <summary>
        /// Contains the entity attribute fields for FinancialTransactionDetail.
        /// </summary>
        public List<AttributeFieldDefinitionBag> TransactionDetailAttributeOptions { get; set; }

        /// <summary>
        /// Gets or sets the title to display above the grid. When empty the title is hidden.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the resolved current view mode ("Transactions" or "Accounts").
        /// </summary>
        public string ViewMode { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the "Show Images" option is offered
        /// in the grid options menu (driven by the "Show Images Toggle" block setting).
        /// </summary>
        public bool IsImagesToggleVisible { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the image column is currently shown
        /// (the user's resolved "show images" preference).
        /// </summary>
        public bool ShowImages { get; set; }

        /// <summary>
        /// Gets or sets the height, in pixels, of the transaction image when the image column is shown.
        /// </summary>
        public int ImageHeight { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the account summary panel is shown below the grid.
        /// </summary>
        public bool ShowAccountSummary { get; set; }

        /// <summary>
        /// used to conditionally render account filter on grid settings modal
        /// </summary>
        public bool AccountConfigured { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the Foreign Key column is shown.
        /// </summary>
        public bool ShowForeignKeyColumn { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the foreign currency column is shown.
        /// </summary>
        public bool IsForeignCurrencyEnabled { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the days-since-last-transaction column is shown.
        /// </summary>
        public bool ShowDaysSinceLastTransaction { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the block is in a Person context.
        /// </summary>
        public bool IsPersonContext { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the block is in a Financial Batch context.
        /// </summary>
        public bool IsBatchContext { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the block is in a Financial Scheduled Transaction context.
        /// </summary>
        public bool IsScheduledTransactionContext { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the block is in a Registration context.
        /// </summary>
        public bool IsRegistrationContext { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the filter drawer is available. Filters are
        /// hidden when the block is showing transactions for a specific batch, scheduled
        /// transaction, or registration.
        /// </summary>
        public bool AreFiltersVisible { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the "Reassign Transactions" action is available
        /// (Person context with edit permission).
        /// </summary>
        public bool IsReassignVisible { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the "Move to Batch" action is available
        /// (Batch context with edit permission on an open, non-automated batch).
        /// </summary>
        public bool IsMoveToBatchVisible { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the "closed batch" warning should be shown.
        /// </summary>
        public bool ShowClosedBatchWarning { get; set; }

        /// <summary>
        /// Gets or sets the currency formatting information used to render amounts on the client.
        /// </summary>
        public CurrencyInfoBag CurrencyInfo { get; set; }
    }
}
