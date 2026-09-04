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

using System;
using System.Collections.Generic;

using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Finance.TransactionDetail
{
    /// <summary>
    /// A single account allocation line item within a financial transaction,
    /// representing a portion of the transaction applied to a specific account.
    /// </summary>
    public class TransactionLineItemBag : EntityBagBase
    {
        /// <summary>
        /// Gets or sets the unique identifier for this line item, used to correlate
        /// client-side additions with server-side records.
        /// </summary>
        public Guid Guid { get; set; }

        /// <summary>
        /// Gets or sets the integer Id of the persisted FinancialTransactionDetail record,
        /// or <c>0</c> for new line items that have not yet been saved.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the financial account this allocation is credited to.
        /// </summary>
        public ListItemBag Account { get; set; }

        /// <summary>
        /// Gets or sets the net amount allocated to the account (excluding fee coverage).
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// Gets or sets the processing fee amount charged by the gateway for this line item.
        /// </summary>
        public decimal? FeeAmount { get; set; }

        /// <summary>
        /// Gets or sets the portion of the processing fee that the donor chose to cover.
        /// </summary>
        public decimal? FeeCoverageAmount { get; set; }

        /// <summary>
        /// Gets or sets the equivalent amount in the donor's foreign currency, when applicable.
        /// </summary>
        public decimal? ForeignCurrencyAmount { get; set; }

        /// <summary>
        /// Gets or sets optional notes or details about this account allocation.
        /// </summary>
        public string Summary { get; set; }

        /// <summary>
        /// Gets or sets the Id of the parent financial transaction.
        /// </summary>
        public int TransactionId { get; set; }

        /// <summary>
        /// Gets or sets the Id of the entity this line item is linked to (e.g. a Registration Id),
        /// or <c>null</c> when not linked to a specific entity.
        /// </summary>
        public int? EntityId { get; set; }

        /// <summary>
        /// Gets or sets the Id of the entity type for the linked entity,
        /// or <c>null</c> when not entity-linked.
        /// </summary>
        public int? EntityTypeId { get; set; }

        /// <summary>
        /// Gets or sets the entity type display information for the linked entity.
        /// </summary>
        public ListItemBag EntityType { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this line item can be edited by the current user.
        /// </summary>
        public bool CanEdit { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this line item can be deleted.
        /// Entity-linked rows (e.g. registration allocations) cannot be deleted.
        /// </summary>
        public bool CanDelete { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this row is a synthetic totals row
        /// appended by the client for display purposes and should not be saved.
        /// </summary>
        public bool IsTotalRow { get; set; }

        /// <summary>
        /// Gets or sets the condensed display values for each attribute on this line item,
        /// keyed by <c>attr_{attributeKey}</c>. Each value contains <c>Html</c> and <c>Text</c>
        /// representations in the format expected by the grid attribute column renderer.
        /// </summary>
        public Dictionary<string, object> AttributeDisplayValues { get; set; }
    }
}
