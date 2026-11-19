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

namespace Rock.ViewModels.Blocks.Event.RegistrationDetail
{
    /// <summary>
    /// The values used to record a manual payment for a registration.
    /// </summary>
    public class ManualPaymentRequestBag
    {
        /// <summary>
        /// Gets or sets the unique identifier of the person alias of the payer.
        /// When null the registrar is used.
        /// </summary>
        public Guid? PayerPersonAliasGuid { get; set; }

        /// <summary>
        /// Gets or sets the amount of the payment.
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// Gets or sets the comments about the payment.
        /// </summary>
        public string Comments { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier of the currency type defined value.
        /// </summary>
        public Guid? CurrencyTypeValueGuid { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier of the credit card type defined value.
        /// Only used when the currency type is credit card.
        /// </summary>
        public Guid? CreditCardTypeValueGuid { get; set; }

        /// <summary>
        /// Gets or sets the transaction code.
        /// </summary>
        public string TransactionCode { get; set; }
    }
}
