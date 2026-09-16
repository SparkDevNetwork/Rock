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

using Rock.ViewModels.Crm;

namespace Rock.ViewModels.Blocks.Crm.PersonDetail.Bio
{
    /// <summary>
    /// Contains all the initial configuration data required to render the
    /// Person Bio block.
    /// </summary>
    public class BioInitializationBox
    {
        /// <summary>
        /// Gets or sets a value indicating whether the block content should be
        /// rendered. This is false when no person could be resolved or the
        /// person record should not be displayed (such as a nameless record).
        /// </summary>
        public bool IsVisible { get; set; }

        /// <summary>
        /// Gets or sets the IdKey of the person being viewed. This is passed
        /// back to the block actions to identify the person.
        /// </summary>
        public string PersonIdKey { get; set; }

        /// <summary>
        /// Gets or sets the full name of the person being viewed.
        /// </summary>
        public string FullName { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the person is deceased,
        /// which applies additional styling to the profile card.
        /// </summary>
        public bool IsDeceased { get; set; }

        /// <summary>
        /// Gets or sets the URL of the person's profile photo.
        /// </summary>
        public string PhotoUrl { get; set; }

        /// <summary>
        /// Gets or sets the friendly account protection profile text. This is
        /// null when the level is low or the current user is not authorized to
        /// view the protection profile, in which case the alert is hidden.
        /// </summary>
        public string AccountProtectionProfileText { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the record represents a
        /// business, which renders the last name as the sole heading.
        /// </summary>
        public bool IsBusiness { get; set; }

        /// <summary>
        /// Gets or sets the person's formal title (such as "Dr."). Only set
        /// when the title defined value is marked as formal.
        /// </summary>
        public string FormalTitle { get; set; }

        /// <summary>
        /// Gets or sets the person's nick name.
        /// </summary>
        public string NickName { get; set; }

        /// <summary>
        /// Gets or sets the person's middle name. Only set when the block is
        /// configured to display middle names and the person has one.
        /// </summary>
        public string MiddleName { get; set; }

        /// <summary>
        /// Gets or sets the person's last name.
        /// </summary>
        public string LastName { get; set; }

        /// <summary>
        /// Gets or sets the person's suffix (such as "Jr.").
        /// </summary>
        public string Suffix { get; set; }

        /// <summary>
        /// Gets or sets the person's first name. Only set when it differs from
        /// the nick name, in which case it renders as a secondary name.
        /// </summary>
        public string FirstName { get; set; }

        /// <summary>
        /// Gets or sets the comma-delimited list of the person's previous last
        /// names. Null when the person has none.
        /// </summary>
        public string PreviousNames { get; set; }

        /// <summary>
        /// Gets or sets the rendered badge content configured for this block.
        /// </summary>
        public List<RenderedBadgeBag> Badges { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the follow button should be
        /// shown for the current user.
        /// </summary>
        public bool IsFollowingVisible { get; set; }

        /// <summary>
        /// Gets or sets the current follow state and follower count for the
        /// person being viewed.
        /// </summary>
        public BioFollowingBag Following { get; set; }

        /// <summary>
        /// Gets or sets the URL for the Text (SMS) action button. Null hides
        /// the button.
        /// </summary>
        public string SmsUrl { get; set; }

        /// <summary>
        /// Gets or sets the URL for the Email action button. Null hides the
        /// button.
        /// </summary>
        public string EmailUrl { get; set; }

        /// <summary>
        /// Gets or sets the tooltip for the Email action button, which calls
        /// out when the person's email preference is "No Mass Emails".
        /// </summary>
        public string EmailButtonTooltip { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the Edit button should be
        /// shown for the current user.
        /// </summary>
        public bool IsEditVisible { get; set; }

        /// <summary>
        /// Gets or sets the URL of the person edit page.
        /// </summary>
        public string EditPersonUrl { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the Impersonate action
        /// should be shown for the current user.
        /// </summary>
        public bool IsImpersonateVisible { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the Impersonate action is
        /// enabled. The action is visible but disabled when person token usage
        /// is not allowed for the person.
        /// </summary>
        public bool IsImpersonateEnabled { get; set; }

        /// <summary>
        /// Gets or sets the URL that downloads the person's vCard.
        /// </summary>
        public string VCardUrl { get; set; }

        /// <summary>
        /// Gets or sets the additional custom action list items configured in
        /// the block settings, rendered as raw HTML inside the actions menu.
        /// </summary>
        public string CustomActionsHtml { get; set; }

        /// <summary>
        /// Gets or sets the workflow actions available in the actions menu.
        /// </summary>
        public List<BioWorkflowActionBag> WorkflowActions { get; set; }

        /// <summary>
        /// Gets or sets the person's gender text.
        /// </summary>
        public string GenderText { get; set; }

        /// <summary>
        /// Gets or sets the person's race and/or ethnicity, joined with a
        /// slash. Null hides the row.
        /// </summary>
        public string RaceEthnicityText { get; set; }

        /// <summary>
        /// Gets or sets the configured label describing the race and ethnicity
        /// values (such as "Race/Ethnicity").
        /// </summary>
        public string RaceEthnicityLabel { get; set; }

        /// <summary>
        /// Gets or sets the person's formatted age (such as "32 yrs old").
        /// Null when the birth year is not known.
        /// </summary>
        public string AgeText { get; set; }

        /// <summary>
        /// Gets or sets the person's birth date text. This is a short date
        /// when the birth year is known, otherwise a month/day value that is
        /// rendered as the primary birth date term.
        /// </summary>
        public string BirthDateText { get; set; }

        /// <summary>
        /// Gets or sets the person's marital status. Null hides the row.
        /// </summary>
        public string MaritalStatusText { get; set; }

        /// <summary>
        /// Gets or sets the humanized wedding anniversary duration (such as
        /// "9 years"). Null when the anniversary should not be displayed.
        /// </summary>
        public string AnniversaryText { get; set; }

        /// <summary>
        /// Gets or sets the person's anniversary date as a short date string.
        /// </summary>
        public string AnniversaryDateText { get; set; }

        /// <summary>
        /// Gets or sets the person's formatted grade. Null hides the row.
        /// </summary>
        public string GradeText { get; set; }

        /// <summary>
        /// Gets or sets the person's graduation text (such as "Graduated 2020"
        /// or "Graduates 2028"). Null hides the row.
        /// </summary>
        public string GraduationText { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the person has graduated,
        /// which controls whether the graduation text renders as a primary or
        /// secondary term.
        /// </summary>
        public bool? HasGraduated { get; set; }

        /// <summary>
        /// Gets or sets the person's phone numbers, ordered by phone type.
        /// </summary>
        public List<BioPhoneNumberBag> PhoneNumbers { get; set; }

        /// <summary>
        /// Gets or sets the rendered HTML for the person's email address,
        /// including any email preference indicators.
        /// </summary>
        public string EmailTagHtml { get; set; }

        /// <summary>
        /// Gets or sets the person's social media links.
        /// </summary>
        public List<BioSocialLinkBag> SocialLinks { get; set; }

        /// <summary>
        /// Gets or sets the resolved custom content HTML configured in the
        /// block settings. Null hides the section.
        /// </summary>
        public string CustomContentHtml { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether phone numbers can originate
        /// a call through the configured PBX component.
        /// </summary>
        public bool IsCallOriginationAvailable { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether call origination is enabled
        /// in the block settings. When enabled without a PBX component, phone
        /// numbers become tel: links on mobile devices.
        /// </summary>
        public bool IsCallOriginationEnabled { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier of the current person, used as
        /// the source when originating a call.
        /// </summary>
        public Guid? CurrentPersonGuid { get; set; }

        /// <summary>
        /// Gets or sets the full name of the current person, used as the
        /// caller id when originating a call.
        /// </summary>
        public string CurrentPersonFullName { get; set; }
    }
}
