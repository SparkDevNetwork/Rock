//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;

namespace Rock.Financial
{
    /// <summary>
    /// Information related to a scheduled payment frequency
    /// </summary>
    public class PaymentSchedule
    {
        /// <summary>
        /// Gets or sets the person id that is authorizing the payment schedule
        /// </summary>
        /// <value>
        /// The person id.
        /// </value>
        public int PersonId { get; set; }

        /// <summary>
        /// Gets or sets the transaction frequency value.
        /// </summary>
        /// <value>
        /// The transaction frequency value.
        /// </value>
        public Rock.Web.Cache.DefinedValueCache TransactionFrequencyValue { get; set; }

        /// <summary>
        /// Gets or sets the start date.
        /// </summary>
        /// <value>
        /// The start date.
        /// </value>
        public DateTime StartDate { get; set; }

        /// <summary>
        /// Gets or sets the end date.
        /// </summary>
        /// <value>
        /// The end date.
        /// </value>
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// Gets or sets the number of payments.
        /// </summary>
        /// <value>
        /// The number of payments.
        /// </value>
        public int? NumberOfPayments { get; set; }

        public override string ToString()
        {
            return string.Format( "{0} starting on {1}",
                TransactionFrequencyValue.Description, StartDate );
        }
    }
}
