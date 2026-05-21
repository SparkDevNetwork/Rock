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

namespace Rock.ViewModels.Blocks.Finance.TransactionDetail
{
    /// <summary>
    /// The additional configuration options for the Transaction Detail block.
    /// </summary>
    public class TransactionDetailOptionsBag
    {
        /// <summary>
        /// Gets or sets the Guid of the Financial Transaction Type defined type,
        /// used to populate the transaction type picker.
        /// </summary>
        public string TransactionTypesGuid { get; set; }

        /// <summary>
        /// Gets or sets the Guid of the Financial Source Type defined type,
        /// used to populate the transaction source picker.
        /// </summary>
        public string TransactionSourceTypesGuid { get; set; }

        /// <summary>
        /// Gets or sets the Guid of the Financial Transaction Refund Reason defined type,
        /// used to populate the refund reason picker.
        /// </summary>
        public string RefundReasonTypesGuid { get; set; }

        /// <summary>
        /// Gets or sets the Guid of the Financial Currency Type defined type,
        /// used to populate the payment method picker.
        /// </summary>
        public string CurrencyTypesGuid { get; set; }

        /// <summary>
        /// Gets or sets the Guid of the Financial Currency Code defined type,
        /// used to populate the foreign currency picker.
        /// </summary>
        public string CurrencyCodesGuid { get; set; }

        /// <summary>
        /// Gets or sets the Guid of the Financial Credit Card Type defined type,
        /// used to populate the credit card type picker.
        /// </summary>
        public string CreditCardTypesGuid { get; set; }

        /// <summary>
        /// Gets or sets the Guid of the Financial Non-Cash Asset Type defined type,
        /// used to populate the non-cash asset type picker.
        /// </summary>
        public string AssetTypes { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the current user may edit this transaction.
        /// This is <c>false</c> when the batch is closed or automated, even if the user
        /// has block-level edit permission.
        /// </summary>
        public bool CanEdit { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the block is in read-only mode.
        /// </summary>
        public bool IsReadOnly { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether editing the associated batch is allowed.
        /// </summary>
        public bool BatchEditAllowed { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether foreign currency fields should be shown,
        /// controlled by the Enable Foreign Currency block setting.
        /// </summary>
        public bool ShowForeignCurrencyFields { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the Transaction Source field is required,
        /// controlled by the Transaction Source Required block setting.
        /// </summary>
        public bool TransactionSourceRequired { get; set; }

        /// <summary>
        /// Gets or sets the URL of the batch detail page, used by the "Save Then View Batch"
        /// footer action when adding a new transaction.
        /// </summary>
        public string BatchDetailPageUrl { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the last used account should be
        /// pre-populated when adding another transaction in the same session.
        /// Only applies when there is exactly one account allocation on the saved transaction.
        /// </summary>
        public bool CarryOverAccount { get; set; }
    }
}
