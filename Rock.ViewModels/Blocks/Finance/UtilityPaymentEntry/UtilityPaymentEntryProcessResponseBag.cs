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

namespace Rock.ViewModels.Blocks.Finance.UtilityPaymentEntry
{
    /// <summary>
    /// The result of processing a gift.
    /// </summary>
    public class UtilityPaymentEntryProcessResponseBag
    {
        /// <summary>
        /// Gets or sets a value indicating whether the gift was charged and recorded successfully.
        /// </summary>
        public bool IsSuccess { get; set; }

        /// <summary>
        /// Gets or sets the resolved Success Page Lava, shown when the gift succeeds.
        /// </summary>
        public string SuccessHtml { get; set; }

        /// <summary>
        /// Gets or sets the resolved Success Page Footer Lava, shown below the success content.
        /// </summary>
        public string SuccessFooterHtml { get; set; }

        /// <summary>
        /// Gets or sets the gateway's confirmation code for the completed transaction.
        /// </summary>
        public string TransactionCode { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the giver is offered the option to save the payment
        /// method for future gifts. True only for a personal (non-business) gift entered with a new
        /// payment method on a gateway that supports saved accounts, and not in Text-to-Give mode. The
        /// shared save-account control is shown when this is true.
        /// </summary>
        public bool IsSaveAccountOffered { get; set; }

        /// <summary>
        /// Gets or sets the Guid of the gateway that processed the gift, passed to the shared save-account
        /// control so it can save the payment method.
        /// </summary>
        public Guid? GatewayGuid { get; set; }

        /// <summary>
        /// Gets or sets the gateway's reusable customer reference for the payment method, passed to the
        /// shared save-account control so the saved account can charge it again later.
        /// </summary>
        public string GatewayPersonIdentifier { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier of the scheduled transaction when the gift was a scheduled
        /// (recurring) gift, so the shared save-account control can save the payment method from it. Null for
        /// a one-time gift.
        /// </summary>
        public Guid? ScheduledTransactionGuid { get; set; }

        /// <summary>
        /// Gets or sets the messages shown to the giver when processing failed.
        /// </summary>
        public List<string> ErrorMessages { get; set; }
    }
}
