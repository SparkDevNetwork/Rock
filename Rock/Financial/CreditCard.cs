//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;

namespace Rock.Financial
{
    /// <summary>
    /// Information about a credit card that is passed to financial gateway
    /// </summary>
    public struct CreditCard
    {
        /// <summary>
        /// The name on card
        /// </summary>
        public string NameOnCard;

        /// <summary>
        /// The last name on card (Only used if gateway provider requires split first name and last name fields
        /// </summary>
        public string LastNameOnCard;

        /// <summary>
        /// The billing street
        /// </summary>
        public string BillingStreet;

        /// <summary>
        /// The billing city
        /// </summary>
        public string BillingCity;

        /// <summary>
        /// The billing state
        /// </summary>
        public string BillingState;

        /// <summary>
        /// The billing zip
        /// </summary>
        public string BillingZip;

        /// <summary>
        /// The credit card number
        /// </summary>
        public string Number;

        /// <summary>
        /// The card SVN number
        /// </summary>
        public string Code;

        /// <summary>
        /// The credit card expiration date
        /// </summary>
        public DateTime ExpirationDate;

        /// <summary>
        /// The information obtained from a card-present swipe
        /// </summary>
        public string SwipeInfo;

        /// <summary>
        /// Initializes a new instance of the <see cref="CreditCard" /> struct.
        /// </summary>
        /// <param name="number">The number.</param>
        /// <param name="code">The code.</param>
        /// <param name="expirationDate">The expiration date.</param>
        public CreditCard( string number, string code, DateTime expirationDate )
        {
            Number = number;
            Code = code;
            ExpirationDate = expirationDate;
            SwipeInfo = string.Empty;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CreditCard" /> struct.
        /// </summary>
        /// <param name="swipeInfo">The swipe info.</param>
        public CreditCard( string swipeInfo )
        {
            Number = string.Empty;
            Code = string.Empty;
            ExpirationDate = DateTime.MinValue;
            SwipeInfo = swipeInfo;
        }
    }
}
