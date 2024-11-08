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
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Financial;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

/*  10-28-2022 MDP
 
   This was originally from https://github.com/Triumph-Tech/Partner-Life.Church/blob/lc_plugins_1.13.2/church.life.SmsGive/SmsActionGive.cs
   and is now moved to core.
 
 */

namespace Rock.Communication.SmsActions
{
    /// <summary>
    /// Class SmsActionGive.
    /// Implements the <see cref="Rock.Communication.SmsActions.SmsActionComponent" />
    /// </summary>
    /// <seealso cref="Rock.Communication.SmsActions.SmsActionComponent" />
    [Description( "Allows an SMS sender to make a payment." )]
    [Export( typeof( SmsActionComponent ) )]
    [ExportMetadata( "ComponentName", "Give" )]

    #region Attributes

    [TextField( "Give Keyword",
        Description = @"The case-insensitive keywords that will be expected at the beginning of the message. Separate multiple values with commas like ""give, giving, gift"".",
        IsRequired = true,
        DefaultValue = "GIVE,TITHE,GIVING,GIFT",
        Order = 1,
        Category = "Giving",
        Key = AttributeKeys.GivingKeywords )]

    [TextField( "Setup Keyword",
        Description = @"The case-insensitive keyword that will be expected at the beginning of the message. Separate multiple values with commas like ""setup, edit, config"".",
        IsRequired = true,
        DefaultValue = "SETUP",
        Order = 2,
        Category = "Giving",
        Key = AttributeKeys.SetupKeywords )]

    [CurrencyField( "Maximum Gift Amount",
        Description = "Leave blank to allow gifts of any size.",
        IsRequired = false,
        Order = 3,
        Category = "Giving",
        Key = AttributeKeys.MaxAmount )]

    [AccountField( "Financial Account",
        Description = "The financial account to designate gifts toward. Leave blank to use the person's default giving designation.",
        IsRequired = false,
        Category = "Giving",
        Order = 4,
        Key = AttributeKeys.FinancialAccount )]

    [LinkedPage( "Setup Page",
        Description = "The page to link with a token for the person to setup their SMS giving",
        IsRequired = true,
        Category = "Giving",
        Order = 5,
        DefaultValue = "42ceee52-adec-48bb-af90-496db2b272c7,a5de71c3-5c98-40d1-bd27-32b298ac4577",
        Key = AttributeKeys.SetupPage )]

    [TextField( "Refund Keyword",
        Description = @"The case-insensitive keyword that is expected to trigger the refund. Leave blank to disable SMS refunds. Separate multiple values with commas like ""refund, undo, oops"".",
        IsRequired = false,
        DefaultValue = "REFUND",
        Order = 6,
        Category = "Refund",
        Key = AttributeKeys.RefundKeywords )]

    [IntegerField( "Processing Delay Minutes",
        Description = "The number of minutes to delay processing the gifts and allow refunds (if the refund keyword is set). Delaying allows SMS requested refunds to completely bypass the financial gateway. Set to zero or leave blank to process gifts immediately and disallow refunds.",
        IsRequired = false,
        DefaultIntegerValue = 10,
        Order = 7,
        Category = "Refund",
        Key = AttributeKeys.ProcessingDelayMinutes )]

    [CodeEditorField( "Help Response",
        EditorMode = CodeEditorMode.Lava,
        EditorTheme = CodeEditorTheme.Rock,
        Description = "The response that will be sent if the sender's message doesn't make sense, there is missing information, or an error occurs. <span class='tip tip-lava'></span> Use {{ Lava | Debug }} to see all available fields.",
        IsRequired = true,
        DefaultValue = "To give, text GIVE & amount. Example: GIVE $250. More help at XXXXXXXXXX. Msg & data rates may apply. Reply STOP to cancel.",
        Order = 8,
        Category = "Response",
        Key = AttributeKeys.HelpResponse )]

    [CodeEditorField( "Max Amount Response",
        EditorMode = CodeEditorMode.Lava,
        EditorTheme = CodeEditorTheme.Rock,
        Description = "The response that will be sent if the sender is trying to give more than the max amount (if configured). <span class='tip tip-lava'></span> Use {{ Lava | Debug }} to see all available fields.",
        IsRequired = false,
        DefaultValue = "Thank you for your generosity but our mobile giving solution cannot process a gift this large. Please give using our website.",
        Order = 9,
        Category = "Response",
        Key = AttributeKeys.MaxAmountResponse )]

    [CodeEditorField( "Setup Response",
        EditorMode = CodeEditorMode.Lava,
        EditorTheme = CodeEditorTheme.Rock,
        Description = "The response that will be sent if the sender is unknown, does not have a saved account, or requests to edit their giving profile. <span class='tip tip-lava'></span> Use {{ Lava | Debug }} to see all available fields.",
        IsRequired = true,
        DefaultValue = @"{{ 'Global' | Attribute:'OrganizationName'}} text giving alerts: Register here: {% if SetupLink and SetupLink != empty %}{{ SetupLink | CreateShortLink }}{% endif %} Msg & data rates may apply. 1 msg/gift. Reply HELP for help, STOP to cancel",
        Order = 10,
        Category = "Response",
        Key = AttributeKeys.SetupResponse )]

    [CodeEditorField( "Success Response",
        EditorMode = CodeEditorMode.Lava,
        EditorTheme = CodeEditorTheme.Rock,
        Description = "The response that will be sent if the payment is successful. <span class='tip tip-lava'></span> Use {{ Lava | Debug }} to see all available fields.",
        IsRequired = true,
        DefaultValue = "Thank you! Your {{ 'Global' | Attribute:'OrganizationName'}} text gift of ${{ Amount | Format:'#,##0.00' }} has been received. A confirmation has been sent to the email address you provided.",
        Order = 11,
        Category = "Response",
        Key = AttributeKeys.SuccessResponse )]

    [CodeEditorField( "Refund Failure Response",
        EditorMode = CodeEditorMode.Lava,
        EditorTheme = CodeEditorTheme.Rock,
        Description = "The response that will be sent if the sender's gift cannot be refunded. <span class='tip tip-lava'></span> Use {{ Lava | Debug }} to see all available fields.",
        IsRequired = true,
        DefaultValue = "We are unable to process a refund for your last gift. Please contact us for assistance.",
        Order = 12,
        Category = "Response",
        Key = AttributeKeys.RefundFailureResponse )]

    [CodeEditorField( "Refund Success Response",
        EditorMode = CodeEditorMode.Lava,
        EditorTheme = CodeEditorTheme.Rock,
        Description = "The response that will be sent if the refund is successful. <span class='tip tip-lava'></span> Use {{ Lava | Debug }} to see all available fields.",
        IsRequired = true,
        DefaultValue = "Your gift for ${{ Amount | Format:'#,##0.00' }} to the {{ AccountName }} has been refunded.",
        Order = 13,
        Category = "Response",
        Key = AttributeKeys.RefundSuccessResponse )]

    [CodeEditorField( "Missing Amount Response",
        EditorMode = CodeEditorMode.Lava,
        EditorTheme = CodeEditorTheme.Rock,
        Description = "The response that will be sent if the amount value is missing from the sender's message. <span class='tip tip-lava'></span> Use {{ Lava | Debug }} to see all available fields.",
        IsRequired = true,
        DefaultValue = "To give, simply respond with a gift amount. For example, '$100' or '$200'.",
        Order = 14,
        Category = "Response",
        Key = AttributeKeys.MissingAmountResponse )]

    [CodeEditorField( "Transaction Failure Response",
        EditorMode = CodeEditorMode.Lava,
        EditorTheme = CodeEditorTheme.Rock,
        Description = "The response that will be sent if the transaction fails. <span class='tip tip-lava'></span> Use {{ Lava | Debug }} to see all available fields.",
        IsRequired = true,
        DefaultValue = "Your transaction could not be processed. Please text GIVE & amount (example: GIVE $250) to try again, or text SETUP if you need to change your payment details.",
        Order = 15,
        Category = "Response",
        Key = AttributeKeys.TransactionFailureResponse )]

    #endregion Attributes

    [Rock.SystemGuid.EntityTypeGuid( "EFB22EDF-49E5-46C9-B204-AD99876E44D6" )]
    public class SmsActionGive : SmsActionComponent
    {
        #region Keys

        /// <summary>
        /// Keys for the attributes
        /// </summary>
        private static class AttributeKeys
        {
            // Giving
            public const string SetupKeywords = "SetupKeyword";
            public const string GivingKeywords = "GivingKeyword";
            public const string MaxAmount = "MaxAmount";
            public const string MaxAmountResponse = "MaxAmountResponse";
            public const string FinancialAccount = "FinancialAccount";
            public const string SetupPage = "SetupPage";

            // Refund
            public const string ProcessingDelayMinutes = "ProcessingDelayMinutes";
            public const string RefundKeywords = "RefundKeyword";

            // Responses
            public const string HelpResponse = "HelpResponse";
            public const string SetupResponse = "SetupResponse";
            public const string SuccessResponse = "SuccessResponse";
            public const string RefundSuccessResponse = "RefundSuccessResponse";
            public const string RefundFailureResponse = "RefundFailureResponse";
            public const string MissingAmountResponse = "MissingAmountResponse";
            public const string TransactionFailureResponse = "TransactionFailureResponse";
        }

        /// <summary>
        /// Keys for the lava merge fields (used in the SMS responses)
        /// </summary>
        private static class LavaMergeFieldKeys
        {
            public const string Message = "Message";
            public const string Keyword = "Keyword";
            public const string Amount = "Amount";
            public const string AccountId = "AccountId";
            public const string AccountName = "AccountName";
            public const string SetupLink = "SetupLink";
            public const string PersonToken = "PersonToken";
            public const string PersonActionIdentifier = "PersonActionIdentifier";
            public const string TransactionError = "TransactionError";
        }

        #endregion Keys

        #region Properties

        /// <summary>
        /// Gets the component title to be displayed to the user.
        /// </summary>
        /// <value>
        /// The component title to be displayed to the user.
        /// </value>
        public override string Title => "Give";

        /// <summary>
        /// Gets the icon CSS class used to identify this component type.
        /// </summary>
        /// <value>
        /// The icon CSS class used to identify this component type.
        /// </value>
        public override string IconCssClass => "fa fa-dollar";

        /// <summary>
        /// Gets the description of this SMS Action.
        /// </summary>
        /// <value>
        /// The description of this SMS Action.
        /// </value>
        public override string Description => "Allows an SMS sender to make a payment and get a refund.";

        #endregion Properties

        #region Base Method Overrides

        /// <summary>
        /// Checks the attributes for this component and determines if the message
        /// should be processed.
        /// </summary>
        /// <param name="action">The action that contains the configuration for this component.</param>
        /// <param name="message">The message that is to be checked.</param>
        /// <param name="errorMessage">If there is a problem processing, this should be set</param>
        /// <returns>
        ///   <c>true</c> if the message should be processed.
        /// </returns>
        public override bool ShouldProcessMessage( SmsActionCache action, SmsMessage message, out string errorMessage )
        {
            errorMessage = string.Empty;

            if ( action == null || message == null )
            {
                errorMessage = "Cannot handle null action or null message";
                return false;
            }

            // Give the base class a chance to check it's own settings to see if we
            // should process this message.
            if ( !base.ShouldProcessMessage( action, message, out errorMessage ) )
            {
                return false;
            }

            var context = new SmsGiveContext( action, message );
            return context.IsGiveMessage || context.IsRefundMessage || context.IsSetupMessage || context.IsHelpMessage;
        }

        /// <summary>
        /// Processes the message that was received from the remote user.
        /// </summary>
        /// <param name="action">The action that contains the configuration for this component.</param>
        /// <param name="message">The message that was received by Rock.</param>
        /// <param name="errorMessage">If there is a problem processing, this should be set</param>
        /// <returns>An SmsMessage that will be sent as the response or null if no response should be sent.</returns>
        public override SmsMessage ProcessMessage( SmsActionCache action, SmsMessage message, out string errorMessage )
        {
            errorMessage = string.Empty;

            if ( action == null || message == null )
            {
                errorMessage = "Cannot handle null action or null message";
                return null;
            }

            var context = new SmsGiveContext( action, message );

            if ( context.IsGiveMessage )
            {
                return DoGift( context, out errorMessage );
            }
            else if ( context.IsRefundMessage )
            {
                return DoRefund( context, out errorMessage );
            }
            else if ( context.IsSetupMessage )
            {
                return DoSetup( context, out errorMessage );
            }
            else if ( context.IsHelpMessage )
            {
                return DoHelp( context, out errorMessage );
            }
            else
            {
                errorMessage = "The message was not a giving related request.";
                return null;
            }
        }

        #endregion Base Method Overrides

        #region Setup

        /// <summary>
        /// Process a setup link request.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="errorMessage"></param>
        private SmsMessage DoSetup( SmsGiveContext context, out string errorMessage )
        {
            errorMessage = string.Empty;

            // Get any configured account id
            if ( context.SmsActionCache.GetAttributeValue( AttributeKeys.FinancialAccount ).IsNotNullOrWhiteSpace() )
            {
                var financialAccount = GetFinancialAccount( context );
                context.LavaMergeFields[LavaMergeFieldKeys.AccountId] = financialAccount?.Id;
            }

            CreatePersonRecordIfNeeded( context );
            SetPersonIdentifier( context );
            SetSetupPageLink( context );

            return GetResolvedSmsResponse( AttributeKeys.SetupResponse, context );
        }

        #endregion Setup

        #region Help

        /// <summary>
        /// Process a help request.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="errorMessage"></param>
        private SmsMessage DoHelp( SmsGiveContext context, out string errorMessage )
        {
            errorMessage = string.Empty;

            CreatePersonRecordIfNeeded( context );
            SetPersonIdentifier( context );
            SetSetupPageLink( context );

            return GetResolvedSmsResponse( AttributeKeys.HelpResponse, context );
        }

        #endregion Help

        #region Giving

        /// <summary>
        /// Process a text gift.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="errorMessage"></param>
        private SmsMessage DoGift( SmsGiveContext context, out string errorMessage )
        {
            errorMessage = string.Empty;

            var giftAmountNullable = GetGiftAmount( context );
            var maxAmount = GetMaxAmount( context );

            CreatePersonRecordIfNeeded( context );
            var defaultSavedAccount = GetDefaultSavedAccount( context );
            var financialAccount = GetFinancialAccount( context );
            context.LavaMergeFields[LavaMergeFieldKeys.AccountId] = financialAccount?.Id;

            SetPersonIdentifier( context );
            SetSetupPageLink( context );

            // If the person doesn't have a configured account designation, or the person
            // doesn't have a default saved payment method, send the "setup" response.
            if ( defaultSavedAccount == null || financialAccount == null )
            {
                return GetResolvedSmsResponse( AttributeKeys.SetupResponse, context );
            }

            // If the amount is not valid, send the "missing amount" response.
            if ( !giftAmountNullable.HasValue || giftAmountNullable.Value < 1m )
            {
                return GetResolvedSmsResponse( AttributeKeys.MissingAmountResponse, context );
            }

            // If the gift amount exceeds the max amount, send the "max amount" response.
            var giftAmount = giftAmountNullable.Value;
            var exceedsMax = maxAmount.HasValue && giftAmount > maxAmount.Value;

            if ( exceedsMax )
            {
                return GetResolvedSmsResponse( AttributeKeys.MaxAmountResponse, context );
            }

            // Validation has passed so prepare the automated payment processor args to charge the payment.
            var automatedPaymentArgs = new AutomatedPaymentArgs
            {
                AuthorizedPersonAliasId = context.SmsMessage.FromPerson.PrimaryAliasId.Value,
                AutomatedGatewayId = defaultSavedAccount.FinancialGatewayId.Value,
                FinancialPersonSavedAccountId = defaultSavedAccount.Id,
                FinancialSourceGuid = Rock.SystemGuid.DefinedValue.FINANCIAL_SOURCE_TYPE_SMS_GIVE.AsGuidOrNull(),
                AutomatedPaymentDetails = new List<AutomatedPaymentArgs.AutomatedPaymentDetailArgs>
                {
                    new AutomatedPaymentArgs.AutomatedPaymentDetailArgs {
                        AccountId = financialAccount.Id,
                        Amount = giftAmount
                    }
                }
            };

            // Determine if this is a future transaction and when it should be processed.
            var minutesDelay = GetDelayMinutes( context );

            if ( minutesDelay.HasValue && minutesDelay > 0 )
            {
                var delayTimeSpan = TimeSpan.FromMinutes( minutesDelay.Value );
                automatedPaymentArgs.FutureProcessingDateTime = RockDateTime.Now.Add( delayTimeSpan );
            }

            // Create the processor.
            var automatedPaymentProcessor = new AutomatedPaymentProcessor( null, automatedPaymentArgs, context.RockContext );

            // If the args are not valid, send the setup response.
            if ( !automatedPaymentProcessor.AreArgsValid( out errorMessage ) )
            {
                return GetResolvedSmsResponse( AttributeKeys.SetupResponse, context );
            }

            // If charge seems like a duplicate or repeat, tell the sender.
            if ( automatedPaymentProcessor.IsRepeatCharge( out errorMessage ) )
            {
                return GetSmsResponse( context, "It looks like you've given very recently. In order for us to avoid accidental charges, please wait several minutes before giving again. Thank you!" );
            }

            // Charge the payment.
            var transaction = automatedPaymentProcessor.ProcessCharge( out errorMessage );

            // If the transaction did not process (possibly a gateway error), send the "transaction failure" response.
            if ( transaction == null || !string.IsNullOrEmpty( errorMessage ) )
            {
                context.LavaMergeFields[LavaMergeFieldKeys.TransactionError] = errorMessage;
                return GetResolvedSmsResponse( AttributeKeys.TransactionFailureResponse, context );
            }

            // Tag the transaction's summary with info from this action.
            transaction.Summary = string.Format( "{0}{1}Text To Give from {2} with the message `{3}`",
                transaction.Summary,
                transaction.Summary.IsNullOrWhiteSpace() ? string.Empty : ". ",
                context.SmsMessage.FromNumber,
                context.MessageText );
            context.RockContext.SaveChanges();

            // Let the sender know that the gift was a success.
            return GetResolvedSmsResponse( AttributeKeys.SuccessResponse, context );
        }

        #endregion Giving

        #region Refund

        /// <summary>
        /// Handles the action of giving a refund if the message requests it. Only future transactions can be refunded.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="errorMessage"></param>
        private SmsMessage DoRefund( SmsGiveContext context, out string errorMessage )
        {
            errorMessage = string.Empty;
            var service = new FinancialTransactionService( context.RockContext );
            var futureTransactionToDelete = GetMostRecentFutureTransactionToDelete( context, service );
            SetPersonIdentifier( context );

            if ( futureTransactionToDelete == null )
            {
                return GetResolvedSmsResponse( AttributeKeys.RefundFailureResponse, context );
            }

            // If there is a future transaction, it can simply be deleted since it has not been processed yet
            var success = service.Delete( futureTransactionToDelete );
            if ( !success )
            {
                return GetResolvedSmsResponse( AttributeKeys.RefundFailureResponse, context );
            }

            context.RockContext.SaveChanges();
            return GetResolvedSmsResponse( AttributeKeys.RefundSuccessResponse, context );
        }

        #endregion Refund

        #region Model Helpers

        /// <summary>
        /// Get the future transaction to delete and sync the context's merge fields.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="service">The service.</param>
        /// <returns>FinancialTransaction.</returns>
        private FinancialTransaction GetMostRecentFutureTransactionToDelete( SmsGiveContext context, FinancialTransactionService service )
        {
            var fromPerson = context.SmsMessage.FromPerson;

            if ( fromPerson == null || !fromPerson.PrimaryAliasId.HasValue )
            {
                context.LavaMergeFields[LavaMergeFieldKeys.Amount] = string.Empty;
                context.LavaMergeFields[LavaMergeFieldKeys.AccountName] = string.Empty;
                return null;
            }

            var smsGiftSourceId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.FINANCIAL_SOURCE_TYPE_SMS_GIVE ).Id;
            var futureTransactionToDelete = service.GetFutureTransactions()
                .Where( ft =>
                    ft.SourceTypeValueId == smsGiftSourceId
                    && ft.AuthorizedPersonAliasId == fromPerson.PrimaryAliasId )
                .OrderByDescending( ft => ft.FutureProcessingDateTime )
                .FirstOrDefault();

            if ( futureTransactionToDelete == null || !service.CanDelete( futureTransactionToDelete, out var errorMessage ) )
            {
                context.LavaMergeFields[LavaMergeFieldKeys.Amount] = string.Empty;
                context.LavaMergeFields[LavaMergeFieldKeys.AccountName] = string.Empty;
                return null;
            }

            // Set the merge fields and then return the transaction
            context.LavaMergeFields[LavaMergeFieldKeys.Amount] = futureTransactionToDelete.TotalAmount;
            var firstDetail = futureTransactionToDelete.TransactionDetails.FirstOrDefault();
            context.LavaMergeFields[LavaMergeFieldKeys.AccountId] = firstDetail.AccountId;
            context.LavaMergeFields[LavaMergeFieldKeys.AccountName] = firstDetail.Account?.PublicName ?? string.Empty;

            return futureTransactionToDelete;
        }

        /// <summary>
        /// Create a new person with the phone number in the SMS message if a person does not already exist.
        /// </summary>
        /// <param name="context"></param>
        private void CreatePersonRecordIfNeeded( SmsGiveContext context )
        {
            if ( context.SmsMessage.FromPerson != null )
            {
                return;
            }

            // If the person is unknown (meaning Rock doesn't have the number, create a new person, store the number
            // and then tie future SMS gifts to this new person
            var person = new Person();
            PersonService.SaveNewPerson( person, context.RockContext );

            var numberTypeId = DefinedValueCache.Get( new Guid( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_MOBILE ) ).Id;
            person.PhoneNumbers.Add( new PhoneNumber
            {
                NumberTypeValueId = numberTypeId,
                Number = PhoneNumber.CleanNumber( context.SmsMessage.FromNumber ),
                IsMessagingEnabled = true
            } );

            context.RockContext.SaveChanges();
            context.SmsMessage.FromPerson = person;
        }

        /// <summary>
        /// Get the person's default saved account if they have one.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns>FinancialPersonSavedAccount.</returns>
        private FinancialPersonSavedAccount GetDefaultSavedAccount( SmsGiveContext context )
        {
            if ( context.SmsMessage.FromPerson == null )
            {
                return null;
            }

            var personSavedAccountsQry = new FinancialPersonSavedAccountService( context.RockContext )
                .GetByPersonId( context.SmsMessage.FromPerson.Id ).Where( a => a.FinancialGatewayId.HasValue );

            var defaultAccount = personSavedAccountsQry.Where( sa => sa.IsDefault ).FirstOrDefault();

            return defaultAccount;
        }

        #endregion Model Helpers

        #region Parsing Helpers

        /// <summary>
        /// Parse the gift amount from the message text.  Expected format is something like "{{keyword}} {{amount}}".
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        private decimal? GetGiftAmount( SmsGiveContext context )
        {
            var messageText = context.MessageText;
            var keyword = context.MatchingGiveKeyword;
            var textWithoutKeyword = keyword.IsNullOrWhiteSpace() ? messageText : Regex.Replace( messageText, keyword, string.Empty, RegexOptions.IgnoreCase ).Trim();

            // First try to parse a decimal like "1123.56".
            var successfulParse = decimal.TryParse( textWithoutKeyword, out var parsedValue );

            // Second try to parse currency like "$1,123.56".
            if ( !successfulParse )
            {
                successfulParse = decimal.TryParse( textWithoutKeyword, NumberStyles.Currency, CultureInfo.CurrentCulture, out parsedValue );
            }

            if ( successfulParse )
            {
                var amount = decimal.Round( parsedValue, 2 );
                context.LavaMergeFields[LavaMergeFieldKeys.Amount] = amount;
                return amount;
            }

            context.LavaMergeFields[LavaMergeFieldKeys.Amount] = string.Empty;
            return null;
        }

        #endregion Parsing Helpers

        #region Attribute Helpers

        /// <summary>
        /// Take the lava template, resolve it with useful text-to-give fields, and generate an SMS object to respond with.
        /// </summary>
        /// <param name="templateAttributeKey"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        private SmsMessage GetResolvedSmsResponse( string templateAttributeKey, SmsGiveContext context )
        {
            var lavaTemplate = context.SmsActionCache.GetAttributeValue( templateAttributeKey );
            var resolvedMessage = lavaTemplate.ResolveMergeFields( context.LavaMergeFields );
            return GetSmsResponse( context, resolvedMessage );
        }

        /// <summary>
        /// Generate an SMS response object to the message in the context with the specified message text.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        private SmsMessage GetSmsResponse( SmsGiveContext context, string message )
        {
            return new SmsMessage
            {
                ToNumber = context.SmsMessage.FromNumber,
                FromNumber = context.SmsMessage.ToNumber,
                Message = message
            };
        }

        #endregion Attribute Helpers

        #region Giving Attribute Getters

        /// <summary>
        /// Get and validate the max amount attribute.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns>System.Nullable&lt;System.Decimal&gt;.</returns>
        private decimal? GetMaxAmount( SmsGiveContext context )
        {
            var maxAmountString = context.SmsActionCache.GetAttributeValue( AttributeKeys.MaxAmount );
            return maxAmountString.AsDecimalOrNull();
        }

        /// <summary>
        /// Get and validate the financial account attribute. If the attribute is omitted, then the person's default account is used.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns>FinancialAccount.</returns>
        private FinancialAccount GetFinancialAccount( SmsGiveContext context )
        {
            var person = context.SmsMessage.FromPerson;
            var rootAccountGuid = context.SmsActionCache.GetAttributeValue( AttributeKeys.FinancialAccount ).AsGuidOrNull();

            if ( !rootAccountGuid.HasValue )
            {
                // No setting was specified, so use the person's default account setting.
                context.LavaMergeFields[LavaMergeFieldKeys.AccountName] = person?.ContributionFinancialAccount?.PublicName ?? string.Empty;
                return person?.ContributionFinancialAccount;
            }

            var financialAccountService = new FinancialAccountService( context.RockContext );
            var campusId = person?.GetFamily( context.RockContext )?.CampusId;
            FinancialAccount financialAccount = null;

            if ( campusId.HasValue )
            {
                financialAccount = financialAccountService.Queryable()
                    .AsNoTracking()
                    .FirstOrDefault( fa =>
                        fa.IsActive &&
                        fa.IsPublic == true &&
                        fa.CampusId == campusId.Value &&
                        fa.ParentAccount.Guid == rootAccountGuid.Value );
            }

            if ( financialAccount == null )
            {
                financialAccount = financialAccountService.Get( rootAccountGuid.Value );
            }

            context.LavaMergeFields[LavaMergeFieldKeys.AccountId] = financialAccount.Id;
            context.LavaMergeFields[LavaMergeFieldKeys.AccountName] = financialAccount?.PublicName ?? string.Empty;
            return financialAccount;
        }

        /// <summary>
        /// Set the setup page link on the context.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        private void SetSetupPageLink( SmsGiveContext context )
        {
            var setupPageAttribute = context.SmsActionCache.GetAttributeValue( AttributeKeys.SetupPage );
            if ( setupPageAttribute.IsNullOrWhiteSpace() )
            {
                context.LavaMergeFields[LavaMergeFieldKeys.SetupLink] = string.Empty;
                return;
            }

            var setupPage = new Rock.Web.PageReference( setupPageAttribute );
            if ( setupPage == null )
            {
                context.LavaMergeFields[LavaMergeFieldKeys.SetupLink] = string.Empty;
                return;
            }

            var personToken = context.LavaMergeFields.GetValueOrNull( LavaMergeFieldKeys.PersonToken ).ToStringSafe();
            if ( !personToken.IsNullOrWhiteSpace() )
            {
                setupPage.Parameters["Person"] = personToken;
            }

            var personActionIdentifier = context.LavaMergeFields.GetValueOrNull( LavaMergeFieldKeys.PersonActionIdentifier ).ToStringSafe();
            if ( !personToken.IsNullOrWhiteSpace() )
            {
                setupPage.Parameters["rckid"] = personActionIdentifier;
            }

            // AccountIds is in the format AccountId^Amount^Enabled, where Enabled indicates if the amount can be changed or is presented as readonly.
            var accountId = context.LavaMergeFields.GetValueOrNull( LavaMergeFieldKeys.AccountId ).ToStringSafe()?.AsIntegerOrNull();
            if ( accountId.HasValue && FinancialAccountCache.Get( accountId.Value ) != null )
            {
                var amount = context.LavaMergeFields.GetValueOrNull( LavaMergeFieldKeys.Amount ).ToStringSafe();

                string accountIdsParameter = $"{accountId}^{amount}^{false}";

                if ( amount.IsNullOrWhiteSpace() )
                {
                    // If no amount was specified, we need to allow the user to enter it on the setup page.
                    accountIdsParameter = $"{accountId}^{amount}^{true}";
                }

                setupPage.Parameters["AccountIds"] = accountIdsParameter;
            }

            // If BuildUrl() throws an exception, it could be because we are in an async task and
            // there is no HttpContext.Current. If so, make sure the caller sets HttpContext.Current before launching the task.
            // Or fix setupPage.BuildUrl so it doesn't require HttpContext.Current.
            try
            {
                context.LavaMergeFields[LavaMergeFieldKeys.SetupLink] = setupPage.BuildUrl();
            }
            catch
            {
                context.LavaMergeFields[LavaMergeFieldKeys.SetupLink] = string.Empty;
            }
        }

        /// <summary>
        /// Set the person token on the context.
        /// </summary>
        /// <param name="context"></param>
        private void SetPersonIdentifier( SmsGiveContext context )
        {
            var person = context.SmsMessage.FromPerson;

            if ( person == null )
            {
                context.LavaMergeFields[LavaMergeFieldKeys.PersonToken] = string.Empty;
                return;
            }

            // create a limited-use person key that will last long enough for them to go through all the postbacks while posting a transaction.
            const int expiresInMinutes = 30;
            var expiresDateTime = RockDateTime.Now.AddMinutes( expiresInMinutes );
            var personKey = person.GetImpersonationToken( expiresDateTime, null, null );
            if ( !personKey.IsNullOrWhiteSpace() )
            {
                context.LavaMergeFields[LavaMergeFieldKeys.PersonToken] = personKey;
            }
            else
            {
                context.LavaMergeFields[LavaMergeFieldKeys.PersonToken] = string.Empty;
            }

            // We'll also need a personActionIdentifier for blocks that use that instead.
            var personActionIdentifier = person.GetPersonActionIdentifier( "transaction" );

            if ( !personActionIdentifier.IsNullOrWhiteSpace() )
            {
                context.LavaMergeFields[LavaMergeFieldKeys.PersonActionIdentifier] = personActionIdentifier;
            }
            else
            {
                context.LavaMergeFields[LavaMergeFieldKeys.PersonActionIdentifier] = string.Empty;
            }
        }

        #endregion Giving Attribute Getters

        #region Refund Attribute Getters

        /// <summary>
        /// Get and validate the delay minutes attribute.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns>System.Nullable&lt;System.Int32&gt;.</returns>
        private int? GetDelayMinutes( SmsGiveContext context )
        {
            return context.SmsActionCache.GetAttributeValue( AttributeKeys.ProcessingDelayMinutes ).AsIntegerOrNull();
        }

        #endregion Refund Attribute Getters

        #region Helper Classes

        /// <summary>
        /// Defined a context of an SMS give action. A context is the handling of a single SMS message. The reason this context
        /// class was created is to provide readonly access to the original parameters to the action and their common derivatives
        /// as these are always passed around.
        /// </summary>
        private class SmsGiveContext
        {
            /// <summary>
            /// Constructor for the SMS context.
            /// </summary>
            /// <param name="smsActionCache"></param>
            /// <param name="smsMessage"></param>
            public SmsGiveContext( SmsActionCache smsActionCache, SmsMessage smsMessage )
            {
                SmsActionId = smsActionCache?.Id ?? throw new ArgumentNullException( "smsActionCache" );
                SmsMessage = smsMessage ?? throw new ArgumentNullException( "smsMessage" );

                MessageText = ( smsMessage.Message ?? string.Empty ).Trim();

                PrimaryKeyword =
                    IsGiveMessage ? GivingKeywords.FirstOrDefault() :
                    IsRefundMessage ? RefundKeywords.FirstOrDefault() :
                    IsSetupMessage ? SetupKeywords.FirstOrDefault() :
                    IsHelpMessage ? HelpKeywords.FirstOrDefault() :
                    string.Empty;

                LavaMergeFields = new Dictionary<string, object> {
                    { LavaMergeFieldKeys.Message, SmsMessage },
                    { LavaMergeFieldKeys.Keyword, PrimaryKeyword },
                    { LavaMergeFieldKeys.Amount, string.Empty },
                    { LavaMergeFieldKeys.AccountId, string.Empty },
                    { LavaMergeFieldKeys.AccountName, string.Empty },
                    { LavaMergeFieldKeys.SetupLink, string.Empty },
                    { LavaMergeFieldKeys.PersonActionIdentifier, string.Empty },
                    { LavaMergeFieldKeys.PersonToken, string.Empty },
                    { LavaMergeFieldKeys.TransactionError, string.Empty }
                };
            }

            /// <summary>
            /// The SMS action that is handling the SMS processing.
            /// </summary>
            public SmsActionCache SmsActionCache => SmsActionCache.Get( SmsActionId );

            private int SmsActionId { get; set; }

            /// <summary>
            /// The object representing the SMS.
            /// </summary>
            public SmsMessage SmsMessage { get; private set; }

            /// <summary>
            /// The text of the SMS message.
            /// </summary>
            public string MessageText { get; private set; }

            /// <summary>
            /// The primary keyword. If this is a refund request and the refund keywords are "refund, oops, undo", then the primary is "refund"
            /// since it is first.
            /// </summary>
            public string PrimaryKeyword { get; private set; }

            /// <summary>
            /// The merge fields that will be used in the SMS response.
            /// </summary>
            public Dictionary<string, object> LavaMergeFields { get; private set; }

            /// <summary>
            /// Get the rock context to use for this giving message.
            /// </summary>
            public RockContext RockContext
            {
                get => _rockContext ?? ( _rockContext = new RockContext() );
            }
            private RockContext _rockContext = null;

            /// <summary>
            /// Get the give keywords.
            /// </summary>
            /// <value>The giving keywords.</value>
            public List<string> GivingKeywords
            {
                get => _givingKeywords ?? ( _givingKeywords = GetKeywords( AttributeKeys.GivingKeywords ) );
            }
            private List<string> _givingKeywords = null;

            /// <summary>
            /// True if one of the give keywords matched.
            /// </summary>
            public bool IsGiveMessage
            {
                // If they provided a GIVE keyword, that indicates a GIVE message,
                // but also magically treat a simple text starting with $ as a GIVE.
                // For example: $150 should give $150.
                get => !MatchingGiveKeyword.IsNullOrWhiteSpace() || MessageText.StartsWith( "$" );
            }

            /// <summary>
            /// The give keyword that matched the message.
            /// </summary>
            public string MatchingGiveKeyword
            {
                get => GivingKeywords.FirstOrDefault( k => MessageText.StartsWith( k, StringComparison.CurrentCultureIgnoreCase ) );
            }

            /// <summary>
            /// Get the refund keywords.
            /// </summary>
            /// <value>The refund keywords.</value>
            public List<string> RefundKeywords
            {
                get => _refundKeywords ?? ( _refundKeywords = GetKeywords( AttributeKeys.RefundKeywords ) );
            }
            private List<string> _refundKeywords = null;

            /// <summary>
            /// True if the message is a refund message.
            /// </summary>
            public bool IsRefundMessage
            {
                get => !MatchingRefundKeyword.IsNullOrWhiteSpace();
            }

            /// <summary>
            /// The refund keyword that matched the message.
            /// </summary>
            public string MatchingRefundKeyword
            {
                get => RefundKeywords.FirstOrDefault( k => MessageText.Equals( k, StringComparison.CurrentCultureIgnoreCase ) );
            }

            /// <summary>
            /// Get the setup keywords.
            /// </summary>
            /// <value>The setup keywords.</value>
            public List<string> SetupKeywords
            {
                get => _setupKeywords ?? ( _setupKeywords = GetKeywords( AttributeKeys.SetupKeywords ) );
            }
            private List<string> _setupKeywords = null;

            /// <summary>
            /// True if the message is a setup message.
            /// </summary>
            public bool IsSetupMessage
            {
                get => !MatchingSetupKeyword.IsNullOrWhiteSpace();
            }

            /// <summary>
            /// The setup keyword that matched the message.
            /// </summary>
            public string MatchingSetupKeyword
            {
                get => SetupKeywords.FirstOrDefault( k => MessageText.Equals( k, StringComparison.CurrentCultureIgnoreCase ) );
            }

            /// <summary>
            /// Get the help keywords.
            /// </summary>
            /// <value>The help keywords.</value>
            public List<string> HelpKeywords
            {
                get => _helpKeywords;
            }
            private List<string> _helpKeywords = new List<string> { "HELP" };

            /// <summary>
            /// True if the message is a help message.
            /// </summary>
            public bool IsHelpMessage
            {
                get => !MatchingHelpKeyword.IsNullOrWhiteSpace();
            }

            /// <summary>
            /// The help keyword that matched the message.
            /// </summary>
            public string MatchingHelpKeyword
            {
                get => HelpKeywords.FirstOrDefault( k => MessageText.Equals( k, StringComparison.CurrentCultureIgnoreCase ) );
            }

            /// <summary>
            /// Get keywords from an attribute value.
            /// </summary>
            /// <param name="attributeKey"></param>
            /// <returns></returns>
            private List<string> GetKeywords( string attributeKey )
            {
                return ( SmsActionCache.GetAttributeValue( attributeKey ) ?? string.Empty )
                    .SplitDelimitedValues( false )
                    .Where( k => !k.IsNullOrWhiteSpace() )
                    .Select( k => k.Trim() )
                    .ToList();
            }
        }

        #endregion Helper Classes
    }
}