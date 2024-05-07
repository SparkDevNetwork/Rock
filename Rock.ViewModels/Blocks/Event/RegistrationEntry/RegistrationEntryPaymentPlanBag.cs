using System;

namespace Rock.ViewModels.Blocks.Event.RegistrationEntry
{
    /// <summary>
    /// Contains the payment plan information for the Registration Entry block.
    /// </summary>
    public class RegistrationEntryPaymentPlanBag
    {
        /// <summary>
        /// Gets or sets the financial scheduled transaction unique identifier.
        /// </summary>
        public Guid FinancialScheduledTransactionGuid { get; set; }

        /// <summary>
        /// Gets or sets the transaction frequency unique identifier.
        /// </summary>
        /// <value>
        /// The transaction frequency unique identifier.
        /// </value>
        public Guid TransactionFrequencyGuid { get; set; }

        /// <summary>
        /// Gets or sets the amount to pay per payment.
        /// </summary>
        /// <value>
        /// The amount to pay per payment.
        /// </value>
        public decimal AmountPerPayment { get; set; }

        /// <summary>
        /// Gets or sets the number of payments.
        /// </summary>
        /// <value>
        /// The number of payments.
        /// </value>
        public int TotalNumberOfPayments { get; set; }

        /// <summary>
        /// Gets or sets the start date of the recurring payments.
        /// </summary>
        /// <value>
        /// The start date of the recurring payments.
        /// </value>
        public DateTimeOffset StartDate { get; set; }
        public string TransactionFrequencyText { get; set; }
        public decimal TotalPlannedAmount { get; set; }
        public decimal RemainingPlannedAmount { get; set; }
        public int RemainingNumberOfPayments { get; set; }
        public int ProcessedNumberOfPayments { get; set; }
        public decimal ProcessedPlannedAmount { get; set; }
    }
}
