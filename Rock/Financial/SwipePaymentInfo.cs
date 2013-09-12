//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;

namespace Rock.Financial
{
    /// <summary>
    /// Information about a credit card swipe payment to be processed by a financial gateway.  A swipe
    /// payment is used when the card is present (physically used)
    /// </summary>
    public class SwipePaymentInfo: PaymentInfo
    {
        /// <summary>
        /// The information obtained from a card-present swipe
        /// </summary>
        public string SwipeInfo { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SwipePaymentInfo" /> struct.
        /// </summary>
        /// <param name="swipeInfo">The swipe info.</param>
        public SwipePaymentInfo( string swipeInfo )
            : base()
        {
            SwipeInfo = swipeInfo;
        }
    }
}
