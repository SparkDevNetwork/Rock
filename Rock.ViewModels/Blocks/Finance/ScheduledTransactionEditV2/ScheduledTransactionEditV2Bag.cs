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

using Rock.ViewModels.Controls;
using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Finance.ScheduledTransactionEditV2
{
    /// <summary>
    /// The runtime data used to initialize the Scheduled Transaction Edit (V2) block.
    /// </summary>
    public class ScheduledTransactionEditV2Bag
    {
        /// <summary>
        /// Gets or sets the unique identifier of the scheduled transaction being edited.
        /// </summary>
        public string ScheduledTransactionGuid { get; set; }

        /// <summary>
        /// Gets or sets a configuration/guard message to display instead of the form.
        /// When set, the form should not be rendered.
        /// </summary>
        public string ErrorMessage { get; set; }

        /// <summary>
        /// Gets or sets the unique identifiers of the accounts that are selectable in the
        /// account/amount picker. The CampusAccountAmountPicker control resolves the account
        /// details (name, campus mapping) from these on the client.
        /// </summary>
        public List<string> SelectableAccountGuids { get; set; }

        /// <summary>
        /// Gets or sets the additional accounts that can be added to the picker on demand.
        /// </summary>
        public List<ListItemBag> AdditionalAccounts { get; set; }

        /// <summary>
        /// Gets or sets the current account/amount allocations for the scheduled transaction.
        /// </summary>
        public List<ScheduledTransactionAccountAmountBag> AccountAmounts { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether amounts can be allocated across more than
        /// one account. This is <c>true</c> when the transaction already spans multiple accounts,
        /// or when the block's Multi-Account Giving setting is enabled.
        /// </summary>
        public bool IsMultiAccountMode { get; set; }

        /// <summary>
        /// Gets or sets the supported payment frequency options.
        /// </summary>
        public List<ListItemBag> FrequencyOptions { get; set; }

        /// <summary>
        /// Gets or sets the currently selected frequency value.
        /// </summary>
        public string SelectedFrequencyValue { get; set; }

        /// <summary>
        /// Gets or sets the next payment (start) date.
        /// </summary>
        public DateTime? NextPaymentDate { get; set; }

        /// <summary>
        /// Gets or sets the optional end date for the recurring gift.
        /// </summary>
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// Gets or sets the earliest date the gateway allows a schedule to start.
        /// </summary>
        public DateTime? EarliestPaymentDate { get; set; }

        /// <summary>
        /// Gets or sets the campus currently associated with the gift.
        /// </summary>
        public ListItemBag Campus { get; set; }

        /// <summary>
        /// Gets or sets the selectable payment methods: the transaction's existing payment
        /// method followed by the person's saved accounts (when available/permitted).
        /// </summary>
        public List<ScheduledTransactionPaymentMethodBag> PaymentMethods { get; set; }

        /// <summary>
        /// Gets or sets the hosted gateway control model used to render the payment iframe.
        /// </summary>
        public GatewayControlBag GatewayControl { get; set; }
    }
}
