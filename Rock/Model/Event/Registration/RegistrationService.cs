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
using System.Linq;

using Rock.Data;
using Rock.ViewModels.Blocks;
using Rock.Web.Cache;

namespace Rock.Model
{
    public partial class RegistrationService
    {
        /// <summary>
        /// Gets the payments.
        /// </summary>
        /// <param name="registrationId">The registration identifier.</param>
        /// <returns></returns>
        public IQueryable<FinancialTransactionDetail> GetPayments( int registrationId )
        {
            int registrationEntityTypeId = EntityTypeCache.Get( typeof( Rock.Model.Registration ) ).Id;
            return new FinancialTransactionDetailService( ( RockContext ) this.Context )
                .Queryable( "Transaction" )
                .Where( t =>
                    t.EntityTypeId == registrationEntityTypeId &&
                    t.EntityId == registrationId );
        }

        /// <summary>
        /// Gets the total payments.
        /// </summary>
        /// <param name="registrationId">The registration identifier.</param>
        /// <returns></returns>
        public decimal GetTotalPayments( int registrationId )
        {
            return GetPayments( registrationId )
                .Select( p => p.Amount ).DefaultIfEmpty()
                .Sum();
        }

        /// <summary>
        /// Gets the generic context about the registration.
        /// </summary>
        /// <param name="registrationInstanceId">The registration instance identifier.</param>
        /// <param name="errorMessage">The error result.</param>
        /// <returns></returns>
        [RockObsolete("1.14.1")]
        [Obsolete( "Use GetRegistrationContext( int registrationInstanceId, int? registrationId, out string errorMessage )" )]
        public RegistrationContext GetRegistrationContext( int registrationInstanceId, out string errorMessage )
        {
            return GetRegistrationContext( registrationInstanceId, null, out errorMessage );
        }

        /// <summary>
        /// Gets the generic context about the registration.
        /// </summary>
        /// <param name="registrationInstanceId">The registration instance identifier.</param>
        /// <param name="registrationId">The registration identifier.</param>
        /// <param name="errorMessage">The error result.</param>
        /// <returns></returns>
        public RegistrationContext GetRegistrationContext( int registrationInstanceId, int? registrationId, out string errorMessage )
        {
            var rockContext = Context as RockContext;
            errorMessage = string.Empty;

            // Load the instance and template
            var registrationInstance = GetActiveRegistrationInstance( registrationInstanceId, registrationId, out errorMessage );
            var registrationTemplate = registrationInstance?.RegistrationTemplate;

            if ( registrationInstance == null || registrationTemplate == null )
            {
                // In this case, errorMessage will already contain the reason for it thanks to GetActiveRegistrationInstance
                return null;
            }

            // Validate that there are enough spots left for this registration
            var registrationInstanceService = new RegistrationInstanceService( rockContext );
            var context = new RegistrationContext
            {
                RegistrationSettings = new RegistrationSettings( registrationTemplate, registrationInstance ),
                Registration = null,
                Discount = null,
                SpotsRemaining = null
            };

            var spotsRemaining = registrationInstanceService.GetSpotsAvailable( context );
            context.SpotsRemaining = spotsRemaining;

            var feeItemCountRemaining = registrationInstanceService.GetFeeItemCountRemaining( context );
            context.FeeItemsCountRemaining = feeItemCountRemaining;

            return context;
        }

        /// <summary>
        /// Gets the context about the registration and performs initial validation
        /// of the data provided, such as making sure the discount code is valid.
        /// </summary>
        /// <param name="registrationInstanceId">The registration instance identifier.</param>
        /// <param name="registrationGuid">The registration unique identifier to load into the context for verification.</param>
        /// <param name="currentPerson">The current person.</param>
        /// <param name="discountCode">The discount code in use that should be verified.</param>
        /// <param name="errorMessage">The error result.</param>
        /// <returns></returns>
        public RegistrationContext GetRegistrationContext( int registrationInstanceId, Guid? registrationGuid, Person currentPerson, string discountCode, out string errorMessage )
        {
            var rockContext = Context as RockContext;
            Registration registration = null;
            if ( registrationGuid.HasValue )
            {
                var registrationService = new RegistrationService( rockContext );
                registration = registrationService.Get( registrationGuid.Value );
            }

            var context = GetRegistrationContext( registrationInstanceId, registration?.Id, out errorMessage );

            if ( !errorMessage.IsNullOrWhiteSpace() )
            {
                return null;
            }

            // Look up and validate the discount by the code unless the registration has already been saved with the discount
            if ( discountCode.IsNotNullOrWhiteSpace() )
            {
                if ( registration == null || registration.DiscountCode.IsNullOrWhiteSpace() )
                {
                    var registrationTemplateDiscountService = new RegistrationTemplateDiscountService( rockContext );

                    context.Discount = registrationTemplateDiscountService.GetDiscountByCodeIfValid( registrationInstanceId, discountCode );

                    if ( context.Discount == null )
                    {
                        errorMessage = "The discount code is not valid";
                        return null;
                    }
                }
                else
                {
                    var registrationDiscount = new RegistrationTemplateDiscountService( new RockContext() ).GetDiscountsForRegistrationInstance( registrationInstanceId ).Where( d => d.Code == discountCode ).FirstOrDefault();
                    if ( registrationDiscount != null )
                    {
                        context.Discount = new RegistrationTemplateDiscountWithUsage
                        {
                            RegistrationTemplateDiscount = registrationDiscount
                        };
                    }
                }
            }

            // Validate the registration
            if ( registration != null )
            {
                var registrationService = new RegistrationService( rockContext );

                context.Registration = registrationService.Get( registrationGuid.Value );

                if ( context.Registration == null )
                {
                    errorMessage = "The registration was not found";
                    return null;
                }

                // Verify this registration is for the same person and instance.
                if ( context.Registration != null )
                {
                    if ( context.Registration.PersonAliasId.HasValue && currentPerson?.Aliases.Any( a => a.Id == context.Registration.PersonAliasId.Value ) != true )
                    {
                        // This existing registration does not belong to this person
                        errorMessage = "Your existing registration was not found";
                        return null;
                    }
                    else if ( context.Registration.RegistrationInstanceId != registrationInstanceId )
                    {
                        // This existing registration is not for this instance
                        errorMessage = "Your existing registration was not found";
                        return null;
                    }
                }
            }

            return context;
        }

        /// <summary>
        /// Gets the active registration instance.
        /// </summary>
        /// <param name="registrationInstanceId">The registration instance identifier.</param>
        /// <param name="registrationId">The registration identifier.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <returns>RegistrationInstance.</returns>
        private RegistrationInstance GetActiveRegistrationInstance( int registrationInstanceId, int? registrationId, out string errorMessage )
        {
            errorMessage = string.Empty;

            var now = RockDateTime.Now;
            var registrationInstanceService = new RegistrationInstanceService( Context as RockContext );
            var registrationInstance = registrationInstanceService.Get( registrationInstanceId );
            var registrationTemplate = registrationInstance?.RegistrationTemplate;

            // Ensure that the registration entities are active
            if ( registrationInstance is null || registrationTemplate is null )
            {
                errorMessage = "We could not find the item you are looking for.";
                return null;
            }
            else if ( !registrationTemplate.IsActive || !registrationInstance.IsActive )
            {
                errorMessage = $"We could not find the {registrationTemplate.RegistrationTerm.ToLower()} you are looking for.";
                return null;
            }

            // Make sure the registration is open
            var isBeforeRegistrationOpens = registrationInstance.StartDateTime.HasValue && registrationInstance.StartDateTime > now;
            var isAfterRegistrationCloses = registrationInstance.EndDateTime.HasValue && registrationInstance.EndDateTime < now;
            bool isExistingRegistration = false;
            if ( registrationId.HasValue )
            {
                isExistingRegistration = new RegistrationService( Context as RockContext ).Get( registrationId.Value ) != null;
            }

            if ( !isExistingRegistration )
            {
                if ( isAfterRegistrationCloses )
                {
                    errorMessage = $"{registrationInstance.Name} closed on {registrationInstance.EndDateTime.ToShortDateString()}.";
                    return null;
                }
                else if ( isBeforeRegistrationOpens )
                {
                    errorMessage = $"{registrationTemplate.RegistrationTerm} for {registrationInstance.Name} does not open until {registrationInstance.StartDateTime.ToShortDateString()}.";
                    return null;
                }
            }

            return registrationInstance;
        }

        /// <summary>
        /// Gets the first name.
        /// </summary>
        /// <param name="settings">The settings.</param>
        /// <param name="registrantInfo">The registrant information.</param>
        /// <returns></returns>
        // TODO JMH Do we need to keep a RockObsolete overload with the previously named RegistrantInfo view model class?
        public string GetFirstName( RegistrationSettings settings, Rock.ViewModels.Blocks.Event.RegistrationEntry.RegistrantBag registrantInfo )
        {
            object value = GetPersonFieldValue( settings, registrantInfo, RegistrationPersonFieldType.FirstName );

            if ( value == null )
            {
                // if FirstName isn't prompted for in a registration form, and using an existing Person, get the person's FirstName/NickName from the database
                if ( registrantInfo.PersonGuid.HasValue )
                {
                    return new PersonService( new RockContext() ).GetSelect( registrantInfo.PersonGuid.Value, s => s.NickName ) ?? string.Empty;
                }
            }
            else
            {
                return value.ToString();
            }

            return string.Empty;
        }

        /// <summary>
        /// Gets the last name.
        /// </summary>
        /// <param name="settings">The settings.</param>
        /// <param name="registrantInfo">The registrant information.</param>
        /// <returns></returns>
        public string GetLastName( RegistrationSettings settings, Rock.ViewModels.Blocks.Event.RegistrationEntry.RegistrantBag registrantInfo )
        {
            object value = GetPersonFieldValue( settings, registrantInfo, RegistrationPersonFieldType.LastName );

            if ( value == null )
            {
                // if LastName isn't prompted for in a registration form, and using an existing Person, get the person's lastname from the database
                if ( registrantInfo.PersonGuid.HasValue )
                {
                    return new PersonService( new RockContext() ).GetSelect( registrantInfo.PersonGuid.Value, s => s.LastName ) ?? string.Empty;
                }
            }
            else
            {
                return value.ToString();
            }

            return string.Empty;
        }

        /// <summary>
        /// Gets the email.
        /// </summary>
        /// <param name="settings">The settings.</param>
        /// <param name="registrantInfo">The registrant information.</param>
        /// <returns></returns>
        public string GetEmail( RegistrationSettings settings, Rock.ViewModels.Blocks.Event.RegistrationEntry.RegistrantBag registrantInfo )
        {
            object value = GetPersonFieldValue( settings, registrantInfo, RegistrationPersonFieldType.Email );

            if ( value == null )
            {
                // if Email isn't prompted for in a registration form, and using an existing Person, get the person's email from the database
                if ( registrantInfo.PersonGuid.HasValue )
                {
                    return new PersonService( new RockContext() ).GetSelect( registrantInfo.PersonGuid.Value, s => s.Email ) ?? string.Empty;
                }
            }
            else
            {
                return value.ToString();
            }

            return string.Empty;
        }

        /// <summary>
        /// Gets a person field value.
        /// </summary>
        /// <param name="settings">The settings.</param>
        /// <param name="registrantInfo">The registrant information.</param>
        /// <param name="personFieldType">Type of the person field.</param>
        /// <returns></returns>
        public object GetPersonFieldValue( RegistrationSettings settings, Rock.ViewModels.Blocks.Event.RegistrationEntry.RegistrantBag registrantInfo, RegistrationPersonFieldType personFieldType )
        {
            if ( settings != null && settings.Forms != null )
            {
                var fieldGuid = settings.Forms
                    .SelectMany( t => t.Fields
                        .Where( f =>
                            f.FieldSource == RegistrationFieldSource.PersonField &&
                            f.PersonFieldType == personFieldType )
                        .Select( f => f.Guid ) )
                    .FirstOrDefault();

                return registrantInfo.FieldValues.GetValueOrNull( fieldGuid );
            }

            return null;
        }
    }

    /// <summary>
    /// Context
    /// </summary>
    public sealed class RegistrationContext
    {
        /// <summary>
        /// Gets or sets the registration settings.
        /// </summary>
        /// <value>
        /// The registration configuration.
        /// </value>
        public RegistrationSettings RegistrationSettings { get; set; }

        /// <summary>
        /// Gets or sets the registration.
        /// </summary>
        /// <value>
        /// The registration.
        /// </value>
        public Registration Registration { get; set; }

        /// <summary>
        /// Gets or sets the discount.
        /// </summary>
        /// <value>
        /// The discount.
        /// </value>
        public RegistrationTemplateDiscountWithUsage Discount { get; set; }

        /// <summary>
        /// Gets or sets the spots remaining.
        /// </summary>
        /// <value>
        /// The spots remaining.
        /// </value>
        public int? SpotsRemaining { get; set; }

        /// <summary>
        /// Gets the fee items count remaining.
        /// </summary>
        /// <value>
        /// The fee items count remaining.
        /// </value>
        public Dictionary<Guid, int?> FeeItemsCountRemaining { get; set; }

        /// <summary>
        /// Gets or sets the transaction code of a recent (this current web session) payment.
        /// </summary>
        /// <value>
        /// The transaction code.
        /// </value>
        public string TransactionCode { get; set; }

        /// <summary>
        /// Gets or sets the gateway person identifier.
        /// </summary>
        /// <value>
        /// The gateway person identifier.
        /// </value>
        public string GatewayPersonIdentifier { get; set; }

        private readonly List<int> _personIdsRegisteredWithinThisSession = new List<int>();

        /// <summary>
        /// Gets the identifiers of the people who have been registered within a registration session.
        /// </summary>
        /// <value>
        /// The identifiers of the people who have been registered within a registration session.
        /// </value>
        public List<int> PersonIdsRegisteredWithinThisSession => _personIdsRegisteredWithinThisSession;
    }

    /// <summary>
    /// A combination of a registration template and a registration instance.
    /// </summary>
    public sealed class RegistrationSettings
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RegistrationSettings"/> class.
        /// </summary>
        /// <param name="template">The registration template.</param>
        /// <param name="instance">The registration instance.</param>
        public RegistrationSettings( RegistrationTemplate template, RegistrationInstance instance )
        {
            RegistrationTemplateId = template.Id;
            RegistrationInstanceId = instance.Id;

            // Cost related
            var setCostOnInstance = template.SetCostOnInstance == true;
            PerRegistrantCost = ( setCostOnInstance ? instance.Cost : template.Cost ) ?? 0;
            PerRegistrantMinInitialPayment = setCostOnInstance ? instance.MinimumInitialPayment : template.MinimumInitialPayment;
            PerRegistrantDefaultInitialPayment = setCostOnInstance ? instance.DefaultPayment : template.DefaultPayment;

            // Models
            Fees = template.Fees.ToList();
            Forms = template.Forms.ToList();
            Discounts = template.Discounts.ToList();

            // Simple properties
            MaxAttendees = instance.MaxAttendees;
            IsTimeoutEnabled = instance.TimeoutIsEnabled;
            TimeoutMinutes = instance.TimeoutIsEnabled ? instance.TimeoutLengthMinutes : null;
            TimeoutThreshold = instance.TimeoutIsEnabled ? instance.TimeoutThreshold : null;
            RegistrarOption = template.RegistrarOption;
            RegistrantsSameFamily = template.RegistrantsSameFamily;
            IsWaitListEnabled = template.WaitListEnabled;
            AreCurrentFamilyMembersShown = template.ShowCurrentFamilyMembers;
            MaxRegistrants = ( template.AllowMultipleRegistrants ? template.MaxRegistrants : 1 ) ?? instance.MaxAttendees;
            IsLoginRequired = template.LoginRequired;
            AllowExternalRegistrationUpdates = template.AllowExternalRegistrationUpdates;
            ShowSmsOptIn = template.ShowSmsOptIn;
            SmsOptInText = Rock.Web.SystemSettings.GetValue( Rock.SystemKey.SystemSetting.SMS_OPT_IN_MESSAGE_LABEL );

            // Workflow type ids
            WorkflowTypeIds = new List<int>();

            if ( template.RegistrationWorkflowTypeId.HasValue )
            {
                WorkflowTypeIds.Add( template.RegistrationWorkflowTypeId.Value );
            }

            if ( instance.RegistrationWorkflowTypeId.HasValue )
            {
                WorkflowTypeIds.Add( instance.RegistrationWorkflowTypeId.Value );
            }

            RegistrantWorkflowTypeId = template.RegistrantWorkflowTypeId;

            // Terms and text
            Instructions = instance.RegistrationInstructions.IsNullOrWhiteSpace() ? template.RegistrationInstructions : instance.RegistrationInstructions;
            FeeTerm = template.FeeTerm.IsNullOrWhiteSpace() ? "Fee" : template.FeeTerm;
            RegistrantTerm = template.RegistrantTerm.IsNullOrWhiteSpace() ? "Person" : template.RegistrantTerm;
            AttributeTitleStart = template.RegistrationAttributeTitleStart.IsNullOrWhiteSpace() ? "Registration Information" : template.RegistrationAttributeTitleStart;
            AttributeTitleEnd = template.RegistrationAttributeTitleEnd.IsNullOrWhiteSpace() ? "Registration Information" : template.RegistrationAttributeTitleEnd;
            RegistrationTerm = template.RegistrationTerm.IsNullOrWhiteSpace() ? "Registration" : template.RegistrationTerm;
            Name = instance.Name.IsNullOrWhiteSpace() ? template.Name : instance.Name;

            // Gateway related
            FinancialGatewayId = template.FinancialGatewayId;
            ExternalGatewayFundId = instance.ExternalGatewayFundId;
            ExternalGatewayMerchantId = instance.ExternalGatewayMerchantId;
            FinancialAccountId = instance.AccountId;
            BatchNamePrefix = template.BatchNamePrefix;

            // Payment plan
            IsPaymentPlanAllowed = template.IsPaymentPlanAllowed;
            PaymentDeadlineDate = instance.PaymentDeadlineDate;
            PaymentPlanFrequencyValueIds = template.PaymentPlanFrequencyValueIdsCollection.ToList();

            // Group placement
            GroupTypeId = template.GroupTypeId;
            GroupMemberRoleId = template.GroupMemberRoleId;
            GroupMemberStatus = template.GroupMemberStatus;

            // Signature Document
            if ( template.RequiredSignatureDocumentTemplate != null && template.RequiredSignatureDocumentTemplate.IsActive )
            {
                SignatureDocumentTemplateId = template.RequiredSignatureDocumentTemplateId;
                IsInlineSignatureRequired = template.RequiredSignatureDocumentTemplateId.HasValue && template.SignatureDocumentAction == SignatureDocumentAction.Embed;
                IsSignatureDrawn = template.RequiredSignatureDocumentTemplate.SignatureType == SignatureType.Drawn;
                SignatureDocumentTerm = template.RequiredSignatureDocumentTemplate?.DocumentTerm;
                SignatureDocumentTemplateName = template.RequiredSignatureDocumentTemplate?.Name;
            }
        }

        /// <summary>
        /// Gets the financial account identifier.
        /// </summary>
        /// <value>
        /// The financial account identifier.
        /// </value>
        public int? FinancialAccountId { get; private set; }

        /// <summary>
        /// Gets or sets the registration template identifier.
        /// </summary>
        /// <value>
        /// The registration template identifier.
        /// </value>
        public int RegistrationTemplateId { get; private set; }

        /// <summary>
        /// Gets or sets the registration instance identifier.
        /// </summary>
        /// <value>
        /// The registration instance identifier.
        /// </value>
        public int RegistrationInstanceId { get; private set; }

        /// <summary>
        /// Gets or sets the per registrant cost. This can be $0.
        /// </summary>
        /// <value>
        /// The per registrant cost.
        /// </value>
        public decimal PerRegistrantCost { get; private set; }

        /// <summary>
        /// Gets or sets the minimum initial payment. This can be $0 to require no payment initially.
        /// If null, then the full payment is required initially.
        /// </summary>
        /// <value>
        /// The minimum initial payment.
        /// </value>
        public decimal? PerRegistrantMinInitialPayment { get; private set; }

        /// <summary>
        /// Gets or sets the default initial payment per registrant.
        /// </summary>
        /// <value>
        /// The default initial payment per registrant.
        /// </value>
        public decimal? PerRegistrantDefaultInitialPayment { get; private set; }

        /// <summary>
        /// Gets or sets the forms.
        /// </summary>
        /// <value>
        /// The forms.
        /// </value>
        public List<RegistrationTemplateForm> Forms { get; private set; }

        /// <summary>
        /// Gets or sets the maximum attendees. If null, there is no cap.
        /// </summary>
        /// <value>
        /// The maximum attendees.
        /// </value>
        public int? MaxAttendees { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether [timeout is enabled].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [timeout is enabled]; otherwise, <c>false</c>.
        /// </value>
        public bool IsTimeoutEnabled { get; private set; }

        /// <summary>
        /// Gets or sets the timeout minutes.
        /// </summary>
        /// <value>
        /// The timeout minutes.
        /// </value>
        public int? TimeoutMinutes { get; private set; }

        /// <summary>
        /// Gets the timeout threshold. The max remaining spots for the timeout to show.
        /// </summary>
        /// <value>
        /// The timeout threshold.
        /// </value>
        public int? TimeoutThreshold { get; private set; }

        /// <summary>
        /// Gets or sets the fees.
        /// </summary>
        /// <value>
        /// The fees.
        /// </value>
        public List<RegistrationTemplateFee> Fees { get; private set; }

        /// <summary>
        /// Gets the registrar option.
        /// </summary>
        /// <value>
        /// The registrar option.
        /// </value>
        public RegistrarOption RegistrarOption { get; private set; }

        /// <summary>
        /// Gets or sets the registrants same family.
        /// </summary>
        /// <value>
        /// The registrants same family.
        /// </value>
        public RegistrantsSameFamily RegistrantsSameFamily { get; private set; }

        /// <summary>
        /// Gets a value indicating whether this instance is wait list enabled.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is wait list enabled; otherwise, <c>false</c>.
        /// </value>
        public bool IsWaitListEnabled { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the SMS opt-in checkbox should be displayed with a mobile phone number
        /// </summary>
        /// <value>
        ///   <c>true</c> if [show SMS opt in]; otherwise, <c>false</c>.
        /// </value>
        public bool ShowSmsOptIn { get; private set; }

        /// <summary>
        /// Gets the SMS opt in text.
        /// </summary>
        /// <value>
        /// The SMS opt in text.
        /// </value>
        public string SmsOptInText { get; private set; }

        /// <summary>
        /// Gets a value indicating whether [are current family members shown].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [are current family members shown]; otherwise, <c>false</c>.
        /// </value>
        public bool AreCurrentFamilyMembersShown { get; private set; }

        /// <summary>
        /// Gets the maximum registrants.
        /// </summary>
        /// <value>
        /// The maximum registrants.
        /// </value>
        public int? MaxRegistrants { get; private set; }

        /// <summary>
        /// Gets the discounts.
        /// </summary>
        /// <value>
        /// The discounts.
        /// </value>
        public List<RegistrationTemplateDiscount> Discounts { get; private set; }

        /// <summary>
        /// Gets the instructions.
        /// </summary>
        /// <value>
        /// The instructions.
        /// </value>
        public string Instructions { get; private set; }

        /// <summary>
        /// Gets the fee term.
        /// </summary>
        /// <value>
        /// The fee term.
        /// </value>
        public string FeeTerm { get; private set; }

        /// <summary>
        /// Gets the registrant term.
        /// </summary>
        /// <value>
        /// The registrant term.
        /// </value>
        public string RegistrantTerm { get; private set; }

        /// <summary>
        /// Gets the attribute title start.
        /// </summary>
        /// <value>
        /// The attribute title start.
        /// </value>
        public string AttributeTitleStart { get; private set; }

        /// <summary>
        /// Gets the attribute title end.
        /// </summary>
        /// <value>
        /// The attribute title end.
        /// </value>
        public string AttributeTitleEnd { get; private set; }

        /// <summary>
        /// Gets the registration term.
        /// </summary>
        /// <value>
        /// The registration term.
        /// </value>
        public string RegistrationTerm { get; private set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get; private set; }

        /// <summary>
        /// Gets or sets the financial gateway identifier.
        /// </summary>
        /// <value>
        /// The financial gateway identifier.
        /// </value>
        public int? FinancialGatewayId { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is log in required.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is log in required; otherwise, <c>false</c>.
        /// </value>
        public bool IsLoginRequired { get; private set; }

        /// <summary>
        /// Gets or sets the external gateway fund identifier.
        /// </summary>
        /// <value>
        /// The external gateway fund identifier.
        /// </value>
        public int? ExternalGatewayFundId { get; private set; }

        /// <summary>
        /// Gets or sets the external gateway merchant identifier.
        /// </summary>
        /// <value>
        /// The external gateway merchant identifier.
        /// </value>
        public int? ExternalGatewayMerchantId { get; private set; }

        /// <summary>
        /// Gets the batch name prefix.
        /// </summary>
        /// <value>
        /// The batch name prefix.
        /// </value>
        public string BatchNamePrefix { get; private set; }

        /// <summary>
        /// Gets value indicating whether registrants should be able to pay their registration costs in multiple, scheduled installments.
        /// </summary>
        /// <value>
        ///   <c>true</c> if registrants should be able to pay their registration costs in multiple, scheduled installments; otherwise, <c>false</c>.
        /// </value>
        public bool IsPaymentPlanAllowed { get; private set; }
        
        /// <summary>
        /// Gets the payment deadline date.
        /// </summary>
        /// <value>
        /// The payment deadline date.
        /// </value>
        public DateTime? PaymentDeadlineDate { get; private set; }

        /// <summary>
        /// Gets the collection of payment plan frequency value IDs from which a registrant can select.
        /// </summary>
        /// <value>
        /// The collection of payment plan frequency value IDs from which a registrant can select.
        /// </value>
        public List<int> PaymentPlanFrequencyValueIds { get; private set; }

        /// <summary>
        /// Gets the group type identifier.
        /// </summary>
        /// <value>
        /// The group type identifier.
        /// </value>
        public int? GroupTypeId { get; private set; }

        /// <summary>
        /// Gets the group member role identifier.
        /// </summary>
        /// <value>
        /// The group member role identifier.
        /// </value>
        public int? GroupMemberRoleId { get; private set; }

        /// <summary>
        /// Gets the group member status.
        /// </summary>
        /// <value>
        /// The group member status.
        /// </value>
        public GroupMemberStatus GroupMemberStatus { get; private set; }

        /// <summary>
        /// Optional workflow type to launch for registrant
        /// </summary>
        /// <value>
        /// The workflow type id.
        /// </value>
        public int? RegistrantWorkflowTypeId { get; private set; }

        /// <summary>
        /// Gets or sets the registration workflow type identifier.
        /// </summary>
        /// <value>
        /// The registration workflow type identifier.
        /// </value>
        public List<int> WorkflowTypeIds { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether [allow registration updates].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [allow registration updates]; otherwise, <c>false</c>.
        /// </value>
        public bool AllowExternalRegistrationUpdates { get; private set; }

        /// <summary>
        /// Gets the <see cref="SignatureDocumentTemplate"/> identifier that
        /// must be signed for each registrant.
        /// </summary>
        /// <value>
        /// Gets the <see cref="SignatureDocumentTemplate"/> identifier.
        /// </value>
        public int? SignatureDocumentTemplateId { get; private set; }

        /// <summary>
        /// Gets a value indicating whether this registration requires the
        /// signature document to be signed inline.
        /// </summary>
        /// <value>
        /// <c>true</c> if this registration requires inline signing; otherwise, <c>false</c>.
        /// </value>
        public bool IsInlineSignatureRequired { get; private set; }

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
        public string SignatureDocumentTerm { get; private set; }

        /// <summary>
        /// Gets the name of the signature document template.
        /// </summary>
        /// <value>
        /// The name of the signature document template.
        /// </value>
        public string SignatureDocumentTemplateName { get; private set; }
    }
}