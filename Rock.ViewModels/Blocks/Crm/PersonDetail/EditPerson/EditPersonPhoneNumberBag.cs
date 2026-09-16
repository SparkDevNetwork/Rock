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

namespace Rock.ViewModels.Blocks.Crm.PersonDetail.EditPerson
{
    /// <summary>
    /// A single editable phone number row in the Edit Person block, keyed by phone number type.
    /// </summary>
    public class EditPersonPhoneNumberBag
    {
        /// <summary>
        /// Gets or sets the phone number type defined value unique identifier.
        /// </summary>
        public Guid PhoneTypeGuid { get; set; }

        /// <summary>
        /// Gets or sets the display label for the phone number type (e.g., "Mobile").
        /// </summary>
        public string PhoneTypeLabel { get; set; }

        /// <summary>
        /// Gets or sets the country code.
        /// </summary>
        public string CountryCode { get; set; }

        /// <summary>
        /// Gets or sets the phone number.
        /// </summary>
        public string Number { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether SMS messaging is enabled for this number.
        /// Only one phone number may have SMS enabled at a time.
        /// </summary>
        public bool IsMessagingEnabled { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this number is unlisted.
        /// </summary>
        public bool IsUnlisted { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the current user may edit the SMS enabled flag
        /// for this number (driven by the EditSMS security action).
        /// </summary>
        public bool IsSmsEditable { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this row is the mobile phone type.
        /// Used to apply the "Mobile SMS Enabled by Default" behavior on a blank mobile number.
        /// </summary>
        public bool IsMobile { get; set; }
    }
}
