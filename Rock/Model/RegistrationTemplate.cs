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
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;

using Newtonsoft.Json;

using Rock.Data;
using Rock.Security;

namespace Rock.Model
{
    /// <summary>
    /// 
    /// </summary>
    [RockDomain( "Event" )]
    [Table( "RegistrationTemplate" )]
    [DataContract]
    public partial class RegistrationTemplate : Model<RegistrationTemplate>, IHasActiveFlag, ICategorized
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets the name of the registration template
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        [Required]
        [MaxLength( 100 )]
        [DataMember( IsRequired = true )]
        [IncludeForReporting]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the category identifier.
        /// </summary>
        /// <value>
        /// The category identifier.
        /// </value>
        [DataMember]
        [IncludeForReporting]
        public int? CategoryId { get; set; }

        /// <summary>
        /// Gets or sets the group type that this registration template applies to
        /// </summary>
        /// <value>
        /// The group type identifier.
        /// </value>
        [DataMember]
        public int? GroupTypeId { get; set; }

        /// <summary>
        /// Gets or sets the group member role that registrants will be added to group as
        /// </summary>
        /// <value>
        /// The group member role identifier.
        /// </value>
        [DataMember]
        public int? GroupMemberRoleId { get; set; }

        /// <summary>
        /// Gets or sets the group member status that registrants will be added to group with.
        /// </summary>
        /// <value>
        /// The group member status.
        /// </value>
        [DataMember]
        public GroupMemberStatus GroupMemberStatus { get; set; }

        /// <summary>
        /// Gets or sets the notify.
        /// </summary>
        /// <value>
        /// The notify.
        /// </value>
        [DataMember]
        public RegistrationNotify Notify { get; set; }

        /// <summary>
        /// Gets or sets the term to use for fee
        /// </summary>
        /// <value>
        /// The fee term.
        /// </value>
        [DataMember]
        [MaxLength( 100 )]
        public string FeeTerm { get; set; }

        /// <summary>
        /// Gets or sets the term to use for registrant
        /// </summary>
        /// <value>
        /// The registrant term.
        /// </value>
        [DataMember]
        [MaxLength( 100 )]
        public string RegistrantTerm { get; set; }

        /// <summary>
        /// Gets or sets the term to use for registration
        /// </summary>
        /// <value>
        /// The registration term.
        /// </value>
        [DataMember]
        [MaxLength( 100 )]
        public string RegistrationTerm { get; set; }

        /// <summary>
        /// Gets or sets the term to use for discount code
        /// </summary>
        /// <value>
        /// The discount code term.
        /// </value>
        [DataMember]
        [MaxLength( 100 )]
        public string DiscountCodeTerm { get; set; }

        /// <summary>
        /// Gets or sets the section title for attributes that are collected at the start of the registration entry process.
        /// </summary>
        /// <value>
        /// The registration attribute title start.
        /// </value>
        [DataMember]
        [MaxLength( 200 )]
        public string RegistrationAttributeTitleStart { get; set; }

        /// <summary>
        /// Gets or sets the section title for attributes that are collected at the end of the registration entry process.
        /// </summary>
        /// <value>
        /// The registration attribute title end.
        /// </value>
        [DataMember]
        [MaxLength( 200 )]
        public string RegistrationAttributeTitleEnd { get; set; }

        /// <summary>
        /// Gets or sets the name of the confirmation from.
        /// </summary>
        /// <value>
        /// The name of the confirmation from.
        /// </value>
        [DataMember]
        [MaxLength( 200 )]
        public string ConfirmationFromName { get; set; }

        /// <summary>
        /// Gets or sets the confirmation from email.
        /// </summary>
        /// <value>
        /// The confirmation from email.
        /// </value>
        [DataMember]
        [MaxLength( 200 )]
        public string ConfirmationFromEmail { get; set; }

        /// <summary>
        /// Gets or sets the confirmation subject.
        /// </summary>
        /// <value>
        /// The confirmation subject.
        /// </value>
        [DataMember]
        [MaxLength( 200 )]
        public string ConfirmationSubject { get; set; }

        /// <summary>
        /// Gets or sets the confirmation email text to send.
        /// </summary>
        /// <value>
        /// The confirmation email template.
        /// </value>
        [DataMember]
        public string ConfirmationEmailTemplate { get; set; }

        /// <summary>
        /// Gets or sets the name of the reminder from.
        /// </summary>
        /// <value>
        /// The name of the reminder from.
        /// </value>
        [DataMember]
        [MaxLength( 200 )]
        public string ReminderFromName { get; set; }

        /// <summary>
        /// Gets or sets the reminder from email.
        /// </summary>
        /// <value>
        /// The reminder from email.
        /// </value>
        [DataMember]
        [MaxLength( 200 )]
        public string ReminderFromEmail { get; set; }

        /// <summary>
        /// Gets or sets the reminder subject.
        /// </summary>
        /// <value>
        /// The reminder subject.
        /// </value>
        [DataMember]
        [MaxLength( 200 )]
        public string ReminderSubject { get; set; }

        /// <summary>
        /// Gets or sets the reminder email template.
        /// </summary>
        /// <value>
        /// The reminder email template.
        /// </value>
        [DataMember]
        public string ReminderEmailTemplate { get; set; }

        /// <summary>
        /// Gets or sets the name of the wait list transition from.
        /// </summary>
        /// <value>
        /// The name of the wait list transition from.
        /// </value>
        [DataMember]
        [MaxLength( 200 )]
        public string WaitListTransitionFromName { get; set; }

        /// <summary>
        /// Gets or sets the wait list transition from email.
        /// </summary>
        /// <value>
        /// The wait list transition from email.
        /// </value>
        [DataMember]
        [MaxLength( 200 )]
        public string WaitListTransitionFromEmail { get; set; }

        /// <summary>
        /// Gets or sets the wait list transition subject.
        /// </summary>
        /// <value>
        /// The wait list transition subject.
        /// </value>
        [DataMember]
        [MaxLength( 200 )]
        public string WaitListTransitionSubject { get; set; }

        /// <summary>
        /// Gets or sets the wait list transition email template.
        /// </summary>
        /// <value>
        /// The wait list transition email template.
        /// </value>
        [DataMember]
        public string WaitListTransitionEmailTemplate { get; set; }

        /// <summary>
        /// Gets or sets the set cost on instance.
        /// </summary>
        /// <value>
        /// The set cost on instance.
        /// </value>
        [DataMember]
        public bool? SetCostOnInstance { get; set; }

        /// <summary>
        /// Gets or sets the cost (if <see cref="SetCostOnInstance"/> == false).
        /// </summary>
        /// <value>
        /// The cost.
        /// </value>
        [DataMember]
        public decimal Cost { get; set; }

        /// <summary>
        /// Gets or sets the minimum initial payment (if <see cref="SetCostOnInstance"/> == false).
        /// </summary>
        /// <value>
        /// The minimum initial payment.
        /// </value>
        [DataMember]
        public decimal? MinimumInitialPayment { get; set; }

        /// <summary>
        /// Gets or sets the default amount to pay per registrant (if <see cref="SetCostOnInstance"/> == false).
        /// If this is null, the default payment will be the <see cref="Cost"/>
        /// </summary>
        /// <value>
        /// The default payment.
        /// </value>
        [DataMember]
        public decimal? DefaultPayment { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [login required].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [login required]; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool LoginRequired { get; set; }

        /// <summary>
        /// Gets or sets flag indicating if registrants registered for this template are typically in same family. values are ( yes, no, ask ).
        /// </summary>
        /// <value>
        /// The registrants same family.
        /// </value>
        [DataMember]
        public RegistrantsSameFamily RegistrantsSameFamily { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [show current family members].
        /// </summary>
        /// <value>
        /// <c>true</c> if [show current family members]; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool ShowCurrentFamilyMembers { get; set; }

        /// <summary>
        /// Gets or sets the name of the request entry.
        /// </summary>
        /// <value>
        /// The name of the request entry.
        /// </value>
        [DataMember]
        public string RequestEntryName { get; set; }

        /// <summary>
        /// Gets or sets the registration instructions.
        /// </summary>
        /// <value>
        /// The registration instructions.
        /// </value>
        [DataMember]
        public string RegistrationInstructions { get; set; }

        /// <summary>
        /// Gets or sets the success title.
        /// </summary>
        /// <value>
        /// The success title.
        /// </value>
        [DataMember]
        public string SuccessTitle { get; set; }

        /// <summary>
        /// Gets or sets the success text.
        /// </summary>
        /// <value>
        /// The success text.
        /// </value>
        [DataMember]
        public string SuccessText { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [allow multiple registrants].
        /// </summary>
        /// <value>
        /// <c>true</c> if [allow multiple registrants]; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool AllowMultipleRegistrants { get; set; }

        /// <summary>
        /// Gets or sets the maximum registrants.
        /// </summary>
        /// <value>
        /// The maximum registrants.
        /// </value>
        [DataMember]
        public int MaxRegistrants { get; set; }

        /// <summary>
        /// Gets or sets the financial gateway identifier.
        /// </summary>
        /// <value>
        /// The financial gateway identifier.
        /// </value>
        [DataMember]
        public int? FinancialGatewayId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is active.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is active; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool IsActive
        {
            get { return _isActive; }
            set { _isActive = value; }
        }

        private bool _isActive = true;

        /// <summary>
        /// Gets or sets a value indicating whether a person note should be added when a person registers for this type of registration.
        /// </summary>
        /// <value>
        ///   <c>true</c> if [add person note]; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool AddPersonNote
        {
            get { return _addPersonNote; }
            set { _addPersonNote = value; }
        }

        private bool _addPersonNote = true;

        /// <summary>
        /// Gets or sets a value indicating whether [allow group placement].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [allow group placement]; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool AllowGroupPlacement { get; set; }

        /// <summary>
        /// Gets or sets the name of the payment reminder from.
        /// </summary>
        /// <value>
        /// The name of the payment reminder from.
        /// </value>
        [DataMember]
        [MaxLength( 200 )]
        public string PaymentReminderFromName { get; set; }

        /// <summary>
        /// Gets or sets the payment reminder from email.
        /// </summary>
        /// <value>
        /// The payment reminder from email.
        /// </value>
        [DataMember]
        [MaxLength( 200 )]
        public string PaymentReminderFromEmail { get; set; }

        /// <summary>
        /// Gets or sets the payment reminder subject.
        /// </summary>
        /// <value>
        /// The payment reminder subject.
        /// </value>
        [DataMember]
        [MaxLength( 200 )]
        public string PaymentReminderSubject { get; set; }

        /// <summary>
        /// Gets or sets the payment reminder email template.
        /// </summary>
        /// <value>
        /// The payment reminder email template.
        /// </value>
        [DataMember]
        public string PaymentReminderEmailTemplate { get; set; }

        /// <summary>
        /// Gets or sets the payment reminder time span in days.
        /// </summary>
        /// <value>
        /// The payment reminder time span in days.
        /// </value>
        [DataMember]
        public int? PaymentReminderTimeSpan { get; set; }

        /// <summary>
        /// Gets or sets the batch name prefix.
        /// </summary>
        /// <value>
        /// The batch name prefix.
        /// </value>
        [DataMember]
        public string BatchNamePrefix { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to allow external registration updates (should a person be able to update their registration on-line after submitting it).
        /// </summary>
        /// <value>
        /// <c>true</c> if [allow external registration updates]; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool AllowExternalRegistrationUpdates
        {
            get { return _allowExternalRegistrationUpdates; }
            set { _allowExternalRegistrationUpdates = value; }
        }

        private bool _allowExternalRegistrationUpdates = true;

        /// <summary>
        /// Optional workflow type to launch at end of registration
        /// </summary>
        /// <value>
        /// The workflow type id.
        /// </value>        
        [DataMember]
        public int? RegistrationWorkflowTypeId { get; set; }

        /// <summary>
        /// Gets or sets the required signature document type identifier.
        /// </summary>
        /// <value>
        /// The required signature document type identifier.
        /// </value>
        [DataMember]
        public int? RequiredSignatureDocumentTemplateId { get; set; }

        /// <summary>
        /// Gets or sets the signature documentation.
        /// </summary>
        /// <value>
        /// The signature documentation.
        /// </value>
        [DataMember]
        public SignatureDocumentAction SignatureDocumentAction { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether a wait list is enabled for this event template
        /// </summary>
        /// <value>
        ///   <c>true</c> if [wait list enabled]; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool WaitListEnabled { get; set; }

        /// <summary>
        /// Gets or sets the registrar option.
        /// </summary>
        /// <value>
        /// The registrar option.
        /// </value>
        [DataMember]
        public RegistrarOption RegistrarOption { get; set; }

        #endregion

        #region Virtual Properties

        /// <summary>
        /// Gets or sets the category.
        /// </summary>
        /// <value>
        /// The category.
        /// </value>
        [LavaInclude]
        public virtual Category Category { get; set; }

        /// <summary>
        /// Gets or sets the type of the group.
        /// </summary>
        /// <value>
        /// The type of the group.
        /// </value>
        [LavaInclude]
        public virtual GroupType GroupType { get; set; }

        /// <summary>
        /// Gets or sets the financial gateway.
        /// </summary>
        /// <value>
        /// The financial gateway.
        /// </value>
        [DataMember]
        public virtual FinancialGateway FinancialGateway { get; set; }

        /// <summary>
        /// Gets or sets the workflow type to launch at end of registration.
        /// </summary>
        /// <value>
        /// The Workflow Type.
        /// </value>
        [DataMember]
        public virtual WorkflowType RegistrationWorkflowType { get; set; }

        /// <summary>
        /// Gets or sets the type of the required signature document.
        /// </summary>
        /// <value>
        /// The type of the required signature document.
        /// </value>
        [DataMember]
        public virtual SignatureDocumentTemplate RequiredSignatureDocumentTemplate { get; set; }

        /// <summary>
        /// Gets or sets the discounts.
        /// </summary>
        /// <value>
        /// The discounts.
        /// </value>
        [DataMember]
        public virtual ICollection<RegistrationTemplateDiscount> Discounts
        {
            get { return _discounts ?? ( _discounts = new Collection<RegistrationTemplateDiscount>() ); }
            set { _discounts = value; }
        }

        private ICollection<RegistrationTemplateDiscount> _discounts;

        /// <summary>
        /// Gets or sets the fees.
        /// </summary>
        /// <value>
        /// The fees.
        /// </value>
        [DataMember]
        public virtual ICollection<RegistrationTemplateFee> Fees
        {
            get { return _fees ?? ( _fees = new Collection<RegistrationTemplateFee>() ); }
            set { _fees = value; }
        }

        private ICollection<RegistrationTemplateFee> _fees;

        /// <summary>
        /// Gets or sets the collection of the current page's child pages.
        /// </summary>
        /// <value>
        /// Collection of child pages
        /// </value>
        [LavaInclude]
        public virtual ICollection<RegistrationInstance> Instances
        {
            get { return _registrationInstances ?? ( _registrationInstances = new Collection<RegistrationInstance>() ); }
            set { _registrationInstances = value; }
        }

        private ICollection<RegistrationInstance> _registrationInstances;

        /// <summary>
        /// Gets or sets the forms.
        /// </summary>
        /// <value>
        /// The forms.
        /// </value>
        [DataMember]
        public virtual ICollection<RegistrationTemplateForm> Forms
        {
            get { return _registrationTemplateForms ?? ( _registrationTemplateForms = new Collection<RegistrationTemplateForm>() ); }
            set { _registrationTemplateForms = value; }
        }

        private ICollection<RegistrationTemplateForm> _registrationTemplateForms;

        /// <summary>
        /// A dictionary of actions that this class supports and the description of each.
        /// </summary>
        public override Dictionary<string, string> SupportedActions
        {
            get
            {
                if ( _supportedActions == null )
                {
                    _supportedActions = new Dictionary<string, string>();
                    _supportedActions.Add( Authorization.VIEW, "The roles and/or users that have access to view." );
                    _supportedActions.Add( "Register", "The roles and/or users that have access to add/edit/remove registrations and registrants." );
                    _supportedActions.Add( Authorization.EDIT, "The roles and/or users that have access to edit." );
                    _supportedActions.Add( Authorization.ADMINISTRATE, "The roles and/or users that have access to administrate." );
                }

                return _supportedActions;
            }
        }

        private Dictionary<string, string> _supportedActions;

        #endregion

        #region Methods

        /// <summary>
        /// Returns a <see cref="string"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="string"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return Name;
        }

        #endregion
    }

    #region Entity Configuration

    /// <summary>
    /// Configuration class.
    /// </summary>
    public partial class RegistrationTemplateConfiguration : EntityTypeConfiguration<RegistrationTemplate>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RegistrationTemplateConfiguration"/> class.
        /// </summary>
        public RegistrationTemplateConfiguration()
        {
            this.HasOptional( t => t.Category ).WithMany().HasForeignKey( t => t.CategoryId ).WillCascadeOnDelete( false );
            this.HasOptional( t => t.GroupType ).WithMany().HasForeignKey( t => t.GroupTypeId ).WillCascadeOnDelete( false );
            this.HasOptional( t => t.FinancialGateway ).WithMany().HasForeignKey( t => t.FinancialGatewayId ).WillCascadeOnDelete( false );
            this.HasOptional( t => t.RegistrationWorkflowType ).WithMany().HasForeignKey( t => t.RegistrationWorkflowTypeId ).WillCascadeOnDelete( false );
            this.HasOptional( t => t.RequiredSignatureDocumentTemplate ).WithMany().HasForeignKey( t => t.RequiredSignatureDocumentTemplateId ).WillCascadeOnDelete( false );
        }
    }

    #endregion

    #region Enumerations

    /// <summary>
    /// Flag indicating if registrants are typically in the same family
    /// </summary>
    public enum RegistrantsSameFamily
    {
        /// <summary>
        /// The no
        /// </summary>
        No = 0,

        /// <summary>
        /// The yes
        /// </summary>
        Yes = 1,

        /// <summary>
        /// The ask
        /// </summary>
        Ask = 2,
    }

    /// <summary>
    /// Flag indicating who should be notified on a new registration
    /// </summary>
    [Flags]
    public enum RegistrationNotify
    {
        /// <summary>
        /// The none
        /// </summary>
        None = 0,

        /// <summary>
        /// The registration contact
        /// </summary>
        RegistrationContact = 1,

        /// <summary>
        /// The group followers
        /// </summary>
        GroupFollowers = 2,

        /// <summary>
        /// The group leaders
        /// </summary>
        GroupLeaders = 4,

        /// <summary>
        /// All
        /// </summary>
        All = RegistrationContact | GroupFollowers | GroupLeaders
    }

    /// <summary>
    /// How signature document should be presented to registrant
    /// </summary>
    public enum SignatureDocumentAction
    {
        /// <summary>
        /// Email document
        /// </summary>
        Email = 0,

        /// <summary>
        /// Embed document in registration
        /// </summary>
        Embed = 1,
    }

    /// <summary>
    /// How registrar information should be collected.
    /// </summary>
    public enum RegistrarOption
    {
        /// <summary>
        /// Prompt for registrar
        /// </summary>
        PromptForRegistrar = 0,

        /// <summary>
        /// Prefill first registrant
        /// </summary>
        PrefillFirstRegistrant = 1,

        /// <summary>
        /// Use first registrant
        /// </summary>
        UseFirstRegistrant = 2,

        /// <summary>
        /// Use the LoggedIn person and keep fields readonly, except for fields that haven't been collected yet
        /// For example, if EmailAddress wasn't known, Email would be prompted vs readonly.
        /// </summary>
        UseLoggedInPerson = 3
    }

    #endregion
}
