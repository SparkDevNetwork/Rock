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
    /// The client-submitted payload for the Refund block action, containing
    /// the amount and reason for the refund.
    /// </summary>
    public class RefundRequestBag
    {
        /// <summary>
        /// Gets or sets the dollar amount to refund.
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// Gets or sets the maximum allowable refund amount, used for server-side validation.
        /// </summary>
        public decimal MaximumAmount { get; set; }

        /// <summary>
        /// Gets or sets the defined value representing the reason for the refund.
        /// </summary>
        public ListItemBag RefundReason { get; set; }

        /// <summary>
        /// Gets or sets the free-text explanation of why the refund was issued.
        /// </summary>
        public string RefundReasonSummary { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to process the refund through
        /// the financial gateway in addition to recording it in Rock.
        /// </summary>
        public bool Process { get; set; }
    }
}
