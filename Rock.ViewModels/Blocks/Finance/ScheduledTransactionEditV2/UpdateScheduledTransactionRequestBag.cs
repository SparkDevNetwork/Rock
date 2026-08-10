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

namespace Rock.ViewModels.Blocks.Finance.ScheduledTransactionEditV2
{
    /// <summary>
    /// The changes to apply to a scheduled transaction.
    /// </summary>
    public class UpdateScheduledTransactionRequestBag
    {
        /// <summary>
        /// Gets or sets the unique identifier of the scheduled transaction being updated.
        /// </summary>
        public string ScheduledTransactionGuid { get; set; }

        /// <summary>
        /// Gets or sets the account/amount allocations to save.
        /// </summary>
        public List<ScheduledTransactionAccountAmountBag> AccountAmounts { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier of the selected campus. Used for campus account
        /// mapping so a parent account can be routed to its campus-specific child account.
        /// </summary>
        public Guid? CampusGuid { get; set; }

        /// <summary>
        /// Gets or sets the selected payment frequency value.
        /// </summary>
        public string FrequencyValue { get; set; }

        /// <summary>
        /// Gets or sets the next payment (start) date.
        /// </summary>
        public DateTime? NextPaymentDate { get; set; }

        /// <summary>
        /// Gets or sets the optional end date for the recurring gift.
        /// </summary>
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the transaction's existing payment
        /// method should be kept.
        /// </summary>
        public bool UseExistingPaymentMethod { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier of the selected saved account, when the user
        /// chose to pay with an existing saved account.
        /// </summary>
        public Guid? SavedAccountGuid { get; set; }

        /// <summary>
        /// Gets or sets the gateway token returned when the user entered a new payment method.
        /// </summary>
        public string GatewayToken { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether a newly entered payment method should be
        /// saved as a saved account.
        /// </summary>
        public bool SaveMethodToAccount { get; set; }

        /// <summary>
        /// Gets or sets the name to use when saving a new payment method as a saved account.
        /// </summary>
        public string SavedAccountName { get; set; }
    }
}
