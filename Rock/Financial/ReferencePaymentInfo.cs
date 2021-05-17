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
using Rock.Web.Cache;

namespace Rock.Financial
{
    /// <summary>
    /// Information about a reference payment to be processed by a financial gateway.  A 
    /// reference payment is initiated using a code returned by previous collected CreditCard/ACH info (i.e. using
    /// a saved account number or payment token)
    /// </summary>
    public class ReferencePaymentInfo : PaymentInfo
    {
        /// <summary>
        /// Gets or sets the transaction code that was used as the "source transaction", and is used by some gateways (PayFlowPro) to lookup the payment info.
        /// For gateways that have the concept of a Customer Vault (NMI and MyWell), <see cref="GatewayPersonIdentifier" /> is what would be used.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the transaction code of the transaction.
        /// </value>
        public string TransactionCode { get; set; }

        /// <summary>
        /// Gets or sets the reference number. Usually a reference to previously collected CreditCard/ACH data.
        /// However, some plug-in gateways might use this as a customer reference.
        /// To use a saved customer record from the payment gateway, set <seealso cref="GatewayPersonIdentifier"/> instead.
        /// </summary>
        public string ReferenceNumber { get; set; }

        /// <summary>
        /// Gets or sets the masked account number.
        /// </summary>
        public string MaskedAccountNumber { get; set; }

        /// <summary>
        /// If the payment method has an expiration date (for example a reference to a Credit Card payment), this is the expiration date
        /// </summary>
        public DateTime? PaymentExpirationDate { get; set; }

        /// <summary>
        /// Gets or sets the initial currency type value.
        /// </summary>
        /// <value>
        /// The initial currency type value.
        /// </value>
        public DefinedValueCache InitialCurrencyTypeValue
        {
            get
            {
                return initialCurrencyTypeValueId.HasValue ? DefinedValueCache.Get( initialCurrencyTypeValueId.Value ) : null;
            }

            set
            {
                initialCurrencyTypeValueId = value?.Id;
            }
        }

        private int? initialCurrencyTypeValueId = null;

        /// <summary>
        /// Gets or sets the initial credit card type value id.
        /// </summary>
        /// <value>
        /// The initial credit card type value id.
        /// </value>
        public DefinedValueCache InitialCreditCardTypeValue
        {
            get
            {
                return initialCreditCardTypeValueId.HasValue ? DefinedValueCache.Get( initialCreditCardTypeValueId.Value ) : null;
            }

            set
            {
                initialCreditCardTypeValueId = value?.Id;
            }
        }

        private int? initialCreditCardTypeValueId = null;

        /// <summary>
        /// Gets or sets the Gateway Person Identifier.
        /// This would indicate id the customer vault information on the gateway (for gateways that have customer vaults (NMI and MyWell) )
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the Gateway Person Identifier of the account.
        /// </value>
        public string GatewayPersonIdentifier { get; set; }

        /// <summary>
        /// Gets or sets the financial person saved account identifier that was used for this payment
        /// </summary>
        /// <value>
        /// The financial person saved account identifier.
        /// </value>
        public int? FinancialPersonSavedAccountId { get; set; }

        /// <summary>
        /// Gets the account number.
        /// </summary>
        public override string MaskedNumber
        {
            get { return MaskedAccountNumber; }
        }

        /// <summary>
        /// Gets the currency type value.
        /// </summary>
        public override DefinedValueCache CurrencyTypeValue
        {
            get { return InitialCurrencyTypeValue; }
        }

        /// <summary>
        /// Gets the credit card type value id.
        /// </summary>
        public override DefinedValueCache CreditCardTypeValue
        {
            get { return InitialCreditCardTypeValue; }
        }
    }
}
