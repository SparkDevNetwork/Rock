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

using Rock.Model;
using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Event.RegistrationTemplateDetail
{
    /// <summary>
    /// The item details for the Registration Template Detail block.
    /// </summary>
    public class RegistrationTemplateBag : EntityBagBase
    {
        #region General

        /// <summary>
        /// Gets or sets the name of the registration template.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the description of the registration template.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the registration template is active.
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Gets or sets the category the registration template belongs to.
        /// </summary>
        public ListItemBag Category { get; set; }

        /// <summary>
        /// Gets or sets the name of the category. Used as a label in view mode.
        /// </summary>
        public string CategoryName { get; set; }

        #endregion General

        #region Group

        /// <summary>
        /// Gets or sets the group type that registrants will be added to.
        /// </summary>
        public ListItemBag GroupType { get; set; }

        /// <summary>
        /// Gets or sets the group member role that new registrants are added to the group with.
        /// </summary>
        public ListItemBag GroupMemberRole { get; set; }

        /// <summary>
        /// Gets or sets the group member status that new registrants are added to the group with.
        /// </summary>
        public GroupMemberStatus GroupMemberStatus { get; set; }

        /// <summary>
        /// Gets or sets the connection status to use for new individuals.
        /// </summary>
        public ListItemBag ConnectionStatus { get; set; }

        /// <summary>
        /// Gets or sets the record source to use for new individuals.
        /// </summary>
        public ListItemBag RecordSource { get; set; }

        #endregion Group

        #region Registrants

        /// <summary>
        /// Gets or sets a value indicating whether multiple registrants can be registered at the same time.
        /// </summary>
        public bool AllowMultipleRegistrants { get; set; }

        /// <summary>
        /// Gets or sets the maximum number of registrants that can be registered at one time.
        /// Only applies when multiple registrants are allowed.
        /// </summary>
        public int? MaxRegistrants { get; set; }

        /// <summary>
        /// Gets or sets the typical relationship of registrants that a person would register.
        /// </summary>
        public RegistrantsSameFamily RegistrantsSameFamily { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the person registering can select people from their family.
        /// </summary>
        public bool ShowCurrentFamilyMembers { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether a wait list is enabled once the maximum attendees has been reached.
        /// </summary>
        public bool WaitListEnabled { get; set; }

        /// <summary>
        /// Gets or sets how the registrar's information should be collected.
        /// </summary>
        public RegistrarOption RegistrarOption { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether an SMS opt-in checkbox is shown next to mobile phone numbers.
        /// </summary>
        public bool ShowSmsOptIn { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the same person record is prevented from registering more than once.
        /// </summary>
        public bool AreDuplicateRegistrantsPrevented { get; set; }

        #endregion Registrants

        #region Notifications

        /// <summary>
        /// Gets or sets who should be notified when new people are registered.
        /// </summary>
        public RegistrationNotify Notify { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether a note is added to the person's record when they register.
        /// </summary>
        public bool AddPersonNote { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the person must be logged in to register.
        /// </summary>
        public bool LoginRequired { get; set; }

        #endregion Notifications

        #region Payment

        /// <summary>
        /// Gets or sets a value indicating whether the cost is set on each registration instance
        /// instead of on the template.
        /// </summary>
        public bool IsSetCostOnInstance { get; set; }

        /// <summary>
        /// Gets or sets the financial gateway used to process registration payments.
        /// </summary>
        public ListItemBag FinancialGateway { get; set; }

        /// <summary>
        /// Gets or sets the optional prefix added to financial batches.
        /// </summary>
        public string BatchNamePrefix { get; set; }

        /// <summary>
        /// Gets or sets the cost per registrant.
        /// </summary>
        public decimal? Cost { get; set; }

        /// <summary>
        /// Gets or sets the minimum amount required per registrant.
        /// </summary>
        public decimal? MinimumInitialPayment { get; set; }

        /// <summary>
        /// Gets or sets the default payment amount per registrant.
        /// </summary>
        public decimal? DefaultPayment { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether registrants can pay in scheduled installments.
        /// </summary>
        public bool IsPaymentPlanAllowed { get; set; }

        /// <summary>
        /// Gets or sets the payment frequencies the registrant can select from. The value of each
        /// item is the unique identifier of the frequency defined value.
        /// </summary>
        public List<ListItemBag> PaymentPlanFrequencies { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether registrants must either pay in full or set up a payment plan.
        /// </summary>
        public bool IsFullPaymentOrPaymentPlanRequired { get; set; }

        /// <summary>
        /// Gets or sets the message displayed when a registration cannot be saved because it is
        /// not paid in full and does not have a valid payment plan.
        /// </summary>
        public string FullPaymentOrPaymentPlanRequiredMessage { get; set; }

        /// <summary>
        /// Gets or sets the capabilities of the selected financial gateway. Only populated in edit mode.
        /// </summary>
        public RegistrationTemplateGatewayFeaturesBag GatewayFeatures { get; set; }

        #endregion Payment

        #region Workflows and Documents

        /// <summary>
        /// Gets or sets the workflow type launched when a new registration is completed.
        /// </summary>
        public ListItemBag RegistrationWorkflowType { get; set; }

        /// <summary>
        /// Gets or sets the workflow type launched for each registrant when a new registration is completed.
        /// </summary>
        public ListItemBag RegistrantWorkflowType { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether saved registrations can be updated online.
        /// </summary>
        public bool AllowExternalRegistrationUpdates { get; set; }

        /// <summary>
        /// Gets or sets the signature document template that must be signed for registrations of this template.
        /// </summary>
        public ListItemBag RequiredSignatureDocumentTemplate { get; set; }

        #endregion Workflows and Documents

        #region Registrant Eligibility

        /// <summary>
        /// Gets or sets the minimum age a registrant must be to register.
        /// </summary>
        public decimal? EligibilityMinimumAge { get; set; }

        /// <summary>
        /// Gets or sets the maximum age a registrant can be to register.
        /// </summary>
        public decimal? EligibilityMaximumAge { get; set; }

        /// <summary>
        /// Gets or sets the age classification a registrant must match to register.
        /// </summary>
        public AgeClassification? EligibilityAgeClassification { get; set; }

        /// <summary>
        /// Gets or sets the minimum grade offset a registrant must have to register.
        /// </summary>
        public int? EligibilityMinimumGradeOffset { get; set; }

        /// <summary>
        /// Gets or sets the maximum grade offset a registrant can have to register.
        /// </summary>
        public int? EligibilityMaximumGradeOffset { get; set; }

        /// <summary>
        /// Gets or sets the gender a registrant must match to register.
        /// </summary>
        public Gender? EligibilityGender { get; set; }

        /// <summary>
        /// Gets or sets the person data view a registrant must be contained in to register.
        /// </summary>
        public ListItemBag EligibilityDataView { get; set; }

        #endregion Registrant Eligibility

        #region Terms and Text

        /// <summary>
        /// Gets or sets the term used to describe a registration.
        /// </summary>
        public string RegistrationTerm { get; set; }

        /// <summary>
        /// Gets or sets the term used to describe a registrant.
        /// </summary>
        public string RegistrantTerm { get; set; }

        /// <summary>
        /// Gets or sets the term used to describe the fees.
        /// </summary>
        public string FeeTerm { get; set; }

        /// <summary>
        /// Gets or sets the term used to describe a discount code.
        /// </summary>
        public string DiscountCodeTerm { get; set; }

        /// <summary>
        /// Gets or sets the section title for attributes collected at the start of the registration entry process.
        /// </summary>
        public string RegistrationAttributeTitleStart { get; set; }

        /// <summary>
        /// Gets or sets the section title for attributes collected at the end of the registration entry process.
        /// </summary>
        public string RegistrationAttributeTitleEnd { get; set; }

        /// <summary>
        /// Gets or sets the heading displayed after successfully completing a registration.
        /// </summary>
        public string SuccessTitle { get; set; }

        /// <summary>
        /// Gets or sets the Lava text displayed after successfully completing a registration.
        /// </summary>
        public string SuccessText { get; set; }

        /// <summary>
        /// Gets or sets the instructions displayed at the beginning of the registration process.
        /// </summary>
        public string RegistrationInstructions { get; set; }

        #endregion Terms and Text

        #region Communications

        /// <summary>
        /// Gets or sets the from name of the confirmation email.
        /// </summary>
        public string ConfirmationFromName { get; set; }

        /// <summary>
        /// Gets or sets the from email address of the confirmation email.
        /// </summary>
        public string ConfirmationFromEmail { get; set; }

        /// <summary>
        /// Gets or sets the subject of the confirmation email.
        /// </summary>
        public string ConfirmationSubject { get; set; }

        /// <summary>
        /// Gets or sets the Lava template of the confirmation email.
        /// </summary>
        public string ConfirmationEmailTemplate { get; set; }

        /// <summary>
        /// Gets or sets the from name of the reminder email.
        /// </summary>
        public string ReminderFromName { get; set; }

        /// <summary>
        /// Gets or sets the from email address of the reminder email.
        /// </summary>
        public string ReminderFromEmail { get; set; }

        /// <summary>
        /// Gets or sets the subject of the reminder email.
        /// </summary>
        public string ReminderSubject { get; set; }

        /// <summary>
        /// Gets or sets the Lava template of the reminder email.
        /// </summary>
        public string ReminderEmailTemplate { get; set; }

        /// <summary>
        /// Gets or sets the from name of the payment reminder email.
        /// </summary>
        public string PaymentReminderFromName { get; set; }

        /// <summary>
        /// Gets or sets the from email address of the payment reminder email.
        /// </summary>
        public string PaymentReminderFromEmail { get; set; }

        /// <summary>
        /// Gets or sets the subject of the payment reminder email.
        /// </summary>
        public string PaymentReminderSubject { get; set; }

        /// <summary>
        /// Gets or sets the Lava template of the payment reminder email.
        /// </summary>
        public string PaymentReminderEmailTemplate { get; set; }

        /// <summary>
        /// Gets or sets the number of days between automatic payment reminders.
        /// </summary>
        public int? PaymentReminderTimeSpan { get; set; }

        /// <summary>
        /// Gets or sets the from name of the wait list transition email.
        /// </summary>
        public string WaitListTransitionFromName { get; set; }

        /// <summary>
        /// Gets or sets the from email address of the wait list transition email.
        /// </summary>
        public string WaitListTransitionFromEmail { get; set; }

        /// <summary>
        /// Gets or sets the subject of the wait list transition email.
        /// </summary>
        public string WaitListTransitionSubject { get; set; }

        /// <summary>
        /// Gets or sets the Lava template of the wait list transition email.
        /// </summary>
        public string WaitListTransitionEmailTemplate { get; set; }

        #endregion Communications

        #region Child Collections

        /// <summary>
        /// Gets or sets the registrant forms and their fields, in display order.
        /// </summary>
        public List<RegistrationTemplateFormBag> Forms { get; set; }

        /// <summary>
        /// Gets or sets the attributes collected for each registration, in display order.
        /// </summary>
        public List<PublicEditableAttributeBag> RegistrationAttributes { get; set; }

        /// <summary>
        /// Gets or sets the fees, in display order.
        /// </summary>
        public List<RegistrationTemplateFeeBag> Fees { get; set; }

        /// <summary>
        /// Gets or sets the discount codes, in display order.
        /// </summary>
        public List<RegistrationTemplateDiscountBag> Discounts { get; set; }

        /// <summary>
        /// Gets or sets the placement configurations, in display order.
        /// </summary>
        public List<RegistrationTemplatePlacementBag> Placements { get; set; }

        #endregion Child Collections

        #region View Mode

        /// <summary>
        /// Gets or sets a value indicating whether any instance of the template has a
        /// completed registration. Used to show an additional warning before deleting.
        /// </summary>
        public bool HasRegistrations { get; set; }

        /// <summary>
        /// Gets or sets the group placement links displayed in view mode.
        /// </summary>
        public List<RegistrationTemplateGroupPlacementBag> GroupPlacements { get; set; }

        #endregion View Mode
    }
}
