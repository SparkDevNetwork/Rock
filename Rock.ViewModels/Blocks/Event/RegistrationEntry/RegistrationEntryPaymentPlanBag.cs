using System;

namespace Rock.ViewModels.Blocks.Event.RegistrationEntry
{
    /// <summary>
    /// Contains the payment plan information for the Registration Entry block.
    /// </summary>
    public class RegistrationEntryPaymentPlanBag
    {
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
        public decimal AmountToPayPerPayment { get; set; }

        /// <summary>
        /// Gets or sets the number of payments.
        /// </summary>
        /// <value>
        /// The number of payments.
        /// </value>
        public int NumberOfPayments { get; set; }

        /// <summary>
        /// Gets or sets the start date of the recurring payments.
        /// </summary>
        /// <value>
        /// The start date of the recurring payments.
        /// </value>
        public DateTimeOffset StartDate { get; set; }
    }
}
