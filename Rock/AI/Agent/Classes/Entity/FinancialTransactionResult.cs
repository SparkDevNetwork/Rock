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

using Rock.AI.Agent.Classes.Common;

namespace Rock.AI.Agent.Classes.Entity
{
    /// <summary>
    /// Lightweight per-transaction projection for list operations.
    /// </summary>
    internal class FinancialTransactionResult : EntityResultBase
    {
        /// <summary>
        /// Transaction date/time in the organization's local time zone (if set).
        /// </summary>
        public DateTime? TransactionDateTime { get; set; }

        /// <summary>
        /// Sum of detail amounts for the transaction (may include multiple funds).
        /// </summary>
        public decimal? TotalAmount { get; set; }

        /// <summary>
        /// Person who authorized the transaction.
        /// </summary>
        public PersonResult AuthorizedPerson { get; set; }

        /// <summary>
        /// Campus associated via the batch (if available).
        /// </summary>
        public CampusResult Campus { get; set; }

        /// <summary>
        /// The financial account associated with this transaction.
        /// </summary>
        public List<FinancialAccountTransactionResult> Accounts { get; set; }

        /// <summary>
        /// The type of currency used for the transaction.
        /// </summary>
        public KeyNameResult CurrencyType { get; set; }

        /// <summary>
        /// The type of credit card, if applicable, used for the transaction.
        /// </summary>
        public KeyNameResult CreditCardType { get; set; }

        /// <summary>
        /// A link to the related refund transaction, or the original transaction
        /// if this is the refund.
        /// </summary>
        public FinancialTransactionRefundLinkResult RefundLink { get; set; }
    }
}
