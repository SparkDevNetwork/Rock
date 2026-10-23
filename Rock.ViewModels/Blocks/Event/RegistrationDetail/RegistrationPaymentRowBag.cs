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

namespace Rock.ViewModels.Blocks.Event.RegistrationDetail
{
    /// <summary>
    /// A single payment made for a registration.
    /// </summary>
    public class RegistrationPaymentRowBag
    {
        /// <summary>
        /// Gets or sets the identifier key of the financial transaction.
        /// </summary>
        public string IdKey { get; set; }

        /// <summary>
        /// Gets or sets the formatted date of the transaction, or null when it has no date.
        /// </summary>
        public string TransactionDateText { get; set; }

        /// <summary>
        /// Gets or sets the formatted time of the transaction, or null when it has no date.
        /// </summary>
        public string TransactionTimeText { get; set; }

        /// <summary>
        /// Gets or sets the name of the person who made the payment.
        /// </summary>
        public string PayerName { get; set; }

        /// <summary>
        /// Gets or sets the currency type and credit card type of the payment.
        /// </summary>
        public string CurrencyAndCreditCardType { get; set; }

        /// <summary>
        /// Gets or sets the masked account number of the payment.
        /// </summary>
        public string AccountNumberMasked { get; set; }

        /// <summary>
        /// Gets or sets the transaction code of the payment.
        /// </summary>
        public string TransactionCode { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the transaction is a refund.
        /// </summary>
        public bool IsRefund { get; set; }

        /// <summary>
        /// Gets or sets the formatted amount of the transaction applied to this registration.
        /// </summary>
        public string AmountText { get; set; }
    }
}
