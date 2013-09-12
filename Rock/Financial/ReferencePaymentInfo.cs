//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;

namespace Rock.Financial
{
    /// <summary>
    /// Information about a reference payment to be processed by a financial gateway.  A 
    /// reference payment is initiated using a code returned by a previous payment (i.e. using
    /// a saved account number)
    /// </summary>
    public class ReferencePaymentInfo : PaymentInfo
    {

        /// <summary>
        /// Gets or sets the reference number.
        /// </summary>
        public string ReferenceNumber { get; set; }

        /// <summary>
        /// Gets or sets the initial payment method.
        /// </summary>
        /// <value>
        /// The payment method.
        /// </value>
        public Rock.Model.PaymentMethod PaymentMethod { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ReferencePaymentInfo" /> struct.
        /// </summary>
        /// <param name="referenceNumber">The reference number.</param>
        public ReferencePaymentInfo( string referenceNumber )
            : base()
        {
            ReferenceNumber = referenceNumber;
        }
    }
}
