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

using Rock.Enums.Blocks.Connection.ConnectionRequestEntry;
using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Connection.ConnectionRequestEntry
{
    /// <summary>
    /// The information required to render the Connection Request Entry form.
    /// </summary>
    public class ConnectionRequestEntryInitializationBox : BlockBox
    {
        #region Configuration

        /// <summary>
        /// Gets or sets a value indicating whether the block's required settings are configured (at least one Connection Type is selected).
        /// </summary>
        public bool IsConfigured { get; set; }

        /// <summary>
        /// Gets or sets the configuration warning shown to individuals who can edit the block when a required setting is missing. Null for visitors who cannot edit the block.
        /// </summary>
        public string ConfigurationMessage { get; set; }

        /// <summary>
        /// Gets or sets the diagnostic shown to individuals who can edit the block when the configured connection type(s) have no active opportunities. Null for visitors who cannot edit the block.
        /// </summary>
        public string OpportunitiesAdminMessage { get; set; }

        #endregion

        #region Banner

        /// <summary>
        /// Gets or sets a value indicating whether the banner is shown at the top of the form.
        /// </summary>
        public bool IsBannerVisible { get; set; }

        /// <summary>
        /// Gets or sets the banner icon CSS class.
        /// </summary>
        public string BannerIconCssClass { get; set; }

        /// <summary>
        /// Gets or sets the banner title.
        /// </summary>
        public string BannerTitle { get; set; }

        /// <summary>
        /// Gets or sets the banner description.
        /// </summary>
        public string BannerDescription { get; set; }

        #endregion

        #region Section Text

        /// <summary>
        /// Gets or sets the Personal Information section title.
        /// </summary>
        public string PersonalInformationTitle { get; set; }

        /// <summary>
        /// Gets or sets the Personal Information section description.
        /// </summary>
        public string PersonalInformationDescription { get; set; }

        /// <summary>
        /// Gets or sets the Contact Information section title.
        /// </summary>
        public string ContactInformationTitle { get; set; }

        /// <summary>
        /// Gets or sets the Contact Information section description.
        /// </summary>
        public string ContactInformationDescription { get; set; }

        /// <summary>
        /// Gets or sets the Connection Opportunities section title.
        /// </summary>
        public string ConnectionOpportunitiesTitle { get; set; }

        /// <summary>
        /// Gets or sets the Connection Opportunities section description.
        /// </summary>
        public string ConnectionOpportunitiesDescription { get; set; }

        /// <summary>
        /// Gets or sets the Additional Information section title.
        /// </summary>
        public string AdditionalInformationTitle { get; set; }

        /// <summary>
        /// Gets or sets the Additional Information section description.
        /// </summary>
        public string AdditionalInformationDescription { get; set; }

        /// <summary>
        /// Gets or sets the label for the additional comments field.
        /// </summary>
        public string AdditionalCommentsLabel { get; set; }

        /// <summary>
        /// Gets or sets the success state title shown after a submission when no redirect URL is configured.
        /// </summary>
        public string SubmissionSuccessTitle { get; set; }

        /// <summary>
        /// Gets or sets the success state description shown after a submission when no redirect URL is configured.
        /// </summary>
        public string SubmissionSuccessDescription { get; set; }

        #endregion

        #region Campus

        /// <summary>
        /// Gets or sets a value indicating whether the campus section is shown (true when more than one active campus exists).
        /// </summary>
        public bool IsCampusVisible { get; set; }

        /// <summary>
        /// Gets or sets the pre-selected campus.
        /// </summary>
        public ListItemBag SelectedCampus { get; set; }

        #endregion

        #region Field Visibility

        /// <summary>
        /// Gets or sets the visibility of the first-time-guest option.
        /// </summary>
        public ConnectionRequestEntryFieldVisibility FirstTimeGuestVisibility { get; set; }

        /// <summary>
        /// Gets or sets the visibility of the title field.
        /// </summary>
        public ConnectionRequestEntryFieldVisibility TitleVisibility { get; set; }

        /// <summary>
        /// Gets or sets the visibility of the suffix field.
        /// </summary>
        public ConnectionRequestEntryFieldVisibility SuffixVisibility { get; set; }

        /// <summary>
        /// Gets or sets the visibility of the birth date field.
        /// </summary>
        public ConnectionRequestEntryFieldVisibility BirthDateVisibility { get; set; }

        /// <summary>
        /// Gets or sets the visibility of the gender field.
        /// </summary>
        public ConnectionRequestEntryFieldVisibility GenderVisibility { get; set; }

        /// <summary>
        /// Gets or sets the visibility of the profile photo field.
        /// </summary>
        public ConnectionRequestEntryFieldVisibility ProfilePhotoVisibility { get; set; }

        /// <summary>
        /// Gets or sets the visibility of the marital status field.
        /// </summary>
        public ConnectionRequestEntryFieldVisibility MaritalStatusVisibility { get; set; }

        /// <summary>
        /// Gets or sets the visibility of the spouse first name field.
        /// </summary>
        public ConnectionRequestEntryFieldVisibility SpouseFirstNameVisibility { get; set; }

        /// <summary>
        /// Gets or sets the visibility of the spouse last name field.
        /// </summary>
        public ConnectionRequestEntryFieldVisibility SpouseLastNameVisibility { get; set; }

        /// <summary>
        /// Gets or sets the visibility of the spouse gender field.
        /// </summary>
        public ConnectionRequestEntryFieldVisibility SpouseGenderVisibility { get; set; }

        /// <summary>
        /// Gets or sets the visibility of the email field.
        /// </summary>
        public ConnectionRequestEntryFieldVisibility EmailVisibility { get; set; }

        /// <summary>
        /// Gets or sets the visibility of the spouse email field.
        /// </summary>
        public ConnectionRequestEntryFieldVisibility SpouseEmailVisibility { get; set; }

        /// <summary>
        /// Gets or sets the visibility of the mobile phone field.
        /// </summary>
        public ConnectionRequestEntryFieldVisibility MobilePhoneVisibility { get; set; }

        /// <summary>
        /// Gets or sets the visibility of the spouse mobile phone field.
        /// </summary>
        public ConnectionRequestEntryFieldVisibility SpouseMobilePhoneVisibility { get; set; }

        /// <summary>
        /// Gets or sets the visibility of the SMS-consent option.
        /// </summary>
        public ConnectionRequestEntryFieldVisibility SmsConsentVisibility { get; set; }

        /// <summary>
        /// Gets or sets the visibility of the address field.
        /// </summary>
        public ConnectionRequestEntryFieldVisibility AddressVisibility { get; set; }

        /// <summary>
        /// Gets or sets the visibility of the additional comments field.
        /// </summary>
        public ConnectionRequestEntryFieldVisibility AdditionalCommentsVisibility { get; set; }

        /// <summary>
        /// Gets or sets the visibility of the preferred service time field (hidden when no schedule category is configured).
        /// </summary>
        public ConnectionRequestEntryFieldVisibility PreferredServiceTimeVisibility { get; set; }

        #endregion

        #region Options

        /// <summary>
        /// Gets or sets the available title options.
        /// </summary>
        public List<ListItemBag> TitleOptions { get; set; }

        /// <summary>
        /// Gets or sets the available suffix options.
        /// </summary>
        public List<ListItemBag> SuffixOptions { get; set; }

        /// <summary>
        /// Gets or sets the available marital status options.
        /// </summary>
        public List<ListItemBag> MaritalStatusOptions { get; set; }

        /// <summary>
        /// Gets or sets the available preferred service time options (Schedules from the configured category).
        /// </summary>
        public List<ListItemBag> PreferredServiceTimeOptions { get; set; }

        #endregion

        #region Form Content

        /// <summary>
        /// Gets or sets the connection opportunities offered for selection.
        /// </summary>
        public List<ConnectionRequestEntryOpportunityBag> Opportunities { get; set; }

        /// <summary>
        /// Gets or sets the public person attributes shown in the Additional Information section.
        /// </summary>
        public Dictionary<string, PublicAttributeBag> PersonAttributes { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether a First Time Guest Opportunity is configured (so a first-time-guest submission can stand alone).
        /// </summary>
        public bool IsFirstTimeGuestOpportunityConfigured { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether CAPTCHA must be satisfied before submitting.
        /// </summary>
        public bool IsCaptchaEnabled { get; set; }

        /// <summary>
        /// Gets or sets the values used to pre-populate the form.
        /// </summary>
        public ConnectionRequestEntryRequestBag PrefilledValues { get; set; }

        #endregion
    }
}
