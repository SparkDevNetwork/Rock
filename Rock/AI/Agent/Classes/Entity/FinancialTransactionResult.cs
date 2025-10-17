using System;
using System.Collections.Generic;

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
        public List<FinancialAccountTransactionSummaryResult> Accounts { get; set; }
    }

    internal class FinancialAccountTransactionSummaryResult : FinancialAccountResult
    {
        public decimal Amount { get; set; }
    }
}
