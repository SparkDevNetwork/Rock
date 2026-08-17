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
using Rock.ViewModels.Finance;
using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Finance.UtilityPaymentEntry
{
    /// <summary>
    /// The configuration the Utility Payment Entry block needs for its initial render.
    /// </summary>
    public class UtilityPaymentEntryOptionsBag
    {
        /// <summary>
        /// Gets or sets a value indicating whether the block header section (title and description) is shown.
        /// </summary>
        public bool IsHeaderSectionShown { get; set; }

        /// <summary>
        /// Gets or sets the title displayed at the top of the block header section.
        /// </summary>
        public string HeaderTitle { get; set; }

        /// <summary>
        /// Gets or sets the supporting text displayed below the header title.
        /// </summary>
        public string HeaderDescription { get; set; }

        /// <summary>
        /// Gets or sets the icon CSS class displayed in the block header.
        /// </summary>
        public string HeaderIcon { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the panel title and section headings are shown.
        /// </summary>
        public bool IsPanelAndSectionHeadingsShown { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the entry sections render in the two-column fluid layout.
        /// When false (the default), the sections stack vertically.
        /// </summary>
        public bool IsFluidLayout { get; set; }

        /// <summary>
        /// Gets or sets the heading text shown at the top of the block panel when a gateway is
        /// configured.
        /// </summary>
        public string PanelTitle { get; set; }

        /// <summary>
        /// Gets or sets the URL the entry-step Back button navigates to (the page the giver came
        /// from). Null when the Back button is off or no referrer is available, in which case no Back
        /// button is shown on the entry step.
        /// </summary>
        public string InitialBackButtonUrl { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether a financial gateway is configured for the
        /// block. When false, the block shows the installed-gateway help instead of the entry flow.
        /// </summary>
        public bool IsGatewayConfigured { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the configured gateway is the Test Gateway. When
        /// true, a notice is shown that no real amounts will be charged.
        /// </summary>
        public bool IsTestGateway { get; set; }

        /// <summary>
        /// Gets or sets the installed hosted gateway components shown as help when no gateway
        /// is configured. Populated only when <see cref="IsGatewayConfigured"/> is false.
        /// </summary>
        public List<SupportedGatewayBag> SupportedGateways { get; set; }

        /// <summary>
        /// Gets or sets the title of the configuration warning shown in place of the entry flow when
        /// the configured gateway cannot be used: both ACH and Credit Card disabled, or a gateway
        /// without a hosted payment interface. Null when the gateway is usable.
        /// </summary>
        public string ConfigurationWarningTitle { get; set; }

        /// <summary>
        /// Gets or sets the message of the configuration warning shown in place of the entry flow.
        /// Null when the gateway is usable. See <see cref="ConfigurationWarningTitle"/>.
        /// </summary>
        public string ConfigurationWarningMessage { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the campus is prompted for even when the
        /// individual's campus is already known.
        /// </summary>
        public bool IsCampusPromptedWhenKnown { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether inactive campuses are included in the campus list.
        /// </summary>
        public bool AreInactiveCampusesIncluded { get; set; }

        /// <summary>
        /// Gets or sets the campus type defined value Guids that limit which campuses appear in the
        /// campus list.
        /// </summary>
        public List<Guid> CampusTypeFilter { get; set; }

        /// <summary>
        /// Gets or sets the campus status defined value Guids that limit which campuses appear in the
        /// campus list.
        /// </summary>
        public List<Guid> CampusStatusFilter { get; set; }

        /// <summary>
        /// Gets or sets the Guid of the campus already known for the individual, used as the initial
        /// campus selection. Null when no campus is known.
        /// </summary>
        public Guid? DefaultCampusGuid { get; set; }

        /// <summary>
        /// Gets or sets the heading shown in the Campus Information section header.
        /// </summary>
        public string CampusSectionTitle { get; set; }

        /// <summary>
        /// Gets or sets the icon CSS class shown in the Campus Information section header.
        /// </summary>
        public string CampusSectionIcon { get; set; }

        /// <summary>
        /// Gets or sets the supporting text shown below the Campus Information section title.
        /// </summary>
        public string CampusSectionDescription { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the giver enters an amount per account
        /// (multiple-account entry) rather than choosing a single account.
        /// </summary>
        public bool IsMultiAccountEntry { get; set; }

        /// <summary>
        /// Gets or sets the accounts shown to the giver for contribution, resolved server-side (name
        /// and Guid), ordered by account order. Resolving on the server lets non-public accounts
        /// specified through the URL be included securely, since the shared accounts endpoint returns
        /// only public accounts.
        /// </summary>
        public List<ListItemBag> Accounts { get; set; }

        /// <summary>
        /// Gets or sets the per-account amounts seeded from the URL account options, each optionally
        /// locked so the giver cannot change it. Empty when no URL account options apply.
        /// </summary>
        public List<UtilityPaymentEntryPresetAccountAmountBag> PresetAccountAmounts { get; set; }

        /// <summary>
        /// Gets or sets the HTML message shown when the URL specifies an invalid or unresolvable
        /// account. Null when every URL account is valid or no Invalid Account Message is configured.
        /// </summary>
        public string UrlInvalidAccountMessage { get; set; }

        /// <summary>
        /// Gets or sets the heading shown in the Contribution Information section header.
        /// </summary>
        public string ContributionSectionTitle { get; set; }

        /// <summary>
        /// Gets or sets the icon CSS class shown in the Contribution Information section header.
        /// </summary>
        public string ContributionSectionIcon { get; set; }

        /// <summary>
        /// Gets or sets the supporting text shown below the Contribution Information section title.
        /// </summary>
        public string ContributionSectionDescription { get; set; }

        /// <summary>
        /// Gets or sets the label shown on the control that lets the giver add another account.
        /// </summary>
        public string AddAccountButtonText { get; set; }

        /// <summary>
        /// Gets or sets the accounts the giver may add beyond the configured list, as a tree (nested
        /// under each parent when hierarchy grouping is on, flat roots otherwise). Populated only when
        /// additional accounts are allowed and a specific account list is configured.
        /// </summary>
        public List<TreeItemBag> AdditionalAccounts { get; set; }

        /// <summary>
        /// Gets or sets the resolved HTML of the Transaction Header Lava template, shown above the
        /// entry sections.
        /// </summary>
        public string TransactionHeaderHtml { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the scheduling ("how often") fields are shown. True
        /// only when scheduled gifts are allowed, the gateway supports a schedule, and Text-to-Give mode
        /// is off.
        /// </summary>
        public bool IsSchedulingShown { get; set; }

        /// <summary>
        /// Gets or sets the frequency options offered to the giver, always including One-Time. Each value
        /// is a transaction-frequency defined value Guid.
        /// </summary>
        public List<ListItemBag> FrequencyOptions { get; set; }

        /// <summary>
        /// Gets or sets the Guid of the One-Time frequency, used to detect a one-time gift (which hides
        /// the end date and changes the start-date label).
        /// </summary>
        public Guid? OneTimeFrequencyGuid { get; set; }

        /// <summary>
        /// Gets or sets the Guid of the frequency selected by default. One-Time unless the Frequency page
        /// parameter specifies another.
        /// </summary>
        public Guid? DefaultFrequencyGuid { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the frequency selection is locked (shown read-only).
        /// Set by the Frequency page parameter's "^false" editable flag.
        /// </summary>
        public bool IsFrequencyLocked { get; set; }

        /// <summary>
        /// Gets or sets the default start date for a scheduled gift, as an ISO date string. Today unless
        /// the StartDate page parameter specifies a later date.
        /// </summary>
        public string DefaultStartDate { get; set; }

        /// <summary>
        /// Gets or sets the earliest start date a recurring gift may use, as an ISO date string, resolved
        /// from the gateway. A recurring gift cannot start before this date (the gateway may already have run
        /// today's automated giving), so the start date is held forward to it.
        /// </summary>
        public string EarliestScheduledStartDate { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the start-date label reads "Next Gift". True when
        /// transferring an existing scheduled gift that has a next payment date; a transfer without one
        /// falls back to the "When" / "First Gift" label.
        /// </summary>
        public bool IsNextGiftLabelShown { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the giver may set an optional end date on a recurring
        /// gift.
        /// </summary>
        public bool IsScheduledEndDateAllowed { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the comment field is shown.
        /// </summary>
        public bool IsCommentShown { get; set; }

        /// <summary>
        /// Gets or sets the label shown on the comment field.
        /// </summary>
        public string CommentLabel { get; set; }

        /// <summary>
        /// Gets or sets the heading shown in the Contact Information section header.
        /// </summary>
        public string ContactSectionTitle { get; set; }

        /// <summary>
        /// Gets or sets the icon CSS class shown in the Contact Information section header.
        /// </summary>
        public string ContactSectionIcon { get; set; }

        /// <summary>
        /// Gets or sets the supporting text shown below the Contact Information section title.
        /// </summary>
        public string ContactSectionDescription { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the giver is prompted for an email address.
        /// </summary>
        public bool IsEmailPrompted { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the giver is prompted for a phone number.
        /// </summary>
        public bool IsPhonePrompted { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the SMS opt-in checkbox is shown (requires phone
        /// prompting).
        /// </summary>
        public bool IsSmsOptInShown { get; set; }

        /// <summary>
        /// Gets or sets the label shown on the SMS opt-in checkbox.
        /// </summary>
        public string SmsOptInLabel { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the Give As Business option is offered. True when
        /// business giving is enabled and the block is not in Text-to-Give mode.
        /// </summary>
        public bool IsBusinessGivingAllowed { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the giver individual is known: the impersonated target
        /// when present, otherwise the signed-in individual. When false and the giver gives as a business,
        /// the business-contact fields are shown to identify who is submitting.
        /// </summary>
        public bool IsGiverIndividualKnown { get; set; }

        /// <summary>
        /// Gets or sets the businesses the signed-in giver may give on behalf of. Empty when the giver has
        /// none on file or is not signed in, in which case only a new business is entered.
        /// </summary>
        public List<UtilityPaymentEntryBusinessBag> Businesses { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether Give As Business starts on. True only when transferring a
        /// scheduled gift that was authorized to one of the giver's businesses.
        /// </summary>
        public bool IsGivingAsBusinessDefault { get; set; }

        /// <summary>
        /// Gets or sets the Guid of the business selected by default. Set only when transferring a business
        /// scheduled gift, to preselect the business the schedule belongs to.
        /// </summary>
        public Guid? DefaultBusinessGuid { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the phone field is shown when giving as a business. Uses
        /// the raw prompt-for-phone setting, since the person-only unlisted-number flip does not apply.
        /// </summary>
        public bool IsBusinessPhonePrompted { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the SMS opt-in checkbox is shown when giving as a
        /// business.
        /// </summary>
        public bool IsBusinessSmsOptInShown { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the giver may choose to give anonymously.
        /// </summary>
        public bool IsAnonymousGivingAllowed { get; set; }

        /// <summary>
        /// Gets or sets the tooltip shown on the Give Anonymously checkbox.
        /// </summary>
        public string AnonymousGivingTooltip { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the first- and last-name entry fields are shown. True
        /// for a new or nameless individual; false shows the read-only current name instead.
        /// </summary>
        public bool IsNameEntryShown { get; set; }

        /// <summary>
        /// Gets or sets the individual's full name, shown read-only when name entry is hidden.
        /// </summary>
        public string CurrentPersonFullName { get; set; }

        /// <summary>
        /// Gets or sets the value prefilled into the first-name field from the resolved individual.
        /// </summary>
        public string FirstName { get; set; }

        /// <summary>
        /// Gets or sets the value prefilled into the last-name field from the resolved individual.
        /// </summary>
        public string LastName { get; set; }

        /// <summary>
        /// Gets or sets the value prefilled into the email field from the resolved individual.
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Gets or sets the value prefilled into the phone field from the resolved individual. Empty when
        /// no number is on file or the number is unlisted.
        /// </summary>
        public string Phone { get; set; }

        /// <summary>
        /// Gets or sets the country code prefilled into the phone field. The default country code when no
        /// number is prefilled.
        /// </summary>
        public string PhoneCountryCode { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the SMS opt-in checkbox starts checked, from the
        /// prefilled number's messaging-enabled flag.
        /// </summary>
        public bool IsSmsOptInChecked { get; set; }

        /// <summary>
        /// Gets or sets the address prefilled into the address control from the resolved individual's
        /// address of the configured type. Null when no address is on file.
        /// </summary>
        public AddressControlBag Address { get; set; }

        /// <summary>
        /// Gets or sets the heading shown in the Payment Information section header.
        /// </summary>
        public string PaymentSectionTitle { get; set; }

        /// <summary>
        /// Gets or sets the icon CSS class shown in the Payment Information section header.
        /// </summary>
        public string PaymentSectionIcon { get; set; }

        /// <summary>
        /// Gets or sets the supporting text shown below the Payment Information section title.
        /// </summary>
        public string PaymentSectionDescription { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the CAPTCHA is shown in the payment section.
        /// </summary>
        public bool IsCaptchaShown { get; set; }

        /// <summary>
        /// Gets or sets the hosted gateway control configuration (file URL and settings) used to render
        /// the card / ACH entry. Null when the configured gateway has no Obsidian hosted control.
        /// </summary>
        public GatewayControlBag GatewayControl { get; set; }

        /// <summary>
        /// Gets or sets the target individual's reusable saved payment methods offered on the payment
        /// step, each carrying the display name, description, and card image. Empty when the individual
        /// has none for the configured gateway and its allowed currency types.
        /// </summary>
        public List<SavedFinancialAccountListItemBag> SavedAccounts { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether a confirmation step is shown for the giver to review
        /// the gift before it is processed. When false, the gift is processed straight from the entry
        /// step.
        /// </summary>
        public bool IsConfirmationStepShown { get; set; }

        /// <summary>
        /// Gets or sets the heading shown in the confirmation review section header.
        /// </summary>
        public string ConfirmationSectionHeading { get; set; }

        /// <summary>
        /// Gets or sets the resolved HTML of the Confirmation Header Lava template, shown above the gift
        /// summary on the confirmation step.
        /// </summary>
        public string ConfirmationHeaderHtml { get; set; }

        /// <summary>
        /// Gets or sets the resolved HTML of the Confirmation Footer Lava template, shown below the gift
        /// summary on the confirmation step.
        /// </summary>
        public string ConfirmationFooterHtml { get; set; }

        /// <summary>
        /// Gets or sets the heading shown in the Save Payment Method section on the success step.
        /// </summary>
        public string SavePaymentMethodSectionHeading { get; set; }

        /// <summary>
        /// Gets or sets the icon CSS class shown in the Save Payment Method section header.
        /// </summary>
        public string SavePaymentMethodSectionIcon { get; set; }

        /// <summary>
        /// Gets or sets the supporting text shown below the Save Payment Method section heading.
        /// </summary>
        public string SavePaymentMethodSectionDescription { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier of the system communication used to confirm a new login when
        /// an anonymous giver saves a payment method, passed to the shared save-account control.
        /// </summary>
        public Guid? AccountConfirmationEmailTemplateGuid { get; set; }
    }
}
