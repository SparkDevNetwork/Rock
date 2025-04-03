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
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Data.Entity;
using System.Linq;

using Rock.Attribute;
using Rock.ClientService.Core.Campus;
using Rock.ClientService.Core.Campus.Options;
using Rock.ClientService.Finance.FinancialPersonSavedAccount;
using Rock.ClientService.Finance.FinancialPersonSavedAccount.Options;
using Rock.Cms.StructuredContent;
using Rock.Data;
using Rock.Enums.Workflow;
using Rock.Financial;
using Rock.Model;
using Rock.Net;
using Rock.ViewModels.Finance;
using Rock.ViewModels.Utility;
using Rock.ViewModels.Workflow;
using Rock.Web.Cache;

namespace Rock.Workflow.Action
{
    /// <summary>
    /// Take a payment from a person using a financial gateway.
    /// </summary>
    [ActionCategory( "Workflow Control" )]
    [Description( "Take a payment from a person using a financial gateway." )]
    [Export( typeof( ActionComponent ) )]
    [ExportMetadata( "ComponentName", "Payment Entry" )]

    #region Action Attributes

    [FinancialGatewayField( "Financial Gateway",
        Description = "Workflow attribute that indicates the automated financial gateway to use.",
        IsRequired = true,
        Key = AttributeKey.FinancialGateway,
        Order = 0 )]

    [WorkflowAttribute( "Authorized Person Attribute",
        Description = "Workflow attribute that contains the person making the payment. If left blank, the current person will be used.",
        FieldTypeClassNames = new[] { "Rock.Field.Types.PersonFieldType" },
        IsRequired = false,
        Key = AttributeKey.AuthorizedPersonAttribute,
        Order = 1 )]

    [WorkflowTextOrAttribute( "Amount", "Amount Attribute",
        Description = "A number or Workflow attribute (currency) that contains the amount to charge. <b>If not provided (or if the value is invalid), the individual will be asked to provide an amount.</b>",
        IsRequired = false,
        Key = AttributeKey.Amount,
        Order = 2 )]

    [WorkflowAttribute( "Account Attribute",
        Description = "Workflow attribute that contains the target financial account.",
        FieldTypeClassNames = new[] { "Rock.Field.Types.AccountFieldType" },
        IsRequired = true,
        Key = AttributeKey.AccountAttribute,
        Order = 3 )]

    [DefinedValueField( "Transaction Type",
        Description = "",
        IsRequired = true,
        DefinedTypeGuid = Rock.SystemGuid.DefinedType.FINANCIAL_TRANSACTION_TYPE,
        Key = AttributeKey.TransactionType,
        Order = 4 )]

    [DefinedValueField( "Transaction Source",
        Description = "",
        IsRequired = true,
        DefinedTypeGuid = Rock.SystemGuid.DefinedType.FINANCIAL_SOURCE_TYPE,
        Key = AttributeKey.TransactionSource,
        Order = 5 )]

    [StructureContentEditorField( "Payment Information Instructions",
        Description = "Instructions for the payment entry step. This will be displayed to the individual to ensure they understand why they're being asked to enter payment information. A \"PaymentConfiguration\" merge field will have these properties: Amount, Entity, AmountEntryLabel, TransactionType, and TransactionSummary. <span class='tip tip-lava'></span> <span class='tip tip-html'></span>",
        DefaultValue = "{\"time\":1743551229152,\"blocks\":[{\"id\":\"sr-B__Yw8P\",\"type\":\"paragraph\",\"data\":{\"text\":\"To continue, please complete the payment below.\"}},{\"id\":\"1UQagMqGLW\",\"type\":\"paragraph\",\"data\":{\"text\":\"<b>Amount</b>: {{ PaymentConfiguration.Amount | FormatAsCurrency }}\"}}],\"version\":\"2.28.0\"}",
        IsRequired = false,
        Key = AttributeKey.PaymentInformationInstructions,
        Order = 6 )]

    [StructureContentEditorField( "Success Message",
        Description = "The message to display upon successful payment, provided there are no subsequent interactive Workflow Actions (e.g. Form) following the Payment Entry action. If additional interactive actions exist, this message is bypassed, and control passes to the next action in the workflow. <span class='tip tip-lava'></span> <span class='tip tip-html'></span>",
        DefaultValue = "{\"time\":1743628289304,\"blocks\":[{\"id\":\"VFya6v3ba_\",\"type\":\"alert\",\"data\":{\"type\":\"success\",\"align\":\"left\",\"message\":\"Success! The amount of {{ TransactionDetail.Amount | FormatAsCurrency }} was applied to the {{ Account.PublicName }}.\"}}],\"version\":\"2.28.0\"}",
        IsRequired = true,
        Key = AttributeKey.SuccessMessage,
        Order = 7 )]

    [WorkflowAttribute( "Result Transaction Attribute",
        Description = "An optional attribute to set to the result transaction",
        IsRequired = false,
        FieldTypeClassNames = new string[] { "Rock.Field.Types.FinancialTransactionFieldType" },
        Key = AttributeKey.ResultTransactionAttribute,
        Order = 8 )]

    #region Campus

    [BooleanField( "Use Account Campus Mapping Logic",
        Description = "If enabled, the accounts will be determined as follows: <ul><li>If the selected account is not associated with a campus, the Selected Account will be the first matching active child account that is associated with the selected campus.</li><li>If the selected account is not associated with a campus, but there are no active child accounts for the selected campus, the parent account (the one the individual sees) will be returned.</li><li>If the selected account is associated with a campus, that account will be returned regardless of campus selection (and it won't use the child account logic)</li><ul>",
        DefaultBooleanValue = false,
        IsRequired = false,
        Key = AttributeKey.UseAccountCampusMappingLogic,
        Category = "Campus",
        Order = 0 )]

    [BooleanField( "Show Campus Picker",
        Description = "This is only used if the Campus Mapping Logic is enabled.",
        DefaultBooleanValue = false,
        IsRequired = false,
        Key = AttributeKey.ShowCampusPicker,
        Category = "Campus",
        Order = 1 )]

    [DefinedValueField( "Campus Types",
        Description = "Filters the displayed campuses by the selected types. If no types are selected, this filter will not limit the results.",
        IsRequired = false,
        AllowMultiple = true,
        DefinedTypeGuid = Rock.SystemGuid.DefinedType.CAMPUS_TYPE,
        Key = AttributeKey.CampusTypes,
        Category = "Campus",
        Order = 2 )]

    [DefinedValueField( "Campus Statuses",
        Description = "Filters the displayed campuses by the selected statuses. If no statuses are selected, this filter will not limit the results.",
        IsRequired = false,
        AllowMultiple = true,
        DefinedTypeGuid = Rock.SystemGuid.DefinedType.CAMPUS_STATUS,
        Key = AttributeKey.CampusStatuses,
        Category = "Campus",
        Order = 3 )]

    #endregion

    #region Captions

    [WorkflowTextOrAttribute( "Amount Entry Label", "Amount Entry Label Attribute",
        Description = "The label to use on the amount entry field.  ‘Amount’ will be used if none is configured.",
        IsRequired = false,
        Key = AttributeKey.AmountEntryLabel,
        Category = "Captions",
        Order = 0 )]

    [TextField( "Save Account Title",
        Description = "The text to display as heading of section for saving payment information.",
        IsRequired = true,
        DefaultValue = "Make Giving Even Easier",
        Key = AttributeKey.SaveAccountTitle,
        Category = "Captions",
        Order = 1 )]

    [WorkflowTextOrAttribute( "Confirm Payment Button Text", "Confirm Payment Button Text Attribute",
        Description = "The text to display on the payment button. Defaults to 'Confirm Payment' if left blank.",
        IsRequired = false,
        Key = AttributeKey.ConfirmPaymentButtonText,
        Category = "Captions",
        Order = 2 )]

    #endregion

    #region Payment / Transaction

    [BooleanField( "Enable ACH",
        Description = "Enabling this will <i>also</i> control which type of Saved Accounts can be used during the payment process. The payment gateway must still be configured to support ACH.",
        DefaultBooleanValue = true,
        IsRequired = true,
        Key = AttributeKey.EnableAch,
        Category = "Payment / Transaction",
        Order = 0 )]

    [BooleanField( "Enable Credit Card",
        Description = "Enabling this will <i>also</i> control which type of Saved Accounts can be used during the payment process. The payment gateway must still be configured to support Credit Card.",
        DefaultBooleanValue = true,
        IsRequired = true,
        Key = AttributeKey.EnableCreditCard,
        Category = "Payment / Transaction",
        Order = 1 )]

    [EntityTypeField( "Entity Type",
        Description = "The EntityType associated to the Transaction’s detail record (such as GroupMember, Registration, etc.)",
        IsRequired = false,
        IncludeGlobalAttributeOption = false,
        Key = AttributeKey.EntityType,
        Category = "Payment / Transaction",
        Order = 2 )]

    [WorkflowAttribute( "Entity Attribute",
        Description = "The Entity associated to the Transaction’s detail record. (such as a group member Id, a registration Id, etc.)",
        IsRequired = false,
        Key = AttributeKey.EntityAttribute,
        Category = "Payment / Transaction",
        Order = 3 )]

    [WorkflowTextOrAttribute( "Transaction Summary", "Transaction Summary Attribute",
        Description = "Optional summary text to record onto the FinancialTransactionDetail record. <span class='tip tip-lava'></span>",
        IsRequired = false,
        Key = AttributeKey.TransactionSummary,
        Category = "Payment / Transaction",
        Order = 4 )]

    [TextField( "Batch Prefix",
        Description = "The batch prefix name to use when creating a new batch. <span class='tip tip-lava'></span>",
        IsRequired = true,
        DefaultValue = "Workflow Payment Entry",
        Key = AttributeKey.BatchPrefix,
        Category = "Payment / Transaction",
        Order = 5 )]

    #endregion

    #endregion

    [Rock.SystemGuid.EntityTypeGuid( "6216cc4d-fca5-48e3-a7fd-fc09b3a58864" )]
    public class PaymentEntry : ActionComponent, IInteractiveAction
    {
        #region Keys

        private static class AttributeKey
        {
            public const string FinancialGateway = "FinancialGateway";
            public const string EnableAch = "EnableAch";
            public const string EnableCreditCard = "EnableCreditCard";
            public const string AuthorizedPersonAttribute = "AuthorizedPersonAttribute";
            public const string PaymentInformationInstructions = "PaymentInformationInstructions";
            public const string Amount = "Amount";
            public const string AmountEntryLabel = "AmountEntryLabel";
            public const string AccountAttribute = "AccountAttribute";
            public const string UseAccountCampusMappingLogic = "UseAccountCampusMappingLogic";
            public const string ShowCampusPicker = "ShowCampusPicker";
            public const string CampusTypes = "CampusTypes";
            public const string CampusStatuses = "CampusStatuses";
            public const string EntityType = "EntityType";
            public const string EntityAttribute = "EntityAttribute";
            public const string TransactionSummary = "TransactionSummary";
            public const string TransactionType = "TransactionType";
            public const string TransactionSource = "TransactionSource";
            public const string BatchPrefix = "BatchPrefix";
            public const string SaveAccountTitle = "SaveAccountTitle";
            public const string ConfirmPaymentButtonText = "ConfirmPaymentButtonText";
            public const string SuccessMessage = "SuccessMessage";
            public const string ResultTransactionAttribute = "ResultTransactionAttribute";
        }

        private static class ComponentConfigurationKey
        {
            public const string AmountLabel = "amountLabel";
            public const string Campuses = "campuses";
            public const string ConfirmPaymentButtonText = "confirmPaymentButtonText";
            public const string EnableSavedAccounts = "enableSavedAccounts";
            public const string ObsidianControlFileUrl = "obsidianControlFileUrl";
            public const string ObsidianControlSettings = "obsidianControlSettings";
            public const string PaymentInformationInstructions = "paymentInformationInstructions";
            public const string SaveAccountLabel = "saveAccountLabel";
            public const string SavedAccounts = "savedAccounts";
        }

        private static class ComponentDataKey
        {
            public const string Amount = "amount";
            public const string Campus = "campus";
            public const string SaveAccount = "saveAccount";
            public const string SaveAccountName = "saveAccountName";
            public const string Token = "token";
            public const string UseSavedAccount = "useSavedAccount";
        }

        #endregion Keys

        #region Methods

        /// <inheritdoc/>
        public override bool Execute( RockContext rockContext, WorkflowAction action, object entity, out List<string> errorMessages )
        {
            errorMessages = new List<string>();

            return false;
        }

        /// <summary>
        /// Load the configuration for the payment entry action from all the
        /// attributes that were configured on the workflow action.
        /// </summary>
        /// <param name="action">The action to load the attributes from.</param>
        /// <param name="rockContext">The context to use when accessing the database.</param>
        /// <param name="requestContext">The context that describes the request being processed.</param>
        /// <returns>A new instance of <see cref="PaymentConfiguration"/>.</returns>
        private PaymentConfiguration LoadConfiguration( WorkflowAction action, RockContext rockContext, RockRequestContext requestContext )
        {
            // Load the financial gateway that will be used to process this
            // workflow action payment.
            var financialGatewayGuid = GetAttributeValue( action, AttributeKey.FinancialGateway ).AsGuidOrNull();
            var financialGateway = financialGatewayGuid.HasValue
                ? new FinancialGatewayService( rockContext ).Get( financialGatewayGuid.Value )
                : null;

            // Load the person who will be associated with the payment as the
            // person who made the payment.
            var authorizedPersonAliasGuid = GetAttributeValue( action, AttributeKey.AuthorizedPersonAttribute, true ).AsGuidOrNull();
            var authorizedPersonAlias = authorizedPersonAliasGuid.HasValue
                ? new PersonAliasService( rockContext ).Queryable()
                    .Include( pa => pa.Person )
                    .FirstOrDefault( pa => pa.Guid == authorizedPersonAliasGuid.Value )
                : requestContext.CurrentPerson?.PrimaryAlias;

            // Load the primary account that was configured on the workflow
            // action.
            var accountGuid = GetAttributeValue( action, AttributeKey.AccountAttribute, true ).AsGuidOrNull();
            var account = accountGuid.HasValue
                ? FinancialAccountCache.Get( accountGuid.Value, rockContext )
                : null;

            // Load the campuses that will be available for selection by
            // the individual.
            var campusClientService = new CampusClientService( rockContext, null )
            {
                EnableSecurity = false
            };
            var campuses = campusClientService.GetCampusesAsListItems( new CampusOptions
            {
                LimitCampusStatuses = GetAttributeValue( action, AttributeKey.CampusStatuses ).SplitDelimitedValues().AsGuidList(),
                LimitCampusTypes = GetAttributeValue( action, AttributeKey.CampusTypes ).SplitDelimitedValues().AsGuidList()
            } );

            // Load the entity that will be associated with the payment.
            var entityTypeCache = EntityTypeCache.Get( GetAttributeValue( action, AttributeKey.EntityType ).AsGuid(), rockContext );
            var entityValue = GetAttributeValue( action, AttributeKey.EntityAttribute, true );
            IEntity entity = null;

            if (  entityTypeCache != null && entityValue.IsNotNullOrWhiteSpace() )
            {
                if ( entityTypeCache.Guid == SystemGuid.EntityType.PERSON.AsGuid() )
                {
                    entity = new PersonAliasService( rockContext ).Get( entityValue, true )?.Person;
                }
                else
                {
                    entity = Reflection.GetIEntityForEntityType( entityTypeCache.GetEntityType(), entityValue, rockContext );
                }
            }

            return new PaymentConfiguration
            {
                FinancialGateway = financialGateway,
                GatewayComponent = financialGateway?.GetGatewayComponent(),
                ObsidianComponent = financialGateway?.GetGatewayComponent() as IObsidianHostedGatewayComponent,
                EnableAch = GetAttributeValue( action, AttributeKey.EnableAch ).AsBoolean(),
                EnableCreditCard = GetAttributeValue( action, AttributeKey.EnableCreditCard ).AsBoolean(),
                EnabledSavedAccounts = authorizedPersonAlias.PersonId == requestContext.CurrentPerson?.Id,
                AuthorizedPersonAlias = authorizedPersonAlias,
                PaymentInformationInstructionsTemplate = GetAttributeValue( action, AttributeKey.PaymentInformationInstructions ),
                Amount = GetAttributeValue( action, AttributeKey.Amount, true ).AsDecimalOrNull(),
                AmountEntryLabel = GetAttributeValue( action, AttributeKey.AmountEntryLabel, true ),
                Account = account,
                UseAccountCampusMappingLogic = GetAttributeValue( action, AttributeKey.UseAccountCampusMappingLogic ).AsBoolean(),
                ShowCampusPicker = GetAttributeValue( action, AttributeKey.ShowCampusPicker ).AsBoolean(),
                Campuses = campuses,
                EntityType = entityTypeCache,
                Entity = entity,
                TransactionSummaryTemplate = GetAttributeValue( action, AttributeKey.TransactionSummary, true ),
                TransactionType = DefinedValueCache.Get( GetAttributeValue( action, AttributeKey.TransactionType ).AsGuid(), rockContext ),
                TransactionSource = DefinedValueCache.Get( GetAttributeValue( action, AttributeKey.TransactionSource ).AsGuid(), rockContext ),
                BatchPrefixTemplate = GetAttributeValue( action, AttributeKey.BatchPrefix ),
                SaveAccountTitle = GetAttributeValue( action, AttributeKey.SaveAccountTitle ),
                ConfirmPaymentButtonText = GetAttributeValue( action, AttributeKey.ConfirmPaymentButtonText, true ),
                SuccessMessageTemplate = GetAttributeValue( action, AttributeKey.SuccessMessage ),
                ResultTransactionAttribute = GetAttributeValue( action, AttributeKey.ResultTransactionAttribute ).AsGuidOrNull()
            };
        }

        /// <summary>
        /// Validates the configuration for the payment entry action.
        /// </summary>
        /// <param name="configuration">The configuration to be validated.</param>
        /// <param name="errorResult">On a <c>false</c> return will contain the error to be sent back.</param>
        /// <returns><c>true</c> if the configuraiton was valid; otherwise <c>false</c>.</returns>
        private static bool ValidateConfiguration( PaymentConfiguration configuration, out InteractiveActionResult errorResult )
        {
            if ( configuration.Account == null )
            {
                errorResult = CreateErrorResult( "Warning: No account was located for this transaction. Please contact us for assistance." );
                return false;
            }

            if ( !configuration.EnableAch && !configuration.EnableCreditCard )
            {
                errorResult = CreateErrorResult( "Payment options are currently unavailable. Please contact us for assistance." );
                return false;
            }

            if ( configuration.AuthorizedPersonAlias == null )
            {
                errorResult = CreateErrorResult( "We couldn't complete your request because we don't have the correct authorized person set up. Please contact us for assistance." );
                return false;
            }

            if ( configuration.FinancialGateway == null )
            {
                errorResult = CreateErrorResult( "The payment system isn't set up right now. Please contact us for assistance." );
                return false;
            }

            if ( configuration.ObsidianComponent == null )
            {
                errorResult = CreateErrorResult( "Administrator, the payment gateway only works with \"hosted\" gateway types. Please select a different gateway." );
                return false;
            }

            errorResult = null;

            return true;
        }

        /// <summary>
        /// Creates a common error message result to be displayed in the UI.
        /// </summary>
        /// <param name="errorMessage">The text of the error message.</param>
        /// <returns>The result object to be send back to the client.</returns>
        private static InteractiveActionResult CreateErrorResult( string errorMessage )
        {
            return new InteractiveActionResult
            {
                IsSuccess = false,
                ProcessingType = InteractiveActionContinueMode.Stop,
                ActionData = new InteractiveActionDataBag
                {
                    Message = new InteractiveMessageBag
                    {
                        Type = InteractiveMessageType.Error,
                        Content = errorMessage
                    }
                }
            };
        }

        /// <summary>
        /// Creates an exception message result to be returned to the client.
        /// </summary>
        /// <param name="errorMessage">The text of the error message.</param>
        /// <returns>The result object to be send back to the client.</returns>
        private static InteractiveActionResult CreateExceptionResult( string errorMessage )
        {
            return new InteractiveActionResult
            {
                IsSuccess = false,
                ProcessingType = InteractiveActionContinueMode.Stop,
                ActionData = new InteractiveActionDataBag
                {
                    Exception = new InteractiveActionExceptionBag
                    {
                        Message = errorMessage
                    }
                }
            };
        }

        /// <summary>
        /// Resolve any merge fields that are present in the payment instructions
        /// and return the final content string.
        /// </summary>
        /// <param name="configuration">The payment action configuration.</param>
        /// <param name="action">The workflow action currently being processed.</param>
        /// <param name="requestContext">The context that describes the current request being processed.</param>
        /// <returns>The content after lava has been processed.</returns>
        private string ResolvePaymentInformationInstructions( PaymentConfiguration configuration, WorkflowAction action, RockRequestContext requestContext )
        {
            var mergeFields = GetMergeFields( action, requestContext );

            mergeFields.Add( "PaymentConfiguration", new Dictionary<string, object>
            {
                ["Amount"] = configuration.Amount,
                ["Entity"] = configuration.Entity,
                ["AmountEntryLabel"] = configuration.AmountEntryLabel,
                ["TransactionType"] = configuration.TransactionType,
                ["TransactionSummary"] = ResolveTransactionSummary( configuration, action, requestContext )
            } );

            var helper = new StructuredContentHelper( configuration.PaymentInformationInstructionsTemplate );

            helper.ResolveMergeFields( mergeFields );

            // Mobile needs the structured content JSON so it can render the
            // data with native controls.
            var isMobile = requestContext.GetHeader( "X-Rock-Mobile-Api-Key" ).Count() > 0;

            return !isMobile
                ? helper.Render()
                : helper.Content;
        }

        /// <summary>
        /// Resolve any merge fields that are present in the transaction summary
        /// and return the final content string.
        /// </summary>
        /// <param name="configuration">The payment action configuration.</param>
        /// <param name="action">The workflow action currently being processed.</param>
        /// <param name="requestContext">The context that describes the current request being processed.</param>
        /// <returns>The content after lava has been processed.</returns>
        private string ResolveTransactionSummary( PaymentConfiguration configuration, WorkflowAction action, RockRequestContext requestContext )
        {
            var mergeFields = GetMergeFields( action, requestContext );

            return configuration.TransactionSummaryTemplate?.ResolveMergeFields( mergeFields ) ?? string.Empty;
        }

        /// <summary>
        /// Resolve any merge fields that are present in the success message
        /// and return the final content string.
        /// </summary>
        /// <param name="configuration">The payment action configuration.</param>
        /// <param name="action">The workflow action currently being processed.</param>
        /// <param name="transactionDetail">The transaction detail that was created.</param>
        /// <param name="requestContext">The context that describes the current request being processed.</param>
        /// <returns>The content after lava has been processed.</returns>
        private string ResolveSuccessMessage( PaymentConfiguration configuration, WorkflowAction action, FinancialTransactionDetail transactionDetail, RockRequestContext requestContext )
        {
            var mergeFields = GetMergeFields( action, requestContext );

            mergeFields.Add( "TransactionDetail", transactionDetail );
            mergeFields.Add( "Account", transactionDetail.Account );

            var helper = new StructuredContentHelper( configuration.SuccessMessageTemplate );

            helper.ResolveMergeFields( mergeFields );

            // Mobile needs the structured content JSON so it can render the
            // data with native controls.
            var isMobile = requestContext.GetHeader( "X-Rock-Mobile-Api-Key" ).Count() > 0;

            return !isMobile
                ? helper.Render()
                : helper.Content;
        }

        /// <summary>
        /// Gets the saved accounts that are associated witht he person and
        /// are valid for the current configuration.
        /// </summary>
        /// <param name="configuration">The workflow action configuration.</param>
        /// <param name="rockContext">The context to use when accessing the database.</param>
        /// <returns>A collection of <see cref="SavedFinancialAccountListItemBag"/> objects representing the saved accounts.</returns>
        private static List<SavedFinancialAccountListItemBag> GetSavedAccounts( PaymentConfiguration configuration, RockContext rockContext )
        {
            var person = configuration.AuthorizedPersonAlias?.Person;

            if ( !configuration.EnabledSavedAccounts || person == null || configuration.FinancialGateway == null )
            {
                return new List<SavedFinancialAccountListItemBag>();
            }

            var savedAccountClientService = new FinancialPersonSavedAccountClientService( rockContext, person );

            var accountOptions = new SavedFinancialAccountOptions
            {
                FinancialGatewayGuids = new List<Guid>
                {
                    configuration.FinancialGateway.Guid
                },
                CurrencyTypeGuids = GetSavedAccountAllowedCurrencyTypes( configuration )
            };

            return savedAccountClientService.GetSavedFinancialAccountsForPersonAsAccountListItems( person.Id, accountOptions );
        }

        /// <summary>
        /// Gets the allowed currency types supported by both the action and the
        /// financial gateway.
        /// </summary>
        /// <param name="configuration">The workflow action configuration.</param>
        /// <returns>A list of <see cref="Guid"/> values that represent the currency types.</returns>
        private static List<Guid> GetSavedAccountAllowedCurrencyTypes( PaymentConfiguration configuration )
        {
            var creditCardCurrency = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.CURRENCY_TYPE_CREDIT_CARD.AsGuid() );
            var achCurrency = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.CURRENCY_TYPE_ACH.AsGuid() );
            var allowedCurrencyTypes = new List<Guid>();

            // Conditionally enable credit card.
            if ( configuration.EnableCreditCard && configuration.GatewayComponent.SupportsSavedAccount( creditCardCurrency ) )
            {
                allowedCurrencyTypes.Add( creditCardCurrency.Guid );
            }

            // Conditionally enable ACH.
            if ( configuration.EnableAch && configuration.GatewayComponent.SupportsSavedAccount( achCurrency ) )
            {
                allowedCurrencyTypes.Add( achCurrency.Guid );
            }

            return allowedCurrencyTypes;
        }

        /// <summary>
        /// Gets the account that should be used for the transaction based on
        /// the campus selection.
        /// </summary>
        /// <remarks>
        /// <list type="bullet">
        ///   <item>If the selected account is not associated with a campus, the Selected Account will be the first matching active child account that is associated with the selected campus.</item>
        ///   <item>If the selected account is not associated with a campus, but there are no active child accounts for the selected campus, the parent account (the one the user sees) will be returned.</item>
        ///   <item>If the selected account is associated with a campus, that account will be returned regardless of campus selection (and it won't use the child account logic).</item>
        /// </list>
        /// </remarks>
        /// <param name="configuration">The workflow action configuration.</param>
        /// <param name="campusGuid">The selected campus unique identifier.</param>
        /// <param name="rockContext">The context to use when accessing the database.</param>
        /// <returns>A <see cref="FinancialAccountCache"/> that represents the account to use for the transaction.</returns>
        private static FinancialAccountCache GetAccountForTransaction( PaymentConfiguration configuration, Guid? campusGuid, RockContext rockContext )
        {
            var account = configuration.Account;

            // If we are not using campus mapping logic or the configured account
            // is associated with a specific campus then we use the configured
            // account without any further filtering.
            if ( !configuration.UseAccountCampusMappingLogic || account.CampusId.HasValue )
            {
                return account;
            }

            var campusId = campusGuid.HasValue
                ? CampusCache.Get( campusGuid.Value, rockContext )?.Id
                : null;

            // If no campus was selected then use the configured account.
            if ( !campusId.HasValue )
            {
                return account;
            }

            var childAccount = account.ChildAccounts
                .Where( a => a.IsActive && a.CampusId == campusId.Value )
                .FirstOrDefault();

            // Return either the matching child account or the configured account
            // if no child account was found.
            return childAccount ?? account;
        }

        /// <summary>
        /// Charges the payment using the provided payment data, creates the
        /// <see cref="FinancialTransaction"/> record in the database and then
        /// saves the payment method as a new saved account if requested.
        /// </summary>
        /// <param name="configuration">The payment action configuration.</param>
        /// <param name="paymentData">The details about the payment to be made.</param>
        /// <param name="rockContext">The context to use when accessing the database.</param>
        /// <param name="requestContext">The context that describes the current request being processed.</param>
        /// <returns>The detail record that describes the account and amount.</returns>
        private static FinancialTransactionDetail CreateFinancialTransaction( PaymentConfiguration configuration, PaymentData paymentData, RockContext rockContext, RockRequestContext requestContext )
        {
            var paymentInfo = GetPaymentInfo( configuration, paymentData, rockContext );

            // Process the payment in the gateway.
            var transaction = ProcessGatewayPayment( configuration, paymentData, paymentInfo, rockContext );

            // The payment was processed successfully, so save the transaction in Rock.
            var transactionDetail = SaveTransaction( configuration, paymentData, paymentInfo, transaction, rockContext );

            if ( configuration.EnabledSavedAccounts && !paymentData.SavedAccountGuid.HasValue && paymentData.SaveAccount && paymentData.SaveAccountName.IsNotNullOrWhiteSpace() )
            {
                try
                {
                    CreateSavedAccount( configuration, paymentData, paymentInfo, transaction, rockContext );
                }
                catch ( Exception ex )
                {
                    // Because this is all done as a single operation, we don't
                    // want to error out the workflow if the saved account
                    // creation fails. Just log it.
                    ExceptionLogService.LogException( ex );
                }
            }

            return transactionDetail;
        }

        /// <summary>
        /// Gets the basic payment info object that describes the payment method
        /// to be charged.
        /// </summary>
        /// <param name="configuration">The payment action configuration.</param>
        /// <param name="paymentData">The details about the payment to be made.</param>
        /// <param name="rockContext">The context to use when accessing the database.</param>
        /// <returns>An instance of <see cref="ReferencePaymentInfo"/> that describes the payment method.</returns>
        private static ReferencePaymentInfo GetPaymentInfo( PaymentConfiguration configuration, PaymentData paymentData, RockContext rockContext )
        {
            ReferencePaymentInfo paymentInfo;

            // Get the payment info from either the saved account or the gateway
            // token when using a new payment method.
            if ( paymentData.SavedAccountGuid.HasValue && configuration.EnabledSavedAccounts )
            {
                var savedAccount = new FinancialPersonSavedAccountService( rockContext )
                    .Queryable()
                    .Where( a => a.Guid == paymentData.SavedAccountGuid.Value
                        && a.PersonAlias.PersonId == configuration.AuthorizedPersonAlias.PersonId )
                    .AsNoTracking()
                    .FirstOrDefault();

                if ( savedAccount != null )
                {
                    paymentInfo = savedAccount.GetReferencePayment();
                }
                else
                {
                    throw new Exception( "There was a problem retrieving the saved account" );
                }
            }
            else
            {
                paymentInfo = new ReferencePaymentInfo
                {
                    ReferenceNumber = paymentData.Token,
                };
            }

            // Update payment info with details about this payment.
            paymentInfo.Amount = paymentData.Amount;
            paymentInfo.Email = configuration.AuthorizedPersonAlias.Person.Email;
            paymentInfo.FirstName = configuration.AuthorizedPersonAlias.Person.NickName;
            paymentInfo.LastName = configuration.AuthorizedPersonAlias.Person.LastName;
            paymentInfo.TransactionTypeValueId = configuration.TransactionType.Id;

            return paymentInfo;
        }

        /// <summary>
        /// Processes the payment on the gateway. This makes the charge and
        /// returns the transaction that will represent the payment in Rock.
        /// </summary>
        /// <param name="configuration">The payment action configuration.</param>
        /// <param name="paymentData">The details about the payment to be made.</param>
        /// <param name="paymentInfo">The object that contains the information about the payment method.</param>
        /// <param name="rockContext">The context to use when accessing the database.</param>
        /// <returns>A <see cref="FinancialTransaction"/> that describes the payment made.</returns>
        private static FinancialTransaction ProcessGatewayPayment( PaymentConfiguration configuration, PaymentData paymentData, ReferencePaymentInfo paymentInfo, RockContext rockContext )
        {
            FinancialTransaction transaction;

            if ( paymentInfo.GatewayPersonIdentifier.IsNullOrWhiteSpace() )
            {
                var customerToken = configuration.ObsidianComponent.CreateCustomerAccount( configuration.FinancialGateway, paymentInfo, out var errorMessage );

                if ( errorMessage.IsNotNullOrWhiteSpace() )
                {
                    throw new Exception( errorMessage );
                }

                paymentInfo.GatewayPersonIdentifier = customerToken;
            }

            if ( paymentData.Token.IsNotNullOrWhiteSpace() && configuration.ObsidianComponent.IsPaymentTokenCharged( configuration.FinancialGateway, paymentData.Token ) )
            {
                // Download the existing payment from the gateway.
                transaction = configuration.ObsidianComponent.FetchPaymentTokenTransaction( rockContext, configuration.FinancialGateway, null, paymentData.Token );
                paymentInfo.Amount = transaction.TotalAmount;
            }
            else
            {
                // Charge a new payment with the tokenized payment method
                transaction = configuration.GatewayComponent.Charge( configuration.FinancialGateway, paymentInfo, out var errorMessage );

                if ( errorMessage.IsNotNullOrWhiteSpace() )
                {
                    throw new Exception( errorMessage );
                }
            }

            return transaction;
        }

        /// <summary>
        /// Saves the transaction in the Rock database. This also configures
        /// the final transaction details for where the money should go.
        /// </summary>
        /// <param name="configuration">The payment action configuration.</param>
        /// <param name="paymentData">The details about the payment to be made.</param>
        /// <param name="paymentInfo">The object that contains the information about the payment method.</param>
        /// <param name="transaction">The transaction that describes the payment that was made.</param>
        /// <param name="rockContext">The context to use when accessing the database.</param>
        /// <returns>A <see cref="FinancialTransactionDetail"/> that describes how the payment was allocated.</returns>
        private static FinancialTransactionDetail SaveTransaction( PaymentConfiguration configuration, PaymentData paymentData, PaymentInfo paymentInfo, FinancialTransaction transaction, RockContext rockContext )
        {
            transaction.AuthorizedPersonAliasId = configuration.AuthorizedPersonAlias.Id;
            transaction.TransactionDateTime = RockDateTime.Now;
            transaction.FinancialGatewayId = configuration.FinancialGateway.Id;

            if ( transaction.FinancialPaymentDetail == null )
            {
                transaction.FinancialPaymentDetail = new FinancialPaymentDetail();
            }

            /* 02/17/2022 MDP
                Note that after the transaction, the HostedGateway knows more about the FinancialPaymentDetail than Rock does
                since it is the gateway that collects the payment info. But just in case paymentInfo has information the the gateway hasn't set,
                we'll fill in any missing details.
                But then we'll want to use FinancialPaymentDetail as the most accurate values for the payment info. 
            */

            if ( paymentInfo != null )
            {
                transaction.FinancialPaymentDetail.SetFromPaymentInfo( paymentInfo, configuration.GatewayComponent, rockContext );
            }

            var currencyTypeValue = transaction.FinancialPaymentDetail?.CurrencyTypeValueId != null
                ? DefinedValueCache.Get( transaction.FinancialPaymentDetail.CurrencyTypeValueId.Value, rockContext )
                : null;

            var creditCardTypeValue = transaction.FinancialPaymentDetail?.CreditCardTypeValueId != null
                ? DefinedValueCache.Get( transaction.FinancialPaymentDetail.CreditCardTypeValueId.Value, rockContext )
                : null;

            transaction.SourceTypeValueId = configuration.TransactionSource.Id;
            transaction.TransactionTypeValueId = configuration.TransactionType.Id;
            transaction.Summary = paymentData.TransactionSummary;

            var transactionDetail = transaction.TransactionDetails.FirstOrDefault();
            if ( transactionDetail == null )
            {
                transactionDetail = rockContext.Set<FinancialTransactionDetail>().Create();
                transaction.TransactionDetails.Add( transactionDetail );
            }

            transactionDetail.Amount = paymentInfo.Amount;
            transactionDetail.AccountId = paymentData.Account.Id;
            transactionDetail.EntityTypeId = configuration.EntityType?.Id;
            transactionDetail.EntityId = configuration.Entity?.Id;

            var batchChanges = new History.HistoryChangeList();

            rockContext.WrapTransaction( () =>
            {
                var batchService = new FinancialBatchService( rockContext );

                // Get or create the batch.
                var batch = batchService.GetForNewTransaction( transaction, configuration.BatchPrefixTemplate );
                FinancialBatchService.EvaluateNewBatchHistory( batch, batchChanges );

                var financialTransactionService = new FinancialTransactionService( rockContext );

                // If this is a new Batch, SaveChanges so that we can get the Batch.Id.
                if ( batch.Id == 0 )
                {
                    rockContext.SaveChanges();
                }

                transaction.BatchId = batch.Id;

                // use the financialTransactionService to add the transaction instead of batch.Transactions to avoid lazy-loading the transactions already associated with the batch
                financialTransactionService.Add( transaction );
                rockContext.SaveChanges();

                batchService.IncrementControlAmount( batch.Id, transaction.TotalAmount, batchChanges );
                rockContext.SaveChanges();
            } );

            HistoryService.SaveChanges(
                rockContext,
                typeof( FinancialBatch ),
                Rock.SystemGuid.Category.HISTORY_FINANCIAL_BATCH.AsGuid(),
                transaction.BatchId.Value,
                batchChanges,
                true,
                configuration.AuthorizedPersonAlias.Id );

            return transactionDetail;
        }

        /// <summary>
        /// Creates and saves the payment method as a new saved account for the
        /// person to use again in the future.
        /// </summary>
        /// <param name="configuration">The payment action configuration.</param>
        /// <param name="paymentData">The details about the payment to be made.</param>
        /// <param name="paymentInfo">The object that contains the information about the payment method.</param>
        /// <param name="transaction">The transaction that describes the payment that was made.</param>
        /// <param name="rockContext">The context to use when accessing the database.</param>
        private static void CreateSavedAccount( PaymentConfiguration configuration, PaymentData paymentData, ReferencePaymentInfo paymentInfo, FinancialTransaction transaction, RockContext rockContext )
        {
            // Validate the arguments
            if ( transaction.TransactionCode.IsNullOrWhiteSpace() != false )
            {
                throw new Exception( "The account information cannot be saved as there's not a valid transaction code to reference" );
            }

            if ( transaction.FinancialPaymentDetail == null )
            {
                throw new Exception( "Sorry, the account information cannot be saved as there's not a valid transaction to reference" );
            }

            var savedAccount = new FinancialPersonSavedAccount
            {
                PersonAliasId = configuration.AuthorizedPersonAlias.Id,
                ReferenceNumber = transaction.TransactionCode,
                GatewayPersonIdentifier = paymentInfo.GatewayPersonIdentifier,
                Name = paymentData.SaveAccountName,
                TransactionCode = transaction.TransactionCode,
                FinancialGatewayId = configuration.FinancialGateway.Id,
                FinancialPaymentDetail = new FinancialPaymentDetail
                {
                    AccountNumberMasked = transaction.FinancialPaymentDetail.AccountNumberMasked,
                    CurrencyTypeValueId = transaction.FinancialPaymentDetail.CurrencyTypeValueId,
                    CreditCardTypeValueId = transaction.FinancialPaymentDetail.CreditCardTypeValueId,
                    NameOnCard = transaction.FinancialPaymentDetail.NameOnCard,
                    ExpirationMonth = transaction.FinancialPaymentDetail.ExpirationMonth,
                    ExpirationYear = transaction.FinancialPaymentDetail.ExpirationYear,
                    BillingLocationId = transaction.FinancialPaymentDetail.BillingLocationId
                }
            };

            var financialPersonSavedAccountService = new FinancialPersonSavedAccountService( rockContext );
            financialPersonSavedAccountService.Add( savedAccount );

            rockContext.SaveChanges();
        }

        #endregion

        #region IInteractiveAction

        /// <inheritdoc/>
        InteractiveActionResult IInteractiveAction.StartAction( WorkflowAction action, RockContext rockContext, RockRequestContext requestContext )
        {
            var configuration = LoadConfiguration( action, rockContext, requestContext );

            if ( !ValidateConfiguration( configuration, out var errorResult ) )
            {
                return errorResult;
            }

            var result = new InteractiveActionResult
            {
                IsSuccess = false,
                ProcessingType = InteractiveActionContinueMode.Stop,
                ActionData = new InteractiveActionDataBag
                {
                    ComponentUrl = requestContext.ResolveRockUrl( "~/Obsidian/Blocks/Workflow/WorkflowEntry/Actions/paymentEntry.obs" ),
                    ComponentConfiguration = new Dictionary<string, string>
                    {
                        [ComponentConfigurationKey.AmountLabel] = configuration.AmountEntryLabel,
                        [ComponentConfigurationKey.Campuses] = configuration.ShowCampusPicker
                            ? configuration.Campuses.ToCamelCaseJson( false, false )
                            : null,
                        [ComponentConfigurationKey.ConfirmPaymentButtonText] = configuration.ConfirmPaymentButtonText,
                        [ComponentConfigurationKey.EnableSavedAccounts] = configuration.EnabledSavedAccounts.ToString(),
                        [ComponentConfigurationKey.ObsidianControlFileUrl] = configuration.ObsidianComponent.GetObsidianControlFileUrl( configuration.FinancialGateway ),
                        [ComponentConfigurationKey.ObsidianControlSettings] = configuration.ObsidianComponent.GetObsidianControlSettings( configuration.FinancialGateway, new HostedPaymentInfoControlOptions
                        {
                            EnableACH = configuration.EnableAch,
                            EnableCreditCard = configuration.EnableCreditCard,
                            EnableBillingAddressCollection = true
                        } ).ToCamelCaseJson( false, false ),
                        [ComponentConfigurationKey.PaymentInformationInstructions] = ResolvePaymentInformationInstructions( configuration, action, requestContext ),
                        [ComponentConfigurationKey.SaveAccountLabel] = configuration.SaveAccountTitle,
                        [ComponentConfigurationKey.SavedAccounts] = GetSavedAccounts( configuration, rockContext ).ToCamelCaseJson( false, false )
                    },
                    ComponentData = new Dictionary<string, string>
                    {
                        [ComponentDataKey.Amount] = configuration.Amount?.ToString(),
                        [ComponentDataKey.Campus] = configuration.AuthorizedPersonAlias?.Person?.PrimaryCampus?.Guid.ToString()
                    }
                }
            };

            return result;
        }

        /// <inheritdoc/>
        InteractiveActionResult IInteractiveAction.UpdateAction( WorkflowAction action, Dictionary<string, string> componentData, RockContext rockContext, RockRequestContext requestContext )
        {
            var configuration = LoadConfiguration( action, rockContext, requestContext );

            if ( !ValidateConfiguration( configuration, out var errorResult ) )
            {
                return errorResult;
            }

            var paymentData = new PaymentData
            {
                Amount = componentData.GetValueOrNull( ComponentDataKey.Amount ).AsDecimal(),
                CampusGuid = componentData.GetValueOrNull( ComponentDataKey.Campus ).AsGuidOrNull(),
                SaveAccount = componentData.GetValueOrNull( ComponentDataKey.SaveAccount ).AsBoolean(),
                SaveAccountName = componentData.GetValueOrNull( ComponentDataKey.SaveAccountName ),
                Token = componentData.GetValueOrNull( ComponentDataKey.Token ),
                SavedAccountGuid = componentData.GetValueOrNull( ComponentDataKey.UseSavedAccount ).AsGuidOrNull()
            };

            paymentData.Account = GetAccountForTransaction( configuration, paymentData.CampusGuid, rockContext );
            paymentData.TransactionSummary = ResolveTransactionSummary( configuration, action, requestContext );

            try
            {
                var transactionDetail = CreateFinancialTransaction( configuration, paymentData, rockContext, requestContext );

                if ( configuration.ResultTransactionAttribute.HasValue )
                {
                    SetWorkflowAttributeValue( action, configuration.ResultTransactionAttribute.Value, transactionDetail.Transaction.Guid.ToString() );
                }

                return new InteractiveActionResult
                {
                    IsSuccess = true,
                    ProcessingType = InteractiveActionContinueMode.Continue,
                    ActionData = new InteractiveActionDataBag
                    {
                        Message = new InteractiveMessageBag
                        {
                            // TODO: When adding support for mobile, we need to add a new
                            // message type of structured content and use that for mobile.
                            Type = InteractiveMessageType.Html,
                            Content = ResolveSuccessMessage( configuration, action, transactionDetail, requestContext )
                        }
                    }
                };
            }
            catch ( Exception ex )
            {
                ExceptionLogService.LogException( ex );

                return CreateExceptionResult( ex.Message );
            }
        }

        #endregion

        #region Support Classes

        private class PaymentConfiguration
        {
            public FinancialGateway FinancialGateway { get; set; }

            public GatewayComponent GatewayComponent { get; set; }

            public IObsidianHostedGatewayComponent ObsidianComponent { get; set; }

            public bool EnableAch { get; set; }

            public bool EnableCreditCard { get; set; }

            public bool EnabledSavedAccounts { get; set; }

            public PersonAlias AuthorizedPersonAlias { get; set; }

            public string PaymentInformationInstructionsTemplate { get; set; }

            public decimal? Amount { get; set; }

            public string AmountEntryLabel { get; set; }

            public FinancialAccountCache Account { get; set; }

            public bool UseAccountCampusMappingLogic { get; set; }

            public bool ShowCampusPicker { get; set; }

            public List<ListItemBag> Campuses { get; set; }

            public EntityTypeCache EntityType { get; set; }

            public IEntity Entity { get; set; }

            public string TransactionSummaryTemplate { get; set; }

            public DefinedValueCache TransactionType { get; set; }

            public DefinedValueCache TransactionSource { get; set; }

            public string BatchPrefixTemplate { get; set; }

            public string SaveAccountTitle { get; set; }

            public string ConfirmPaymentButtonText { get; set; }

            public string SuccessMessageTemplate { get; set; }

            public Guid? ResultTransactionAttribute { get; set; }
        }

        private class PaymentData
        {
            public FinancialAccountCache Account { get; set; }

            public decimal Amount { get; set; }

            public Guid? CampusGuid { get; set; }

            public bool SaveAccount { get; set; }

            public string SaveAccountName { get; set; }

            public string Token { get; set; }

            public Guid? SavedAccountGuid { get; set; }

            public string TransactionSummary { get; set; }
        }

        #endregion
    }
}
