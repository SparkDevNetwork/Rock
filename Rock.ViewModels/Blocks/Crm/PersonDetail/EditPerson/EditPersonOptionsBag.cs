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

using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Crm.PersonDetail.EditPerson
{
    /// <summary>
    /// Configuration, feature flags, and option sources that control how the Edit Person form renders.
    /// </summary>
    public class EditPersonOptionsBag
    {
        #region Block Setting Flags

        /// <summary>
        /// Gets or sets a value indicating whether the grade and graduation year fields are hidden
        /// (they are always shown or hidden together).
        /// </summary>
        public bool IsGradeHidden { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the anniversary date field is hidden.
        /// </summary>
        public bool IsAnniversaryDateHidden { get; set; }

        /// <summary>
        /// Gets or sets whether the race field is hidden, optional, or required ("Hide", "Optional", "Required").
        /// </summary>
        public string RaceOption { get; set; }

        /// <summary>
        /// Gets or sets whether the ethnicity field is hidden, optional, or required ("Hide", "Optional", "Required").
        /// </summary>
        public string EthnicityOption { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the birth year is required once a month and day are present.
        /// </summary>
        public bool IsCompleteBirthDateRequired { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether SMS is enabled by default on a new (blank) mobile number.
        /// </summary>
        public bool IsMobileSmsEnabledByDefault { get; set; }

        #endregion Block Setting Flags

        #region Demographics

        /// <summary>
        /// Gets or sets the base year used to convert between grade offset and graduation year on the client
        /// (the current year at the grade transition). Grade and Graduation Year stay in sync from this.
        /// </summary>
        public int GradeTransitionYear { get; set; }

        /// <summary>
        /// Gets or sets the offset added when converting grade to graduation year (1 once this year's grade
        /// transition date has passed, otherwise 0), so the two fields agree with the server's grade logic.
        /// </summary>
        public int GradeOffsetAdjustment { get; set; }

        #endregion Demographics

        #region Security / Visibility

        /// <summary>
        /// Gets or sets a value indicating whether the current user may edit the connection status.
        /// </summary>
        public bool IsConnectionStatusEditable { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the current user may edit the record status,
        /// inactive reason, deceased date, and inactive reason note.
        /// </summary>
        public bool IsRecordStatusEditable { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the current user may edit the record source.
        /// </summary>
        public bool IsRecordSourceEditable { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the giving group and envelope number section is shown.
        /// </summary>
        public bool IsGivingSectionVisible { get; set; }

        #endregion Security / Visibility

        #region Feature Flags

        /// <summary>
        /// Gets or sets a value indicating whether the chat preferences section is shown.
        /// </summary>
        public bool IsChatVisible { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the giving envelope number field is shown.
        /// </summary>
        public bool IsEnvelopeNumberVisible { get; set; }

        #endregion Feature Flags

        #region Option Sources

        /// <summary>
        /// Gets or sets the families whose giving this person's gifts may be combined with.
        /// </summary>
        public List<ListItemBag> GivingGroups { get; set; }

        /// <summary>
        /// Gets or sets the search key types available when adding a search key.
        /// </summary>
        public List<ListItemBag> SearchKeyTypes { get; set; }

        #endregion Option Sources

        #region Header / Banners

        /// <summary>
        /// Gets or sets the name of the person's primary family, shown as a panel header label.
        /// </summary>
        public string FamilyName { get; set; }

        /// <summary>
        /// Gets or sets the name of the person's primary campus, shown as a panel header label.
        /// </summary>
        public string CampusName { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the person is the only active member of their
        /// primary family, so marking them inactive would also inactivate the family.
        /// </summary>
        public bool IsOnlyActiveFamilyMember { get; set; }

        /// <summary>
        /// Gets or sets the placeholder photo URL (the person's initials avatar) shown when they have no photo.
        /// </summary>
        public string NoPictureUrl { get; set; }

        /// <summary>
        /// Gets or sets the account protection profile warning message, when applicable.
        /// </summary>
        public string AccountProtectionProfileMessage { get; set; }

        /// <summary>
        /// Gets or sets the notification box type for the account protection profile message
        /// (e.g., "Warning" or "Danger").
        /// </summary>
        public string AccountProtectionProfileAlertType { get; set; }

        #endregion Header / Banners
    }
}
