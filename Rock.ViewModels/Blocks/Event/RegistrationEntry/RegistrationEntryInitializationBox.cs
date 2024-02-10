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

namespace Rock.ViewModels.Blocks.Event.RegistrationEntry
{
    /// <summary>
    /// RegistrationEntryBlockViewModel
    /// </summary>
    public sealed class RegistrationEntryInitializationBox
    {
        /// <summary>
        /// Gets or sets the current person family unique identifier.
        /// </summary>
        /// <value>The current person family unique identifier.</value>
        public Guid? CurrentPersonFamilyGuid { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [allow registration updates].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [allow registration updates]; otherwise, <c>false</c>.
        /// </value>
        public bool AllowRegistrationUpdates { get; set; }

        /// <summary>
        /// Gets or sets the timeout minutes.
        /// </summary>
        /// <value>
        /// The timeout minutes.
        /// </value>
        public int? TimeoutMinutes { get; set; }

        /// <summary>
        /// Gets or sets the registration entry block session. If the registration is existing, then these are the args sent
        /// to create it.
        /// </summary>
        /// <value>
        /// The registration entry block arguments.
        /// </value>
        public RegistrationEntrySessionBag Session { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is unauthorized.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is unauthorized; otherwise, <c>false</c>.
        /// </value>
        public bool IsUnauthorized { get; set; }

        /// <summary>
        /// Gets or sets the instructions HTML.
        /// </summary>
        /// <value>
        /// The instructions HTML.
        /// </value>
        public string InstructionsHtml { get; set; }

        /// <summary>
        /// Gets or sets the registrant term.
        /// </summary>
        /// <value>
        /// The registrant term.
        /// </value>
        public string RegistrantTerm { get; set; }

        /// <summary>
        /// Gets or sets the registrant plural term.
        /// </summary>
        /// <value>
        /// The registrant plural term.
        /// </value>
        public string PluralRegistrantTerm { get; set; }

        /// <summary>
        /// Gets or sets the plural fee term.
        /// </summary>
        /// <value>
        /// The plural fee term.
        /// </value>
        public string PluralFeeTerm { get; set; }

        /// <summary>
        /// Gets or sets the registrant forms.
        /// </summary>
        /// <value>
        /// The registrant forms.
        /// </value>
        public List<RegistrationEntryFormBag> RegistrantForms { get; set; }

        /// <summary>
        /// Gets or sets the fees.
        /// </summary>
        /// <value>
        /// The fees.
        /// </value>
        public List<RegistrationEntryFeeBag> Fees { get; set; }

        /// <summary>
        /// Gets or sets the family members.
        /// </summary>
        /// <value>
        /// The family members.
        /// </value>
        public List<RegistrationEntryFamilyMemberBag> FamilyMembers { get; set; }

        /// <summary>
        /// Gets or sets the registration attribute title end.
        /// </summary>
        /// <value>
        /// The registration attribute title end.
        /// </value>
        public string RegistrationAttributeTitleEnd { get; set; }

        /// <summary>
        /// Gets or sets the registration attribute title start.
        /// </summary>
        /// <value>
        /// The registration attribute title start.
        /// </value>
        public string RegistrationAttributeTitleStart { get; set; }

        /// <summary>
        /// Gets or sets the registration attributes start.
        /// </summary>
        /// <value>
        /// The registration attributes start.
        /// </value>
        public List<PublicAttributeBag> RegistrationAttributesStart { get; set; }

        /// <summary>
        /// Gets or sets the registration attributes end.
        /// </summary>
        /// <value>
        /// The registration attributes end.
        /// </value>
        public List<PublicAttributeBag> RegistrationAttributesEnd { get; set; }

        /// <summary>
        /// Gets or sets the maximum registrants.
        /// </summary>
        /// <value>
        /// The maximum registrants.
        /// </value>
        public int MaxRegistrants { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [do ask for family].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [do ask for family]; otherwise, <c>false</c>.
        /// </value>
        public int RegistrantsSameFamily { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [force email update].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [force email update]; otherwise, <c>false</c>.
        /// </value>
        public bool ForceEmailUpdate { get; set; }

        /// <summary>
        /// Gets or sets the registrar option.
        /// </summary>
        /// <value>
        /// The registrar option.
        /// </value>
        public int RegistrarOption { get; set; }

        /// <summary>
        /// Gets or sets the cost.
        /// </summary>
        /// <value>
        /// The cost.
        /// </value>
        public decimal Cost { get; set; }

        /// <summary>
        /// Gets or sets the gateway control.
        /// </summary>
        /// <value>
        /// The gateway control.
        /// </value>
        public GatewayControlBag GatewayControl { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is redirect gateway.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is redirect gateway; otherwise, <c>false</c>.
        /// </value>
        public bool IsRedirectGateway { get; set; }

        /// <summary>
        /// Gets or sets the registration term.
        /// </summary>
        /// <value>
        /// The registration term.
        /// </value>
        public string RegistrationTerm { get; set; }

        /// <summary>
        /// Gets or sets the spots remaining.
        /// </summary>
        /// <value>
        /// The spots remaining.
        /// </value>
        public int? SpotsRemaining { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [wait list enabled].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [wait list enabled]; otherwise, <c>false</c>.
        /// </value>
        public bool WaitListEnabled { get; set; }

        /// <summary>
        /// Gets or sets the name of the instance.
        /// </summary>
        /// <value>
        /// The name of the instance.
        /// </value>
        public string InstanceName { get; set; }

        /// <summary>
        /// Gets or sets the plural registration term.
        /// </summary>
        /// <value>
        /// The plural registration term.
        /// </value>
        public string PluralRegistrationTerm { get; set; }

        /// <summary>
        /// Gets or sets the amount due today.
        /// </summary>
        /// <value>
        /// The amount due today.
        /// </value>
        public decimal? AmountDueToday { get; set; }

        /// <summary>
        /// Gets or sets the initial amount to pay.
        /// </summary>
        /// <value>
        /// The initial amount to pay.
        /// </value>
        public decimal? InitialAmountToPay { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance has discounts available.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance has discounts available; otherwise, <c>false</c>.
        /// </value>
        public bool HasDiscountsAvailable { get; set; }

        /// <summary>
        /// Gets or sets the redirect gateway URL.
        /// </summary>
        /// <value>
        /// The redirect gateway URL.
        /// </value>
        public string RedirectGatewayUrl { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [login required to register].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [login required to register]; otherwise, <c>false</c>.
        /// </value>
        public bool LoginRequiredToRegister { get; set; }

        /// <summary>
        /// Gets or sets the success view model.
        /// </summary>
        /// <value>
        /// The success view model.
        /// </value>
        public RegistrationEntrySuccessBag SuccessViewModel { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [start at beginning].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [start at beginning]; otherwise, <c>false</c>.
        /// </value>
        public bool StartAtBeginning { get; set; }

        /// <summary>
        /// Gets a value indicating whether or not this is an existing registration.
        /// </summary>
        /// <value>
        /// The value indicating whether or not this is an existing registration.
        /// </value>
        public bool IsExistingRegistration { get; set; }

        /// <summary>
        /// Gets or sets the gateway unique identifier.
        /// </summary>
        /// <value>
        /// The gateway unique identifier.
        /// </value>
        public Guid? GatewayGuid { get; set; }

        /// <summary>
        /// Gets or sets the campuses available for the user to select.
        /// </summary>
        /// <value>
        /// The campuses available for the user to select.
        /// </value>
        public List<ListItemBag> Campuses { get; set; }

        /// <summary>
        /// Gets or sets the marital statuses available for the user to select.
        /// </summary>
        /// <value>
        /// The marital statuses available for the user to select.
        /// </value>
        public List<ListItemBag> MaritalStatuses { get; set; }

        /// <summary>
        /// Gets or sets the connection statuses available for the user to select.
        /// </summary>
        /// <value>
        /// The connection statuses available for the user to select.
        /// </value>
        public List<ListItemBag> ConnectionStatuses { get; set; }

        /// <summary>
        /// Gets or sets the grades available for the user to select.
        /// </summary>
        /// <value>
        /// The grades available for the user to select.
        /// </value>
        public List<ListItemBag> Grades { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the save account option should be available.
        /// </summary>
        /// <value>
        /// <c>true</c> if the save account option should be available; otherwise, <c>false</c>.
        /// </value>
        public bool EnableSaveAccount { get; set; }

        /// <summary>
        /// Gets or sets the saved accounts that can be offered to use for payment.
        /// </summary>
        /// <value>
        /// The saved accounts that can be offered to use for payment.
        /// </value>
        public List<SavedFinancialAccountListItemBag> SavedAccounts { get; set; }

        /// <summary>
        /// Gets or sets a message to show when the registration instance cannot be used,
        /// either because it cannot be found, or is outside the time you're allowed to register.
        /// </summary>
        /// <value>
        /// A message to show to the indiviual when the registration instance cannot be found.
        /// </value>
        public string RegistrationInstanceNotFoundMessage { get; set; }

        /// <summary>
        /// Gets a value indicating whether this registration requires the
        /// signature document to be signed inline.
        /// </summary>
        /// <value>
        /// <c>true</c> if this registration requires inline signing; otherwise, <c>false</c>.
        /// </value>
        public bool IsInlineSignatureRequired { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the signature should be drawn.
        /// </summary>
        /// <value>
        /// <c>true</c> if this the signature is drawn; otherwise, <c>false</c>.
        /// </value>
        public bool IsSignatureDrawn { get; set; }

        /// <summary>
        /// Gets the signature document term.
        /// </summary>
        /// <value>
        /// The signature document term.
        /// </value>
        public string SignatureDocumentTerm { get; set; }

        /// <summary>
        /// Gets the name of the signature document template.
        /// </summary>
        /// <value>
        /// The name of the signature document template.
        /// </value>
        public string SignatureDocumentTemplateName { get; set; }

        /// <summary>
        /// Gets or sets the races available for the user to select.
        /// </summary>
        /// <value>
        /// The races available for the user to select.
        /// </value>
        public List<ListItemBag> Races { get; set; }

        /// <summary>
        /// Gets or sets the ethnicities available for the user to select.
        /// </summary>
        /// <value>
        /// The ethnicities available for the user to select.
        /// </value>
        public List<ListItemBag> Ethnicities { get; set; }

        /// <summary>
        /// Gets or sets a value that indicates if the progress bar should be hidden.
        /// </summary>
        /// <value>
        /// <c>true</c> if the progress bar should be hidden; otherwise <c>false</c>.
        /// </value>
        public bool HideProgressBar { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to show the SMS OptIn checkbox under the phone number
        /// </summary>
        /// <value>
        ///   <c>true</c> if [show SMS opt in]; otherwise, <c>false</c>.
        /// </value>
        public bool ShowSmsOptIn { get; set; }

        /// <summary>
        /// Gets value indicating whether registrants should be able to pay their registration costs in multiple, scheduled installments.
        /// </summary>
        /// <value>
        ///   <c>true</c> if registrants should be able to pay their registration costs in multiple, scheduled installments; otherwise, <c>false</c>.
        /// </value>
        public bool IsPaymentPlanAllowed { get; set; }
        
        /// <summary>
        /// Gets the payment deadline date.
        /// </summary>
        /// <value>
        /// The payment deadline date.
        /// </value>
        public DateTimeOffset? PaymentDeadlineDate { get; set; }

        /// <summary>
        /// Gets the collection of payment plan frequencies from which a registrant can select.
        /// </summary>
        /// <value>
        /// The collection of payment plan frequencies from which a registrant can select.
        /// </value>
        public List<ListItemBag> PaymentPlanFrequencies { get; set; }

        /// <summary>
        /// Gets or sets the currency information.
        /// </summary>
        /// <value>
        /// The currency information.
        /// </value>
        public CurrencyInfoBag CurrencyInfo { get; set; }
    }
}
