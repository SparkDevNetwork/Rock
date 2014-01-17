// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System;

using Rock.Web.Cache;

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

        /// <summary>
        /// Gets the account number.
        /// </summary>
        public override string MaskedNumber
        {
            get { return "Swiped"; }
        }

        /// <summary>
        /// Gets the currency type value.
        /// </summary>
        public override DefinedValueCache CurrencyTypeValue
        {
            get { return DefinedValueCache.Read( new Guid( Rock.SystemGuid.DefinedValue.CURRENCY_TYPE_CREDIT_CARD ) ); }
        }

    }
}
