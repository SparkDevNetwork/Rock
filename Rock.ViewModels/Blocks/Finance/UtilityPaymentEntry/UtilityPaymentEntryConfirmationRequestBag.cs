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
    /// The gift details the giver entered, sent to resolve the confirmation-step review content.
    /// </summary>
    public class UtilityPaymentEntryConfirmationRequestBag
    {
        /// <summary>
        /// Gets or sets the per-account amounts the giver entered.
        /// </summary>
        public List<UtilityPaymentEntryAccountAmountBag> AccountAmounts { get; set; }

        /// <summary>
        /// Gets or sets the Guid of the campus the gift is associated with, used to map each account to
        /// its campus-specific child account so the summary names match what will be saved. Null when no
        /// campus was selected.
        /// </summary>
        public Guid? CampusGuid { get; set; }

        /// <summary>
        /// Gets or sets the Guid of the selected transaction-frequency defined value, used to build the
        /// "when" summary. Null or the One-Time frequency means an immediate gift.
        /// </summary>
        public Guid? FrequencyGuid { get; set; }

        /// <summary>
        /// Gets or sets the scheduled start date as an ISO date string, used to build the "when" summary
        /// for a recurring gift.
        /// </summary>
        public string StartDate { get; set; }

        /// <summary>
        /// Gets or sets the optional scheduled end date as an ISO date string, used to build the "when"
        /// summary for a recurring gift.
        /// </summary>
        public string EndDate { get; set; }

        /// <summary>
        /// Gets or sets the giver's first name, shown in the summary.
        /// </summary>
        public string FirstName { get; set; }

        /// <summary>
        /// Gets or sets the giver's last name, shown in the summary.
        /// </summary>
        public string LastName { get; set; }

        /// <summary>
        /// Gets or sets the giver's email address, shown in the summary.
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Gets or sets the giver's phone number, shown in the summary when phone prompting is enabled.
        /// </summary>
        public string Phone { get; set; }

        /// <summary>
        /// Gets or sets the country code for the giver's phone number, used to format the number shown in
        /// the summary.
        /// </summary>
        public string PhoneCountryCode { get; set; }

        /// <summary>
        /// Gets or sets the billing address the giver entered, shown in the summary.
        /// </summary>
        public AddressControlBag Address { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the gift is given on behalf of a business. When true,
        /// the summary shows the business name and the business-contact fields are validated.
        /// </summary>
        public bool IsGivingAsBusiness { get; set; }

        /// <summary>
        /// Gets or sets the business name, shown in the summary in place of the individual's name.
        /// </summary>
        public string BusinessName { get; set; }

        /// <summary>
        /// Gets or sets the first name of the individual submitting on the business's behalf, validated
        /// only when the giver is not signed in.
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
    }
}
