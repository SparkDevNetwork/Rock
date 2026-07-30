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

namespace Rock.ViewModels.Blocks.Finance.UtilityPaymentEntry
{
    /// <summary>
    /// The gift details submitted by the giver to be charged and recorded.
    /// </summary>
    public class UtilityPaymentEntryProcessRequestBag
    {
        /// <summary>
        /// Gets or sets the idempotency Guid minted on the client when entry began. The process action
        /// checks for an existing transaction with this Guid before charging, guarding against a double
        /// charge from a retry or double-click.
        /// </summary>
        public Guid TransactionGuid { get; set; }

        /// <summary>
        /// Gets or sets the payment token the hosted gateway control returned after tokenizing the
        /// entered card or bank account. Empty when the giver chose a saved account.
        /// </summary>
        public string GatewayToken { get; set; }

        /// <summary>
        /// Gets or sets the Guid of the saved payment method the giver chose to charge instead of
        /// entering a new one. Null when the giver entered a new card or bank account.
        /// </summary>
        public Guid? SavedAccountGuid { get; set; }

        /// <summary>
        /// Gets or sets the per-account amounts the giver entered.
        /// </summary>
        public List<UtilityPaymentEntryAccountAmountBag> AccountAmounts { get; set; }

        /// <summary>
        /// Gets or sets the Guid of the campus the gift is associated with, used to map each account to
        /// its campus-specific child account. Null when no campus was selected.
        /// </summary>
        public Guid? CampusGuid { get; set; }

        /// <summary>
        /// Gets or sets the Guid of the selected transaction-frequency defined value. Null or the One-Time
        /// frequency with a start date of today or earlier means an immediate one-time gift; any other value
        /// creates a scheduled transaction.
        /// </summary>
        public Guid? FrequencyGuid { get; set; }

        /// <summary>
        /// Gets or sets the scheduled start date as an ISO date string. Drives the schedule start for a
        /// recurring or future-dated gift.
        /// </summary>
        public string StartDate { get; set; }

        /// <summary>
        /// Gets or sets the optional scheduled end date as an ISO date string, applied only to a recurring
        /// gift when scheduled end dates are allowed.
        /// </summary>
        public string EndDate { get; set; }

        /// <summary>
        /// Gets or sets the giver's first name, used for the billing name on the payment.
        /// </summary>
        public string FirstName { get; set; }

        /// <summary>
        /// Gets or sets the giver's last name, used for the billing name on the payment.
        /// </summary>
        public string LastName { get; set; }

        /// <summary>
        /// Gets or sets the giver's email address.
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Gets or sets the giver's phone number.
        /// </summary>
        public string Phone { get; set; }

        /// <summary>
        /// Gets or sets the country code for the giver's phone number, used to format the number sent to
        /// the gateway.
        /// </summary>
        public string PhoneCountryCode { get; set; }

        /// <summary>
        /// Gets or sets the billing address the giver entered.
        /// </summary>
        public AddressControlBag Address { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the giver opted in to SMS messaging on the entered
        /// phone number.
        /// </summary>
        public bool IsSmsOptIn { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the gift should be recorded as anonymous.
        /// </summary>
        public bool IsAnonymous { get; set; }

        /// <summary>
        /// Gets or sets the comment the giver entered, appended to the resolved Payment Comment Template.
        /// </summary>
        public string Comment { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the gift is given on behalf of a business. When true,
        /// the Email, Phone, PhoneCountryCode, Address, and IsSmsOptIn values describe the business, and
        /// the gift is authorized to the business rather than the individual.
        /// </summary>
        public bool IsGivingAsBusiness { get; set; }

        /// <summary>
        /// Gets or sets the Guid of the existing business the gift is for. Null to match by name or create
        /// a new business.
        /// </summary>
        public Guid? BusinessGuid { get; set; }

        /// <summary>
        /// Gets or sets the business name.
        /// </summary>
        public string BusinessName { get; set; }

        /// <summary>
        /// Gets or sets the first name of the individual submitting on the business's behalf, entered only
        /// when the giver is not signed in.
        /// </summary>
        public string BusinessContactFirstName { get; set; }

        /// <summary>
        /// Gets or sets the last name of the individual submitting on the business's behalf.
        /// </summary>
        public string BusinessContactLastName { get; set; }

        /// <summary>
        /// Gets or sets the email of the individual submitting on the business's behalf.
        /// </summary>
        public string BusinessContactEmail { get; set; }

        /// <summary>
        /// Gets or sets the phone number of the individual submitting on the business's behalf.
        /// </summary>
        public string BusinessContactPhone { get; set; }

        /// <summary>
        /// Gets or sets the country code for the business contact's phone number.
        /// </summary>
        public string BusinessContactPhoneCountryCode { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the business contact opted in to SMS messaging on the
        /// entered phone number.
        /// </summary>
        public bool IsBusinessContactSmsOptIn { get; set; }
    }
}
