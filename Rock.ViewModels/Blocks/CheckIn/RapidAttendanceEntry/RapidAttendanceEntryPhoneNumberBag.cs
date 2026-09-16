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

namespace Rock.ViewModels.Blocks.CheckIn.RapidAttendanceEntry
{
    /// <summary>
    /// One phone number row shown in the Add Person and Edit Person modals, for a single configured phone type.
    /// </summary>
    public class RapidAttendanceEntryPhoneNumberBag
    {
        /// <summary>
        /// Gets or sets the unique identifier of the phone number type (a Person Phone Type defined value).
        /// </summary>
        public Guid PhoneTypeGuid { get; set; }

        /// <summary>
        /// Gets or sets the phone number type's display name, shown as the row's label.
        /// </summary>
        public string PhoneTypeName { get; set; }

        /// <summary>
        /// Gets or sets the country code portion of the number.
        /// </summary>
        public string CountryCode { get; set; }

        /// <summary>
        /// Gets or sets the entered phone number. An empty number removes the number from the person.
        /// </summary>
        public string Number { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether SMS messaging is enabled on this number. Only one number may
        /// have messaging enabled.
        /// </summary>
        public bool IsMessagingEnabled { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this number is unlisted.
        /// </summary>
        public bool IsUnlisted { get; set; }
    }
}
