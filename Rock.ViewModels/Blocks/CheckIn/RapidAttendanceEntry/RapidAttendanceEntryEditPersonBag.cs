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

using Rock.Enums.Communication;
using Rock.Model;
using Rock.ViewModels.Controls;
using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.CheckIn.RapidAttendanceEntry
{
    /// <summary>
    /// The editable individual shown in the Add Person and Edit Person modals. Returned populated when loading a
    /// person to edit (within the options bag) and sent back to save the person.
    /// </summary>
    public class RapidAttendanceEntryEditPersonBag
    {
        /// <summary>
        /// Gets or sets the unique identifier of the person being edited. Null when adding a new family member.
        /// </summary>
        public Guid? PersonGuid { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier of the family the person belongs to (or is being added to).
        /// </summary>
        public Guid FamilyGuid { get; set; }

        /// <summary>
        /// Gets or sets the person's first name.
        /// </summary>
        public string FirstName { get; set; }

        /// <summary>
        /// Gets or sets the person's last name.
        /// </summary>
        public string LastName { get; set; }

        /// <summary>
        /// Gets or sets the person's suffix (a Person Suffix defined value), or null.
        /// </summary>
        public ListItemBag Suffix { get; set; }

        /// <summary>
        /// Gets or sets the person's gender.
        /// </summary>
        public Gender Gender { get; set; }

        /// <summary>
        /// Gets or sets the person's birth date as month, day, and year parts. A future date is rolled back by
        /// whole centuries on save until it is not after today.
        /// </summary>
        public DatePartsPickerValueBag BirthDate { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier of the family role (adult or child) the person holds in the family.
        /// </summary>
        public Guid RoleGuid { get; set; }

        /// <summary>
        /// Gets or sets the person's marital status (a Marital Status defined value), or null. Editable for adults
        /// only.
        /// </summary>
        public ListItemBag MaritalStatus { get; set; }

        /// <summary>
        /// Gets or sets the person's grade, whose value is the grade offset (years until graduation), or null.
        /// Editable for children only.
        /// </summary>
        public ListItemBag Grade { get; set; }

        /// <summary>
        /// Gets or sets the person's race (a Race defined value), or null.
        /// </summary>
        public ListItemBag Race { get; set; }

        /// <summary>
        /// Gets or sets the person's ethnicity (an Ethnicity defined value), or null.
        /// </summary>
        public ListItemBag Ethnicity { get; set; }

        /// <summary>
        /// Gets or sets the person's email address.
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the person's email address is active.
        /// </summary>
        public bool IsEmailActive { get; set; }

        /// <summary>
        /// Gets or sets the person's communication preference. Editable for adults only.
        /// </summary>
        public CommunicationType CommunicationPreference { get; set; }

        /// <summary>
        /// Gets or sets the person's phone numbers. On load these are the person's existing numbers across all
        /// types; on save they are the edited rows for the active role's configured phone types.
        /// </summary>
        public List<RapidAttendanceEntryPhoneNumberBag> PhoneNumbers { get; set; }

        /// <summary>
        /// Gets or sets the person attribute values, keyed by attribute key. Holds the values for both the adult
        /// and child attribute sets; the role determines which set is applied on save.
        /// </summary>
        public Dictionary<string, string> AttributeValues { get; set; }
    }
}
