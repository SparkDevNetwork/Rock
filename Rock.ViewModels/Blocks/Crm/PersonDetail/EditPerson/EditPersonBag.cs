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

using System.Collections.Generic;

using Rock.Enums.Communication;
using Rock.Model;
using Rock.ViewModels.Controls;
using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Crm.PersonDetail.EditPerson
{
    /// <summary>
    /// The editable values for a single person in the Edit Person block. Used both to
    /// pre-fill the form on initialization and as the payload sent back on save.
    /// </summary>
    public class EditPersonBag
    {
        /// <summary>
        /// Gets or sets the identifier key of the person being edited.
        /// </summary>
        public string IdKey { get; set; }

        #region Identity

        /// <summary>
        /// Gets or sets the person's photo binary file.
        /// </summary>
        public ListItemBag Photo { get; set; }

        /// <summary>
        /// Gets or sets the title (Mr., Mrs., etc.) defined value.
        /// </summary>
        public ListItemBag Title { get; set; }

        /// <summary>
        /// Gets or sets the first (legal) name.
        /// </summary>
        public string FirstName { get; set; }

        /// <summary>
        /// Gets or sets the nick name (the name the person goes by).
        /// </summary>
        public string NickName { get; set; }

        /// <summary>
        /// Gets or sets the middle name.
        /// </summary>
        public string MiddleName { get; set; }

        /// <summary>
        /// Gets or sets the last name.
        /// </summary>
        public string LastName { get; set; }

        /// <summary>
        /// Gets or sets the suffix defined value.
        /// </summary>
        public ListItemBag Suffix { get; set; }

        #endregion Identity

        #region Demographics

        /// <summary>
        /// Gets or sets the gender.
        /// </summary>
        public Gender Gender { get; set; }

        /// <summary>
        /// Gets or sets the birth date.
        /// </summary>
        public BirthdayPickerBag BirthDate { get; set; }

        /// <summary>
        /// Gets or sets the grade (graduation offset) defined value.
        /// </summary>
        public ListItemBag Grade { get; set; }

        /// <summary>
        /// Gets or sets the high school graduation year.
        /// </summary>
        public int? GraduationYear { get; set; }

        /// <summary>
        /// Gets or sets the marital status defined value.
        /// </summary>
        public ListItemBag MaritalStatus { get; set; }

        /// <summary>
        /// Gets or sets the anniversary date (ISO 8601), when marital status is married.
        /// </summary>
        public string AnniversaryDate { get; set; }

        /// <summary>
        /// Gets or sets the race defined value.
        /// </summary>
        public ListItemBag Race { get; set; }

        /// <summary>
        /// Gets or sets the ethnicity defined value.
        /// </summary>
        public ListItemBag Ethnicity { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the person is locked as a child
        /// regardless of age or family role ("Consider Person a Child").
        /// </summary>
        public bool IsLockedAsChild { get; set; }

        #endregion Demographics

        #region Status

        /// <summary>
        /// Gets or sets the record status defined value.
        /// </summary>
        public ListItemBag RecordStatus { get; set; }

        /// <summary>
        /// Gets or sets the inactive reason defined value (shown when record status is inactive).
        /// </summary>
        public ListItemBag RecordStatusReason { get; set; }

        /// <summary>
        /// Gets or sets the free-form note describing why the record is inactive.
        /// </summary>
        public string InactiveReasonNote { get; set; }

        /// <summary>
        /// Gets or sets the deceased date (ISO 8601), when the inactive reason is deceased.
        /// </summary>
        public string DeceasedDate { get; set; }

        /// <summary>
        /// Gets or sets the connection status defined value.
        /// </summary>
        public ListItemBag ConnectionStatus { get; set; }

        /// <summary>
        /// Gets or sets the record source defined value.
        /// </summary>
        public ListItemBag RecordSource { get; set; }

        #endregion Status

        #region Contact

        /// <summary>
        /// Gets or sets the editable phone number rows (one per active phone number type).
        /// </summary>
        public List<EditPersonPhoneNumberBag> PhoneNumbers { get; set; }

        /// <summary>
        /// Gets or sets the email address.
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the email is active ("Email Is Deliverable").
        /// </summary>
        public bool IsEmailActive { get; set; }

        /// <summary>
        /// Gets or sets the email preference.
        /// </summary>
        public EmailPreference EmailPreference { get; set; }

        /// <summary>
        /// Gets or sets the communication preference (email or SMS).
        /// </summary>
        public CommunicationType CommunicationPreference { get; set; }

        /// <summary>
        /// Gets or sets whether the person's chat profile is public. Null inherits the system default.
        /// </summary>
        public bool? IsChatProfilePublic { get; set; }

        /// <summary>
        /// Gets or sets whether the person allows open direct messages in chat. Null inherits the system default.
        /// </summary>
        public bool? IsChatOpenDirectMessageAllowed { get; set; }

        #endregion Contact

        #region Advanced

        /// <summary>
        /// Gets or sets the unique identifier of the family whose giving this person's gifts are combined with.
        /// </summary>
        public System.Guid? GivingGroupGuid { get; set; }

        /// <summary>
        /// Gets or sets the giving envelope number.
        /// </summary>
        public string GivingEnvelopeNumber { get; set; }

        /// <summary>
        /// Gets or sets the person's previous last names.
        /// </summary>
        public List<string> PreviousLastNames { get; set; }

        /// <summary>
        /// Gets or sets the alternate identifiers (barcode, fingerprint, etc.) used by check-in.
        /// </summary>
        public List<string> AlternateIds { get; set; }

        /// <summary>
        /// Gets or sets the additional search keys for this person.
        /// </summary>
        public List<EditPersonSearchKeyBag> SearchKeys { get; set; }

        #endregion Advanced
    }
}
