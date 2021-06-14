// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System;
using System.Collections.Generic;
using System.Linq;

using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace Rock.Financial
{
    /// <summary>
    /// Information about a payment to be processed by a financial gateway
    /// </summary>
    public class PaymentInfo
    {
        /// <summary>
        /// Gets or sets the additional parameters.
        /// </summary>
        /// <value>
        /// The additional parameters.
        /// </value>
        public Dictionary<string, string> AdditionalParameters { get; set; }

        /// <summary>
        /// Gets or sets the amount.
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// Gets or sets the DefinedValueId of the TransactionType <see cref="Rock.Model.DefinedValue"/> indicating
        /// the type of the transaction.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the DefinedValueId of the TransactionType <see cref="Rock.Model.DefinedValue"/> for this transaction.
        /// </value>
        public int? TransactionTypeValueId { get; set; }

        /// <summary>
        /// Gets or sets the first name.
        /// </summary>
        public string FirstName { get; set; }

        /// <summary>
        /// Gets or sets the last name.
        /// </summary>
        public string LastName { get; set; }

        /// <summary>
        /// Gets or sets the name of the business (if giving as a business)
        /// </summary>
        /// <value>
        /// The name of the business.
        /// </value>
        public string BusinessName { get; set; }

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
        /// The billing street.
        /// NOTE: If this is a <see cref="CreditCardPaymentInfo" /> and the Gateway is configured to prompt for a separate Billing Address (like NMI), <see cref="CreditCardPaymentInfo.BillingStreet1" />, etc, might be different then <see cref="PaymentInfo.Street1"/>, etc
        /// </summary>
        public string Street1 { get; set; }

        /// <summary>
        /// The billing street
        /// </summary>
        public string Street2 { get; set; }

        /// <summary>
        /// The billing city
        /// </summary>
        public string City { get; set; }

        /// <summary>
        /// The billing state
        /// </summary>
        public string State { get; set; }

        /// <summary>
        /// The billing zip/postal code
        /// </summary>
        public string PostalCode { get; set; }

        /// <summary>
        /// The billing country
        /// </summary>
        public string Country { get; set; }

        /// <summary>
        /// Returns True if the payment info includes Address Data (Street1 or Street2).
        /// For example, if updating a scheduled transaction, the address data could be null since the gateway would already know this.
        /// </summary>
        /// <returns></returns>
        public bool IncludesAddressData()
        {
            return this.Street1.IsNotNullOrWhiteSpace() || this.Street2.IsNotNullOrWhiteSpace();
        }

        /// <summary>
        /// Gets or sets the ip address.
        /// </summary>
        /// <value>
        /// The ip address.
        /// </value>
        public string IPAddress { get; set; }

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

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        public virtual string Description { get; set; }

        /// <summary>
        /// Gets or sets the first comment line.
        /// </summary>
        public virtual string Comment1 { get; set; }

        /// <summary>
        /// Gets or sets the second comment line.
        /// </summary>
        public virtual string Comment2 { get; set; }

        /// <summary>
        /// The defined value id for the currency code of the amount field. If this is not the organization's currency then the actual amount
        /// may be more or less depending on this currency's exchange rate. The actual amount should be returned from the payment processor.
        /// </summary>
        public int? AmountCurrencyCodeValueId { get; set; }

        /// <summary>
        /// Gets the formatted value.
        /// </summary>
        /// <value>
        /// The formatted value.
        /// </value>
        public string FormattedValue
        {
            get
            {
                var tempLocation = new Rock.Model.Location
                {
                    Street1 = this.Street1,
                    Street2 = this.Street2,
                    City = this.City,
                    State = this.State,
                    PostalCode = this.PostalCode,
                    Country = this.Country,
                };

                return tempLocation.GetFullStreetAddress();
                
            }
        }

        /// <summary>
        /// Updates the address fields from an address control.
        /// </summary>
        /// <param name="addressControl">The address control.</param>
        public void UpdateAddressFieldsFromAddressControl( AddressControl addressControl )
        {
            Street1 = addressControl.Street1;
            Street2 = addressControl.Street2;
            City = addressControl.City;
            State = addressControl.State;
            PostalCode = addressControl.PostalCode;
            Country = addressControl.Country;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PaymentInfo"/> class.
        /// </summary>
        public PaymentInfo()
        {
            AdditionalParameters = new Dictionary<string, string>();
        }

    }
}
