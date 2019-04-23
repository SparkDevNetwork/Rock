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
using Rock.Attribute;
using Rock.Data;
using Rock.Financial;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace Rock.Communication.SmsActions
{
    [Description( "Allows an SMS sender to make a payment." )]
    [Export( typeof( SmsActionComponent ) )]
    [ExportMetadata( "ComponentName", "Give" )]

    #region Attributes

    [TextField(
        name: "Give Keyword",
        description: "The case-insensitive keyword that will be expected at the beginning of the message.",
        required: true,
        defaultValue: "GIVE",
        order: 1,
        category: "Giving",
        key: AttributeKeys.GivingKeyword )]

    [TextField(
        name: "Setup Keyword",
        description: "The case-insensitive keyword that will be expected at the beginning of the message.",
        required: true,
        defaultValue: "SETUP",
        order: 2,
        category: "Giving",
        key: AttributeKeys.SetupKeyword )]

    [CurrencyField(
        name: "Maximum Gift Amount",
        description: "Leave blank to allow gifts of any size.",
        required: false,
        order: 3,
        category: "Giving",
        key: AttributeKeys.MaxAmount )]

    [AccountField(
        name: "Financial Account",
        description: "The financial account to designate gifts toward. Leave blank to use the person's default giving designation.",
        required: false,
        category: "Giving",
        order: 4,
        key: AttributeKeys.FinancialAccount )]

    [LinkedPage(
        name: "Setup Page",
        description: "The page to link with a token for the person to setup their SMS giving",
        required: false,
        defaultValue: SystemGuid.Page.TEXT_TO_GIVE_SETUP + "," + SystemGuid.PageRoute.TEXT_TO_GIVE_SETUP,
        category: "Giving",
        order: 5,
        key: AttributeKeys.SetupPage )]

    [TextField(
        name: "Refund Keyword",
        description: "The case-insensitive keyword that is expected to trigger the refund. Leave blank to disable SMS refunds.",
        required: false,
        defaultValue: "REFUND",
        order: 6,
        category: "Refund",
        key: AttributeKeys.RefundKeyword )]

    [IntegerField(
        name: "Processing Delay Minutes",
        description: "The number of minutes to delay processing the gifts and allow refunds (if the refund keyword is set). Delaying allows SMS requested refunds to completely bypass the financial gateway. Set to zero or leave blank to process gifts immediately and disallow refunds.",
        required: false,
        defaultValue: 30,
        order: 7,
        category: "Refund",
        key: AttributeKeys.ProcessingDelayMinutes )]

    [CodeEditorField(
        mode: CodeEditorMode.Lava,
        theme: CodeEditorTheme.Rock,
        name: "Help Response",
        description: "The response that will be sent if the sender's message doesn't make sense, there is missing information, or an error occurs. <span class='tip tip-lava'></span> Use {{ Lava | Debug }} to see all available fields.",
        required: true,
        defaultValue: "Something went wrong. To give, simply text ‘{{ Keyword }} 100’ or ‘{{ Keyword }} $123.45’. Please contact us if you need help.",
        order: 8,
        category: "Response",
        key: AttributeKeys.HelpResponse )]

    [CodeEditorField(
        mode: CodeEditorMode.Lava,
        theme: CodeEditorTheme.Rock,
        name: "Max Amount Response",
        description: "The response that will be sent if the sender is trying to give more than the max amount (if configured). <span class='tip tip-lava'></span> Use {{ Lava | Debug }} to see all available fields.",
        required: false,
        defaultValue: "Thank you for your generosity but our mobile giving solution cannot process a gift this large. Please give using our website.",
        order: 9,
        category: "Response",
        key: AttributeKeys.MaxAmountResponse )]

    [CodeEditorField(
        mode: CodeEditorMode.Lava,
        theme: CodeEditorTheme.Rock,
        name: "Setup Response",
        description: "The response that will be sent if the sender is unknown, does not have a saved account, or requests to edit their giving profile. <span class='tip tip-lava'></span> Use {{ Lava | Debug }} to see all available fields.",
        required: true,
        defaultValue: "Welcome! Let's set up your device to securely give using this link: {{ SetupLink | CreateShortLink }}",
        order: 10,
        category: "Response",
        key: AttributeKeys.SetupResponse )]

    [CodeEditorField(
        mode: CodeEditorMode.Lava,
        theme: CodeEditorTheme.Rock,
        name: "Success Response",
        description: "The response that will be sent if the payment is successful. <span class='tip tip-lava'></span> Use {{ Lava | Debug }} to see all available fields.",
        required: true,
        defaultValue: "Thank you! We received your gift of {{ Amount }} to the {{ AccountName }}.",
        order: 11,
        category: "Response",
        key: AttributeKeys.SuccessResponse )]

    [CodeEditorField(
        mode: CodeEditorMode.Lava,
        theme: CodeEditorTheme.Rock,
        name: "Refund Failure Response",
        description: "The response that will be sent if the sender's gift cannot be refunded. <span class='tip tip-lava'></span> Use {{ Lava | Debug }} to see all available fields.",
        required: true,
        defaultValue: "We are unable to process a refund for your last gift. Please contact us for assistance.",
        order: 12,
        category: "Response",
        key: AttributeKeys.RefundFailureResponse )]

    [CodeEditorField(
        mode: CodeEditorMode.Lava,
        theme: CodeEditorTheme.Rock,
        name: "Refund Success Response",
        description: "The response that will be sent if the refund is successful. <span class='tip tip-lava'></span> Use {{ Lava | Debug }} to see all available fields.",
        required: true,
        defaultValue: "Your gift for {{ Amount }} to the {{ AccountName }} has been refunded.",
        order: 13,
        category: "Response",
        key: AttributeKeys.RefundSuccessResponse )]

    #endregion

    public class SmsActionGive : SmsActionComponent
    {
        private static class AttributeKeys
        {
            // Giving
            public const string SetupKeyword = "SetupKeyword";
            public const string GivingKeyword = "GivingKeyword";
            public const string MaxAmount = "MaxAmount";
            public const string MaxAmountResponse = "MaxAmountResponse";
            public const string FinancialAccount = "FinancialAccount";
            public const string SetupPage = "SetupPage";

            // Refund
            public const string ProcessingDelayMinutes = "ProcessingDelayMinutes";
            public const string RefundKeyword = "RefundKeyword";

            // Responses
            public const string HelpResponse = "HelpResponse";
            public const string SetupResponse = "SetupResponse";
            public const string SuccessResponse = "SuccessResponse";
            public const string RefundSuccessResponse = "RefundSuccessResponse";
            public const string RefundFailureResponse = "RefundFailureResponse";
        }

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

        #endregion

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

            if ( action == null || message == null || message.Message.IsNullOrWhiteSpace() )
            {
                errorMessage = "Cannot handle null action, null message, or empty message text";
                return false;
            }

            // Give the base class a chance to check it's own settings to see if we
            // should process this message.
            if ( !base.ShouldProcessMessage( action, message, out errorMessage ) )
            {
                return false;
            }

            var messageText = message.Message.Trim();
            return IsGivingMessage( action, messageText ) || IsRefundMessage( action, messageText ) || IsSetupMessage( action, messageText );
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
            var messageText = message.Message.Trim();

            if ( IsGivingMessage( action, messageText ) )
            {
                return DoGift( action, message, out errorMessage );
            }
            else if ( IsRefundMessage( action, messageText ) )
            {
                return DoRefund( action, message, out errorMessage );
            }
            else if ( IsSetupMessage( action, messageText ) )
            {
                return DoSetup( action, message, out errorMessage );
            }
            else
            {
                errorMessage = "The message was not a giving or refund request.";
                return null;
            }
        }

        #endregion

        #region Setup

        /// <summary>
        /// Return true if the message text is requesting setup
        /// </summary>
        /// <param name="action"></param>
        /// <param name="messageText"></param>
        /// <returns></returns>
        public bool IsSetupMessage( SmsActionCache action, string messageText )
        {
            var keyword = GetSetupKeyword( action );

            if ( keyword.IsNullOrWhiteSpace() )
            {
                return false;
            }

            return messageText.Equals( keyword, StringComparison.CurrentCultureIgnoreCase );
        }

        /// <summary>
        /// Process a setup link request
        /// </summary>
        /// <param name="action"></param>
        /// <param name="message"></param>
        /// <param name="errorMessage"></param>
        /// <returns></returns>
        private SmsMessage DoSetup( SmsActionCache action, SmsMessage message, out string errorMessage )
        {
            errorMessage = string.Empty;
            var rockContext = new RockContext();

            var setupKeyword = GetSetupKeyword( action );
            var person = message.FromPerson ?? CreateNewPerson( rockContext, message );
            var setupLink = GetSetupPageLink( action, person );

            var lavaTemplate = GetSetupResponse( action );
            return GetResolvedSmsResponse( lavaTemplate, setupKeyword, message, null, null, setupLink );
        }

        #endregion Setup

        #region Giving

        /// <summary>
        /// Determine if the message has the giving keyword
        /// </summary>
        /// <param name="action"></param>
        /// <param name="messageText"></param>
        /// <returns></returns>
        private bool IsGivingMessage( SmsActionCache action, string messageText )
        {
            var givingKeyword = GetGivingKeyword( action );
            return messageText.StartsWith( givingKeyword, StringComparison.CurrentCultureIgnoreCase );
        }

        /// <summary>
        /// Process a gift if the sender requests it
        /// </summary>
        /// <param name="action"></param>
        /// <param name="message"></param>
        /// <param name="errorMessage"></param>
        /// <returns></returns>
        private SmsMessage DoGift( SmsActionCache action, SmsMessage message, out string errorMessage )
        {
            errorMessage = string.Empty;
            var rockContext = new RockContext();

            var messageText = message.Message.Trim();
            var givingKeyword = GetGivingKeyword( action );
            var giftAmountNullable = GetGiftAmount( givingKeyword, messageText );
            var maxAmount = GetMaxAmount( action );

            var person = message.FromPerson ?? CreateNewPerson( rockContext, message );
            var defaultSavedAccount = GetDefaultSavedAccount( rockContext, person );

            var financialAccount = GetFinancialAccount( action, rockContext );

            if ( financialAccount == null && person != null )
            {
                financialAccount = person.ContributionFinancialAccount;
            }

            var setupLink = GetSetupPageLink( action, person );

            // If the number is not recognized, the person doesn't have a configured account designation, or the person
            // doesn't have a default saved payment method, send the "setup" response
            if ( defaultSavedAccount == null || person == null || financialAccount == null || !person.PrimaryAliasId.HasValue )
            {
                var lavaTemplate = GetSetupResponse( action );
                return GetResolvedSmsResponse( lavaTemplate, givingKeyword, message, null, financialAccount, setupLink );
            }

            // If the amount is not valid (missing, not valid decimal, or not valid currency format), send back the
            // "help" response
            if ( !giftAmountNullable.HasValue || giftAmountNullable.Value < 1m )
            {
                var lavaTemplate = GetHelpResponse( action );
                return GetResolvedSmsResponse( lavaTemplate, givingKeyword, message, null, financialAccount, setupLink );
            }

            // If the gift amount exceeds the max amount, send the "max amount" response
            var giftAmount = giftAmountNullable.Value;
            var exceedsMax = maxAmount.HasValue && giftAmount > maxAmount.Value;

            if ( exceedsMax )
            {
                var lavaTemplate = GetMaxAmountResponse( action );
                return GetResolvedSmsResponse( lavaTemplate, givingKeyword, message, giftAmount.FormatAsCurrency(), financialAccount, setupLink );
            }

            // Validation has passed so prepare the automated payment processor args to charge the payment
            var automatedPaymentArgs = new AutomatedPaymentArgs
            {
                AuthorizedPersonAliasId = person.PrimaryAliasId.Value,
                AutomatedGatewayId = defaultSavedAccount.FinancialGatewayId.Value,
                FinancialPersonSavedAccountId = defaultSavedAccount.Id,
                FinancialSourceGuid = SystemGuid.DefinedValue.FINANCIAL_SOURCE_TYPE_SMS_GIVE.AsGuidOrNull(),
                AutomatedPaymentDetails = new List<AutomatedPaymentArgs.AutomatedPaymentDetailArgs>
                {
                    new AutomatedPaymentArgs.AutomatedPaymentDetailArgs {
                        AccountId = financialAccount.Id,
                        Amount = giftAmount
                    }
                }
            };

            // Determine if this is a future transaction and when it should be processed
            var minutesDelay = GetDelayMinutes( action );

            if ( minutesDelay.HasValue && minutesDelay > 0 )
            {
                var delayTimeSpan = TimeSpan.FromMinutes( minutesDelay.Value );
                automatedPaymentArgs.FutureProcessingDateTime = RockDateTime.Now.Add( delayTimeSpan );
            }

            // Create the processor
            var automatedPaymentProcessor = new AutomatedPaymentProcessor( null, automatedPaymentArgs, rockContext );

            // If the args are not valid send the setup response
            if ( !automatedPaymentProcessor.AreArgsValid( out errorMessage ) )
            {
                var lavaTemplate = GetHelpResponse( action );
                return GetResolvedSmsResponse( lavaTemplate, givingKeyword, message, giftAmount.FormatAsCurrency(), financialAccount, setupLink );
            }

            // If charge seems like a duplicate or repeat, tell the sender
            if ( automatedPaymentProcessor.IsRepeatCharge( out errorMessage ) )
            {
                return new SmsMessage
                {
                    ToNumber = message.FromNumber,
                    FromNumber = message.ToNumber,
                    Message = "It looks like you've given very recently. In order for us to avoid accidental charges, please wait several minutes before giving again. Thank you!"
                };
            }

            // Charge the payment and send an appropriate response
            var transaction = automatedPaymentProcessor.ProcessCharge( out errorMessage );

            if ( transaction == null || !string.IsNullOrEmpty( errorMessage ) )
            {
                var lavaTemplate = GetHelpResponse( action );
                return GetResolvedSmsResponse( lavaTemplate, givingKeyword, message, giftAmount.FormatAsCurrency(), financialAccount, setupLink );
            }

            // Tag the transaction's summary with info from this action
            transaction.Summary = string.Format( "{0}{1}Text To Give from {2} with the message `{3}`",
                transaction.Summary,
                transaction.Summary.IsNullOrWhiteSpace() ? string.Empty : ". ",
                message.FromNumber,
                messageText );
            rockContext.SaveChanges();

            // Let the sender know that the gift was a success
            var successTemplate = GetSuccessResponse( action );
            return GetResolvedSmsResponse( successTemplate, givingKeyword, message, giftAmount.FormatAsCurrency(), financialAccount, setupLink );
        }

        #endregion

        #region Refund

        /// <summary>
        /// Return true if the message text is requesting a refund
        /// </summary>
        /// <param name="action"></param>
        /// <param name="messageText"></param>
        /// <returns></returns>
        public bool IsRefundMessage( SmsActionCache action, string messageText )
        {
            var refundKeyword = GetRefundKeyword( action );

            if ( refundKeyword.IsNullOrWhiteSpace() )
            {
                return false;
            }

            return messageText.Equals( refundKeyword, StringComparison.CurrentCultureIgnoreCase );
        }

        /// <summary>
        /// Handles the action of giving a refund if the message requests it
        /// </summary>
        /// <param name="action"></param>
        /// <param name="message"></param>
        /// <param name="errorMessage"></param>
        /// <returns></returns>
        private SmsMessage DoRefund( SmsActionCache action, SmsMessage message, out string errorMessage )
        {
            errorMessage = string.Empty;
            var rockContext = new RockContext();

            var person = message.FromPerson;
            var keyword = GetRefundKeyword( action );

            if ( person == null || !person.PrimaryAliasId.HasValue )
            {
                var template = GetRefundFailureResponse( action );
                return GetResolvedRefundSmsResponse( template, keyword, message, null );
            }

            var smsGiftSourceId = DefinedValueCache.GetId( SystemGuid.DefinedValue.FINANCIAL_SOURCE_TYPE_SMS_GIVE );
            var service = new FinancialTransactionService( rockContext );
            var futureTransactionToDelete = service.GetFutureTransactions()
                .Where( ft =>
                    ft.SourceTypeValueId == smsGiftSourceId
                    && ft.AuthorizedPersonAliasId == person.PrimaryAliasId )
                .OrderByDescending( ft => ft.FutureProcessingDateTime )
                .FirstOrDefault();

            // If there is a future transaction, it can simply be deleted since it has not been processed yet
            if ( futureTransactionToDelete != null )
            {
                // Generate the responses ahead of time because once the transaction is deleted, the object is modified and the amount and account
                // are not accessible
                var errorResponse = GetResolvedRefundSmsResponse( GetRefundFailureResponse( action ), keyword, message, futureTransactionToDelete );
                var canDelete = service.CanDelete( futureTransactionToDelete, out errorMessage );

                if ( !canDelete )
                {
                    return errorResponse;
                }

                var successResponse = GetResolvedRefundSmsResponse( GetRefundSuccessResponse( action ), keyword, message, futureTransactionToDelete );
                var success = service.Delete( futureTransactionToDelete );

                rockContext.SaveChanges();

                return success ? successResponse : errorResponse;
            }

            var lavaTemplate = GetRefundFailureResponse( action );
            return GetResolvedRefundSmsResponse( lavaTemplate, keyword, message, null );
        }

        #endregion

        #region Model Helpers

        /// <summary>
        /// Create a new person with the phone number in the SMS message
        /// </summary>
        /// <param name="rockContext"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        private Person CreateNewPerson( RockContext rockContext, SmsMessage message )
        {
            if ( message.FromPerson != null )
            {
                return message.FromPerson;
            }

            // If the person is unknown (meaning Rock doesn't have the number, create a new person, store the number
            // and then tie future SMS gifts to this new person
            var person = new Person();
            PersonService.SaveNewPerson( person, rockContext );

            var numberTypeId = DefinedValueCache.Get( new Guid( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_MOBILE ) ).Id;
            person.PhoneNumbers.Add( new PhoneNumber
            {
                NumberTypeValueId = numberTypeId,
                Number = PhoneNumber.CleanNumber( message.FromNumber ),
                IsMessagingEnabled = true
            } );

            rockContext.SaveChanges();
            return person;
        }

        /// <summary>
        /// Get the person's default saved account if they have one.
        /// </summary>
        /// <param name="rockContext"></param>
        /// <param name="person"></param>
        /// <returns></returns>
        private static FinancialPersonSavedAccount GetDefaultSavedAccount( RockContext rockContext, Person person )
        {
            if ( person == null )
            {
                return null;
            }

            return new FinancialPersonSavedAccountService( rockContext )
                .GetByPersonId( person.Id )
                .AsNoTracking()
                .FirstOrDefault( sa => sa.IsDefault && sa.FinancialGatewayId.HasValue );
        }

        #endregion

        #region Parsing Helpers

        /// <summary>
        /// Parse the gift amount from the message text.  Expected format is something like "{{keyword}} {{amount}}"
        /// </summary>
        /// <param name="keyword"></param>
        /// <param name="messageText"></param>
        /// <returns></returns>
        private static decimal? GetGiftAmount( string keyword, string messageText )
        {
            messageText = messageText.Trim();
            keyword = keyword.Trim();
            var textWithoutKeyword = Regex.Replace( messageText, keyword, string.Empty, RegexOptions.IgnoreCase ).Trim();

            // First try to parse a decimal like "1123.56"
            var successfulParse = decimal.TryParse( textWithoutKeyword, out var parsedValue );

            // Second try to parse currency like "$1,123.56"
            if ( !successfulParse )
            {
                successfulParse = decimal.TryParse( textWithoutKeyword, NumberStyles.Currency, CultureInfo.CurrentCulture, out parsedValue );
            }

            if ( successfulParse )
            {
                return decimal.Round( parsedValue, 2 );
            }

            return null;
        }

        #endregion

        #region Attribute Helpers

        /// <summary>
        /// Use this method with the refund action to get a formatted response
        /// </summary>
        /// <param name="lavaTemplate"></param>
        /// <param name="keyword"></param>
        /// <param name="message"></param>
        /// <param name="transactionToRefund"></param>
        /// <returns></returns>
        private static SmsMessage GetResolvedRefundSmsResponse( string lavaTemplate, string keyword, SmsMessage message, FinancialTransaction transactionToRefund )
        {
            if ( transactionToRefund != null && transactionToRefund.TransactionDetails != null && transactionToRefund.TransactionDetails.Any() )
            {
                var formattedGiftAmount = transactionToRefund.TotalAmount.FormatAsCurrency();
                var firstDetail = transactionToRefund.TransactionDetails.FirstOrDefault();
                var account = firstDetail?.Account;
                return GetResolvedSmsResponse( lavaTemplate, keyword, message, formattedGiftAmount, account, null );
            }

            return GetResolvedSmsResponse( lavaTemplate, keyword, message, null, null, null );
        }

        /// <summary>
        /// Take the lava template, resolve it with useful text-to-give fields, and generate an SMS object to respond with
        /// </summary>
        /// <param name="lavaTemplate"></param>
        /// <param name="keyword"></param>
        /// <param name="message"></param>
        /// <param name="formattedAmount"></param>
        /// <param name="accountName"></param>
        /// <returns></returns>
        private static SmsMessage GetResolvedSmsResponse( string lavaTemplate, string keyword, SmsMessage message, string formattedAmount, FinancialAccount financialAccount, string setupLink )
        {
            var accountName = financialAccount == null ? null : financialAccount.PublicName;

            var mergeObjects = new Dictionary<string, object>
            {
                { "Message", message },
                { "Keyword", keyword },
                { "Amount", formattedAmount },
                { "AccountName", accountName },
                { "SetupLink", setupLink }
            };

            // Resolve the lava template with the lava fields
            var resolvedMessage = lavaTemplate.ResolveMergeFields( mergeObjects );

            // Generate a reply SMS object
            return new SmsMessage
            {
                ToNumber = message.FromNumber,
                FromNumber = message.ToNumber,
                Message = resolvedMessage
            };
        }

        #endregion

        #region Giving Attribute Getters

        /// <summary>
        /// Get and validate the keyword attribute
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        private static string GetGivingKeyword( SmsActionCache action )
        {
            if ( action == null )
            {
                throw new ArgumentException( "Parameter cannot be null", "action" );
            }

            var keyword = action.GetAttributeValue( AttributeKeys.GivingKeyword );

            if ( string.IsNullOrWhiteSpace( keyword ) )
            {
                throw new ArgumentException( "Attribute cannot be empty", AttributeKeys.GivingKeyword );
            }

            return keyword.Trim();
        }

        /// <summary>
        /// Get and validate the setup keyword attribute
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        private static string GetSetupKeyword( SmsActionCache action )
        {
            if ( action == null )
            {
                throw new ArgumentException( "Parameter cannot be null", "action" );
            }

            var keyword = action.GetAttributeValue( AttributeKeys.SetupKeyword );

            if ( string.IsNullOrWhiteSpace( keyword ) )
            {
                throw new ArgumentException( "Attribute cannot be empty", AttributeKeys.SetupKeyword );
            }

            return keyword.Trim();
        }

        /// <summary>
        /// Get and validate the max amount attribute
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        private static decimal? GetMaxAmount( SmsActionCache action )
        {
            if ( action == null )
            {
                throw new ArgumentException( "Parameter cannot be null", "action" );
            }

            var maxAmountString = action.GetAttributeValue( AttributeKeys.MaxAmount );
            return maxAmountString.AsDecimalOrNull();
        }

        /// <summary>
        /// Get and validate the financial account attribute
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        private static FinancialAccount GetFinancialAccount( SmsActionCache action, RockContext rockContext )
        {
            if ( action == null )
            {
                throw new ArgumentException( "Parameter cannot be null", "action" );
            }

            var guid = action.GetAttributeValue( AttributeKeys.FinancialAccount ).AsGuidOrNull();

            if ( !guid.HasValue )
            {
                return null;
            }

            var service = new FinancialAccountService( rockContext );
            return service.Get( guid.Value );
        }

        /// <summary>
        /// Get and validate the setup page link
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        private static string GetSetupPageLink( SmsActionCache action, Person person )
        {
            if ( action == null )
            {
                throw new ArgumentException( "Parameter cannot be null", "action" );
            }

            if ( person == null )
            {
                throw new ArgumentException( "Parameter cannot be null", "person" );
            }

            var setupPageAttribute = action.GetAttributeValue( AttributeKeys.SetupPage );

            if ( setupPageAttribute.IsNullOrWhiteSpace() )
            {
                return null;
            }

            var setupPage = new Rock.Web.PageReference( setupPageAttribute );

            if ( setupPage == null )
            {
                return null;
            }

            // create a limited-use personkey that will last long enough for them to go thru all the 'postbacks' while posting a transaction
            var expiresInMinutes = 30;
            var expiresDateTime = RockDateTime.Now.AddMinutes( expiresInMinutes );
            var personKey = person.GetImpersonationToken( expiresDateTime, null, null );

            setupPage.Parameters["Person"] = personKey;
            return setupPage.BuildUrl();
        }

        #endregion

        #region Refund Attribute Getters

        /// <summary>
        /// Get and validate the keyword attribute
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        private static string GetRefundKeyword( SmsActionCache action )
        {
            if ( action == null )
            {
                throw new ArgumentException( "Parameter cannot be null", "action" );
            }

            var keyword = action.GetAttributeValue( AttributeKeys.RefundKeyword );

            if ( keyword.IsNullOrWhiteSpace() )
            {
                return null;
            }

            return keyword.Trim();
        }

        /// <summary>
        /// Get and validate the delay minutes attribute
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        private static int? GetDelayMinutes( SmsActionCache action )
        {
            if ( action == null )
            {
                throw new ArgumentException( "Parameter cannot be null", "action" );
            }

            var minutes = action.GetAttributeValue( AttributeKeys.ProcessingDelayMinutes ).AsIntegerOrNull();
            return minutes;
        }

        #endregion

        #region Response Attribute Getters

        /// <summary>
        /// Get and validate the success response attribute
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        private static string GetSuccessResponse( SmsActionCache action )
        {
            if ( action == null )
            {
                throw new ArgumentException( "Parameter cannot be null", "action" );
            }

            var response = action.GetAttributeValue( AttributeKeys.SuccessResponse );

            if ( string.IsNullOrWhiteSpace( response ) )
            {
                throw new ArgumentException( "Attribute cannot be empty", AttributeKeys.SuccessResponse );
            }

            return response.Trim();
        }

        /// <summary>
        /// Get and validate the help response attribute
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        private static string GetHelpResponse( SmsActionCache action )
        {
            if ( action == null )
            {
                throw new ArgumentException( "Parameter cannot be null", "action" );
            }

            var helpResponse = action.GetAttributeValue( AttributeKeys.HelpResponse );

            if ( string.IsNullOrWhiteSpace( helpResponse ) )
            {
                throw new ArgumentException( "Attribute cannot be empty", AttributeKeys.HelpResponse );
            }

            return helpResponse.Trim();
        }

        /// <summary>
        /// Get and validate the setup response attribute
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        private static string GetSetupResponse( SmsActionCache action )
        {
            if ( action == null )
            {
                throw new ArgumentException( "Parameter cannot be null", "action" );
            }

            var response = action.GetAttributeValue( AttributeKeys.SetupResponse );

            if ( string.IsNullOrWhiteSpace( response ) )
            {
                throw new ArgumentException( "Attribute cannot be empty", AttributeKeys.SetupResponse );
            }

            return response.Trim();
        }

        /// <summary>
        /// Get and validate the max amount exceeded response attribute
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        private static string GetMaxAmountResponse( SmsActionCache action )
        {
            if ( action == null )
            {
                throw new ArgumentException( "Parameter cannot be null", "action" );
            }

            var response = action.GetAttributeValue( AttributeKeys.MaxAmountResponse );
            return response?.Trim();
        }

        /// <summary>
        /// Get and validate the success response attribute
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        private static string GetRefundSuccessResponse( SmsActionCache action )
        {
            if ( action == null )
            {
                throw new ArgumentException( "Parameter cannot be null", "action" );
            }

            var response = action.GetAttributeValue( AttributeKeys.RefundSuccessResponse );

            if ( string.IsNullOrWhiteSpace( response ) )
            {
                throw new ArgumentException( "Attribute cannot be empty", AttributeKeys.RefundSuccessResponse );
            }

            return response.Trim();
        }

        /// <summary>
        /// Get and validate the failure response attribute
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        private static string GetRefundFailureResponse( SmsActionCache action )
        {
            if ( action == null )
            {
                throw new ArgumentException( "Parameter cannot be null", "action" );
            }

            var response = action.GetAttributeValue( AttributeKeys.RefundFailureResponse );

            if ( string.IsNullOrWhiteSpace( response ) )
            {
                throw new ArgumentException( "Attribute cannot be empty", AttributeKeys.RefundFailureResponse );
            }

            return response.Trim();
        }

        #endregion
    }
}
