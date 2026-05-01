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

using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Finance.TransactionDetail
{
    /// <summary>
    /// Details about the refund associated with a transaction, including a link
    /// back to the original transaction that was refunded.
    /// </summary>
    public class RefundDetailBag
    {
        /// <summary>
        /// Gets or sets the integer Id of the original transaction that this refund reverses.
        /// </summary>
        public int? OriginalTransactionId { get; set; }

        /// <summary>
        /// Gets or sets the obfuscated IdKey of the original transaction, used to build
        /// the link to the original transaction detail page.
        /// </summary>
        public string OriginalTransactionIdKey { get; set; }

        /// <summary>
        /// Gets or sets the defined value representing the reason for the refund.
        /// </summary>
        public ListItemBag RefundReason { get; set; }

        /// <summary>
        /// Gets or sets the free-text explanation of why the refund was issued.
        /// </summary>
        public string RefundReasonSummary { get; set; }

        /// <summary>
        /// Gets or sets the total amount refunded.
        /// </summary>
        public decimal RefundAmount { get; set; }
    }
}
