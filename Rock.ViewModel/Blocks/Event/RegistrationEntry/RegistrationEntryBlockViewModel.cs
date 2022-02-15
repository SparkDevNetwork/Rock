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
using Rock.ViewModel.Controls;
using Rock.ViewModel.NonEntities;

namespace Rock.ViewModel.Blocks.Event.RegistrationEntry
{
    /// <summary>
    /// RegistrationEntryBlockViewModel
    /// </summary>
    /// <seealso cref="Rock.ViewModel.IViewModel" />
    public sealed class RegistrationEntryBlockViewModel : IViewModel
    {
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
        public RegistrationEntryBlockSession Session { get; set; }

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
        public IEnumerable<RegistrationEntryBlockFormViewModel> RegistrantForms { get; set; }

        /// <summary>
        /// Gets or sets the fees.
        /// </summary>
        /// <value>
        /// The fees.
        /// </value>
        public IEnumerable<RegistrationEntryBlockFeeViewModel> Fees { get; set; }

        /// <summary>
        /// Gets or sets the family members.
        /// </summary>
        /// <value>
        /// The family members.
        /// </value>
        public IEnumerable<RegistrationEntryBlockFamilyMemberViewModel> FamilyMembers { get; set; }

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
        public IEnumerable<PublicEditableAttributeValueViewModel> RegistrationAttributesStart { get; set; }

        /// <summary>
        /// Gets or sets the registration attributes end.
        /// </summary>
        /// <value>
        /// The registration attributes end.
        /// </value>
        public IEnumerable<PublicEditableAttributeValueViewModel> RegistrationAttributesEnd { get; set; }

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
        public GatewayControlViewModel GatewayControl { get; set; }

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
        public RegistrationEntryBlockSuccessViewModel SuccessViewModel { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [start at beginning].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [start at beginning]; otherwise, <c>false</c>.
        /// </value>
        public bool StartAtBeginning { get; set; }

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
        public List<ListItemViewModel> Campuses { get; set; }

        /// <summary>
        /// Gets or sets the marital statuses available for the user to select.
        /// </summary>
        /// <value>
        /// The marital statuses available for the user to select.
        /// </value>
        public List<ListItemViewModel> MaritalStatuses { get; set; }

        /// <summary>
        /// Gets or sets the connection statuses available for the user to select.
        /// </summary>
        /// <value>
        /// The connection statuses available for the user to select.
        /// </value>
        public List<ListItemViewModel> ConnectionStatuses { get; set; }

        /// <summary>
        /// Gets or sets the grades available for the user to select.
        /// </summary>
        /// <value>
        /// The grades available for the user to select.
        /// </value>
        public List<ListItemViewModel> Grades { get; set; }

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
        public List<SavedFinancialAccountListItemViewModel> SavedAccounts { get; set; }
    }

    /// <summary>
    /// RegistrationEntryBlockFamilyMemberViewModel
    /// </summary>
    public sealed class RegistrationEntryBlockFamilyMemberViewModel
    {
        /// <summary>
        /// Gets or sets the unique identifier.
        /// </summary>
        /// <value>
        /// The unique identifier.
        /// </value>
        public Guid Guid { get; set; }

        /// <summary>
        /// Gets or sets the family unique identifier.
        /// </summary>
        /// <value>
        /// The family unique identifier.
        /// </value>
        public Guid FamilyGuid { get; set; }

        /// <summary>
        /// Gets or sets the full name.
        /// </summary>
        /// <value>
        /// The full name.
        /// </value>
        public string FullName { get; set; }

        /// <summary>
        /// Gets or sets the field values.
        /// </summary>
        /// <value>
        /// The field values.
        /// </value>
        public IDictionary<Guid, object> FieldValues { get; set; }
    }

    /// <summary>
    /// RegistrationEntryBlockFeeViewModel
    /// </summary>
    public sealed class RegistrationEntryBlockFeeViewModel
    {
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier.
        /// </summary>
        /// <value>
        /// The unique identifier.
        /// </value>
        public Guid Guid { get; set; }

        /// <summary>
        /// Gets or sets the items.
        /// </summary>
        /// <value>
        /// The items.
        /// </value>
        public IEnumerable<RegistrationEntryBlockFeeItemViewModel> Items { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [allow multiple].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [allow multiple]; otherwise, <c>false</c>.
        /// </value>
        public bool AllowMultiple { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is required.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is required; otherwise, <c>false</c>.
        /// </value>
        public bool IsRequired { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [discount applies].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [discount applies]; otherwise, <c>false</c>.
        /// </value>
        public bool DiscountApplies { get; set; }
    }

    /// <summary>
    /// RegistrationEntryBlockFeeItemViewModel
    /// </summary>
    public sealed class RegistrationEntryBlockFeeItemViewModel
    {
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the cost.
        /// </summary>
        /// <value>
        /// The cost.
        /// </value>
        public decimal Cost { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier.
        /// </summary>
        /// <value>
        /// The unique identifier.
        /// </value>
        public Guid Guid { get; set; }

        /// <summary>
        /// Gets or sets the count remaining.
        /// </summary>
        /// <value>
        /// The count remaining.
        /// </value>
        public int? CountRemaining { get; set; }
    }

    /// <summary>
    /// RegistrationEntryBlockFormViewModel
    /// </summary>
    public sealed class RegistrationEntryBlockFormViewModel
    {
        /// <summary>
        /// Gets or sets the fields.
        /// </summary>
        /// <value>
        /// The fields.
        /// </value>
        public IEnumerable<RegistrationEntryBlockFormFieldViewModel> Fields { get; set; }
    }

    /// <summary>
    /// RegistrationEntryBlockFormFieldViewModel
    /// </summary>
    public sealed class RegistrationEntryBlockFormFieldViewModel
    {
        /// <summary>
        /// Gets or sets the unique identifier.
        /// </summary>
        /// <value>
        /// The unique identifier.
        /// </value>
        public Guid Guid { get; set; }

        /// <summary>
        /// Gets or sets the field source.
        /// </summary>
        /// <value>
        /// The field source.
        /// </value>
        public int FieldSource { get; set; }

        /// <summary>
        /// Gets or sets the type of the person field.
        /// </summary>
        /// <value>
        /// The type of the person field.
        /// </value>
        public int PersonFieldType { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is required.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is required; otherwise, <c>false</c>.
        /// </value>
        public bool IsRequired { get; set; }

        /// <summary>
        /// Gets or sets the attribute.
        /// </summary>
        /// <value>
        /// The attribute.
        /// </value>
        public PublicEditableAttributeValueViewModel Attribute { get; set; }

        /// <summary>
        /// Gets or sets the type of the visibility rule.
        /// </summary>
        /// <value>
        /// The type of the visibility rule.
        /// </value>
        public int VisibilityRuleType { get; set; }

        /// <summary>
        /// Gets or sets the visibility rules.
        /// </summary>
        /// <value>
        /// The visibility rules.
        /// </value>
        public IEnumerable<RegistrationEntryBlockVisibilityViewModel> VisibilityRules { get; set; }

        /// <summary>
        /// Gets or sets the pre HTML.
        /// </summary>
        /// <value>
        /// The pre HTML.
        /// </value>
        public string PreHtml { get; set; }

        /// <summary>
        /// Gets or sets the post HTML.
        /// </summary>
        /// <value>
        /// The post HTML.
        /// </value>
        public string PostHtml { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [show on wait list].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [show on wait list]; otherwise, <c>false</c>.
        /// </value>
        public bool ShowOnWaitList { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is shared value.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is shared value; otherwise, <c>false</c>.
        /// </value>
        public bool IsSharedValue { get; set; }
    }

    /// <summary>
    /// RegistrationEntryBlockVisibilityViewModel
    /// </summary>
    public sealed class RegistrationEntryBlockVisibilityViewModel
    {
        /// <summary>
        /// Gets or sets the compared to registration template form field unique identifier.
        /// </summary>
        /// <value>
        /// The compared to registration template form field unique identifier.
        /// </value>
        public Guid ComparedToRegistrationTemplateFormFieldGuid { get; set; }

        /// <summary>
        /// Gets or sets the type of the comparison.
        /// </summary>
        /// <value>
        /// The type of the comparison.
        /// </value>
        public int ComparisonType { get; set; }

        /// <summary>
        /// Gets or sets the compared to value.
        /// </summary>
        /// <value>
        /// The compared to value.
        /// </value>
        public string ComparedToValue { get; set; }
    }

    /// <summary>
    /// RegistrationEntryBlockLineItemViewModel
    /// </summary>
    public sealed class RegistrationEntryBlockLineItemViewModel
    {
        /// <summary>
        /// Gets or sets the key.
        /// </summary>
        /// <value>
        /// The key.
        /// </value>
        public Guid Key { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is fee.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is fee; otherwise, <c>false</c>.
        /// </value>
        public bool IsFee { get; set; }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        /// <value>
        /// The description.
        /// </value>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the amount.
        /// </summary>
        /// <value>
        /// The amount.
        /// </value>
        public decimal Amount { get; set; }

        /// <summary>
        /// Gets or sets the discounted amount.
        /// </summary>
        /// <value>
        /// The discounted amount.
        /// </value>
        public decimal DiscountedAmount { get; set; }

        /// <summary>
        /// Gets or sets the discount help.
        /// </summary>
        /// <value>
        /// The discount help.
        /// </value>
        public string DiscountHelp { get; set; }
    }
}
