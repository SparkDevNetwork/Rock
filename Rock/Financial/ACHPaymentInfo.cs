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
    /// Information about a bank payment to be processed by a financial gateway
    /// </summary>
    public class ACHPaymentInfo : PaymentInfo
    {
        /// <summary>
        /// Gets or sets the name of the bank.
        /// </summary>
        [Obsolete( "BankName is not needed")]
        public string BankName { get; set; }

        /// <summary>
        /// The account number
        /// </summary>
        public string BankAccountNumber { get; set; }

        /// <summary>
        /// The routing number
        /// </summary>
        public string BankRoutingNumber { get; set; }

        /// <summary>
        /// The account type
        /// </summary>
        public BankAccountType AccountType { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ACHPaymentInfo"/> class.
        /// </summary>
        public ACHPaymentInfo()
            : base()
        {
            AccountType = BankAccountType.Checking;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ACHPaymentInfo"/> class.
        /// </summary>
        /// <param name="bankAccountNumber">The account number.</param>
        /// <param name="bankRoutingNumber">The routing number.</param>
        /// <param name="accountType">Type of the account.</param>
        public ACHPaymentInfo( string bankAccountNumber, string bankRoutingNumber, BankAccountType accountType )
            : this()
        {
            BankAccountNumber = bankAccountNumber;
            BankRoutingNumber = bankRoutingNumber;
            AccountType = accountType;
        }

        /// <summary>
        /// Gets the account number.
        /// </summary>
        public override string MaskedNumber
        {
            get { return BankAccountNumber.Masked( true ); }
        }

        /// <summary>
        /// Gets the currency type value.
        /// </summary>
        public override DefinedValueCache CurrencyTypeValue
        {
            get { return DefinedValueCache.Read( new Guid( Rock.SystemGuid.DefinedValue.CURRENCY_TYPE_ACH ) ); }
        }

    }

    /// <summary>
    /// Type of bank account
    /// </summary>
    public enum BankAccountType
    {
        /// <summary>
        /// Checking Account
        /// </summary>
        Checking = 0,

        /// <summary>
        /// Savings Account
        /// </summary>
        Savings = 1,

    }
 
}
