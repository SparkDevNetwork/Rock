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

namespace Rock.ViewModels.Blocks.Finance.ScheduledTransactionEditV2
{
    /// <summary>
    /// A selectable payment method shown in the Scheduled Transaction Edit (V2) block. This is
    /// either the transaction's existing payment method or one of the person's saved accounts.
    /// </summary>
    public class ScheduledTransactionPaymentMethodBag
    {
        /// <summary>
        /// Gets or sets the identifier used to select this payment method. For a saved account
        /// this is the account's unique identifier; for the existing payment method it is a
        /// sentinel value understood by the block.
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this represents the transaction's existing
        /// payment method, as opposed to one of the person's saved accounts.
        /// </summary>
        public bool IsExistingPaymentMethod { get; set; }

        /// <summary>
        /// Gets or sets the display name / title (for example, the saved account name or the
        /// currency type of the existing method).
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this payment method is a credit card (as
        /// opposed to ACH). Credit cards have an expiration date; ACH accounts do not.
        /// </summary>
        public bool IsCreditCard { get; set; }

        /// <summary>
        /// Gets or sets the card type or currency type (for example, "Visa" or "ACH").
        /// </summary>
        public string CardType { get; set; }

        /// <summary>
        /// Gets or sets the masked account number (for example, "************6789").
        /// </summary>
        public string AccountNumberMasked { get; set; }

        /// <summary>
        /// Gets or sets the formatted expiration date, when applicable (for example, "02/30").
        /// </summary>
        public string ExpirationDate { get; set; }
    }
}
