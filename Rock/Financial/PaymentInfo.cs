//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;

namespace Rock.Financial
{
    /// <summary>
    /// Information about a payment to be processed by a financial gateway
    /// </summary>
    public abstract class PaymentInfo
    {
        /// <summary>
        /// Gets or sets the amount.
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// Gets or sets the first name.
        /// </summary>
        public string FirstName { get; set; }

        /// <summary>
        /// Gets or sets the last name.
        /// </summary>
        public string LastName { get; set; }

        /// <summary>
        /// Gets the full name.
        /// </summary>
        public string FullName
        {
            get { return string.Format( "{0} {1}", FirstName, LastName ); }
        }

        /// <summary>
        /// Gets or sets the email.
        /// </summary>
        /// <value>
        /// The email.
        /// </value>
        public string Email { get; set; }

        /// <summary>
        /// Gets or sets the phone.
        /// </summary>
        /// <value>
        /// The phone.
        /// </value>
        public string Phone { get; set; }

        /// <summary>
        /// The billing street
        /// </summary>
        public string Street { get; set; }

        /// <summary>
        /// The billing city
        /// </summary>
        public string City { get; set; }

        /// <summary>
        /// The billing state
        /// </summary>
        public string State { get; set; }

        /// <summary>
        /// The billing zip
        /// </summary>
        public string Zip { get; set; }
    }
}
