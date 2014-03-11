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
    /// Information about a payment to be processed by a financial gateway
    /// </summary>
    public class PaymentInfo
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

        /// <summary>
        /// Gets the account number.
        /// </summary>
        public virtual string MaskedNumber { get { return string.Empty; } }

        /// <summary>
        /// Gets the currency type value.
        /// </summary>
        public virtual DefinedValueCache CurrencyTypeValue { get { return null; } }

        /// <summary>
        /// Gets the credit card type value id.
        /// </summary>
        public virtual DefinedValueCache CreditCardTypeValue { get { return null; } }
    }
}
